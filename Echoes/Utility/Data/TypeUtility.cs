using System;
using System.Collections.Generic;
using System.Linq;

namespace Utility
{
    /// <summary>
    /// Type conversion utilities and extensions
    /// </summary>
    public static class TypeUtility
    {
        /// <summary>
        /// Fault safe type conversion with output reference and verification (like value type TryConvert)
        /// </summary>
        /// <typeparam name="T">the type to convert to</typeparam>
        /// <param name="thing">the thing being converted</param>
        /// <param name="newThing">the converted thing as output</param>
        /// <param name="def">The default value, which defaults to default(type)</param>
        /// <returns>success status</returns>
        public static bool TryConvert<T>(object thing, ref T newThing, T def = default(T))
        {
            try
            {
                if (thing == null)
                    return false;

                if (typeof(T).IsEnum)
                {
                    if (thing is short || thing is int)
                        newThing = (T)thing;
                    else
                        newThing = (T)Enum.Parse(typeof(T), thing.ToString());
                }
                else
                    newThing = (T)Convert.ChangeType(thing, typeof(T));

                return true;
            }
            catch
            {
                //dont error on tryconvert, it's called TRY for a reason
                newThing = def;
            }

            return false;
        }

        /// <summary>
        /// Fault safe type conversion with fluent return
        /// </summary>
        /// <typeparam name="T">the type to convert to</typeparam>
        /// <param name="thing">the thing being converted</param>
        /// <param name="newThing">the converted thing</param>
        /// <returns>the actual value returned</returns>
        public static T TryConvert<T>(object thing, T def = default(T))
        {
            var newThing = def;

            try
            {
                if (thing != null)
                {
                    if (typeof(T).IsEnum)
                    {
                        if (thing is short || thing is int)
                            newThing = (T)thing;
                        else
                            newThing = (T)Enum.Parse(typeof(T), thing.ToString());
                    }
                    else
                        newThing = (T)Convert.ChangeType(thing, typeof(T));
                }
            }
            catch
            {
                //dont error on tryconvert, it's called tryconvert for a reason
                newThing = default(T);
            }

            return newThing;
        }

        /// <summary>
        /// Gets all system types and interfaces implemented by or with for a type, including itself
        /// </summary>
        /// <param name="t">the system type in question</param>
        /// <returns>all types that touch the input type</returns>
        public static IEnumerable<Type> GetAllImplimentingedTypes(Type t)
        {
            var implimentedTypes = t.Assembly.GetTypes().Where(ty => ty.GetInterfaces().Contains(t) || ty == t);
            return implimentedTypes.Concat(t.GetInterfaces());
        }
    }
}

