using CefSharp;
using CefSharp.Handler;
using ScChrom.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom.Handler {
    
    public class CustomRequestHandler : RequestHandler {

        private bool _allowExternalLinks = false;
        private string[] _allowedUrls = null;        
        private bool _allowRedirects = false;
        private string _initialUrl = null;
        private string _onBeforeBrowserHandler = null;

        public CustomRequestHandler(string[] allowedUrls = null, bool allowRedirects = true, string onBeforeBrowseHandler = null, bool allowExternalLinks = false) {
            _allowedUrls = allowedUrls;
            _allowRedirects = allowRedirects;
            _allowExternalLinks = allowExternalLinks;

            if (allowedUrls != null) {
                string urls = "";
                foreach (string url in allowedUrls)
                    urls += " " + url;
                Logger.Log("Only allowed page changes to:" + urls, Logger.LogLevel.debug);                
            }

            if(allowedUrls == null && allowRedirects) {
                _allowedUrls = new string[] { "*" };
            }

            _onBeforeBrowserHandler = onBeforeBrowseHandler;
        }
        
        

        protected override bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect) {

            Logger.Log("Trying to browse to: " + request.Url, Logger.LogLevel.debug);

            
            // ignore inner frames
            if(request.ResourceType != ResourceType.MainFrame)
                return base.OnBeforeBrowse(chromiumWebBrowser, browser, frame, request, userGesture, isRedirect);

            if (!string.IsNullOrWhiteSpace(_onBeforeBrowserHandler)) {
                JSEngine.Instance.SetValue("targetUrl", request.Url);
                JSEngine.Instance.SetValue("currentUrl", browser.MainFrame.Url);
                JSEngine.Instance.SetValue("isRedirect", isRedirect.ToString());                
                string result = JSEngine.Instance.ExecuteResult(_onBeforeBrowserHandler, "on-before-browser");                
                if(result == "true"){
                    Logger.Log("Prevent browsing to (due to on-before-browser handler): " + request.Url, Logger.LogLevel.debug);
                    return true;
                };
            }

            // always handle first request normally
            if (_initialUrl == null) {
                _initialUrl = request.Url;
                return base.OnBeforeBrowse(chromiumWebBrowser, browser, frame, request, userGesture, isRedirect);
            }

            
            // provide a way to open websites in external browser            
            if (_allowExternalLinks) {
                if(request.Url.StartsWith("external:")) {
                    string externalUrl = request.Url.Substring("external:".Length);
                    Logger.Log("Opening url in system browser: " + request.Url, Logger.LogLevel.debug);
                    Common.OpenLinkInDefaultBrowser(externalUrl);
                    return true;
                }
            }

            if (!_allowRedirects) {
                Tools.Logger.Log("Prevent browsing to (due disallowed redirects): " + request.Url, Logger.LogLevel.debug);
                return true;
            }

            if (_allowedUrls == null || _allowedUrls.Length <= 0)
                return base.OnBeforeBrowse(chromiumWebBrowser, browser, frame, request, userGesture, isRedirect);


            Uri uri = new Uri(request.Url.ToLower());            
            foreach (string curPattern in _allowedUrls) {
                if (Common.MatchText(request.Url.ToLower(), curPattern)) {
                    Logger.Log("Allowed browsing to: " + request.Url, Logger.LogLevel.debug);
                    return base.OnBeforeBrowse(chromiumWebBrowser, browser, frame, request, userGesture, isRedirect);
                }
            }

            Tools.Logger.Log("Prevent browsing to (due to missing in allowed urls): " + request.Url, Logger.LogLevel.debug);
            return true;
            
        }
        
    }
    
}
