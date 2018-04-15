namespace Echoes.DataStructure.Contextual
{
    /// <summary>
    /// Base for context related items
    /// </summary>
    public interface IContext
    {
        /// <summary>
        /// The keyword for the descriptor
        /// </summary>
        string Name { get; set; }
    }
}
