using CefSharp;
using ScChrom.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom.Handler {
    public class DownloadHandler: IDownloadHandler {

        BrowserForm mainForm;
        private bool _rejectDownloads = false;
        private Dictionary<RequestIdentifier, string> _beforeDownloadScripts;
        private Dictionary<RequestIdentifier, string> _progressDownloadScripts;

        public DownloadHandler(BrowserForm form, bool rejectDownloads = false, Dictionary<RequestIdentifier, string> beforeDownloadScripts = null, Dictionary<RequestIdentifier, string> progressDownloadScripts = null) {
            mainForm = form;
            
            _rejectDownloads = rejectDownloads;
            _beforeDownloadScripts = beforeDownloadScripts == null ? new Dictionary<RequestIdentifier, string>() : beforeDownloadScripts;
            _progressDownloadScripts = progressDownloadScripts == null ? new Dictionary<RequestIdentifier, string>() : progressDownloadScripts;
        }

        private bool executeBeforeDownloadScript(string script, string addressPattern, ref bool showDialog, DownloadItem downloadItem) {
           
            Logger.Log("Executing custom script for on-before-download handler with url: " + downloadItem.OriginalUrl, Logger.LogLevel.debug);
            
            var key_suggestedFilename = new Jint.Key("suggested_filename");
            JSEngine.Instance.Engine.SetValue(ref key_suggestedFilename, downloadItem.SuggestedFileName);

            var key_pageurl = new Jint.Key("page_url");
            JSEngine.Instance.Engine.SetValue(ref key_pageurl, downloadItem.OriginalUrl);

            var key_url = new Jint.Key("url");
            JSEngine.Instance.Engine.SetValue(ref key_url, downloadItem.Url);

            var key_downloadFilename = new Jint.Key("fullpath");
            JSEngine.Instance.Engine.SetValue(ref key_downloadFilename, downloadItem.FullPath);

            var key_totalBytes = new Jint.Key("total_bytes");
            JSEngine.Instance.Engine.SetValue(ref key_totalBytes, downloadItem.TotalBytes);

            var key_mimeType = new Jint.Key("mime_type");
            JSEngine.Instance.Engine.SetValue(ref key_mimeType, downloadItem.MimeType);

            var key_show_dialog = new Jint.Key("show_dialog");
            JSEngine.Instance.Engine.SetValue(ref key_show_dialog, "true");            

            string result = JSEngine.Instance.ExecuteResult(script, "on-before-download<" + addressPattern + ">");

            if (result.ToString().Trim().ToLower() == "true") 
                return true;

            string fullpath = JSEngine.Instance.Engine.GetValue("fullpath").ToString();
            Console.WriteLine(fullpath);            
            if(fullpath.ToLower().StartsWith("%desktop%")) {
                string pathend = fullpath.Substring("%desktop%".Length);
                fullpath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + pathend;
                Console.WriteLine(fullpath);
            }
            fullpath = fullpath.Replace("/", "\\");
            downloadItem.SuggestedFileName = fullpath;

            showDialog = JSEngine.Instance.Engine.GetValue("show_dialog").ToString().Trim().ToLower() == "true";

            return false;
        }

        public void OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback) {

         
            if (callback.IsDisposed)
                return;

            if (_rejectDownloads) {
                Logger.Log("Rejected download due to set reject_download to true; URL was: " + downloadItem.OriginalUrl, Logger.LogLevel.debug);
                return;
            }
            

            bool showDialog = true;

            foreach(var kv in _beforeDownloadScripts) {
                if (kv.Key.Match(downloadItem.OriginalUrl)) {
                    if(executeBeforeDownloadScript(kv.Value, kv.Key.AddressPattern, ref showDialog, downloadItem)) {
                        Logger.Log("Rejected download due to result of the on-download handler; URL was: " + downloadItem.OriginalUrl, Logger.LogLevel.debug);
                        return;
                    };
                    break;
                }
            }
                        
            if (!callback.IsDisposed) {
                using (callback) {
                    callback.Continue(downloadItem.SuggestedFileName, showDialog: showDialog);                    
                }
            }
                                    
        }

        public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback) {
            
            if (!callback.IsDisposed && _rejectDownloads) {
                Logger.Log("Rejected download due to set reject_download to true; URL was: " + downloadItem.OriginalUrl, Logger.LogLevel.debug);
                callback.Cancel();
                return;
            }
            
            if(string.IsNullOrWhiteSpace(downloadItem.FullPath)) 
                return;            
            

            foreach (var kv in _progressDownloadScripts) {
                if (!kv.Key.Match(downloadItem.OriginalUrl))
                    continue;

                Logger.Log("Executing custom script for on-progress-download handler with url: " + downloadItem.OriginalUrl, Logger.LogLevel.debug);

                string script = kv.Value;

                var key_fullpath = new Jint.Key("fullpath");
                JSEngine.Instance.Engine.SetValue(ref key_fullpath, downloadItem.FullPath);

                var key_pageurl = new Jint.Key("pageurl");
                JSEngine.Instance.Engine.SetValue(ref key_pageurl, downloadItem.OriginalUrl);

                var key_url = new Jint.Key("url");
                JSEngine.Instance.Engine.SetValue(ref key_url, downloadItem.Url);

                var key_suggestedFilename = new Jint.Key("suggested_filename");
                JSEngine.Instance.Engine.SetValue(ref key_suggestedFilename, downloadItem.SuggestedFileName);

                var key_percentage = new Jint.Key("percentage");
                JSEngine.Instance.Engine.SetValue(ref key_percentage, downloadItem.PercentComplete);

                var key_receivedBytes = new Jint.Key("received_bytes");
                JSEngine.Instance.Engine.SetValue(ref key_receivedBytes, downloadItem.ReceivedBytes);

                var key_totalBytes = new Jint.Key("total_bytes");
                JSEngine.Instance.Engine.SetValue(ref key_totalBytes, downloadItem.TotalBytes);

                var key_currentSpeed = new Jint.Key("current_speed");
                JSEngine.Instance.Engine.SetValue(ref key_currentSpeed, downloadItem.CurrentSpeed);

                var key_isComplete = new Jint.Key("is_complete");
                JSEngine.Instance.Engine.SetValue(ref key_isComplete, downloadItem.IsComplete);


                string result = JSEngine.Instance.ExecuteResult(script, "on-progress-download<" + kv.Key.AddressPattern + ">");

                if (result.ToString().Trim().ToLower() == "true") {
                    callback.Cancel();
                    callback.Dispose();
                    Logger.Log("Canceled download due to result of the on-progress handler; URL was: " + downloadItem.OriginalUrl, Logger.LogLevel.debug);
                    return;
                }
                
                break;
            }
            
        }
    }
}
