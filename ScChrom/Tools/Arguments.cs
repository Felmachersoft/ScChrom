using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom.Tools {
    public static class Arguments {

        private static string _configfilePath = null;

        private static Dictionary<string, string> _args = null;
        private static Dictionary<string, Dictionary<string, string>> _stackedArgs = null;
        private static Dictionary<string, string> _infos = null;

        #region Properties
        /// <summary>
        /// The absolute path of the loaded config file. Null if no config file was used.
        /// </summary>
        public static string ConfigfilePath {
            get {
                return _configfilePath;
            }
        }

        /// <summary>
        /// The absolute directy path of the loaded config file. Null if no config file was used.
        /// </summary>
        public static string ConfigfileDirectory {
            get {
                if (_configfilePath == null)
                    return null;
                return System.IO.Path.GetDirectoryName(_configfilePath);
            }
        }

        /// <summary>
        /// Contains all none stacked arguments. Use a getter instead of searching through this.
        /// </summary>
        public static IReadOnlyDictionary<string, string> AllArguments {
            get {
                if(_args == null) {                    
                    var commandLineArgs = Environment.GetCommandLineArgs();                    
                    parseCommandLineArgs(commandLineArgs);
                }               
                return _args;
            }
        }

        /// <summary>
        /// Contains all stacked arguments. Use a getter instead of searching through this.
        /// </summary>
        public static IReadOnlyDictionary<string, Dictionary<string, string>> StackedArguments {
            get {
                if (_stackedArgs == null) {
                    _stackedArgs = new Dictionary<string, Dictionary<string, string>>();
                }

                return _stackedArgs;
            }
        }

        /// <summary>
        /// Additional infos given in the config file. Always empty if only using command line arguments.
        /// </summary>
        public static IReadOnlyDictionary<string, string> ConfigfileInfos {
            get {
                if (_infos == null) {
                    _infos = new Dictionary<string, string>();
                }

                return _infos;
            }
        }        

        #endregion

        #region ArgumentGetter
        /// <summary>
        /// Get argument as string. If no argument given for parameter name, defaultValue will be returned.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetArgument(string parameterName, string defaultValue = null) {
            string ret = null;
            if (!AllArguments.TryGetValue(parameterName, out ret)) {
                Dictionary<string, string> tempStacked = null;
                if (StackedArguments.TryGetValue(parameterName, out tempStacked) && tempStacked.Count == 1) {
                    foreach (var entry in tempStacked)
                        return entry.Value;                    
                }
                return defaultValue;
            }
            return ret;
        }

        /// <summary>
        /// Gets the stacked arguments as stackname to content for parameters defined like: --parameterName&lt;stackname&gt;=content
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetStackedArguments(string parameterName) {
            Dictionary<string, string> ret = null;
            if (!StackedArguments.TryGetValue(parameterName, out ret)) {
                // maybe in the none stacked arguments?
                var arg = GetArgument(parameterName);
                if (arg != null) {
                    return new Dictionary<string, string> { { "", arg} };
                }
                return null;
            }
            return ret;
        }


        private static Tuple<RequestIdentifier, string> GetSeperatedIdsAndVal(string argmentName, string argumentValue) {
           
            // check if \n or space comes first and seperates url from script part
            int indexOfSpace = argumentValue.IndexOf(' ');
            int indexOfNewLine = argumentValue.IndexOf('\n');
            if (indexOfSpace <= 0 && indexOfNewLine <= 0) {
                Tools.Logger.Log("Identifier and script are not correctly seperated by either a space or new line (values ignored), name of the parameter was: " + argmentName, Tools.Logger.LogLevel.error);
                return null;
            }

            string[] addressAndValue = null;

            if (argumentValue.TrimStart().StartsWith("\"")) {
                if (argumentValue.Count((c) => c == '"') < 2) {
                    Tools.Logger.Log("Identifier starts with \" but misses a second \" to delimit the identifier: " + argmentName, Tools.Logger.LogLevel.error);
                    return null;
                }

                int idEndIndex = argumentValue.IndexOf('"', 1) - 1;

                string id = argumentValue.Substring(1, idEndIndex);
                string val = argumentValue.Substring(idEndIndex + 2).TrimStart();

                addressAndValue = new string[] { id, val };

            } else {

                string splittingChar = "\n";
                if (indexOfSpace <= 0)
                    splittingChar = "\n";
                else if (indexOfNewLine > indexOfSpace) {
                    splittingChar = " ";
                }
                addressAndValue = argumentValue.Split(splittingChar.ToCharArray(), 2);
                if (addressAndValue.Length < 2) {
                    Tools.Logger.Log("Identifier and script are not correctly seperated by either a space or new line (values ignored), name of the parameter was: " + argmentName, Tools.Logger.LogLevel.error);
                    return null;
                }
            }

            var ri = new RequestIdentifier(addressAndValue[0]);            

            string value = addressAndValue[1];
            return new Tuple<RequestIdentifier, string>(
                ri,
                value
            );
        }

        /// <summary>
        /// Get a stacked parameters that are written in a config-file like --parameterName&lt;stackname&gt;=myUrl.com content
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns>The dictionay keys are the stackname like &lt;stackname&gt;, the value is a tuple of the given urlpattern and the content </returns>
        public static Dictionary<string, Tuple<RequestIdentifier, string>> GetStackedArgsWithIds(string parameterName) {
            Dictionary<string, string> arguments = GetStackedArguments(parameterName);
            if (arguments == null || arguments.Count <= 0) 
                return null;
            
            
            var ret = new Dictionary<string, Tuple<RequestIdentifier, string>>();
            foreach (var arg in arguments) {
                string argStack = arg.Key;
                string argValue = arg.Value;

                string argName = parameterName;
                if (!string.IsNullOrWhiteSpace(argStack))
                    argName += "<" + argStack +">";

                ret.Add(argStack, GetSeperatedIdsAndVal(argName, argValue));
            }
            
            if (ret.Count <= 0)
                return null;

            return ret;
        }

        /// <summary>
        /// Get argument parsed as long.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="defaultValue">Returned if parameter not exists or can't be parsed to long.</param>
        /// <returns></returns>
        public static long GetArgumentLong(string parameterName, long defaultValue = 0) {

            string paramVal = GetArgument(parameterName);
            if (paramVal == null)
                return defaultValue;

            long ret = defaultValue;
            if (!long.TryParse(paramVal, out ret))
                Tools.Logger.Log("Invalid value for parameter '" + parameterName + "': " + paramVal, Tools.Logger.LogLevel.error);

            return ret;
        }

        /// <summary>
        /// Get argument parsed as int.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="defaultValue">Returned if parameter not exists or can't be parsed to int.</param>
        /// <returns></returns>
        public static int GetArgumentInt(string parameterName, int defaultValue = 0) {
            long longVal = GetArgumentLong(parameterName, defaultValue);

            int ret = defaultValue;
            try {
                ret = Convert.ToInt32(longVal);
            } catch (OverflowException) {
                Tools.Logger.Log("Value to large for parameter '" + parameterName + "'", Tools.Logger.LogLevel.error);
            }

            return ret;
        }

        /// <summary>
        /// Get argument parsed as double (invariant culture).
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="defaultValue">Returned if parameter not exists or can't be parsed to double.</param>
        /// <returns></returns>
        public static double GetArgumentDouble(string parameterName, double defaultValue = double.NaN) {

            string paramVal = GetArgument(parameterName);
            if (paramVal == null)
                return defaultValue;

            double ret = defaultValue;
            if (!double.TryParse(paramVal, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out ret))
                Tools.Logger.Log("Invalid value for parameter '" + parameterName + "': " + paramVal, Tools.Logger.LogLevel.error);

            return ret;
        }
        #endregion

        #region Parsing 

        private static void parseCommandLineArgs(string[] commandLineArgs) {
                        
            // check if only config file given
            if (commandLineArgs.Length == 2 && !commandLineArgs.Last().StartsWith("--")) {
                string path = commandLineArgs.Last();
                if(path.StartsWith(@"""") && path.EndsWith(@"""")) {
                    path = path.Substring(1, path.Length - 2);
                }

                // is a file? => load it
                if (Common.IsValidLocalPath(path) && System.IO.File.Exists(path)) {                                        
                    string configFileArg = @"--config-file=""" + path + @"""";
                    commandLineArgs[1] = configFileArg;                    
                } else {
                    // direct error output because no logger setting specified yet
                    Console.Error.WriteLine("Invalid path to config file given: " + commandLineArgs.Last());
                    Environment.Exit(2);
                }
            }

            List<string> arguments = new List<string>();

            // load default script if nothing given
            if (commandLineArgs.Length == 1) {
                string defaultString = Encoding.UTF8.GetString(ScChrom.Properties.Resources._default);
                arguments.AddRange(defaultString.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
            }

            foreach (string arg in commandLineArgs) {
                if (!arg.StartsWith("--")) {
                    continue;
                }

                string keyval = arg.Substring(2);

                string val = "";
                string key = null;

                if (!keyval.Contains("=")) {
                    key = keyval;
                } else {
                    int indexOfSeperator = keyval.IndexOf('=');
                    key = keyval.Substring(0, indexOfSeperator);
                    val = keyval.Substring(indexOfSeperator + 1);
                    if (val.StartsWith(@"""") && val.EndsWith(@"""")) {
                        val = val.Substring(1, val.Length - 2);
                    }
                    if (key.Contains("<") && key.EndsWith(">")) {
                        var parts = key.Split('<');
                        key = parts[0];
                        string stackedId = parts[1].Replace(">", "");

                        if (!StackedArguments.ContainsKey(key)) {
                            _stackedArgs.Add(key, new Dictionary<string, string>());
                        }
                        _stackedArgs[key].Add(stackedId, val);

                        continue;
                    }
                }

                arguments.Add("--" + key + "=" + val);

                string[] lines = null;
                if (key == "config-file") {
                    string path = null;
                    try {
                        path = System.IO.Path.GetFullPath(val);
                    } catch (Exception) {
                        Console.Error.WriteLine("Invalid path for config file given: " + path);
                        Environment.Exit(1);
                    }
                    
                    if (!System.IO.File.Exists(path)) {
                        Console.Error.WriteLine("Could not find specified config file: " + path);
                        Environment.Exit(2);
                    }

                    _configfilePath = path;

                    lines = readConfigFile(val);
                }
                if (key == "config-base64")
                    lines = DecodeBase64String(val);
                
                if(lines != null && lines.Length > 0) {
                    _infos = ParseInfos(lines[0]);
                    arguments.AddRange(lines);
                }
                
            }

            ParseConfigArgs(arguments.ToArray());
        }

        private static string[] readConfigFile(string path){
                        
            string[] lines = null;

            try {
                lines = System.IO.File.ReadAllLines(path);
            } catch (Exception ex) {
                Console.Error.WriteLine("Could not read from config file: " + ex.Message);
                Environment.Exit(3);
            }

            return lines;
        }

        /// <summary>
        /// Decodes a base64 string and splits the result at the line breaks. 
        /// Used by the config-base64 parameter.
        /// </summary>
        /// <param name="base64string"></param>
        /// <returns></returns>
        public static string[] DecodeBase64String(string base64string) {
            string configContent = null;
            try {
                configContent = Tools.Common.Base64Decode(base64string);
            } catch (Exception ex) {
                Console.Error.WriteLine("Could not decode Base64. Error was: " + ex.Message);
                Environment.Exit(3);
            }
            var configLines = configContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            return configLines;
        }

        /// <summary>
        /// Parses all given lines (of a config-file) and stores the parameter values from them
        /// </summary>
        /// <param name="lines"></param>
        public static void ParseConfigArgs(string[] lines) {
            _args = new Dictionary<string, string>();            

            List<List<string>> paramLines = new List<List<string>>();
            List<string> curParamLines = new List<string>();

            foreach (string line in lines) {
                if (line.StartsWith("//")) {
                    continue;
                }
                
                if (line.StartsWith("--")) {
                    if (curParamLines.Count > 0) {
                        paramLines.Add(new List<string>(curParamLines));
                    }
                    curParamLines.Clear();
                }

                curParamLines.Add(line);
            }

            if (curParamLines.Count > 0) {
                paramLines.Add(curParamLines);
            }

            Dictionary<string, string> paramAndValues = new Dictionary<string, string>();
            Dictionary<string, Dictionary<string, string>> stackedParamAndValues = new Dictionary<string,Dictionary<string,string>>();
            foreach (var pKLines in paramLines) { 
                if(pKLines.Count <= 0)
                    continue;

                string firstLine = pKLines.First();
                int ei = firstLine.IndexOf("=");
                if (ei <= 3) {
                    Console.Error.WriteLine("Missing '=' in config-file for line: " + firstLine);
                    continue;
                }

                // remove leading --
                firstLine = firstLine.Substring(2); 

                var nameVal = firstLine.Split("=".ToArray(), 2);

                string paramName = nameVal[0];
                string paramVal = nameVal[1];
                
                if (!string.IsNullOrWhiteSpace(paramVal) && pKLines.Count > 1)
                    paramVal += "\n";
                
                string stackedId = null;
                
                bool isStacked = paramName.Contains("<") && paramName.Contains(">");
                
                if(!isStacked){
                    if(paramAndValues.ContainsKey(paramName)){
                        Console.Error.WriteLine("Parameter " + paramName + " is defined multiple times, all but last value are ignored");
                    }
                } else {                    
                    var parts = paramName.Split('<');
                    paramName = parts[0];
                    stackedId = parts[1].Replace(">", "");
                }


                for (int i = 1; i < pKLines.Count; i++) {              
                    paramVal += pKLines[i];
                    
                    // if not last line of param add line break
                    if (i < pKLines.Count - 1)
                        paramVal += "\n";

                }

                if (!isStacked){
                    paramAndValues[paramName] = paramVal;
                } else {
                    if(!stackedParamAndValues.ContainsKey(paramName))
                        stackedParamAndValues[paramName] = new Dictionary<string,string>();
                    stackedParamAndValues[paramName][stackedId] = paramVal;
                }
                
            }

            foreach(var kv in paramAndValues)
                _args[kv.Key] = kv.Value;

            _stackedArgs = new Dictionary<string, Dictionary<string, string>>();
            foreach (var kv in stackedParamAndValues)
                _stackedArgs[kv.Key] = kv.Value;
           
        }

        /// <summary>
        /// Parses the infos of a config file. (Infos are JSON encoded in the first line as a comment)
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ParseInfos(string line) {
            if (!line.StartsWith("//{"))
                return null;

            // remove slashes
            line = line.Substring(2);

            Dictionary<string, string> ret = null;
            
            try {
                ret = Newtonsoft.Json.Linq.JObject.Parse(line).ToObject<Dictionary<string, string>>();
            } catch (Exception ex) {
                Logger.Log("Failed to parse meta infos in the first line, error was: " + ex.Message, Logger.LogLevel.error);
                return null;
            }

            return ret;
        }

        #endregion
        

        /// <summary>
        /// Splits a single string at the line breaks into string array.
        /// </summary>
        /// <param name="testScript"></param>
        /// <returns></returns>
        public static string[] GetScriptLines(string testScript) {
            var lines = testScript.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            // remove prefix spaces/tabs
            return lines.Select((s) => {
                string trimmed = s.TrimStart();
                if (trimmed.StartsWith("--"))
                    return trimmed;
                else
                    return s;
            }).ToArray();
        }
    }
}
