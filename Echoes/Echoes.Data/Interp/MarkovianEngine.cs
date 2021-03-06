﻿using Cottontail.Cache;
using Cottontail.FileSystem;
using Cottontail.FileSystem.Logging;
using Echoes.Data.Contextual;
using Echoes.Data.Entity;
using Echoes.DataStructure.Contextual;
using Echoes.DataStructure.Entity;
using Echoes.DataStructure.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Utility;

namespace Echoes.Data.Interp
{
    // Yes this file should be in its own project but I was sick of fighting circular dependencies

    /// <summary>
    /// Engine for interpreting and merging observances
    /// </summary>
    public class MarkovianEngine
    {
        internal StoredDataCache DataCache { get; private set; }
        internal StoredDataFileAccessor DataStore { get; private set; }
        internal FileLogger Logger { get; private set; }

        public MarkovianEngine(StoredDataFileAccessor storedData, StoredDataCache storedDataCache, FileLogger logger)
        {
            DataStore = storedData;
            DataCache = storedDataCache;
            Logger = logger;
        }

        /// <summary>
        /// Initial experience, takes in an observance and emits the new contexts related to it. Merge and Convey will merge new contexts into existing ones
        /// </summary>
        /// <param name="observer">Who did the observations</param>
        /// <param name="actor">Who did the action</param>
        /// <param name="observance">The raw input that was observed</param>
        /// <param name="acting">Whether or not this is an action or speech</param>
        /// <returns>A list of the new contexts generated</returns>
        public IEnumerable<IContext> Experience(IEntity observer, IEntity actor, string observance, bool acting)
        {
            var returnList = Enumerable.Empty<IContext>();
            var sentences = IsolateSentences(observance);

            foreach (var sentence in sentences)
            {
                var words = IsolateIndividuals(sentence, observer, actor);

                //can't parse nothing
                if (words.Count == 0)
                    return returnList;

                //Get rid of the imperative self declaration
                if (words.First().Item1.Equals("i") || words.First().Item1.Equals("me"))
                    words.RemoveAt(0);

                if (acting)
                    returnList = ParseAction(observer, actor, words);
                else
                    returnList = ParseSpeech(observer, actor, words);
            }

            return returnList;
        }

        /// <summary>
        /// Merges a new set of contexts into the existing set
        /// </summary>
        /// <param name="originContext">The existing context</param>
        /// <param name="newContext">The new context</param>
        public IEnumerable<IContext> Merge(IEnumerable<IContext> originContext, IEnumerable<IContext> newContext)
        {
            var returnContext = new List<IContext>(originContext);

            foreach (var item in newContext)
            {
                item.Strength++;

                if (originContext.Any(ctx => ctx.Name.Equals(item.Name)))
                {
                    foreach (var currentContext in originContext.Where(ctx => ctx.Name.Equals(item.Name)))
                    {
                        returnContext.Remove(currentContext);

                        if (currentContext.GetType() != item.GetType())
                        {
                            currentContext.Strength--;

                            if (currentContext.Strength <= 0)
                            {
                                item.Strength = 1;
                                returnContext.Add(item);
                            }
                            else
                                returnContext.Add(currentContext);
                        }
                        else
                        {
                            item.Strength += currentContext.Strength;
                            returnContext.Add(item);
                        }
                    }
                }
                else
                    returnContext.Add(item);
            }

            return returnContext;
        }

        /*
         * TODO: Wow this is inefficient, maybe clean up how many loops we do
         */
        private IEnumerable<IContext> ParseAction(IEntity observer, IEntity actor, IList<Tuple<string, bool>> words)
        {
            /*
             * I kick the can 
             * kick the can
             * kicks the can
             * kick the red can
             * kick the large red can
             */
            var returnList = new List<IContext>();
            IPlace currentPlace = (IPlace)observer.Position;

            var brandedWords = BrandWords(observer, actor, words, currentPlace);

            IVerb currentVerb = null;

            //No verb?
            if (!brandedWords.Any(ctx => ctx.Value?.GetType() == typeof(Verb)))
            {
                var verbWord = brandedWords.First(ctx => ctx.Value == null).Key;

                currentVerb = new Verb() { Name = verbWord };
                brandedWords[verbWord] = currentVerb;
            }
            else
                currentVerb = (IVerb)brandedWords.FirstOrDefault(ctx => ctx.Value?.GetType() == typeof(Verb)).Value;

            //We might have nouns already
            if (!brandedWords.Any(ctx => ctx.Value?.GetType() == typeof(Noun)))
            {
                string targetWord = string.Empty;

                //No valid nouns to make the target? Pick the last one
                if (!brandedWords.Any(ctx => ctx.Value == null))
                    targetWord = brandedWords.LastOrDefault().Key;
                else
                    targetWord = brandedWords.LastOrDefault(ctx => ctx.Value == null).Key;

                brandedWords[targetWord] = new Noun() { Name = targetWord };
            }

            var descriptors = new List<Descriptor>();
            foreach (var adjective in brandedWords.Where(ctx => ctx.Value == null))
            {
                descriptors.Add(new Descriptor() { Name = adjective.Key });
            }

            //Add the nonadjectives and the adjectives
            returnList.AddRange(brandedWords.Where(bws => bws.Value != null).Select(bws => bws.Value));
            returnList.AddRange(descriptors.Select(desc => desc));

            //Don't add things unless we're in the place itself
            if (observer == currentPlace)
            {
                var contextList = new List<IContext>();

                foreach (var item in returnList.Where(it => it?.GetType() == typeof(Descriptor)))
                {
                    var desc = (Descriptor)item;

                    contextList.Add(new Descriptor()
                    {
                        Applied = true,
                        Name = desc.Name,
                        Opposite = desc.Opposite
                    });
                }

                foreach (var noun in brandedWords.Where(ctx => ctx.Value?.GetType() == typeof(Noun)))
                {
                    if (currentPlace.Name.Equals(noun.Key) || currentPlace.PersonaInventory.Any(thing => thing.Name.Equals(noun.Key, StringComparison.InvariantCultureIgnoreCase)))
                        continue;

                    if (!currentPlace.ThingInventory.Any(thing => thing.Name.Equals(noun.Key, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        //make new thing
                        var newThing = new Thing(DataStore, DataCache, Logger)
                        {
                            Name = noun.Key
                        };

                        newThing.FullContext = contextList;

                        newThing.Create();

                        currentPlace.MoveInto(newThing);
                    }
                    else
                    {
                        var newThing = currentPlace.ThingInventory.FirstOrDefault(thing => thing.Name.Equals(noun.Key, StringComparison.InvariantCultureIgnoreCase));

                        newThing.ConveyMeaning(contextList);
                        newThing.Save();
                    }
                }
            }

            return returnList;
        }

        /*
         * TODO: First pass: parse out existing things, the place we're in and decorators
         * Second pass: search for places in the world to make links
         * Third pass: More robust logic to avoid extra merging later
         */
        private IEnumerable<IContext> ParseSpeech(IEntity observer, IEntity actor, IList<Tuple<string, bool>> words)
        {
            /*
             * hello
             * hi there
             * did you go to the store
             * what are you doing there
             * I saw a red ball in the living room
             */
            var returnList = new List<IContext>();

            IPlace currentPlace = (IPlace)observer.Position;

            var brandedWords = BrandWords(observer, actor, words, currentPlace);

            var allOtherPlaces = DataCache.GetAll<IPlace>().Where(place => place != currentPlace);

            var linkedPlaces = new List<IPlace>();
            foreach (var place in allOtherPlaces)
            {
                if (brandedWords.ContainsKey(place.Name))
                    continue;

                brandedWords.Remove(place.Name);
                linkedPlaces.Add(place);
            }

            var targetWord = string.Empty;

            //No valid nouns to make the target? Pick the last one
            if (!brandedWords.Any(ctx => ctx.Value == null))
                targetWord = brandedWords.LastOrDefault().Key;
            else
                targetWord = brandedWords.LastOrDefault(ctx => ctx.Value == null).Key;

            brandedWords.Remove(targetWord);

            var descriptors = new List<IDescriptor>();
            foreach (var adjective in brandedWords.Where(ctx => ctx.Value == null || ctx.Value?.GetType() == typeof(Descriptor)))
            {
                if (adjective.Value != null)
                    descriptors.Add((Descriptor)adjective.Value);
                else
                    descriptors.Add(new Descriptor() { Name = adjective.Key });
            }

            returnList.AddRange(descriptors);

            if (observer == currentPlace)
            {
                //Make a new place
                if (!allOtherPlaces.Any(place => place.Name.Equals(targetWord, StringComparison.InvariantCultureIgnoreCase)))
                {
                    var newPlace = new Place(DataStore, DataCache, Logger);
                    newPlace.Name = targetWord;
                    newPlace.LinkedPlaces.Add(currentPlace);
                    newPlace.Create();

                    currentPlace.LinkPlace(newPlace);
                }

                //Add the place links and places
                foreach (var place in linkedPlaces.Where(pl => !currentPlace.LinkedPlaces.Contains(pl)))
                {
                    currentPlace.LinkPlace(place);
                }

                currentPlace.Save();
            }

            return returnList;
        }

        private Dictionary<string, IContext> BrandWords(IEntity observer, IEntity actor, IList<Tuple<string, bool>> words, IPlace currentPlace)
        {
            var brandedWords = new Dictionary<string, IContext>();

            //Brand all the words with their current meaning. Continues are in there because the listword inflation might cause collision
            foreach (var word in words.Distinct())
            {
                if (brandedWords.ContainsKey(word.Item1))
                    continue;

                //We have a comma/and list
                if (word.Item2)
                {
                    var listWords = word.Item1.Split(new string[] { "and", ",", " " }, StringSplitOptions.RemoveEmptyEntries);

                    IContext listMeaning = null;
                    foreach (var listWord in listWords)
                    {
                        if (listMeaning != null)
                            break;

                        if (brandedWords.ContainsKey(listWord))
                            listMeaning = brandedWords[listWord];

                        if (listMeaning == null)
                            listMeaning = GetExistingMeaning(listWord, observer, actor, currentPlace);
                    }

                    foreach (var listWord in listWords)
                    {
                        if (brandedWords.ContainsKey(listWord))
                            continue;

                        brandedWords.Add(listWord, listMeaning);
                    }

                    continue;
                }

                brandedWords.Add(word.Item1, GetExistingMeaning(word.Item1, observer, actor, currentPlace));
            }

            return brandedWords;
        }

        private IContext GetExistingMeaning(string word, IEntity observer, IEntity actor, IPlace currentPlace)
        {
            var allContext = new List<string>();

            allContext.AddRange(actor.Position.GetThings().Select(thing => thing.Name));
            allContext.AddRange(actor.Position.GetPersonas().Select(thing => thing.Name));
            allContext.Add(actor.Position.Name);

            IContext existingMeaning = null;

            //It's a thing
            if (allContext.Contains(word))
            {
                existingMeaning = new Noun() { Name = word };
            }
            else
            {
                var observerType = observer.FullContext.FirstOrDefault(ctx => ctx.Name.Equals(word));

                if (observerType == null)
                {
                    var placeType = currentPlace.FullContext.FirstOrDefault(ctx => ctx.Name.Equals(word));

                    if (placeType != null)
                        existingMeaning = placeType;
                }
                else
                    existingMeaning = observerType;
            }

            return existingMeaning;
        }

        private IList<Tuple<string, bool>> IsolateIndividuals(string baseString, IEntity observer, IEntity actor)
        {
            int iterator = 0;
            baseString = baseString.ToLower();

            var foundStrings = ParseQuotesOut(ref baseString, ref iterator);

            foundStrings.AddRange(ParseEntitiesOut(observer, actor, ref iterator, ref baseString));

            foundStrings.AddRange(ParseCommaListsOut(ref iterator, ref baseString));

            var originalStrings = new List<string>();
            originalStrings.AddRange(RemoveGrammaticalNiceities(baseString.Split(new char[] { ' ', ',', ':' }, StringSplitOptions.RemoveEmptyEntries)));

            //So thanks to the commalist puller potentially adding replacement strings to the found collection we have to do a pass there first
            var cleanerList = new List<Tuple<int, string>>();
            foreach (var dirtyString in foundStrings.Where(str => str.Item1.Contains("%%")))
            {
                var dirtyIndex = foundStrings.IndexOf(dirtyString);
                var cleanString = dirtyString.Item1;

                while (cleanString.Contains("%%"))
                {
                    var i = TypeUtility.TryConvert<int>(cleanString.Substring(cleanString.IndexOf("%%") + 2, 1));
                    cleanString = cleanString.Replace(String.Format("%%{0}%%", i), foundStrings[i].Item1);
                }

                cleanerList.Add(new Tuple<int, string>(dirtyIndex, cleanString));
            }

            foreach (var cleaner in cleanerList)
            {
                var dirtyIndex = cleaner.Item1;
                var cleanString = cleaner.Item2;

                foundStrings[dirtyIndex] = new Tuple<string, bool>(cleanString, foundStrings[dirtyIndex].Item2);
            }

            //Either add the modified one or add the normal one
            var returnStrings = new List<Tuple<string, bool>>();
            foreach (var returnString in originalStrings)
            {
                if (returnString.StartsWith("%%") && returnString.EndsWith("%%"))
                {
                    var i = TypeUtility.TryConvert<int>(returnString.Substring(2, returnString.Length - 4));
                    returnStrings.Add(foundStrings[i]);
                }
                else
                    returnStrings.Add(new Tuple<string, bool>(returnString, false));
            }

            return returnStrings;
        }

        /*
         * word, word, word, word- ([a-zA-Z0-9_.-|(%%\d%%)]+)((,|,\s)[a-zA-Z0-9_.-|(%%\d%%)]+)+
         * word, word, word and word- ([a-zA-Z0-9_.-|(%%\d%%)]+)((,|,\s)[a-zA-Z0-9_.-|(%%\d%%)]+)+(\sand\s)([a-zA-Z0-9_.-|(%%\d%%)]+)
         * word, word, word, and word ([a-zA-Z0-9_.-|(%%\d%%)]+)((,|,\s)[a-zA-Z0-9_.-|(%%\d%%)]+)+(,\sand\s)([a-zA-Z0-9_.-|(%%\d%%)]+)
         * word and word and word and word- ([a-zA-Z0-9_.-|(%%\d%%)]+)((\sand\s)[a-zA-Z0-9_.-|(%%\d%%)]+)+
         */
        private IList<Tuple<string, bool>> ParseCommaListsOut(ref int iterator, ref string baseString)
        {
            var foundStrings = new List<Tuple<string, bool>>();
            var cccPattern = new Regex(@"([a-zA-Z0-9_.-|(%%\d%%)]+)((,|,\s)[a-zA-Z0-9_.-|(%%\d%%)]+)+", RegexOptions.IgnorePatternWhitespace);
            var ccaPattern = new Regex(@"([a-zA-Z0-9_.-|(%%\d%%)]+)((,|,\s)[a-zA-Z0-9_.-|(%%\d%%)]+)+(\sand\s)([a-zA-Z0-9_.-|(%%\d%%)]+)", RegexOptions.IgnorePatternWhitespace);
            var ccacPattern = new Regex(@"([a-zA-Z0-9_.-|(%%\d%%)]+)((,|,\s)[a-zA-Z0-9_.-|(%%\d%%)]+)+(,\sand\s)([a-zA-Z0-9_.-|(%%\d%%)]+)", RegexOptions.IgnorePatternWhitespace);
            var aaaPattern = new Regex(@"([a-zA-Z0-9_.-|(%%\d%%)]+)((\sand\s)[a-zA-Z0-9_.-|(%%\d%%)]+)+", RegexOptions.IgnorePatternWhitespace);

            foundStrings.AddRange(RunListPattern(ccacPattern, ref iterator, ref baseString));
            foundStrings.AddRange(RunListPattern(ccaPattern, ref iterator, ref baseString));
            foundStrings.AddRange(RunListPattern(aaaPattern, ref iterator, ref baseString));
            foundStrings.AddRange(RunListPattern(cccPattern, ref iterator, ref baseString));

            return foundStrings;
        }

        private IList<Tuple<string, bool>> RunListPattern(Regex capturePattern, ref int iterator, ref string baseString)
        {
            var foundStrings = new List<Tuple<string, bool>>();

            var cccMatches = capturePattern.Matches(baseString);
            for (var i = 0; i < cccMatches.Count; i++)
            {
                var currentMatch = cccMatches[i];

                if (currentMatch == null || !currentMatch.Success)
                    continue;

                var cccCaptures = currentMatch.Captures;
                for (var iC = 0; iC < cccCaptures.Count; iC++)
                {
                    var currentCapture = cccCaptures[iC];

                    if (currentCapture == null || currentCapture.Length == 0)
                        continue;

                    var commaList = currentCapture.Value;

                    foundStrings.Add(new Tuple<string, bool>(commaList, true));
                    baseString = baseString.Replace(commaList, "%%" + iterator.ToString() + "%%");
                    iterator++;
                }
            }

            return foundStrings;
        }

        private IList<Tuple<string, bool>> ParseEntitiesOut(IEntity observer, IEntity actor, ref int iterator, ref string baseString)
        {
            var foundStrings = new List<Tuple<string, bool>>();
            var allContext = new List<string>();

            allContext.AddRange(actor.Position.GetThings().Select(thing => thing.Name));
            allContext.AddRange(actor.Position.GetPersonas().Select(thing => thing.Name));
            allContext.Add(actor.Position.Name);

            allContext.AddRange(observer.FullContext.Select(ctx => ctx.Name));
            allContext.AddRange(actor.Position.FullContext.Select(ctx => ctx.Name));

            //Brand all the words with their current meaning
            foreach (var word in allContext.Distinct())
            {
                if (baseString.Contains(word))
                {
                    foundStrings.Add(new Tuple<string, bool>(word, false));
                    baseString = baseString.Replace(word, "%%" + iterator.ToString() + "%%");
                    iterator++;
                }
            }

            return foundStrings;
        }

        /// <summary>
        /// Scrubs "s and 's out and figures out what the parameters really are
        /// </summary>
        /// <returns>the right parameters</returns>
        private List<Tuple<string, bool>> ParseQuotesOut(ref string baseString, ref int iterator)
        {
            var foundStrings = new List<Tuple<string, bool>>();

            baseString = IsolateStrings(baseString, "\"", foundStrings, ref iterator);
            baseString = IsolateStrings(baseString, "'", foundStrings, ref iterator);

            return foundStrings;
        }

        //Do we have magic string collectors? quotation marks demarcate a single parameter being passed in
        private string IsolateStrings(string baseString, string closure, List<Tuple<string, bool>> foundStrings, ref int foundStringIterator)
        {
            while (baseString.Contains("\""))
            {
                var firstQuoteIndex = baseString.IndexOf('"');
                var secondQuoteIndex = baseString.IndexOf('"', firstQuoteIndex + 1);

                //What? Why would this even happen
                if (firstQuoteIndex < 0)
                    break;

                //Only one means let's just kill the stupid quotemark and move on
                if (secondQuoteIndex < 0)
                {
                    baseString = baseString.Replace("\"", string.Empty);
                    break;
                }

                var foundString = baseString.Substring(firstQuoteIndex + 1, secondQuoteIndex - firstQuoteIndex - 1);

                foundStrings.Add(new Tuple<string, bool>(foundString, false));
                baseString = baseString.Replace(string.Format("\"{0}\"", foundString), "%%" + foundStringIterator.ToString() + "%%");
                foundStringIterator++;
            }

            return baseString;
        }

        IEnumerable<string> IsolateSentences(string input)
        {
            var sentences = new List<string>();

            //TODO: recognize "and <verb>"
            var initialSplit = input.Split(new string[] { ";", "?", ". ", "!" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var phrase in initialSplit)
            {
                var potentialWords = phrase.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (potentialWords.Count() > 1)
                {
                    sentences.Add(phrase);
                }
            }

            if (sentences.Count > 1)
                return sentences;

            //Fall back to just the initial sentence because we couldn't find multiple full sentences.
            return new List<string>() { input };
        }

        /// <summary>
        /// Removes stuff we don't care about like to, into, the, etc
        /// </summary>
        /// <param name="currentParams">The current set of params</param>
        /// <returns>the scrubbed params</returns>
        private IList<string> RemoveGrammaticalNiceities(IList<string> currentParams)
        {
            var parmList = currentParams.ToList();

            parmList.RemoveAll(str => str.Equals("the", StringComparison.InvariantCulture)
                                        || str.Equals("of", StringComparison.InvariantCulture)
                                        || str.Equals("to", StringComparison.InvariantCulture)
                                        || str.Equals("into", StringComparison.InvariantCulture)
                                        || str.Equals("in", StringComparison.InvariantCulture)
                                        || str.Equals("from", StringComparison.InvariantCulture)
                                        || str.Equals("inside", StringComparison.InvariantCulture)
                                        || str.Equals("at", StringComparison.InvariantCulture)
                                        || str.Equals("a", StringComparison.InvariantCulture)
                                        || str.Equals("an", StringComparison.InvariantCulture)
                                        || str.Equals("that", StringComparison.InvariantCulture)
                                        || str.Equals("this", StringComparison.InvariantCulture)
                                  );

            return parmList;
        }
    }
}
