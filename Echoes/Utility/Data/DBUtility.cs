using System.Data;

namespace Utility
{
    public static class DBUtility
    {
        /// <summary>
        /// Get's a single column's data from a datarow (fault safe)
        /// </summary>
        /// <typeparam name="T">the data type</typeparam>
        /// <param name="dr">the data row</param>
        /// <param name="columnName">the column to extract</param>
        /// <param name="thing">the output object</param>
        /// <returns>success status</returns>
        public static T GetFromDataRow<T>(DataRow dr, string columnName)
        {
            T thing = default(T);

            try
            {
                if (dr == null || !dr.Table.Columns.Contains(columnName))
                    return thing;

                TypeUtility.TryConvert<T>(dr[columnName], ref thing);
            }
            catch
            {
                //dont error on this, it is supposed to be safe
                thing = default(T);
            }

            return thing;
        }
    }
}
