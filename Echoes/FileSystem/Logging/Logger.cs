using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

    /// <summary>
    /// Publically available wrapper for logging
    /// </summary>
    public static class LoggingUtility
    {
        /// <summary>
        /// Log an exception
        /// </summary>
        /// <param name="ex">the exception</param>
        public static void LogError(string rootPath, Exception ex)
        {
            var errorContent = String.Format("{0}: {1}{2}{3}", ex.GetType().Name, ex.Message, Environment.NewLine, ex.StackTrace);

            CommitLog(rootPath, errorContent, "SystemError");
        }

        /// <summary>
        /// Log an admin command being used
        /// </summary>
        /// <param name="commandString">the command being used</param>
        /// <param name="accountName">the account using it (user not character)</param>
        public static void LogAdminCommandUsage(string rootPath, string commandString, string accountName)
        {
            var content = String.Format("{0}: {1}", accountName, commandString);

            CommitLog(rootPath, content, "AdminCommandUse");
        }

        /// <summary>
        /// Gets all the current log file names in Current
        /// </summary>
        /// <returns>a list of the log file names</returns>
        public static IEnumerable<string> GetCurrentLogNames(string rootPath)
        {
            var logger = new Logger(rootPath);

            return logger.GetCurrentLogNames();
        }

        /// <summary>
        /// Gets a current log file's contents
        /// </summary>
        /// <param name="channel">the log file name</param>
        /// <returns>the content</returns>
        public static string GetCurrentLogContent(string rootPath, string channel)
        {
            var logger = new Logger(rootPath);

            return logger.GetCurrentLogContent(channel);
        }

        /// <summary>
        /// Log one entry to a pre-determined channel
        /// </summary>
        /// <param name="content">the content to log</param>
        /// <param name="channel">which log to append it to</param>
        /// <param name="keepItQuiet">Announce it in game or not</param>
        public static void Log(string rootPath, string content, LogChannels channel)
        {
            CommitLog(rootPath, content, channel.ToString());
        }

        /// <summary>
        /// Archives a log file
        /// </summary>
        /// <param name="channel">the log file to archive</param>
        /// <returns>success status</returns>
        public static bool RolloverLog(string rootPath, string channel)
        {
            var logger = new Logger(rootPath);

            return logger.RolloverLog(channel);
        }

        /// <summary>
        /// commits content to a log channel
        /// </summary>
        /// <param name="content">the content to log</param>
        /// <param name="channel">which log to append it to</param>
        /// <param name="keepItQuiet">Announce it in game or not</param>
        private static void CommitLog(string rootPath, string content, string channel)
        {
            var logger = new Logger(rootPath);

            logger.WriteToLog(content, channel);
        }
    }

    /// <summary>
    /// Internal file access for logging
    /// </summary>
    internal class Logger : FileAccessor
    {
        /// <summary>
        /// Base directory to push logs to
        /// </summary>
        public override string BaseDirectory
        {
            get
            {
                return base.BaseDirectory + "Logs/";
            }
        }

        public Logger(string rootPath) : base(rootPath) { }

        /// <summary>
        /// Write to a log
        /// </summary>
        /// <param name="content">the content to log</param>
        /// <param name="beQuiet">Announce it in game or not</param>
        public void WriteToLog(string content)
        {
            WriteToLog(content, "General");
        }

        /// <summary>
        /// Write to a log
        /// </summary>
        /// <param name="content">the content to log</param>
        /// <param name="channel">which log to append it to</param>
        /// <param name="keepItQuiet">Announce it in game or not</param>
        public void WriteToLog(string content, string channel)
        {
            //Write to the log file first
            WriteLine(content, channel);
        }

        /// <summary>
        /// Gets all the current log file names in Current
        /// </summary>
        /// <returns>a list of the log file names</returns>
        public IEnumerable<string> GetCurrentLogNames()
        {
            var names = Enumerable.Empty<string>();

            if (VerifyDirectory(CurrentDirectoryName, false))
                names = Directory.EnumerateFiles(BaseDirectory + "Current/", "*.txt", SearchOption.TopDirectoryOnly);

            return names.Select(nm => nm.Substring(nm.LastIndexOf('/') + 1, nm.Length - nm.LastIndexOf('/') - 5));
        }

        /// <summary>
        /// Archives a log file
        /// </summary>
        /// <param name="channel">the log file to archive</param>
        /// <returns>success status</returns>
        public bool RolloverLog(string channel)
        {
            var archiveLogName = String.Format("{0}_{1}{2}{3}_{4}{5}{6}.txt",
                    channel
                    , DateTime.Now.Year
                    , DateTime.Now.Month
                    , DateTime.Now.Day
                    , DateTime.Now.Hour
                    , DateTime.Now.Minute
                    , DateTime.Now.Second);

            return ArchiveFile(channel + ".txt", archiveLogName);
        }

        /// <summary>
        /// Gets a current log file's contents
        /// </summary>
        /// <param name="channel">the log file name</param>
        /// <returns>the content</returns>
        public string GetCurrentLogContent(string channel)
        {
            var content = String.Empty;

            var bytes = ReadCurrentFileByPath(channel + ".txt");

            if(bytes.Length > 0)
                content = Encoding.UTF8.GetString(bytes);

            return content;
        }

        /// <summary>
        /// Writes content to a log file
        /// </summary>
        /// <param name="content">the content to write</param>
        /// <param name="channel">the log file to append it to</param>
        private void WriteLine(string content, string channel)
        {
            var dirName = BaseDirectory + CurrentDirectoryName;

            if (!VerifyDirectory(CurrentDirectoryName))
                throw new Exception("Unable to locate or create base live logs directory.");

            var fileName = channel + ".txt";
            var timeStamp = String.Format("[{0:0000}/{1:00}/{2:00} {3:00}:{4:00}:{5:00}]:  ", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            //Add a line terminator PLEASE
            content += Environment.NewLine;

            var bytes = Encoding.UTF8.GetBytes(timeStamp + content);

            WriteToFile(dirName + fileName, bytes, FileMode.Append);
        }
    }
}