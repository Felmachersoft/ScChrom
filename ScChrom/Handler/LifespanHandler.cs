using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom.Handler {
    public class LifespanHandler : ILifeSpanHandler {       

        public LifespanHandler() {
        }

        bool ILifeSpanHandler.OnBeforePopup(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser) {
            
            Tools.Logger.Log("Popup prevented for URL: " + targetUrl, Tools.Logger.LogLevel.debug);
       
            //stop open popup window
            newBrowser = null;
            return true;
        }

        bool ILifeSpanHandler.DoClose(IWebBrowser browserControl, IBrowser browser) { return false; }

        void ILifeSpanHandler.OnBeforeClose(IWebBrowser browserControl, IBrowser browser) { }

        void ILifeSpanHandler.OnAfterCreated(IWebBrowser browserControl, IBrowser browser) { }
    }
}
