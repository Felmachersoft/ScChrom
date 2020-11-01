using CefSharp;
using CefSharp.WinForms;
using ScChrom.BrowserJs;
using ScChrom.Filter;
using ScChrom.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScChrom {
    public class MainController {

        private static MainController _instance = null;

        /// <summary>
        /// The version of ScChrom
        /// </summary>
        public static string Version {
            get {
                return System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            }
        }

        /// <summary>
        /// Note: All registered handlers will be removed after mainwindow closed
        /// </summary>
        public static event Action<string> WrittenOut;

        /// <summary>
        /// Note: All registered handlers will be removed after mainwindow closed
        /// </summary>
        public static event Action<string> ErrorOut;

        public static MainController Instance {
            get {
                if (_instance == null)
                    _instance = new MainController();
                return _instance;
            }
        }        

        private bool _isRunning = false;

        public bool IsRunning {
            get {
                return _isRunning;
            }
        }        
        
        private bool _restart = false;

        /// <summary>
        /// Restarts the maincontroller by closing the old mainwindow
        /// and starting again.
        /// Change the CommandLineArgs before calling this to change Settings.
        /// </summary>
        public void Restart() {

            if(_instance.WindowInstance.IsHandleCreated) {
                _restart = true;
                _instance.WindowInstance.Invoke(new Action(() => {
                    _instance.WindowInstance.Close();
                }));
            } else {
                _isRunning = false;
                _restart = false;
                Start();                
            }
        }

        
        public BrowserForm WindowInstance { get; private set; }

        public BrowserJsController BrowserJsController { get; private set; }
        
        public event Action Closed;        
        public event Action Started;

        private MainController() { }

        /// <summary>
        /// Everything si initilized and run here.
        /// Blocks till main window closes.
        /// </summary>
        private void run() {
            
            Tools.Logger.Log("Starting the MainController", Logger.LogLevel.debug);

            if (Arguments.AllArguments == null || Arguments.AllArguments.Count == 0) {
                Tools.Logger.Log("No command line arguments given, showing startup page", Logger.LogLevel.debug);
                Arguments.ParseConfigArgs(new string[]{ "--url=ScChrom://internal/startup" });
            }

            // prepare to cleanup
            Closed += () => {                
                Started = null;
                WrittenOut = null;
                ErrorOut = null;
            };
            
            WrittenOut += (text) => {
                Console.WriteLine(text);
            };

            ErrorOut += (text) => {
                Console.Error.WriteLine(text);
            };

            //For Windows 7 and above, best to include relevant app.manifest entries as well
            Cef.EnableHighDPISupport();

            int max_runtime = Arguments.GetArgumentInt("max-runtime", 0);
            if (max_runtime > 0) {
                Task.Run(() => {
                    System.Threading.ManualResetEvent waiting = new System.Threading.ManualResetEvent(false);
                    Action handlePriorClosing = () => {
                        waiting.Set();
                    };
                    Closed += handlePriorClosing;

                    if (!waiting.WaitOne(1000 * max_runtime)) {
                        Closed -= handlePriorClosing;
                        Application.Exit();
                    } else {
                        Tools.Logger.Log("Prevented closing due to max-runtime", Logger.LogLevel.debug);
                    }
                });
            }
                                                                     
                        

            if (!Cef.IsInitialized) {
                var settings = new CefSettings() {                    
                    // the positiv value enables uncaught exception
                    UncaughtExceptionStackSize = 10
                };
                
                string backgroundColor = Arguments.GetArgument("background-color", "").Trim();
                if (!string.IsNullOrWhiteSpace(backgroundColor)) {
                    try {
                        settings.BackgroundColor = Convert.ToUInt32(backgroundColor, 16);
                    } catch (Exception) {
                        Logger.Log("Ignored invalid color code given for attribute initial-background-color.", Logger.LogLevel.error);
                    }
                }

                var scchromOptions = CefSharp.Enums.SchemeOptions.Secure |
                    CefSharp.Enums.SchemeOptions.FetchEnabled |
                    CefSharp.Enums.SchemeOptions.CspBypassing |
                    CefSharp.Enums.SchemeOptions.CorsEnabled;
                settings.RegisterScheme(new CefCustomScheme("scchrom", scchromOptions));

                string remoteDebuggingPort = Arguments.GetArgument("remote-debugging-port", "").Trim();
                if(!string.IsNullOrWhiteSpace(remoteDebuggingPort)) {
                    int port = 0;
                    if(int.TryParse(remoteDebuggingPort, out port)) {
                        settings.RemoteDebuggingPort = port;
                    } else {
                        Logger.Log("Ignored invalid number given for attribute remote-debugging-port: " + port, Logger.LogLevel.error);
                    }                    
                }


                string useragent = Arguments.GetArgument("useragent", "").Trim();
                if(!string.IsNullOrWhiteSpace(useragent)) {
                    settings.UserAgent = useragent;
                }
                

                string cachePath = Arguments.GetArgument("cache-path", "false").Trim();
                if (cachePath != "false") {
                    string fullPath = null;
                    try {
                        if(cachePath.Contains("%configfile_dir%")) {                            
                            if (Arguments.ConfigfileDirectory == null) {
                                cachePath = cachePath.Replace("%configfile_dir%", Environment.CurrentDirectory);
                            } else {
                                cachePath = cachePath.Replace("%configfile_dir%", Arguments.ConfigfileDirectory);
                            }
                        }
                        fullPath = Path.GetFullPath(cachePath);                        
                    } catch (Exception ex) {
                        // path not valid
                        Logger.Log("Invalid cache-path given (" + cachePath + "), error was: " + ex.Message, Logger.LogLevel.error);
                        Environment.Exit(4);
                    }
                    
                    cachePath = Environment.ExpandEnvironmentVariables(cachePath);
                    if (!Path.IsPathRooted(cachePath)) {
                        cachePath = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), cachePath);
                    }

                    settings.CachePath = cachePath;
                } else {
                    // disable creation of the gpu cache
                    settings.CefCommandLineArgs.Add("disable-gpu-shader-disk-cache", "1");
                }
                

                // prevent interference with default command line args for cef
                settings.CommandLineArgsDisabled = true;
                               
                if(Arguments.GetArgument("webrtc-media-enabled", "false") == "true")
                    settings.CefCommandLineArgs.Add("enable-media-stream", "1");

                
                // necessary to enable some special features for the scchrom 'protocol'
                settings.RegisterScheme(new CefCustomScheme("scchrom", CefSharp.Enums.SchemeOptions.Standard | CefSharp.Enums.SchemeOptions.CorsEnabled | CefSharp.Enums.SchemeOptions.FetchEnabled));
                                

                string proxy_settings = Arguments.GetArgument("proxy-settings");
                if (proxy_settings != null) {
                    var parts = proxy_settings.Split('|');
                    string proxy_ip = "", proxy_port = "", proxy_username = "", proxy_password = "", proxy_ignoreList = "";
                    if (parts.Length > 0) {
                        proxy_ip = parts[0];
                        if (parts.Length > 1)
                            proxy_port = parts[1];
                        if (parts.Length > 2)
                            proxy_username = parts[2];
                        if (parts.Length > 3)
                            proxy_password = parts[3];
                        if (parts.Length > 4)
                            proxy_ignoreList = parts[4];

                        Logger.Log("Applying following proxy settings: Ip: " + proxy_ip + " Port: " + proxy_port + " username: " + proxy_username + " ignorelist: " + proxy_ignoreList, Logger.LogLevel.debug);
                        CefSharpSettings.Proxy = new ProxyOptions(proxy_ip, proxy_password, proxy_username, proxy_password, proxy_ignoreList);

                        // The previous setting alone might not work, so also add it to the command line arguments
                        settings.CefCommandLineArgs.Add("proxy-server", proxy_ip + ":" + proxy_port);
                    } else {
                        Logger.Log("Ignored invalid value parameter proxy-settings", Logger.LogLevel.info);
                    }
                }                

                //Perform dependency check to make sure all relevant resources are in our output directory.
                Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
            }

            setupSchemes();
            

            BrowserJsController = new BrowserJsController(Arguments.GetArgument("browser-js-allow_objects", "false"));

            var mainWindow = new BrowserForm();
            WindowInstance = mainWindow;
            
            if (Started != null)
                Started.Invoke();
                                   
            mainWindow.ShowDialog();
            
        }

       
        /// <summary>
        /// Enables requests to certain schemes (http, https and scchrom) to be exchanged and manipulated.
        /// Also adds the scchrom scheme to provide scchrom internal stuff like the editors help content.
        /// </summary>
        private void setupSchemes() {
            
            var whitelist = new List<RequestIdentifier>();
            string whitelistString = Arguments.GetArgument("request-whitelist", "").Trim();
            if (!string.IsNullOrWhiteSpace(whitelistString)) {

                string[] whitelistEntries = whitelistString.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                foreach (string entry in whitelistEntries) {                    
                    whitelist.Add(new RequestIdentifier(entry));
                }
            }

            List<Tuple<RequestIdentifier, string>> response_exchanges_utf8 = null;
            var exchangeResponseHandler = Tools.Arguments.GetStackedArguments("exchange-response-utf8");
            if (exchangeResponseHandler != null) {
                response_exchanges_utf8 = new List<Tuple<RequestIdentifier, string>>();
                foreach (var rrh in exchangeResponseHandler) {
                    
                    var filter = new RequestIdentifier(rrh.Key);
                    response_exchanges_utf8.Add(new Tuple<RequestIdentifier, string>(filter, rrh.Value));
                }
            }

            List<Tuple<RequestIdentifier, string>> js_Rewrites = null;
            var js_rewriteHandler = Tools.Arguments.GetStackedArguments("exchange-response-utf8_script");
            if (js_rewriteHandler != null) {
                js_Rewrites = new List<Tuple<RequestIdentifier, string>>();
                foreach (var rrh in js_rewriteHandler) {                    
                    var filter = new RequestIdentifier(rrh.Key);
                    js_Rewrites.Add(new Tuple<RequestIdentifier, string>(filter, rrh.Value));
                }
            }


            var resourceSchemehandler = new Handler.SchemeHandlerFactory(whitelist, response_exchanges_utf8, js_Rewrites);
            
            // remove old schemes and ...
            Cef.ClearSchemeHandlerFactories();

            // ... register all new schemes
            var globalRequestContext = Cef.GetGlobalRequestContext();
            globalRequestContext.RegisterSchemeHandlerFactory("http", "", resourceSchemehandler);
            globalRequestContext.RegisterSchemeHandlerFactory("https", "", resourceSchemehandler);
            globalRequestContext.RegisterSchemeHandlerFactory("scchrom", "", resourceSchemehandler);
            
        }

        /// <summary>
        /// Starts the preparation to run the main controller and handles restarts.
        /// <para>Must be called before using the MainController instance.</para>
        /// <para>Blocks until mainwindow closes and no restart is done.</para>
        /// <para>If unsure if already running check via IsRunning and use Restart than.</para>
        /// </summary>
        public void Start() {

            if (_isRunning) {
                Tools.Logger.Log("Starting the MainController a second time is forbidden. Use the Restart method.", Logger.LogLevel.error);
                throw new ApplicationException("Starting the MainController a second time is forbidden. Use the Restart method.");
            }
            _isRunning = true;

            run();
            
            if(Closed != null)
                Closed.Invoke();

            if (_restart) {
                _restart = false;

                // help gc
                _instance = null;
                Instance.Start();
            }
            
        }

        public void WriteOut(string text) {
            if (WrittenOut != null)
                WrittenOut.Invoke(text);
        }

        public void WriteErrorOut(string text) {
            if (ErrorOut != null)
                ErrorOut.Invoke(text);
        }
    }
}
