using System;
using System.Windows.Forms;
using CefSharp.WinForms;
using System.Collections.Generic;
using System.IO;
using CefSharp;
using ScChrom.CustomResourceRequestHandlers;
using ScChrom.Tools;
using System.Threading.Tasks;
using ScChrom.Handler;
using ScChrom.View;
using System.Drawing;

namespace ScChrom {

    public partial class BrowserForm : Form
    {
#region private fields

        private readonly ChromiumWebBrowser browser;

        private bool _output_browser_console = false;
        private bool _firstShow = true;
        private bool _preventStealingFocus = false;
        private bool _keepTopmost = false;

        private ChromeWidgetMessageInterceptor messageInterceptor;

        private Dictionary<string, string> _browserConsoleScripts = new Dictionary<string, string>();

#endregion


#region properties
        /// <summary>
        /// This hack prevents the window from stealing focus when it shows for the first time
        /// </summary>
        protected override CreateParams CreateParams {
            get {
                CreateParams ret = base.CreateParams;
                
                if (_preventStealingFocus && _firstShow) {
                    const int WS_EX_NOACTIVATE = 0x08000000;
                    ret.ExStyle |= WS_EX_NOACTIVATE;
                }
                
                if(_keepTopmost) {
                    const int WS_EX_TOPMOST = 0x00000008;
                    ret.ExStyle |= WS_EX_TOPMOST;
                }
                
                return ret;
            }
        }
                
        protected override bool ShowWithoutActivation {
            get { return _preventStealingFocus; }
        }

        public bool IsTopMost {
            get {
                return TopMost || _keepTopmost;
            }
            set {
                if (!value)
                    _keepTopmost = false;
                TopMost = value;
            }
        }

        public ChromiumWebBrowser ChromeBrowserInstance { get { return browser; } }
        #endregion


        public BrowserForm() {
            InitializeComponent();

            Text = "loading...";


            string html = Tools.Arguments.GetArgument("html", null);
            string url = null;
            if (html != null) {
                browser = new ChromiumWebBrowser((CefSharp.Web.HtmlString)html);
            } else {
                url = Tools.Arguments.GetArgument("url", "ScChrom://internal/startup").Trim();
                // get absolute path of file if relative 
                if (url.StartsWith(".."))
                    url = Path.Combine(Environment.CurrentDirectory, url.Replace("/", "\\"));
                browser = new ChromiumWebBrowser(url);
            }
            

            browser.Dock = DockStyle.Fill;
            browser.ActivateBrowserOnCreation = false;


            toolStripContainer.ContentPanel.Controls.Add(browser);


            if (Tools.Arguments.GetArgument("cache-disabled", "false").Trim() == "true")
                browser.BrowserSettings.ApplicationCache = CefState.Disabled;

            if (Tools.Arguments.GetArgument("disable-localstorage", "false").Trim() == "true")
                browser.BrowserSettings.LocalStorage = CefState.Disabled;


            if (Arguments.GetArgument("ignore-crossorigin-from-files") == "true") {
                browser.BrowserSettings.FileAccessFromFileUrls = CefState.Enabled;
                browser.BrowserSettings.UniversalAccessFromFileUrls = CefState.Enabled;
            }

            if (Arguments.GetArgument("disable-browser-js") == "true") {
                browser.BrowserSettings.Javascript = CefState.Disabled;
            }

            // setup listener
            browser.LoadingStateChanged += OnLoadingStateChanged;
            browser.ConsoleMessage += OnBrowserConsoleMessage;
            browser.LoadError += browser_LoadError;            
            browser.TitleChanged += OnBrowserTitleChanged;
            browser.AddressChanged += OnBrowserAddressChanged;
                                    


            init_formsettings(url);

            browser.ResourceRequestHandlerFactory = new CustomResourceRequestHandlerFactory();

            init_parameter_keyboard();

            init_parameter_draghandler();

            init_event_taskbaricon_contextmenu();

            init_event_contextmenu();

            init_parameter_event_download();

            init_event_onRequestResponseUtf8();

            init_event_onBeforeRequest();

            init_event_onResponse();

            init_parameter_append_to_mainpage();

            init_parameter_cookie();

            init_event_onJsDialog();

            init_event_onFileChange();
        }


#region public methods

        public void ShowInfoBalloon(string title, string content = "", int milliseconds = 30000) {

            bool oldVisibility = notifyIcon.Visible;

            // forces last balloon to vanish
            notifyIcon.Visible = true;

            notifyIcon.BalloonTipTitle = title;

            if (!string.IsNullOrEmpty(content)) {
                notifyIcon.BalloonTipText = content;
            }

            notifyIcon.ShowBalloonTip(milliseconds);
            notifyIcon.BalloonTipClicked += NotifyIcon_BalloonTipClicked;

            // hide the notifyIcon again, if it wasn't visible before
            if(!oldVisibility) {
                Task.Run(() => {
                    System.Threading.Thread.Sleep(milliseconds);
                    BeginInvoke(new Action(() => {
                        notifyIcon.Visible = false;
                    }));
                });
            }
        }
        
        public void LoadUrl(string url) {
            if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute)) {
                browser.Load(url);
            }
        }

        public void BrowserReload() {
            browser.Reload();
        }

        public void CallInBrowserCallback(string id, string value) {
            string mainControllerName = Arguments.GetArgument("browser-js-mainController_name", "ScChrom");
            string js = "if(" + mainControllerName + ") ";
            // escape '
            value = value.Replace("'", "\\'");
            js += mainControllerName + ".jintCallbacks['" + id + "']('" + value + "');";
            try {
                MainController.Instance.WindowInstance.BeginInvoke(new Action(() => {
                    MainController.Instance.WindowInstance.ExecuteJSinBrowserContext(js);
                }));
            } catch (Exception ex) {
                Tools.Logger.Log("Error occured while calling in browser callback from jint context: " + ex.Message, Tools.Logger.LogLevel.error);
            }
        }

        public void ExecuteJSinBrowserContext(string javascript) {
            browser.ExecuteScriptAsync(javascript);            
        }

        public void IsBrowserMainJsControllerReady(string mainControllerName, Action<bool> callback) {

            string checkscript = "typeof " + mainControllerName + " === 'undefined' ? false : " + mainControllerName + ".ready";
            
            System.Threading.ManualResetEventSlim waitForBrowser = new System.Threading.ManualResetEventSlim(false);
            var t = browser.EvaluateScriptAsync(checkscript).ContinueWith((resp) => {
                bool ret = Convert.ToBoolean(resp.Result.Result);
                waitForBrowser.Set();
                callback(ret);
            });           
        }        

        #endregion


#region private methods

        private void setCanGoBack(bool canGoBack) {
            this.InvokeOnUiThreadIfRequired(() => backButton.Enabled = canGoBack);
        }

        private void setCanGoForward(bool canGoForward) {
            this.InvokeOnUiThreadIfRequired(() => forwardButton.Enabled = canGoForward);
        }
        
        private void setIsLoading(bool isLoading) {
            this.InvokeOnUiThreadIfRequired(() => {
                goButton.Text = isLoading ?
                "Stop" :
                "Go";
                goButton.Image = isLoading ?
                    Properties.Resources.nav_plain_red :
                    Properties.Resources.nav_plain_green;

                HandleToolStripLayout();
            });            
        }

        private void HandleToolStripLayout(object sender, LayoutEventArgs e){
            HandleToolStripLayout();
        }

        private void HandleToolStripLayout() {
            var width = ts_top.Width;
            foreach (ToolStripItem item in ts_top.Items)
            {
                if (item != urlTextBox)
                {
                    width -= item.Width - item.Margin.Horizontal;
                }
            }
            urlTextBox.Width = Math.Max(0, width - urlTextBox.Margin.Horizontal - 18);
        }

        /// <summary>
        /// The ChromiumWebBrowserControl does not fire MouseEnter/Move/Leave events, because Chromium handles these.
        /// This method provides a demo of hooking the Chrome_RenderWidgetHostHWND handle to receive low level messages.
        /// You can likely hook other window messages using this technique, drag/drop etc
        /// </summary>
        private void SetupMessageInterceptor() {
            if (messageInterceptor != null) {
                messageInterceptor.ReleaseHandle();
                messageInterceptor = null;
            }

            IntPtr browserHandle = default(IntPtr);
            Invoke(new Action(() => {
                browserHandle = browser.Handle;
            }));

            Task.Run(() => {
                try {
                    while (true) {
                        IntPtr chromeWidgetHostHandle;
                        if (ChromeWidgetHandleFinder.TryFindHandle(browserHandle, out chromeWidgetHostHandle)) {
                            messageInterceptor = new ChromeWidgetMessageInterceptor((Control)browser, chromeWidgetHostHandle, message => {

                                // Render process switch happened, need to find the new handle
                                if (message.Msg == ViewTools.WM_DESTROY) {
                                    SetupMessageInterceptor();
                                    return;
                                }

                                if (message.Msg == ViewTools.WM_LBUTTONDOWN) {
                                    Point point = new Point(message.LParam.ToInt32());                                    
                                    // handdles moving the window via -webkit-app-region: drag;
                                    if (((DragHandler)browser.DragHandler).draggableRegion.IsVisible(point)) {
                                        ViewTools.ReleaseCapture();

                                        this.InvokeOnUiThreadIfRequired(() => {
                                            Task.Run(() => ViewTools.SendMessage(Handle, ViewTools.WM_NCLBUTTONDOWN, ViewTools.HT_CAPTION, 0));
                                        });

                                    }
                                }
                            });

                            break;
                        } else {
                            // Chrome hasn't yet set up its message-loop window.
                            System.Threading.Thread.Sleep(10);
                        }
                    }
                } catch {
                    // Errors are likely to occur if browser is disposed, and no good way to check from another thread
                }
            });
        }

#endregion


#region eventhandler
        private void GoButtonClick(object sender, EventArgs e){
            LoadUrl(urlTextBox.Text);
        }

        private void BackButtonClick(object sender, EventArgs e){
            browser.Back();
        }

        private void ForwardButtonClick(object sender, EventArgs e){
            browser.Forward();
        }

        private void UrlTextBoxKeyUp(object sender, KeyEventArgs e){
            if (e.KeyCode != Keys.Enter){
                return;
            }

            LoadUrl(urlTextBox.Text);
        }        

        private void BrowserForm_Shown(object sender, EventArgs e) {
            _firstShow = false;
        }

        private void BrowserForm_FormClosing(object sender, FormClosingEventArgs e) {
            notifyIcon.Visible = false;

            var onBeforeCloseHandler = Tools.Arguments.GetArgument("on-before-close");
            if (onBeforeCloseHandler != null) {
                JSEngine.Instance.Execute(onBeforeCloseHandler, "on-before-close");
            }
        }

        private void tSddb_menu_DropDownOpening(object sender, EventArgs e) {
            tsmi_topmost.Checked = TopMost || _keepTopmost;
        }

        private void tsmi_ShowDevTools_Click(object sender, EventArgs e) {
            browser.ShowDevTools();
        }
        private void tsmiRefresh_Click(object sender, EventArgs e) {
            browser.Refresh();
        }

        private void tsmi_topmost_Click(object sender, EventArgs e) {            
            TopMost = !(TopMost || _keepTopmost);
            _keepTopmost = TopMost;
        }

        private void NotifyIcon_BalloonTipClicked(object sender, EventArgs e) {
            bool notifyIconVisible = (Arguments.GetArgument("window-show_notifyicon") == "true");
            if (!notifyIconVisible) {
                notifyIcon.Visible = false;
            }
        }

        void browser_LoadError(object sender, LoadErrorEventArgs e) {
            Tools.Logger.Log("Failed (code " + e.ErrorCode + ") '" + e.FailedUrl + "' ", Tools.Logger.LogLevel.info);
        }

        private void OnBrowserConsoleMessage(object sender, ConsoleMessageEventArgs args) {

            Tools.Logger.Log("[CONSOLE]" + args.Message, Tools.Logger.LogLevel.info);
            // Console.WriteLine(string.Format("Line: {0}, Source: {1}, Message: {2}", args.Line, args.Source, args.Message));
            if (this._output_browser_console)
                Console.WriteLine(args.Message);

            foreach (var kv in _browserConsoleScripts) {
                string paramName = "on-console-message";

                if (!string.IsNullOrWhiteSpace(kv.Key))
                    paramName += "<" + kv.Key + ">";

                // inject values before execution
                Tools.JSEngine.Instance.SetValue("content", args.Message);
                Tools.JSEngine.Instance.SetValue("level", Enum.GetName(typeof(LogSeverity), args.Level));

                Tools.JSEngine.Instance.Execute(kv.Value, paramName);
            }

        }


        private void OnLoadingStateChanged(object sender, LoadingStateChangedEventArgs args) {
            setCanGoBack(args.CanGoBack);
            setCanGoForward(args.CanGoForward);
            setIsLoading(args.IsLoading);

            if (!args.IsLoading) {
                
                Tools.Logger.Log("Finished loading of " + args.Browser.MainFrame.Url, Logger.LogLevel.debug);                                            

                string mainControllerName = Arguments.GetArgument("browser-js-mainController_name", "ScChrom");
                string postInitScript = Tools.Arguments.GetArgument("injected-javascript", "");
                                
                MainController.Instance.BrowserJsController.InitBrowser(browser, mainControllerName, postInitScript);                
            }

        }

        private void OnBrowserTitleChanged(object sender, TitleChangedEventArgs args) {
            this.InvokeOnUiThreadIfRequired(() => Text = args.Title);
        }

        private void OnBrowserAddressChanged(object sender, AddressChangedEventArgs args) {
            this.InvokeOnUiThreadIfRequired(() => urlTextBox.Text = args.Address);
        }
#endregion


#region Init methods

        private void init_parameter_keyboard() {

            string onBeforeKey_script = Arguments.GetArgument("on-before-key", null);
            string onAfterKey_script = Arguments.GetArgument("on-after-key", null);

            browser.KeyboardHandler = new KeyboardHandler(onBeforeKey_script, onAfterKey_script);
        }

        private void init_parameter_draghandler() {
            browser.DragHandler = new DragHandler();
            // without this the logic in SetupMessageInterceptor will cause freezes while debugging!
            CheckForIllegalCrossThreadCalls = false;

            browser.IsBrowserInitializedChanged += (e,v)=>{
                if(browser.IsBrowserInitialized)
                    SetupMessageInterceptor();
            };
        }

        private void init_formsettings(string url) {
            _preventStealingFocus = (Arguments.GetArgument("window-prevent_stealing_focus", "").ToLower().Trim() == "true");
            _keepTopmost = Arguments.GetArgument("window-topmost", "false").ToLower().Trim() == "true";
            

            ShowInTaskbar = !(Arguments.GetArgument("window-show_in_taskbar", "true").ToLower().Trim() == "false");
            ts_top.Visible = !(Arguments.GetArgument("window-hide_url_field", "false").ToLower().Trim() == "true");

            string windowBorder = Arguments.GetArgument("window-border", "sizable").Trim().ToLower();
            switch (windowBorder) {
                case "none":
                    FormBorderStyle = FormBorderStyle.None;
                    break;
                case "fixed":
                    FormBorderStyle = FormBorderStyle.FixedSingle;
                    MaximizeBox = false;
                    break;
                default:
                    FormBorderStyle = FormBorderStyle.Sizable;
                    break;
            }

            // position and size
            Width = Arguments.GetArgumentInt("window-width", 800);
            Height = Arguments.GetArgumentInt("window-height", 500);

            int windowPosX = Arguments.GetArgumentInt("window-pos-x", 400);
            if (windowPosX < 0)
                windowPosX = Screen.GetWorkingArea(this).Width - this.Width + windowPosX;
            int windowPosY = Arguments.GetArgumentInt("window-pos-y", 400);
            if (windowPosY < 0)
                windowPosY = Screen.GetWorkingArea(this).Height - this.Height + windowPosY;
            
            StartPosition = FormStartPosition.Manual;
            Location = new System.Drawing.Point(windowPosX, windowPosY);

            // window state
            string windowStateString = Arguments.GetArgument("window-state", "normal").ToLower().Trim();
            switch (windowStateString) {
                case "maximized": this.WindowState = FormWindowState.Maximized; break;
                case "minimized": this.WindowState = FormWindowState.Minimized; break;
                default:
                    this.WindowState = FormWindowState.Normal;
                    if (!string.IsNullOrWhiteSpace(windowStateString) && windowStateString != "normal")
                        Tools.Logger.Log("Invalid value given for parameter 'window-state': " + windowStateString, Tools.Logger.LogLevel.error);
                    break;
            }

            this.ShowInTaskbar = Arguments.GetArgument("window-show_in_taskbar", "true").ToLower().Trim() == "true";


            bool allowRedirects = !(Arguments.GetArgument("prevent-pagechange", "false").ToLower().Trim() == "true");
            string preventPagechangeUrls = Arguments.GetArgument("allowed-pagechange_urls", "");
            string[] ppUrls = new string[] { };
            if (!string.IsNullOrEmpty(preventPagechangeUrls))
                ppUrls = preventPagechangeUrls.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            string onBeforeBrowseHandlerScript = Arguments.GetArgument("on-before-browse");

            bool allowExternalLinks = Arguments.GetArgument("allow-external-links", "false").ToLower().Trim() == "true";
            browser.RequestHandler = new CustomRequestHandler(ppUrls, allowRedirects, onBeforeBrowseHandlerScript, allowExternalLinks);

            // TODO hier den parameter so machen, dass wenn man damit das öffnen in einem neuen tab im aktuellen browser machen kann (z.B. auf startpage.de)
            bool preventPopups = Arguments.GetArgument("prevent-popups", "true") != "false";
            if (preventPopups) {
                // prevents popups
                browser.LifeSpanHandler = new LifespanHandler();
            }

            this._output_browser_console = Tools.Arguments.GetArgument("output-browser-console", "false").Trim().ToLower() == "true";
            var browserConsoleScripts = Tools.Arguments.GetStackedArguments("on-console-message");
            if (browserConsoleScripts != null) {
                _browserConsoleScripts = browserConsoleScripts;
            }


            notifyIcon.Visible = (Arguments.GetArgument("window-show_notifyicon", "false").Trim().ToLower() == "true");
            
            ContextMenuStrip cms = new ContextMenuStrip();
            cms.Items.Add(new ToolStripMenuItem(url, null) { Enabled = false });
            cms.Items.Add(new ToolStripMenuItem("Restart", null, (e, s) => {
                MainController.Instance.Restart();
            }));
            cms.Items.Add(new ToolStripSeparator());
            cms.Items.Add("Exit", null, (e, s) => {
                Application.Exit();
            });
            notifyIcon.ContextMenuStrip = cms;
        }

        private void init_event_taskbaricon_contextmenu() {
            
            // creates the custom context menu if set
            string onbeforeContextmenu_script = Arguments.GetArgument("on-before-notifyicon-contextmenu");
            if(!string.IsNullOrWhiteSpace(onbeforeContextmenu_script)) {
                string parameterName = "on-before-notifyicon-contextmenu";

                notifyIcon.MouseDown += (object sender, MouseEventArgs e) => {
                    if(e.Button != System.Windows.Forms.MouseButtons.Right)
                        return;

                    // Remove custom items before showing again
                    var indicesToRemove = new List<int>();
                    for (int i = 0; i < notifyIcon.ContextMenuStrip.Items.Count; i++) {
                        if (notifyIcon.ContextMenuStrip.Items[i].Tag != null)
                            indicesToRemove.Add(i);
                    }
                    indicesToRemove.Reverse();
                    foreach(int index in indicesToRemove)
                        notifyIcon.ContextMenuStrip.Items.RemoveAt(index);
                    

                    Tools.Logger.Log("Executing custom script for " + parameterName + " handler", Logger.LogLevel.debug);

                    var key_url = new Jint.Key("url");
                    JSEngine.Instance.Engine.SetValue(ref key_url, browser.GetMainFrame().Url);

                    string result_string = JSEngine.Instance.ExecuteResult(onbeforeContextmenu_script, "on-before-taskbaricon-contextmenu");
                    if (string.IsNullOrWhiteSpace(result_string) || result_string == "undefined") {
                        Tools.Logger.Log("Ignored empty result from " + parameterName, Tools.Logger.LogLevel.info);
                        return;
                    }
                    
                    Common.ApplyJsonToContextmenustrip(notifyIcon.ContextMenuStrip, result_string, "on-before-notifyicon-contextmenu");                                        
                };
            }

            string onContextmenuClick_script = Arguments.GetArgument("on-click-notifyicon-contextmenu");
            if(!string.IsNullOrWhiteSpace(onContextmenuClick_script)) {
                string parameterName = "on-click-notifyicon-contextmenu";

                notifyIcon.ContextMenuStrip.ItemClicked += (object sender, ToolStripItemClickedEventArgs e) => {
                
                    if(e.ClickedItem.Tag == null)
                        return;

                    string tag = e.ClickedItem.Tag.ToString();

                    Tools.Logger.Log("Executing custom script for " + parameterName + " handler with tag " + tag, Logger.LogLevel.debug);

                    var key_id = new Jint.Key("clicked_id");
                    JSEngine.Instance.Engine.SetValue(ref key_id, tag);

                    Tools.Logger.Log("Executing custom script for " + parameterName + " handler", Logger.LogLevel.debug);
                    JSEngine.Instance.ExecuteResult(onContextmenuClick_script, parameterName);
                };
            }
        }


        private void init_event_contextmenu() {
            
            var beforeContextmenuHandlers_stack = Tools.Arguments.GetStackedArguments("on-before-contextmenu");
            Dictionary<RequestIdentifier, string> beforeContextmenuHandlers = null;
            if (beforeContextmenuHandlers_stack != null) {
                beforeContextmenuHandlers = new Dictionary<RequestIdentifier, string>();
                foreach (var kv in beforeContextmenuHandlers_stack) {
                    beforeContextmenuHandlers.Add(
                        new RequestIdentifier(kv.Key),
                        kv.Value
                    );
                }
            }

            var onContextmenuHandlers_stack = Tools.Arguments.GetStackedArguments("on-click-contextmenu");
            Dictionary<RequestIdentifier, string> onContextmenuCLickHandlers = null;
            if (onContextmenuHandlers_stack != null) {
                onContextmenuCLickHandlers = new Dictionary<RequestIdentifier, string>();
                foreach (var kv in onContextmenuHandlers_stack) {
                    onContextmenuCLickHandlers.Add(
                        new RequestIdentifier(kv.Key),
                        kv.Value
                    );
                }
            }

            browser.MenuHandler = new ContextmenuHandler(beforeContextmenuHandlers, onContextmenuCLickHandlers);
        }

        private void init_parameter_event_download() {

            var beforeDownloadHandlers_stack = Tools.Arguments.GetStackedArguments("on-before-download");
            Dictionary<RequestIdentifier, string> beforeDownloadHandlers = null;
            if (beforeDownloadHandlers_stack != null) {
                beforeDownloadHandlers = new Dictionary<RequestIdentifier, string>();
                foreach(var kv in beforeDownloadHandlers_stack) {
                    beforeDownloadHandlers.Add(
                        new RequestIdentifier(kv.Key),
                        kv.Value
                    );
                }
            }

            var progressDownloadHandlers_stack = Tools.Arguments.GetStackedArguments("on-progress-download");
            Dictionary<RequestIdentifier, string> progressDownloadHandlers = null;
            if (progressDownloadHandlers_stack != null) {
                progressDownloadHandlers = new Dictionary<RequestIdentifier, string>();
                foreach (var kv in progressDownloadHandlers_stack) {
                    progressDownloadHandlers.Add(
                        new RequestIdentifier(kv.Key),
                        kv.Value
                    );
                }
            }

            string rejectDownloadString = Tools.Arguments.GetArgument("reject-downloads", "false").ToLower().Trim();
            bool reject_downloads = rejectDownloadString == "true";
            
            browser.DownloadHandler = new Handler.DownloadHandler(this, reject_downloads, beforeDownloadHandlers, progressDownloadHandlers);
        }

        private void init_event_onRequestResponseUtf8() {
            var requestResponseHandler = Tools.Arguments.GetStackedArguments("on-request-response-utf8");
            if (requestResponseHandler != null) {
                foreach (var rrh in requestResponseHandler) {
                    string addressPattern = rrh.Key;

                    string parameterName = "on-request-response-utf8";
                    if (!string.IsNullOrWhiteSpace(addressPattern))
                        parameterName += "<" + addressPattern + ">";
                    else
                        addressPattern = "*";

                    ((CustomResourceRequestHandlerFactory)browser.ResourceRequestHandlerFactory).AddRessourceRequestHandler(
                        new Tuple<RequestIdentifier, IResourceRequestHandler>(
                            new RequestIdentifier(addressPattern),
                            new OutputResponse_ScriptedEvent(null, rrh.Value, parameterName)
                    ));
                }
            }
        }

        private void init_event_onBeforeRequest() {
            var onBeforeResponseHandler = Tools.Arguments.GetStackedArguments("on-before-request");
            if (onBeforeResponseHandler != null) {
                foreach (var brh in onBeforeResponseHandler) {
                    string addressPattern = brh.Key;

                    string parameterName = "on-before-request";
                    if (!string.IsNullOrWhiteSpace(addressPattern))
                        parameterName += "<" + addressPattern + ">";

                    if (string.IsNullOrWhiteSpace(addressPattern))
                        addressPattern = "*";

                    ((CustomResourceRequestHandlerFactory)browser.ResourceRequestHandlerFactory).AddRessourceRequestHandler(
                        new Tuple<RequestIdentifier, IResourceRequestHandler>(
                            new RequestIdentifier(addressPattern),
                            new OutputResponse_ScriptedEvent(brh.Value, null, parameterName)
                    ));
                }
            }
        }        

        private void init_event_onResponse() {
            
            var manipulateResponseHandler_once = Arguments.GetStackedArguments("on-response");
            if (manipulateResponseHandler_once == null)
                return;
            foreach (var brh in manipulateResponseHandler_once) {
                string urlPattern = brh.Key;

                string parameterName = "on-response";
                if (!string.IsNullOrWhiteSpace(urlPattern))
                    parameterName += "<" + urlPattern + ">";

                // no pattern? Only execute once
                bool executeOnlyOnce = false;
                
                RequestIdentifier ri = null;
                if (urlPattern == null || urlPattern == "") {
                    executeOnlyOnce = true;
                    urlPattern = "*";
                    Logger.Log("Added on-response handler for pattern " + urlPattern, Logger.LogLevel.debug);
                } else {
                    Logger.Log("Added on-response handler for pattern " + urlPattern, Logger.LogLevel.debug);
                }

                ri = new RequestIdentifier(urlPattern);
                var or_m = new OutputResponse_Manipulate(brh.Value, null, parameterName, executeOnlyOnce);

                
                ((CustomResourceRequestHandlerFactory)browser.ResourceRequestHandlerFactory).AddRessourceRequestHandler(
                    new Tuple<RequestIdentifier, IResourceRequestHandler>(
                        ri,
                        or_m
                ));
            }
        }

        private void init_event_onFileChange() {
            var fileChangeHandler = Tools.Arguments.GetStackedArguments("on-file-change");
            if (fileChangeHandler != null) {

                foreach (var fch in fileChangeHandler) {

                    string parameterName = "on-file-change";
                    if (!string.IsNullOrWhiteSpace(fch.Key))
                        parameterName += "<" + fch.Key + ">";

                    string path = fch.Key;
                    bool isFile = File.Exists(path);
                    bool isDirectory = Directory.Exists(path);

                    if (!isFile && !isDirectory) {
                        Tools.Logger.Log("Given path '" + path + "' is invalid, name of the parameter was:" + parameterName, Tools.Logger.LogLevel.error);
                        continue;
                    }

                    FileSystemWatcher watcher = new FileSystemWatcher();
                    if (isFile) {
                        watcher.Path = Path.GetFullPath(Path.GetDirectoryName(path));
                        watcher.Filter = Path.GetFileName(path);
                    } else {
                        watcher.Path = path;
                        watcher.Filter = "*.*";
                    }

                    watcher.IncludeSubdirectories = true;
                    watcher.NotifyFilter = NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName;

                    // create a cache to prevent unnecessary firing
                    Dictionary<string, DateTime> changeCache = new Dictionary<string, DateTime>();
                    int cacheMilliseconds = 150;


                    string script = fch.Value;
                    watcher.Changed += (object sender, FileSystemEventArgs e) => {

                        // used to handle multiple calls for single change
                        DateTime now = DateTime.Now;
                        DateTime lastTime;
                        if (changeCache.TryGetValue(e.FullPath, out lastTime)) {
                            var pastTime = now - lastTime;
                            if (pastTime.Milliseconds <= cacheMilliseconds)
                                return;
                        }
                        changeCache[e.FullPath] = now;
                        
                        string changeType = Enum.GetName(typeof(WatcherChangeTypes), e.ChangeType).ToLower();
                        JSEngine.Instance.SetValue("type", changeType);
                        JSEngine.Instance.SetValue("file", e.FullPath);
                        JSEngine.Instance.Execute(script, parameterName);
                    };
                    watcher.EnableRaisingEvents = true;
                }
            }
        }

        private void init_event_onJsDialog() {
            string jsDialogScript_paramName = "on-js-dialog";
            string jsDialogScript = Arguments.GetArgument(jsDialogScript_paramName);

            browser.JsDialogHandler = new JsDialogHandler(
                Arguments.GetArgument("prevent-JS-dialog", "false").ToLower().Trim() == "true",
                jsDialogScript,
                jsDialogScript_paramName
            );
        }

        private void init_parameter_append_to_mainpage() {

            string attachedText = Arguments.GetArgument("append-to-mainpage");
            if(string.IsNullOrWhiteSpace(attachedText)) 
                return;

            ((CustomResourceRequestHandlerFactory)browser.ResourceRequestHandlerFactory).AddRessourceRequestHandler(
                new Tuple<RequestIdentifier, IResourceRequestHandler>(
                    new RequestIdentifier("*"),
                    new OutputResponse_Manipulate(null, attachedText, "append-to-mainpage", true)
            ));
            
        }

        private void init_parameter_cookie() {
            var cookies = Tools.Arguments.GetStackedArguments("cookies");
            if (cookies != null) {

                EventHandler setCookies = null;
                setCookies = (e, v) => {

                    if (!browser.IsBrowserInitialized)
                        return;

                    browser.IsBrowserInitializedChanged -= setCookies;

                    foreach (string cookieUrl in cookies.Keys) {                        
                        RequestIdentifier ri = new RequestIdentifier(cookieUrl);
                        string contents = cookies[cookieUrl];
                        if (!contents.Contains("\t")) {
                            try {
                                if (File.Exists(contents))
                                    foreach (var c in Tools.Common.ParseCookieFile(contents)) {
                                        if (browser.GetCookieManager().SetCookie(cookieUrl, c)) {
                                            Logger.Log("Added cookie for url " + cookieUrl + " and path " + c.Path, Logger.LogLevel.debug);
                                        } else {
                                            Logger.Log("Failed to add cookie for url " + cookieUrl + " and path " + c.Path, Logger.LogLevel.error);
                                        }
                                    }
                            } catch (Exception ex) {
                                Logger.Log("Error while trying to read from cookie file: " + contents, Logger.LogLevel.error);
                                Logger.Log("Error was: " + ex.Message, Logger.LogLevel.error);
                                Application.Exit();
                            }
                        } else {
                            try {
                                var lines = contents.Split(Environment.NewLine.ToCharArray());
                                foreach (var c in Tools.Common.ParseCookieLines(new List<string>(lines))) {
                                    if (browser.GetCookieManager().SetCookie(cookieUrl, c)) {
                                        Logger.Log("Added cookie for url " + cookieUrl + " and path " + c.Path, Logger.LogLevel.debug);
                                    } else {
                                        Logger.Log("Failed to add cookie for url " + cookieUrl + " and path " + c.Path, Logger.LogLevel.error);
                                    }
                                }
                            } catch (Exception ex) {
                                Logger.Log("Error while trying add cookeis for url: " + cookieUrl, Logger.LogLevel.error);
                                Logger.Log("Error was: " + ex.Message, Logger.LogLevel.error);
                                Application.Exit();
                            }
                        }
                    }
                };

                browser.IsBrowserInitializedChanged += setCookies;
            }
        }
        #endregion
        
    }
}
