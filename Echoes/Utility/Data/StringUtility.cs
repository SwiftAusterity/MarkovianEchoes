namespace Utility
{
    public static class StringUtility
    {
        /// <summary>
        /// Pads a string with characters
        /// </summary>
        /// <param name="str">the string to pad</param>
        /// <param name="padAmount">how many to pad with</param>
        /// <param name="padString">what you're padding with</param>
        /// <param name="toTheLeft">is this to the left or right</param>
        /// <returns>the padded string</returns>
        public static string PadWithString(this string str, int padAmount, string padString, bool toTheLeft)
        {
            while (padAmount > 0)
            {
                if (toTheLeft)
                    str = padString + str;
                else
                    str = str + padString;

                padAmount--;
            }

            return str;
        }
    }
}
