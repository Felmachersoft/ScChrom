using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom.Tools {
    /// <summary>
    /// A very simple mostly static logger. Call init before first use.
    /// </summary>
    public class Logger {

        public enum LogLevel {
            none  = 0,  // nothing
            error = 1,  // only errors, default
            info  = 2,  // infos and errors
            debug = 3   // debug output, infos and errors
        }

        private static List<Tuple<LogLevel, string>> _preInitLog = new List<Tuple<LogLevel, string>>();
        private static Logger _instance = null;
        private static object _locker = new object();
        public static event Action<LogLevel, string> ContentLogged;
        
        private string _logFilePath = null;
        private LogLevel _currentLoglevel = LogLevel.error;
       
        private Logger(LogLevel loglevel, string logfilepath = null) {
            _currentLoglevel = loglevel;

            if (logfilepath != null)
                _logFilePath = logfilepath;            
        }

        public static void Init(string logLvl = null, string logfilepath = null) {

            LogLevel logLevel = LogLevel.error;

            if(!string.IsNullOrWhiteSpace(logLvl)){
                logLvl = logLvl.ToLower();
                if (logLvl == "none") logLevel = LogLevel.none;
                if (logLvl == "debug") logLevel = LogLevel.debug;
                if (logLvl == "info") logLevel = LogLevel.info;
            }


            if (!string.IsNullOrWhiteSpace(logfilepath)) {
                try {
                    if (File.Exists(logfilepath))
                        File.Delete(logfilepath);                    
                } catch (Exception ex) {
                    throw new ArgumentException("Error while preparing log file: " + ex.Message, "logfilepath");
                }
            } else {
                logfilepath = null; // ensure it's null
            }

            _instance = new Logger(logLevel, logfilepath);

            foreach (var preLogLine in _preInitLog)
                Log(preLogLine.Item2, preLogLine.Item1);
            
        }
        
        public static void Log(string content, LogLevel loglevel = LogLevel.info){            
            lock(_locker){

                if(_instance == null) {
                    _preInitLog.Add(new Tuple<LogLevel, string>(loglevel, content));
                    return;
                }

                if(loglevel > _instance._currentLoglevel)
                    return;
                
                string logLine = "[" + Enum.GetName(typeof(LogLevel), loglevel) + "] " + DateTime.Now.ToShortTimeString() + ": " + content;

                Console.WriteLine(logLine);
                                
                if(_instance._logFilePath != null)
                    File.AppendAllText(_instance._logFilePath, logLine + Environment.NewLine);                
                
                if(ContentLogged != null)
                    ContentLogged.Invoke(loglevel, content);
            }
        }
    }
}
