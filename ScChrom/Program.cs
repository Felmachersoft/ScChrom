using System;
using ScChrom.Tools;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Threading;
using static ScChrom.Tools.DependencyInstaller;
using System.Diagnostics;

namespace ScChrom {
    public class Program {

        public enum Exitcode {
            Ok = 0,
            InvalidConfigPath = 1,
            ConfigfileNotFound = 2,
            UnableToReadConfig = 3,
            InvalidCachePath = 4,
            CmdInstallFailed = 5,
            CheckFailed = 6,
            UpdateError = 7
        }

        public static string LatestReleaseDownloadUrl = "https://github.com/Felmachersoft/ScChrom/releases/latest/download/ScChrom.exe";

        public static string LatestReleaseInfoUrl = "https://api.github.com/repos/Felmachersoft/ScChrom/releases/latest";

        private static string nugetBaseUrl = "https://www.nuget.org/api/v2/package/";

        public static List<OnlineDependency> Dependencies {
            get {

                var ret = new List<OnlineDependency>() {
                    new OnlineDependency() {
                        Name = "CefSharp.Common",
                        TotalBytes = 21934080,
                        URL = nugetBaseUrl + "CefSharp.Common/94.4.50",
                        SourceDirectories = new string[] {
                            Path.Combine("CefSharp", "x" + (Environment.Is64BitOperatingSystem ? "64" : "32")),
                            Path.Combine("lib", "net452")
                        }
                    },
                    new OnlineDependency() {
                        Name = "CefSharp.WinForms",
                        TotalBytes = 147456,
                        URL = nugetBaseUrl + "CefSharp.WinForms/94.4.50",
                        SourceDirectories = new string[] {
                            Path.Combine("lib","net452")
                        }
                    },
                    new OnlineDependency() {
                        Name = "Jint",
                        TotalBytes = 428983,
                        URL = nugetBaseUrl + "jint/3.0.0-beta-1632",
                        SourceDirectories = new string[] {
                            Path.Combine("lib", "net45")
                        }
                    },
                    new OnlineDependency() {
                        Name = "Esprima",
                        TotalBytes = 204800,
                        URL = nugetBaseUrl + "esprima/1.0.1251",
                        SourceDirectories = new string[] {
                            Path.Combine("lib", "net45")
                        }
                    },
                    new OnlineDependency() {
                        Name = "Newtonsoft.Json",
                        TotalBytes = 2068480,
                        URL = nugetBaseUrl + "Newtonsoft.Json/13.0.1",
                        SourceDirectories = new string[] {
                            Path.Combine("lib", "net45")
                        }
                    },
                    new OnlineDependency() {
                        Name = "System.Buffers",
                        TotalBytes = 94208,
                        URL = nugetBaseUrl + "System.Buffers/4.5.1",
                        SourceDirectories = new string[] {
                            Path.Combine("lib", "net461")
                        }
                    },
                    new OnlineDependency() {
                        Name = "DnsClient.NET",
                        TotalBytes = 585728,
                        URL = nugetBaseUrl + "DnsClient/1.5.0",
                        SourceDirectories = new string[] {
                            Path.Combine("lib", "net45")
                        }
                    },

                };

                if (Environment.Is64BitOperatingSystem) {
                    ret.Add(new OnlineDependency() {
                        Name = "Redis64",
                        TotalBytes = 99184640,
                        URL = nugetBaseUrl + "cef.redist.x64/94.4.5",
                        SourceDirectories = new string[] { "CEF" }
                    });
                } else {
                    ret.Add(new OnlineDependency() {
                        Name = "Redis32",
                        TotalBytes = 93081600,
                        URL = nugetBaseUrl + "cef.redist.x86/94.4.5",
                        SourceDirectories = new string[] { "CEF" }
                    });
                }

                return ret;
            }
        }

        public static Version CefVersion {
            get {
                return new Version(94, 4, 50);
            }
        }

        public static bool OnlyCheck {
            get; private set; 
        }

        [STAThread]
        public static void Main() {

            if (handleCommandlineCommands())
                return;
                        
            // if an exception should be thrown and reach top level of excecution, log it(outside UI Thread)
            Application.ThreadException += new ThreadExceptionEventHandler((sender, ev) => {
                Logger.Log("Error occured: " + ev.Exception.StackTrace, Logger.LogLevel.error);
            });

            // if an exception should be thrown and reach top level of excecution, log it (inside UI Thread)           
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((sender, ev) => {
                Logger.Log("Error occured in gui thread: " + ((Exception)ev.ExceptionObject).StackTrace, Logger.LogLevel.error);
                Logger.Log("Error was: " + ((Exception)ev.ExceptionObject).Message, Logger.LogLevel.error);
            });


            initLogger();


            try {
                MainController.Instance.Start();
	        } catch (FileNotFoundException ex) { // occurs if the dependencies are mssing, so installation starts
                Logger.Log("Missing dependency: " + ex.FileName, Logger.LogLevel.error);
                startSetup();
            }
            
        }

        public static void ExitWithError(int exitCode, string errorMsg, bool writeToLog = false) {
            if(writeToLog)
                Logger.Log(errorMsg, Logger.LogLevel.error);

            if(Debugger.IsAttached) {
                Console.WriteLine("Error occured:");
                Console.WriteLine(errorMsg);
            }

            Console.Error.WriteLine(errorMsg);
            Environment.Exit(exitCode);
        }

        public static string GetCmdUpdateArgs(bool withGui = false, bool startArfter = false) {
            
            string allDependencies = "installedDependencies:";
            allDependencies += getDependenciesString();

            string cmdArgs = "cmd_install,";
            cmdArgs += "processId:" + Process.GetCurrentProcess().Id + ",";    
            if (startArfter)
                cmdArgs += "startAfter,";
            if(withGui)
                cmdArgs += "showUpdateGui,";
            cmdArgs += allDependencies + "=" + Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            return cmdArgs;
        }

        private static void initLogger() {
            string logLevel = Arguments.GetArgument("log-level");
            try {
                Logger.Init(logLevel, Arguments.GetArgument("logfile"));
            } catch (ArgumentException ex) {
                if (ex.ParamName == "logfilepath")
                    throw new ArgumentException("Command line argument 'logfile' had invalid value: " + Arguments.GetArgument("logfile"));
            }
            Logger.Log("Logger started");

            Logger.Log("Starting program");


            if (logLevel == "debug") {
                Logger.Log("Started with following arguments:", Logger.LogLevel.debug);
                foreach (var kv in Arguments.AllArguments) {
                    Logger.Log(kv.Key, Logger.LogLevel.debug);
                    Logger.Log("=", Logger.LogLevel.debug);
                    Logger.Log(kv.Value, Logger.LogLevel.debug);
                }
                Logger.Log("", Logger.LogLevel.debug);
            }
        }

        private static bool handleCommandlineCommands() {
            
            var args = Environment.GetCommandLineArgs();

            if (args.Length < 2)
                return false;

            if(args.Length > 2) {
                string temp = "";
                for (int i = 1; i < args.Length; i++) 
                    temp += args[i] + " ";
                temp = temp.Substring(0, temp.Length - 1);
                args = new string[] { args[0], temp };
            }

            if (args[1] == "version") {
                Console.WriteLine(MainController.Version);
                return true;
            }

            if (args[1] == "latest_version_info") {
                writeOutLatestReleaseInfo();
                return true;
            }

            if (args[1] == "cef_version") {
                Console.WriteLine(CefVersion.ToString());
                return true;
            }

            if (args[1] == "dependencies") {
                Console.WriteLine(getDependenciesString());
                return true;
            }

            if (args[1] == "update_args") {
                Console.WriteLine(GetCmdUpdateArgs());
                return true;
            }

            if (args[1] == "check") {
                OnlyCheck = true;

                try {
                    MainController.Instance.Start();
                } catch (FileNotFoundException) {
                    ExitWithError((int)Exitcode.CheckFailed, "At least one dependency is missing");
                } catch (Exception ex) {
                    ExitWithError((int)Exitcode.CheckFailed, "Unknown error occured: " + ex.Message);
                }

                Console.WriteLine("Ok");

                return true;
            }

            if (args[1].StartsWith("cmd_install")) {
                
                if(!args[1].Contains("="))
                    ExitWithError((int)Exitcode.CmdInstallFailed, "No installation path given.");

                var parts = args[1].Split('=');
                string installPath = parts[1];
                int processId = -1;
                bool checkInstallation = true;
                bool cleanup = false;
                bool onlyCopy = false;
                bool startAfter = false;
                bool showUpdateGui = false;

                // get settings if any
                if (parts[0].Contains(",")) {
                    var settings = parts[0].Split(',');
                    foreach(string setting in settings) {
                        if(setting.StartsWith("processId:")) {
                            string idStr = setting.Replace("processId:", "");
                            int.TryParse(idStr, out processId);
                        }
                        if(setting.StartsWith("dontCheck"))
                            checkInstallation = false;
                        if (setting.StartsWith("cleanup"))
                            cleanup = true;
                        if (setting.StartsWith("installedDependencies:")) {
                            // check if the necessary dependencies are installed
                            var necessaryDependencies = Dependencies;
                            var installedDependencies = setting.Split(':');
                            for (int i = 1; i < installedDependencies.Length; i++) {
                                string curDepency = installedDependencies[i];
                                necessaryDependencies.RemoveAll(v => v.URL.Replace(nugetBaseUrl, "") == curDepency);
                            }
                            onlyCopy = necessaryDependencies.Count == 0;
                            cleanup = necessaryDependencies.Count > 0;
                        }
                        if(setting.StartsWith("showUpdateGui"))
                            showUpdateGui = true;
                        if (setting.StartsWith("startAfter"))
                            startAfter = true;

                    }
                }
               
                if (processId > -1) {
                    // wait for old process to finish
                    int remainingMs = 5000;
                    bool stillRunning = true;

                    while (remainingMs > 0) {
                        try {
                            Process.GetProcessById(processId);
                        } catch (Exception) {
                            stillRunning = false;
                            break;
                        }
                        Thread.Sleep(500);
                        remainingMs -= 500;
                    }

                    if (stillRunning)
                        ExitWithError((int)Exitcode.CmdInstallFailed, "Reached waiting for process (" + processId + ") to stop after five seconds");

                }

                if(showUpdateGui && !onlyCopy) {
                    View.MissingDependenciesForm form = new View.MissingDependenciesForm();

                    form.IsUpdate = true;
                    form.InstallDirectory = installPath;

                    cleanupInstallationDirectory(installPath);

                    form.ShowDialog();
                   
                    return true;
                }

                commandlineSetup(installPath, checkInstallation, cleanup, onlyCopy);

                if(startAfter)
                    Process.Start(Path.Combine(installPath, "ScChrom.exe"));
                
                return true;
            }

            return false;
        }

        private static void cleanupInstallationDirectory(string destination) {
            Console.WriteLine("Cleaning destination path " + destination);
            try {
                string localsPath = Path.Combine(destination, "locales");
                if (Directory.Exists(localsPath))
                    Directory.Delete(localsPath, true);
                string swiftshaderPath = Path.Combine(destination, "swiftshader");
                if (Directory.Exists(swiftshaderPath))
                    Directory.Delete(swiftshaderPath, true);

                var files = Directory.GetFiles(destination);
                foreach (var file in files) {
                    if (file.ToLower().EndsWith(".sccf"))
                        continue;
                    File.Delete(file);
                }
            } catch (Exception ex) {
                ExitWithError((int)Exitcode.CmdInstallFailed, "Failed to cleanup destination: " + ex.Message);
            }
        }

        private static string getDependenciesString() {
            string allDependencies = "";
            foreach (var d in Dependencies)
                allDependencies += d.URL.Replace(nugetBaseUrl, "") + ":";
            allDependencies = allDependencies.Substring(0, allDependencies.Length - 1);
            return allDependencies;
        }

        private static void commandlineSetup(string destination, bool checkInstallation = true, bool cleanup = false, bool onlyCopy = false) {            

            destination = destination.Replace("\"", "");
            if (!Common.IsValidLocalPath(destination) || !Directory.Exists(destination)) 
                ExitWithError((int)Exitcode.CmdInstallFailed, "Invalid or none existing destination given");

            if(onlyCopy) {
                File.Copy(Assembly.GetEntryAssembly().Location, Path.Combine(destination, "ScChrom.exe"), true);
                return;
            }

            if(cleanup) {
                cleanupInstallationDirectory(destination);                
            }

            var waitTillFinished = new ManualResetEvent(false);
            var installer = new DependencyInstaller(Dependencies, true);
            
            installer.DownloadCanceled += (ex) => {
                string text = "Error occured during download, stopping. Error was: " + ex.Message;
                ExitWithError((int)Exitcode.CmdInstallFailed, text);
            };
            installer.ExtractionStarted += (dependencyName) => {
                Console.WriteLine("Extracting " + dependencyName);
            };
            installer.DownloadStarted += () => {
                Console.WriteLine("Downloading next dependency");
            };
            installer.ProgressChanged += (ProgressState progress) => {
                if(progress.Progress > 0)
                    Console.WriteLine(progress.Progress + "% finished");
            };
            installer.ErrorOccured += (ex) => {
                string text = "Error occured during setup, stopping. Error was: " + ex.Message;
                ExitWithError((int)Exitcode.CmdInstallFailed, text);
            };
            installer.InstallationFinished += () => {
                
                if(checkInstallation) {
                    try {
                        var proc = Process.Start(Path.Combine(installer.DestinationDirectory, "ScChrom.exe"), "check");
                        if (!proc.WaitForExit(30 * 1000)) {
                            ExitWithError((int)Exitcode.CmdInstallFailed, "Check of the new setup ScChrom failed because of timeout of new installation");
                        }
                    } catch (Exception ex) {
                        ExitWithError((int)Exitcode.CmdInstallFailed, "Check of setup ScChrom failed: " + ex.Message);
                    }
                    Console.WriteLine("Check finished");
                }

                Console.WriteLine("Installation finished");

                waitTillFinished.Set();
            };
            
            installer.DestinationDirectory = destination;
            installer.DownloadDependencies();

            // Wait max half an hour (enough for DSL 1000) before closing with error
            if(!waitTillFinished.WaitOne(30 * 60 * 60)) {
                ExitWithError((int)Exitcode.CmdInstallFailed, "Automatically canceled installation after timeout");
            }
        }        

        private static void startSetup() {
            Logger.Log("Starting setup", Logger.LogLevel.info);            
            var dia = new View.MissingDependenciesForm();
            Application.Run(dia);
        }
        
        private static void writeOutLatestReleaseInfo() {
            // necessary cause github enforces tls 1.2
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            System.Net.WebClient wc = new System.Net.WebClient();
            // without user agent github api will respond with 403
            wc.Headers.Add("User-Agent:" + "ScChrom");

            string resultString = null;
            try {
               
                resultString = wc.DownloadString(LatestReleaseInfoUrl);
            
                // simplified parsing to reduce dependencies
                string tagNameStart = "\"tag_name\":";
                string tagNameEnd = ",";

                int startIndex = resultString.IndexOf(tagNameStart) + tagNameStart.Length;
                int endIndex = resultString.IndexOf(tagNameEnd, startIndex);

                string tagNameString = resultString.Substring(startIndex, endIndex - startIndex);
                tagNameString = tagNameString.Replace("\"", "").Replace(" ", "");

                Version onlineVersion = null;
                Version.TryParse(tagNameString, out onlineVersion);
                Version currentVersion = null;
                Version.TryParse(Application.ProductVersion, out currentVersion);

                Console.WriteLine(tagNameString);
                Console.WriteLine(LatestReleaseDownloadUrl);
                if(onlineVersion != null)
                    Console.WriteLine("up to date:" + (onlineVersion <= currentVersion).ToString());
            } catch (Exception ex) {
                ExitWithError((int)Exitcode.UpdateError, "Error while getting infos about latest version: " + ex.Message);
                return;
            }
        }
        
        public static void DownloadAndStartUpdate() {            
            
            string updatePath = Path.Combine(Path.GetTempPath(), "ScChrom.exe");

            ManualResetEventSlim waitForFinish = new ManualResetEventSlim(false);

            // necessary cause github enforces tls 1.2
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            System.Net.WebClient wc = new System.Net.WebClient();
            Exception err = null;
            int lastPercentage = -1;

            wc.DownloadProgressChanged += (sender, e) => {
                if (lastPercentage >= e.ProgressPercentage)
                    return;
                lastPercentage = e.ProgressPercentage;

                var jobj = new Newtonsoft.Json.Linq.JObject();
                jobj["progress"] = lastPercentage;
                jobj["received_bytes"] = e.BytesReceived;
                jobj["total_bytes"] = e.TotalBytesToReceive;
                
                MainController.Instance.WindowInstance.CallInBrowserCallback("update_progress", jobj.ToString(Newtonsoft.Json.Formatting.None));
            };

            wc.DownloadFileCompleted += (sender, e) => {
                err = e.Error;
                waitForFinish.Set();
            };

            wc.DownloadFileAsync(new Uri(LatestReleaseDownloadUrl), updatePath);
            
            waitForFinish.Wait(2 * 60 * 1000);

            if(err != null) {
                var jobj = new Newtonsoft.Json.Linq.JObject();
                jobj["error"] = err.Message;

                MainController.Instance.WindowInstance.CallInBrowserCallback("update_progress", jobj.ToString(Newtonsoft.Json.Formatting.None));

                Logger.Log("Error while downloading update: " + err.Message, Logger.LogLevel.error);

                return;
            }
            
            string cmdArgs = GetCmdUpdateArgs(true, true);           

            Process.Start(updatePath, cmdArgs);

            Logger.Log("Closing ScChrom and starting update via: " + updatePath + " " + cmdArgs);

            Application.Exit();

        }
        
    }
}
