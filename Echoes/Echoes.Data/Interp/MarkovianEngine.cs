using Cottontail.Cache;
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

namespace Echoes.Data.Interp
{
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
            var words = RemoveGrammaticalNiceities(ParseQuotesOut(observance));

            //can't parse nothing man
            if (words.Count == 0)
                return returnList;

            if (acting)
                returnList = ParseAction(observer, actor, words);
            else
                returnList = ParseSpeech(observer, actor, words);

            return returnList;
        }

        /// <summary>
        /// Merges a new set of contexts into the existing set
        /// </summary>
        /// <param name="originContext">The existing context</param>
        /// <param name="newContext">The new context</param>
        public IEnumerable<IContext> Merge(List<IContext> originContext, IEnumerable<IContext> newContext)
        {
            originContext.AddRange(newContext);

            return originContext;
        }

        /*
         * TODO: First pass: parse out new things and verbs
         * Second pass: check existing room for things and known decorators
         * Third pass: More robust logic to avoid extra merging later
         *
         */
        private IEnumerable<IContext> ParseAction(IEntity observer, IEntity actor, IList<string> words)
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

            //Get rid of the imperative self declaration
            if (words.First().Equals("i") || words.First().Equals("me"))
                words.RemoveAt(0);

            var brandedWords = new Dictionary<string, IContext>();

            //Brand all the words with their current meaning
            foreach(var word in words.Distinct())
            {
                IContext existingMeaning = null;
                var actorType = actor.FullContext.FirstOrDefault(ctx => ctx.Name.Equals(word));

                if (actorType == null)
                {
                    var placeType = currentPlace.FullContext.FirstOrDefault(ctx => ctx.Name.Equals(word));

                    if (placeType == null)
                    {
                        var observerType = observer.FullContext.FirstOrDefault(ctx => ctx.Name.Equals(word));

                        if (observerType != null)
                            existingMeaning = observerType;
                    }
                    else
                        existingMeaning = placeType;
                }
                else
                    existingMeaning = actorType;

                brandedWords.Add(word, existingMeaning);
            }

            IVerb currentVerb = null;

            //No verb?
            if (!brandedWords.Any(ctx => ctx.Value?.GetType() == typeof(IVerb)))
            {
                var verbWord = brandedWords.First(ctx => ctx.Value == null).Key;

                currentVerb = new Verb() { Name = verbWord };
                brandedWords[verbWord] = currentVerb;
            }
            else
                currentVerb = (IVerb)brandedWords.FirstOrDefault(ctx => ctx.Value.GetType() == typeof(IVerb)).Value;

            string targetWord = string.Empty;

            //No valid nouns to make the target? Pick the last one
            if (!brandedWords.Any(ctx => ctx.Value == null))
                targetWord = brandedWords.LastOrDefault().Key;
            else
                targetWord = brandedWords.LastOrDefault(ctx => ctx.Value == null).Key;

            HashSet<Tuple<ActionType, string>> actionsList = new HashSet<Tuple<ActionType, string>>();

            if (currentVerb.Affects.ContainsKey(targetWord))
                actionsList = currentVerb.Affects[targetWord];

            var descriptors = new List<IDescriptor>();
            foreach (var adjective in brandedWords.Where(ctx => !ctx.Key.Equals(targetWord) && (ctx.Value == null || ctx.Value?.GetType() == typeof(IDescriptor))))
            {
                var existingPair = actionsList.FirstOrDefault(value => value.Item2.Equals(adjective.Key));

                actionsList.Add(new Tuple<ActionType, string>(ActionType.Apply, adjective.Key));

                if (adjective.Value != null)
                    descriptors.Add((IDescriptor)adjective.Value);
                else
                    descriptors.Add(new Descriptor() { Name = adjective.Key });
            }

            returnList.AddRange(descriptors);
            returnList.Add(currentVerb);

            if (!currentPlace.ThingInventory.Any(thing => thing.Name.Equals(targetWord, StringComparison.InvariantCultureIgnoreCase))
                && !currentPlace.PersonaInventory.Any(thing => thing.Name.Equals(targetWord, StringComparison.InvariantCultureIgnoreCase)))
            {
                //make new thing
                var newThing = new Thing(DataStore, DataCache, Logger)
                {
                    Name = targetWord
                };

                currentPlace.MoveInto(newThing);

                newThing.FullContext = returnList;

                newThing.Create();
            }

            return returnList;
        }

        /*
         * TODO: First pass: parse out existing things, the place we're in and decorators
         * Second pass: search for places in the world to make links
         * Third pass: More robust logic to avoid extra merging later
         *
         */
        private IEnumerable<IContext> ParseSpeech(IEntity observer, IEntity actor, IList<string> words)
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
            var existingNames = new List<string>
            {
                currentPlace.Name
            };

            existingNames.AddRange(currentPlace.ThingInventory.Select(thing => thing.Name));
            existingNames.AddRange(currentPlace.PersonaInventory.Select(thing => thing.Name));

            var adjectives = new List<Tuple<string, string>>();
            var descriptors = new List<IDescriptor>();

            if (words.Count > 1)
            {
                for (int i = 1; i < words.Count; i++)
                {
                    if (existingNames.Contains(words[i]) && !existingNames.Contains(words[i - 1]))
                    {
                        adjectives.Add(new Tuple<string, string>(words[i - 1], words[i]));
                        descriptors.Add(new Descriptor() { Name = words[i - 1] });
                    }
                }
            }

            returnList.AddRange(descriptors);

            return returnList;
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
                                  );

            return parmList;
        }

        /// <summary>
        /// Scrubs "s out and figures out what the parameters really are
        /// </summary>
        /// <returns>the right parameters</returns>
        private IList<string> ParseQuotesOut(string baseString)
        {
            baseString = baseString.ToLower();
            var originalStrings = new List<string>();

            int foundStringIterator = 0;
            var foundStrings = new List<string>();

            //Do we have magic string collectors? quotation marks demarcate a single parameter being passed in
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

                foundStrings.Add(foundString);
                baseString = baseString.Replace(string.Format("\"{0}\"", foundString), "%%" + foundStringIterator.ToString() + "%%");
                foundStringIterator++;
            }

            originalStrings.AddRange(baseString.Split(new char[] { ' ', ',', ';', '?', '.' }, StringSplitOptions.RemoveEmptyEntries));

            //Either add the modified one or add the normal one
            var iterator = 0;
            var returnStrings = new List<string>();
            foreach (var returnString in originalStrings)
            {
                if (returnString.Equals("%%" + iterator.ToString() + "%%"))
                {
                    returnStrings.Add(foundStrings[iterator]);
                    iterator++;
                }
                else
                    returnStrings.Add(returnString);
            }

            return returnStrings;
        }

    }
}
