using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom.Handler {
    public class JsDialogHandler: IJsDialogHandler {

        private bool _preventJsDialogs = false;
        private string _jsHandlerScript = null;
        private string _jsHandlerParameterName = null;

        public JsDialogHandler(bool preventJsDialogs, string jsHandlerScript = null, string jsHandlerParameterName = null) {
            _preventJsDialogs = preventJsDialogs;
            _jsHandlerScript = jsHandlerScript;
            _jsHandlerParameterName = jsHandlerParameterName;
        }

        public bool OnJSDialog(IWebBrowser browserControl, IBrowser browser, string originUrl, CefJsDialogType dialogType, string messageText, string defaultPromptText, IJsDialogCallback callback, ref bool suppressMessage) {

            bool skipDialog = _preventJsDialogs;
            string inputText = "";
            bool success = true;

            if(_jsHandlerScript != null) {
                string dtype = Enum.GetName(typeof(CefJsDialogType), dialogType).ToLower();
                
                
                // set all values
                Tools.JSEngine.Instance.SetValue("dialog_type", dtype);
                Tools.JSEngine.Instance.SetValue("dialog_messageText", messageText);
                Tools.JSEngine.Instance.SetValue("dialog_url", originUrl);
                Tools.JSEngine.Instance.SetValue("dialog_success", "");
                if (dialogType == CefJsDialogType.Prompt)
                    Tools.JSEngine.Instance.SetValue("dialog_inputtext", "");

                string result = Tools.JSEngine.Instance.ExecuteResult(_jsHandlerScript, " for parameter '" + _jsHandlerParameterName + "': ");
                if(result != null) 
                    skipDialog = result.ToLower() == "true";
                

                if(dialogType == CefJsDialogType.Prompt)
                    inputText = Tools.JSEngine.Instance.GetValue("dialog_inputtext");

                if(dialogType != CefJsDialogType.Alert) {
                    string succVal = Tools.JSEngine.Instance.GetValue("dialog_success");
                    if(succVal != null)
                        success = succVal.ToLower() == "true";
                }
                
            }

            if (skipDialog) {
                if (dialogType == CefJsDialogType.Prompt)
                    callback.Continue(success, inputText);
                else
                    callback.Continue(success);
                return true;
            } else {
                return false;
            } 
        }

        public bool OnJSBeforeUnload(IWebBrowser browserControl, IBrowser browser, string message, bool isReload, IJsDialogCallback callback) {
            return true;
        }

        public void OnResetDialogState(IWebBrowser browserControl, IBrowser browser) {

        }

        public void OnDialogClosed(IWebBrowser browserControl, IBrowser browser) {

        }

        public bool OnBeforeUnloadDialog(IWebBrowser chromiumWebBrowser, IBrowser browser, string messageText, bool isReload, IJsDialogCallback callback) {
            throw new NotImplementedException();
        }
    }
}
