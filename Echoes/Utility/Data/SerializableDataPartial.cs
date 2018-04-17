using Cottontail.Structure;
using System.Text;
using Utility;

namespace Cottontail.Utility.Data
{
    public abstract class SerializableDataPartial : IFileStored
    {
        /// <summary>
        /// Serialize this live entity to a json string
        /// </summary>
        /// <returns>json string</returns>
        public virtual string Serialize()
        {
            return Serialization.SerializeToJson(this);
        }

        /// <summary>
        /// Deserialize a json string into this entity
        /// </summary>
        /// <param name="jsonData">string to deserialize</param>
        /// <returns>the entity</returns>
        public virtual IFileStored DeSerialize(string jsonData)
        {
            return Serialization.Deserialize(jsonData, this.GetType());
        }

        /// <summary>
        /// Serialize this live entity to a binary stream
        /// </summary>
        /// <returns>binary stream</returns>
        public virtual byte[] ToBytes()
        {
            return Encoding.ASCII.GetBytes(Serialize());
        }

        /// <summary>
        /// Deserialize a binary stream into this entity
        /// </summary>
        /// <param name="bytes">binary to deserialize</param>
        /// <returns>the entity</returns>
        public virtual IFileStored FromBytes(byte[] bytes)
        {
            var strData = Encoding.ASCII.GetString(bytes);

            return DeSerialize(strData);
        }
    }
}
