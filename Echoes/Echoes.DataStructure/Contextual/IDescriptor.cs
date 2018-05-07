namespace Echoes.DataStructure.Contextual
{
    /// <summary>
    /// An adjective or thing that describes entities
    /// </summary>
    public interface IDescriptor : IContext
    {
        /// <summary>
        /// What context is the potential opposite of this (removes on being added)
        /// </summary>
        string Opposite { get; set; }

        /// <summary>
        /// Is this a known thing or actually applied to the entity
        /// </summary>
        bool Applied { get; set; }
    }
}
