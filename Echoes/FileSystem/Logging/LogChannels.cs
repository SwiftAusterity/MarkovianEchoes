namespace Cottontail.FileSystem.Logging
{
    /// <summary>
    /// Static expected log types for easier log coalation
    /// </summary>
    public enum LogChannels
    {
        CommandUse,
        Restore,
        Backup,
        BackingDataAccess,
        AccountActivity,
        Authentication,
        ProcessingLoops,
        SocketCommunication
    }
}