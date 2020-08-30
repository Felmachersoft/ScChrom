using CefSharp;
using CefSharp.Handler;
using ScChrom.Filter;
using ScChrom.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScChrom.CustomResourceRequestHandlers {
    public class OutputResponse_ScriptedEvent: ResourceRequestHandler {

        private MemoryStream memoryStream;

        public string RequestResponseUTF8_script { get; set; }
        public string BeforeRequest_script{ get; set; }
        public string ParamaterName { get; private set; }

        public OutputResponse_ScriptedEvent(string beforeRequest_script, string requestResponseUTF8_script, string prameterName) : base(){
            this.RequestResponseUTF8_script = requestResponseUTF8_script;
            this.BeforeRequest_script = beforeRequest_script;
            this.ParamaterName = prameterName;
        }
        
        protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback) {
            if (string.IsNullOrWhiteSpace(BeforeRequest_script))
                return base.OnBeforeResourceLoad(chromiumWebBrowser, browser, frame, request, callback);

            if (!string.IsNullOrWhiteSpace(ParamaterName)) {
                Logger.Log("Executing on-before-request handler with parametername " + ParamaterName + " for url: " + request.Url, Logger.LogLevel.debug);
            } else {
                Logger.Log("Executing on-before-request handler for url: " + request.Url, Logger.LogLevel.debug);
            }
                

            var key_url = new Jint.Key("url");
            JSEngine.Instance.Engine.SetValue(ref key_url, request.Url);

            var key_currenturl = new Jint.Key("current_url");
            JSEngine.Instance.Engine.SetValue(ref key_currenturl, request.ReferrerUrl);            

            string result = JSEngine.Instance.ExecuteResult(BeforeRequest_script, ParamaterName);
            if(!string.IsNullOrWhiteSpace(result)) {
                if(result.ToLower() == "false") {
                    Logger.Log("Canceled request to: " + request.Url, Logger.LogLevel.debug);
                    return CefReturnValue.Cancel;
                }
                if(result != "undefined") {
                    Logger.Log("Changed request url from " + request.Url + " to " + result, Logger.LogLevel.debug);
                    request.Url = result;
                }
            }
            

            return base.OnBeforeResourceLoad(chromiumWebBrowser, browser, frame, request, callback);
        }

        protected override IResponseFilter GetResourceResponseFilter(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response) {
            memoryStream = new MemoryStream();
            return new StreamResponseFilter(memoryStream);
        }
        
        protected override void OnResourceLoadComplete(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength) {
            if (memoryStream == null) {
                base.OnResourceLoadComplete(chromiumWebBrowser, browser, frame, request, response, status, receivedContentLength);
                return;
            }


            var data = memoryStream.ToArray();
            var dataLength = data.Length;
            // For now only utf-8 enconding
            var dataAsUtf8String = Encoding.UTF8.GetString(data);

            if(!string.IsNullOrWhiteSpace(RequestResponseUTF8_script)){

                Logger.Log("Executing on-request-response-utf8 handler for " + request.Url, Logger.LogLevel.debug);

                var key = new Jint.Key("response");
                JSEngine.Instance.Engine.SetValue(ref key, dataAsUtf8String);
                var urlkey = new Jint.Key("url");
                JSEngine.Instance.Engine.SetValue(ref urlkey, request.Url);
                JSEngine.Instance.Execute(RequestResponseUTF8_script, ParamaterName);
            }
            base.OnResourceLoadComplete(chromiumWebBrowser, browser, frame, request, response, status, receivedContentLength);
        }
    }
}
