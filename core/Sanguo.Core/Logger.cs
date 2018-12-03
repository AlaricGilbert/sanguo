using System;
using System.IO;
using System.Text;

namespace Sanguo.Core
{
    /// <summary>  
    /// Represents a logger provides basic log service.
    /// </summary>  
    [Serializable]
    public class Logger
    {
        /// <summary>
        /// The severity of the information.
        /// </summary>
        public enum LogLevel
        {
            Infos,
            Warns,
            Error
        };

        private readonly StringBuilder _logStringBuilder = new StringBuilder();

        private string _time
        {
            get
            {
                string timeString = DateTime.Now.ToString();
                char[] timeChar = timeString.ToCharArray();
                timeChar[4] = timeChar[5] != '0' ? '0' : (char)0;
                timeChar[6] = timeString[7] != '0' ? '0' : (char)0;
                timeString = new string(timeChar);
                return timeString.Replace(' ', '-').Replace(":", "").Substring(2);
            }
        }

        private string _fileName;

        private string _fileDirectory = "";

        private void Initialize(string logFileName, string customLogFileDirection)
        {
            _fileDirectory = customLogFileDirection;
            _fileName = $"{logFileName}.{_time}.log";
        }

        /// <summary>
        /// Initialize a logger which saves the log files to default path.
        /// </summary>
        /// <param name="logFileName">The prefix of the log file.</param>
        public Logger(string logFileName)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "logs");
            Initialize(logFileName, path);
        }

        /// <summary>
        /// Initialize a logger which saves the log files to specified path.
        /// </summary>
        /// <param name="logFileName">The prefix of the log file.</param>
        /// <param name="customLogFileDirection">The specified path.</param>
        public Logger(string logFileName, string customLogFileDirection) => Initialize(logFileName, customLogFileDirection);

        /// <summary>
        /// Logs a Exception.
        /// </summary>
        /// <param name="exception">The Exception to be logged.</param>
        public virtual void Write(Exception exception) => Write(exception.ToString(), "Exception", LogLevel.Error);

        /// <summary>
        /// Log as you wanted to.
        /// </summary>
        /// <param name="logInfo">The information you want to record.</param>
        /// <param name="logObject"></param>
        /// <param name="level">The severity of the information.</param>
        public virtual void Write(string logInfo, string logObject, LogLevel level)
        {
            string log = $"[{DateTime.Now}] [{logObject}] [{level}]: {logInfo}";
            _logStringBuilder.AppendLine(log);
#if DEBUG
            switch (level)
            {
                case LogLevel.Infos:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                case LogLevel.Warns:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }
            Console.WriteLine(log);
            Console.ForegroundColor = ConsoleColor.White;
#endif
        }

        /// <summary>
        /// Returns the content of the Logger.
        /// </summary>
        /// <returns>String Logs</returns>
        public override string ToString() => _logStringBuilder.ToString();

        /// <summary>
        /// Save the log file.
        /// </summary>
        public virtual void Save()
        {
            if (!Directory.Exists(_fileDirectory))
                Directory.CreateDirectory(_fileDirectory);
            File.AppendAllText(Path.Combine(_fileDirectory, _fileName), _logStringBuilder.ToString());
        }
    }
}