using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Utility
{
    public static class Serialization
    {
        #region Json
        public static string SerializeToJson(object thing)
        {
            var serializer = GetSerializer();

            var sb = new StringBuilder();
            var writer = new StringWriter(sb);

            serializer.Serialize(writer, thing);

            return sb.ToString();
        }

        private static JsonSerializer GetSerializer()
        {
            var serializer = JsonSerializer.Create();

            serializer.TypeNameHandling = TypeNameHandling.Auto;

            return serializer;
        }
        #endregion

        #region Hashkeys
        public static int GenerateHashKey(List<object> parameters)
        {
            return GenerateHashKey(parameters.ToArray());
        }

        public static int GenerateHashKey(object[] parameters)
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;

                foreach (object param in parameters)
                {
                    var stringCode = param == null ? 0 : param.GetHashCode();

                    hash = hash * 23 + stringCode;
                }

                return hash;
            }
        }
        #endregion

        #region "XML"
        /// <summary>
        /// Safely get the value of an element
        /// </summary>
        /// <typeparam name="T">the type of data you're looking for</typeparam>
        /// <param name="element">the parent element</param>
        /// <param name="xName">the XName of the element you want the value of</param>
        /// <returns>the value or default(T)</returns>
        public static T GetSafeElementValue<T>(this XContainer element, string xName)
        {
            var returnValue = default(T);

            try
            {
                if (element != null && element.Element(xName) != null)
                {
                    if (!TypeUtility.TryConvert<T>(element.Element(xName).Value, ref returnValue))
                        returnValue = default(T);
                }
            }
            catch
            {
                //its safe return, dont barf please
            }

            return returnValue;
        }

        /// <summary>
        /// Safely get the value of an element only used for strings
        /// </summary>
        /// <param name="element">the parent element</param>
        /// <param name="xName">the XName of the attribute you want the value of</param>
        /// <returns>the value or string.empty</returns>
        public static string GetSafeElementValue(this XContainer element, string xName)
        {
            var returnValue = string.Empty;

            try
            {
                if (element != null && element.Element(xName) != null)
                    returnValue = element.Element(xName).Value;
            }
            catch
            {
                //its safe return, dont barf please
            }

            return returnValue;
        }

        /// <summary>
        /// Safely get the value of an attribute
        /// </summary>
        /// <typeparam name="T">the type of data you're looking for</typeparam>
        /// <param name="element">the parent element</param>
        /// <param name="xName">the XName of the attribute you want the value of</param>
        /// <returns>the value or default(T)</returns>
        public static T GetSafeAttributeValue<T>(this XElement element, string xName)
        {
            var returnValue = default(T);

            try
            {
                if (element != null && element.Attribute(xName) != null)
                {
                    if (!TypeUtility.TryConvert<T>(element.Attribute(xName).Value, ref returnValue))
                        returnValue = default(T);
                }
            }
            catch
            {
                //its safe return, dont barf please
            }

            return returnValue;
        }

        /// <summary>
        /// Safely get the value of an attribute only used for strings
        /// </summary>
        /// <param name="element">the parent element</param>
        /// <param name="xName">the XName of the attribute you want the value of</param>
        /// <returns>the value or string.empty</returns>
        public static string GetSafeAttributeValue(this XElement element, string xName)
        {
            var returnValue = string.Empty;

            try
            {
                if (element != null && element.Attribute(xName) != null)
                    returnValue = element.Attribute(xName).Value;
            }
            catch
            {
                //its safe return, dont barf please
            }

            return returnValue;
        }

        /// <summary>
        /// Partial class for an xml binary file, for using xml format data storage
        /// </summary>
        public partial class EntityFileData
        {
            /// <summary>
            /// The binary data of the file
            /// </summary>
            public byte[] XmlBinary { get; set; }

            /// <summary>
            /// Creates a new file accessor for backing data from the binary data
            /// </summary>
            /// <param name="bytes">the binary data stream of the file</param>
            public EntityFileData(byte[] bytes)
            {
                XmlBinary = bytes;
            }

            /// <summary>
            /// Creates a new file accessor from the xml document
            /// </summary>
            /// <param name="xDoc">the xmldocument format of the file data</param>
            public EntityFileData(XDocument xDoc)
            {
                XDoc = xDoc;
            }

            /// <summary>
            /// String version of the file's data
            /// </summary>
            public string XmlString
            {
                get
                {
                    var xml = Encoding.UTF8.GetString(XmlBinary);
                    xml = Regex.Replace(xml, @"[^\u0000-\u007F]", string.Empty); // Removes non ascii characters
                    return xml;
                }
                set
                {
                    value = Regex.Replace(value, @"[^\u0000-\u007F]", string.Empty); // Removes non ascii characters
                    XmlBinary = Encoding.UTF8.GetBytes(value);
                }
            }

            /// <summary>
            /// XML document version of the file's data
            /// </summary>
            public XDocument XDoc
            {
                get
                {
                    using (var memoryStream = new MemoryStream(XmlBinary))
                    using (var xmlReader = XmlReader.Create(memoryStream))
                    {
                        var xml = XDocument.Load(xmlReader);
                        return xml;
                    }
                }
                set
                {
                    var settings = new XmlWriterSettings { OmitXmlDeclaration = true, Encoding = Encoding.UTF8 };
                    using (var memoryStream = new MemoryStream())
                    using (var xmlWriter = XmlWriter.Create(memoryStream, settings))
                    {
                        value.WriteTo(xmlWriter);
                        xmlWriter.Flush();
                        XmlBinary = memoryStream.ToArray();
                    }
                }
            }
        }
        #endregion
    }
}
