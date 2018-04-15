using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Utility
{
    public static class RandomDataGenerator
    {
        #region "Raw value generation"
        /// <summary>
        /// Generates a randomized value populated object class, it must have a parameterless constructor for this to work
        /// </summary>
        /// <typeparam name="T">The Class type to generate</typeparam>
        /// <returns>The class type with all ValueType parameters filled in with randomized data</returns>
        public static T GenerateRawTestValue<T>() where T : class
        {
            var returnValue = GenerateRawTestValueForClass(typeof(T), null);

            return (T)returnValue;
        }

        /// <summary>
        /// Generate a test value for a type, only to be used for ValueTypes
        /// </summary>
        /// <param name="t">The type to generate</param>
        /// <returns>a generic object of the value</returns>
        public static object GenerateRawTestValue(Type t, Random rnd = null)
        {
            return GenerateTestValue(t, 0, 100, rnd);
        }

        /// <summary>
        /// Generate randomized data, it is understood that the low/high wont always be the actual caps for longs, doubles, unsigneds, etc
        /// </summary>
        /// <typeparam name="T">The type of data we want</typeparam>
        /// <param name="low">Low cap, used for numerics and minimum string length</param>
        /// <param name="high">High cap, used for numerics and maximum string length</param>
        /// <returns>One T</returns>
        public static T GenerateRawTestValue<T>(int low, int high, Random rnd = null) where T : struct
        {
            T returnValue = default(T);

            Type baseType = typeof(T);

            var testValue = GenerateTestValue(baseType, low, high, rnd);

            if (testValue != null)
            {
                returnValue = TypeUtility.TryConvert<T>(testValue);
            }

            return returnValue;
        }

        /// <summary>
        /// Generate randomized data specifically for DateTimes
        /// </summary>
        /// <param name="low">Low cap for date</param>
        /// <param name="high">High cap for date</param>
        /// <returns>the new datetime</returns>
        public static DateTime GenerateRawTestValue(DateTime low, DateTime high, Random rnd = null)
        {
            if (low == null)
                low = DateTime.MinValue;

            if (high == null)
                high = DateTime.MaxValue;

            return GenerateTestValue(low, high, rnd); ;
        }

        /// <summary>
        /// Returns an array of randomized Ts
        /// </summary>
        /// <typeparam name="T">The type to generate, structs only</typeparam>
        /// <param name="low">Low cap, used for numerics and minimum string length</param>
        /// <param name="high">High cap, used for numerics and maximum string length</param>
        /// <param name="count">The number of values to put in the array</param>
        /// <returns>Enumerable of types values</returns>
        public static IEnumerable<T> GenerateRawTestValue<T>(int low, int high, int count) where T : struct
        {
            //Why? Maybe you want an empty array but you could have made that yourself
            if (count <= 0)
            {
                return Enumerable.Empty<T>();
            }

            List<T> returnList = new List<T>();

            for (int i = 1; i < count; i++)
            {
                returnList.Add(GenerateRawTestValue<T>(low, high));
            }

            return returnList;
        }
        #endregion

        #region "Random extensions and type generation"
        /// <summary>
        /// Generate a random char from the pool of only alphanumerics
        /// </summary>
        /// <returns></returns>
        public static char GenerateNextAlphaNumericChar(this Random rnd)
        {
            return rnd.GenerateNextChar(true, true, true);
        }

        /// <summary>
        /// Generate a random char from the pool of only letters
        /// </summary>
        /// <returns></returns>
        public static char GenerateNextAlphaChar(this Random rnd)
        {
            return rnd.GenerateNextChar(true, true, false);
        }

        /// <summary>
        /// Generate a random char from the pool of only alphas and/or numerics
        /// </summary>
        /// <returns></returns>
        public static char GenerateNextChar(this Random rnd, bool lowercase, bool uppercase, bool numerics)
        {
            List<char> alphabet = new List<char>();

            if (lowercase)
            {
                alphabet.AddRange(new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' });
            }

            if (uppercase)
            {
                alphabet.AddRange(new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' });
            }

            if (numerics)
            {
                alphabet.AddRange(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });
            }

            return alphabet[rnd.Next(0, alphabet.Count() - 1)];
        }

        /// <summary>
        /// EXTENSION: Generate a random int with a low and high range, more accurate than the base method
        /// </summary>
        /// <param name="rnd">the randomizer</param>
        /// <param name="low">low range</param>
        /// <param name="high">high range</param>
        /// <returns>an int</returns>
        public static int NextInt32(this Random rnd, int low, int high)
        {
            unchecked
            {
                int firstBits = rnd.Next(low, 1 << 4) << 28;
                int lastBits = rnd.Next(low, 1 << 28);

                return Math.Max(Math.Min(high, firstBits | lastBits), low);
            }
        }

        /// <summary>
        /// EXTENSION: Generate a random decimal with a low and high range
        /// </summary>
        /// <param name="rnd">the randomizer</param>
        /// <param name="low">low range</param>
        /// <param name="high">high range</param>
        /// <returns>an int</returns>     
        public static decimal NextDecimal(this Random rnd, int low, int high)
        {
            byte scale = (byte)rnd.Next(29);
            bool sign = rnd.Next(2) == 1;

            return new decimal(rnd.NextInt32(low, high),
                               rnd.NextInt32(low, high),
                               rnd.NextInt32(low, high),
                               sign,
                               scale);
        }

        /// <summary>
        /// EXTENSION: Generate a random float with a low and high range
        /// </summary>
        /// <param name="rnd">the randomizer</param>
        /// <param name="low">low range</param>
        /// <param name="high">high range</param>
        /// <returns>an int</returns>      
        public static float NextFloat(this Random rnd, float low, float high)
        {
            double mantissa = (rnd.NextDouble() * 2.0) - 1.0;
            double exponent = Math.Pow(2.0, rnd.Next(-126, 128));

            return (float)Math.Max(Math.Min(high, mantissa * exponent), low);
        }

        /// <summary>
        /// EXTENSION: Generate a random double with a low and high range
        /// </summary>
        /// <param name="rnd">the randomizer</param>
        /// <param name="low">low range</param>
        /// <param name="high">high range</param>
        /// <returns>an int</returns>     
        public static double NextDouble(this Random rnd, int low, int high)
        {
            var next = rnd.NextDouble();

            return low + (next * (high - low));
        }

        /// <summary>
        /// EXTENSION: Generate a random boolean
        /// </summary>
        /// <param name="rnd">the randomizer</param>
        /// <param name="low">low range</param>
        /// <param name="high">high range</param>
        /// <returns>an int</returns>      
        public static bool NextBool(this Random rnd)
        {
            return rnd.Next(1, 100) > 50;
        }

        public static DateTime NextDateTime(this Random rnd, DateTime low, DateTime high)
        {
            var fullTimespan = low - high;

            //Really?
            if (fullTimespan.TotalSeconds < 1)
            {
                return low.AddMilliseconds(rnd.NextDouble(1, (int)fullTimespan.TotalMilliseconds));
            }
            else if (fullTimespan.TotalMinutes < 1)
            {
                return low.AddSeconds(rnd.NextDouble(1, (int)fullTimespan.TotalSeconds));
            }
            else if (fullTimespan.TotalHours < 1)
            {
                return low.AddMinutes(rnd.NextDouble(1, (int)fullTimespan.TotalMinutes));
            }

            return low.AddHours(rnd.NextDouble(1, (int)fullTimespan.TotalHours));
        }
        #endregion

        #region "Helpers"
        public static object GenerateRawTestValueForClass(Type baseType, Random rnd = null)
        {
            if (!baseType.IsClass)
                return null;

            var returnValue = Activator.CreateInstance(baseType);

            PropertyInfo[] properties = baseType.GetProperties();

            if (rnd == null)
                rnd = new Random();

            foreach (PropertyInfo property in properties.Where(prop => prop.CanWrite))
            {
                try
                {
                    if (property.PropertyType.IsValueType || property.PropertyType == typeof(String))
                        property.SetValue(returnValue, GenerateRawTestValue(property.PropertyType, rnd), null);
                    else if (property.PropertyType.IsClass && property.PropertyType != baseType)
                        property.SetValue(returnValue, GenerateRawTestValueForClass(property.PropertyType, rnd), null);
                }
                catch
                {
                    //just go on
                }
            }

            return returnValue;
        }

        private static DateTime GenerateTestValue(DateTime low, DateTime high, Random rnd = null)
        {
            if (rnd == null)
                rnd = new Random();

            return rnd.NextDateTime(low, high);
        }

        private static object GenerateTestValue(Type type, int low, int high, Random rnd = null)
        {
            if (rnd == null)
                rnd = new Random();

            //Why? This would be silly otherwise, only structs here
            if (!type.IsArray && !type.IsAbstract && !type.IsInterface)
            {
                //This one is a bit weird
                if (type.IsEnum)
                {
                    Array values = Enum.GetValues(type);

                    return values.GetValue(rnd.Next(values.Length));
                }

                //Would rather have a switch but switching on types gets weird looking
                if (type == typeof(short))
                {
                    return rnd.Next(Math.Max(low, 0), Math.Min(high, 255));
                }
                else if (type == typeof(ushort))
                {
                    return rnd.Next(Math.Max(low, 0), Math.Min(high, 65535));
                }
                else if (type == typeof(int))
                {
                    return rnd.NextInt32(low, high);
                }
                else if (type == typeof(uint))
                {
                    return rnd.NextInt32(Math.Max(low, 0), high);
                }
                else if (type == typeof(long))
                {
                    return rnd.Next(low, high);
                }
                else if (type == typeof(ulong))
                {
                    return rnd.Next(Math.Min(low, 0), high);
                }
                else if (type == typeof(double))
                {
                    return rnd.NextDouble(low, high);
                }
                else if (type == typeof(decimal))
                {
                    return rnd.NextDecimal(low, high);
                }
                else if (type == typeof(float))
                {
                    return rnd.NextFloat(low, high);
                }
                else if (type == typeof(string))
                {
                    var newString = string.Empty;

                    //Maybe cap strings incase of madness
                    var maxCount = rnd.Next(Math.Max(0, low), Math.Min(255, high));

                    for (int i = 1; i < maxCount; i++)
                        newString += rnd.GenerateNextAlphaNumericChar();

                    return newString;
                }
                else if (type == typeof(char))
                {
                    return rnd.GenerateNextAlphaNumericChar();
                }
                else if (type == typeof(byte))
                {
                    var buffer = new byte[high];
                    rnd.NextBytes(buffer);

                    return buffer[rnd.Next(0, high - 1)];
                }
                else if (type == typeof(bool))
                {
                    return rnd.NextBool();
                }
            }

            return null;
        }
        #endregion

    }
}


