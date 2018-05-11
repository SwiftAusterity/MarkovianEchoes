using Echoes.Data.Contextual;
using Echoes.DataStructure.Contextual;
using System.Linq;
using System.Text;
using Utility;

namespace Echoes.Data.Interp
{
    public static class GrammaticalReconstructor
    {
        public static string ConstructFromMemory(IAkashicEntry memory)
        {
            var sb = new StringBuilder();

            //spoken is just regurgiation at the moment
            if(memory.Spoken)
            {
                sb.Append(memory.Observance);
            }
            else
            {
                var verbs = memory.Context.Where(ctx => ctx.GetType() == typeof(Verb));
                var adjectives = memory.Context.Where(ctx => ctx.GetType() == typeof(Descriptor));
                var nouns = memory.Context.Where(ctx => ctx.GetType() == typeof(Noun));

                //Simple form
                var verbString = verbs.Select(verb => verb.Name.EndsWith("s") ? verb.Name : verb.Name + "s").CommaList(StringUtility.SplitListType.OxfordComma);
                var adjString = adjectives.Select(adj =>adj.Name).CommaList(StringUtility.SplitListType.OxfordComma);
                var subjectString = nouns.Select(adj => adj.Name).CommaList(StringUtility.SplitListType.OxfordComma);

                if (subjectString.StartsWith("the "))
                    subjectString.Replace("the ", string.Empty);

                adjString = "the " + adjString;

                sb.AppendFormat("{0} {1} {2}{3}", memory.Actor.Name, verbString, adjString, subjectString);
            }

            return sb.ToString();
        }
    }
}
