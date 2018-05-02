using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cottontail.FileSystem.Logging
{

    /// <summary>
    /// Internal file access for logging
    /// </summary>
    public class FileLogger : FileAccessor
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

        public FileLogger(IHostingEnvironment hostingEnvironment) : base(hostingEnvironment.ContentRootPath)
        {
            Logger = this;
        }

        /// <summary>
        /// Log an exception
        /// </summary>
        /// <param name="ex">the exception</param>
        public void LogError(Exception ex)
        {
            var errorContent = String.Format("{0}: {1}{2}{3}", ex.GetType().Name, ex.Message, Environment.NewLine, ex.StackTrace);

            WriteToLog(errorContent, "SystemError");
        }

        /// <summary>
        /// Log an admin command being used
        /// </summary>
        /// <param name="commandString">the command being used</param>
        /// <param name="accountName">the account using it (user not character)</param>
        public void LogAdminCommandUsage(string commandString, string accountName)
        {
            var content = String.Format("{0}: {1}", accountName, commandString);

            WriteToLog(content, "AdminCommandUse");
        }

        /// <summary>
        /// Write to a log
        /// </summary>
        /// <param name="content">the content to log</param>
        public void WriteToLog(string content)
        {
            WriteToLog(content, "General");
        }

        /// <summary>
        /// Write to a log
        /// </summary>
        /// <param name="content">the content to log</param>
        /// <param name="channel">which log to append it to</param>
        public void WriteToLog(string content, string channel)
        {
            //Write to the log file first
            WriteLine(content, channel);
        }

        /// <summary>
        /// Write to a log
        /// </summary>
        /// <param name="content">the content to log</param>
        /// <param name="channel">which log to append it to</param>
        public void WriteToLog(string content, LogChannels channel)
        {
            //Write to the log file first
            WriteLine(content, channel.ToString());
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

            if (bytes.Length > 0)
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