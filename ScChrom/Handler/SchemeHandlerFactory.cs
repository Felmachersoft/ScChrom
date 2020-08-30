using CefSharp;
using ScChrom.Filter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScChrom.Handler {
    public class SchemeHandlerFactory : ISchemeHandlerFactory {
        
        private List<RequestIdentifier> _whitelistedUrls = null;
        private List<Tuple<RequestIdentifier, string>> _utf8_exchanges = null;
        private List<Tuple<RequestIdentifier, string>> _js_Rewrites = null;        

        public SchemeHandlerFactory(
                List<RequestIdentifier> whitelistedUrls = null,
                List<Tuple<RequestIdentifier, string>> utf8_exchanges = null,
                List<Tuple<RequestIdentifier, string>> js_Rewrites = null) {
       
            if (utf8_exchanges == null)
                utf8_exchanges = new List<Tuple<RequestIdentifier, string>>();
            if (js_Rewrites == null)
                js_Rewrites = new List<Tuple<RequestIdentifier, string>>();

            _whitelistedUrls = whitelistedUrls;
            _utf8_exchanges = utf8_exchanges;
            _js_Rewrites = js_Rewrites;
        }

        public IResourceHandler Create(IBrowser browser, IFrame frame, string schemeName, IRequest request) {

            //Note:
            // - Avoid doing lots of processing in this method as it will affect performance.            

            string url = request.Url;
            Tools.Logger.Log("Requested uri: " + url, Tools.Logger.LogLevel.debug);
            url = url.ToLower();

            Uri uri = null;
            Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri);

            // handle utf8 exchanges
            foreach (var utf8ex in _utf8_exchanges) {
                if (utf8ex.Item1.Match(url)) {                    
                    var rh = new ResourceHandler(stream: new MemoryStream(ResourceHandler.GetByteArray(utf8ex.Item2, Encoding.UTF8, true)), autoDisposeStream: true);
                    // necessary to enable cross scheme fetches
                    if (!rh.Headers.AllKeys.Contains("Access-Control-Allow-Origin"))
                        rh.Headers.Add("Access-Control-Allow-Origin", "*");
                    return rh;
                }
            }

            // handle js manipulations
            foreach (var js_rewrite in _js_Rewrites) {
                if (js_rewrite.Item1.Match(url)) {

                    string rewriteScript = "";
                    rewriteScript += "var url = '" + url + "';";
                    rewriteScript += "var method = '" + request.Method.ToLower() + "';\n";
                    rewriteScript += js_rewrite.Item2;

                    var jsInstance = Tools.JSEngine.Instance;
                    string newText = jsInstance.ExecuteResult(rewriteScript, " for parameter 'exchange-response-utf8_script': ");
                    var rh = new ResourceHandler(stream: new MemoryStream(ResourceHandler.GetByteArray(newText, Encoding.UTF8, true)), autoDisposeStream:true);
                    // necessary to enable cross scheme fetches
                    if(!rh.Headers.AllKeys.Contains("Access-Control-Allow-Origin"))
                        rh.Headers.Add("Access-Control-Allow-Origin", "*");                    

                    return rh;
                }
            }

            // handle internal ressources / files 
            if (uri != null && uri.Scheme.ToLower() == "scchrom" ) {

                MemoryStream content = null;
                string mimeType = "text/html";

                if (uri.Host.ToLower() == "internal") {

                    if(Tools.Arguments.GetArgument("allow-internal_resource", "true") == "false") {
                        Tools.Logger.Log("Refused request due to allow-internal_resource being false, url: " + uri.ToString(), Tools.Logger.LogLevel.info);
                        return new ResourceHandler();
                    }

                    string resourceName = uri.LocalPath.Replace("/", "");
                    var res = getRessource(resourceName);
                    if(res != null) {
                        mimeType = res.Item1;
                        content = res.Item2;
                    } else {
                        Tools.Logger.Log("Failed to load resource for uri " + uri.ToString(), Tools.Logger.LogLevel.error);
                        return new ResourceHandler();
                    }
                }
                
                if (uri.Host.ToLower() == "file") {

                    if (Tools.Arguments.GetArgument("allow-file_resource", "false") != "true") {
                        Tools.Logger.Log("Refused request due to allow-file_resource not being true, url: " + uri.ToString(), Tools.Logger.LogLevel.info);
                        return new ResourceHandler();
                    }

                    var res = getFile(uri.LocalPath);
                    if (res != null) {
                        mimeType = res.Item1;
                        content = res.Item2;
                    } else {
                        Tools.Logger.Log("Failed to load resource for uri " + uri.ToString(), Tools.Logger.LogLevel.error);
                        return new ResourceHandler();
                    }
                }

                if (content == null) 
                    return new ResourceHandler();
                
                
                var rh = new ResourceHandler(mimeType,content, true);
                // necessary to enable cross scheme fetches
                if (!rh.Headers.AllKeys.Contains("Access-Control-Allow-Origin"))
                    rh.Headers.Add("Access-Control-Allow-Origin", "*");
                return rh;
            }

            
            // check whitelist is set
            if (_whitelistedUrls != null && _whitelistedUrls.Count > 0) {                

                bool isWhitelisted = false;

                foreach (var filter in _whitelistedUrls) {                
                    if (filter.Match(url)) {
                        isWhitelisted = true;
                        break;
                    }
                }

                if(!isWhitelisted) {
                    Tools.Logger.Log("REJECTED uri: " + url);
                    return new ResourceHandler();
                }

            }
            
            // just handle the request the default way
            return null;
            
        }

        private Tuple<string, MemoryStream> getRessource(string resourceName) {

            string mimeType = "text/html";
            MemoryStream content = null;
            
            object resObj = null;
            try {
                resObj = Properties.Resources.ResourceManager.GetObject(resourceName);
            } catch (Exception ex) {
                Tools.Logger.Log("Failed to load resource with name '" + resourceName + "'. Error was: " + ex.Message, Tools.Logger.LogLevel.error);
                // provide empty response
                return null;
            }

            if (resObj == null) {
                Tools.Logger.Log("Tried to load none existent resource '" + resourceName + "'", Tools.Logger.LogLevel.error);
                return null;
            }            

            if (resObj is string) {
                content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes((string)resObj));
            } else if (resObj is Bitmap) {

                // set mime type
                Bitmap bitmap = resObj as Bitmap;
                ImageFormat imageFormat = bitmap.RawFormat;
                if (imageFormat.Equals(System.Drawing.Imaging.ImageFormat.Jpeg)) {
                    mimeType = "image/jpeg";
                } else if (imageFormat.Equals(System.Drawing.Imaging.ImageFormat.Png)) {
                    mimeType = "image/png";
                } else if (imageFormat.Equals(System.Drawing.Imaging.ImageFormat.Gif)) {
                    mimeType = "image/gif";
                } else if (imageFormat.Equals(System.Drawing.Imaging.ImageFormat.Bmp)) {
                    mimeType = "image/bmp";
                } else if (imageFormat.Equals(System.Drawing.Imaging.ImageFormat.Icon)) {
                    mimeType = "image/vnd.microsoft.icon";
                }

                // get content
                var ic = new ImageConverter();
                var byteData = ic.ConvertTo(((Bitmap)resObj), typeof(byte[])) as byte[];
                content = new MemoryStream(byteData);

            } else if (resObj is byte[]) {
                content = new MemoryStream(resObj as byte[]);
                mimeType = "application/octet-stream";
            } else {
                Tools.Logger.Log("Existent resource '" + resourceName + "' is an object of not matching class " + resObj.GetType().FullName, Tools.Logger.LogLevel.error);
                // provide empty response
                return null;
            }
            
            return new Tuple<string, MemoryStream>(mimeType, content);
        }

        private Tuple<string, MemoryStream> getFile(string path) {
            
            string basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string fullPath = Path.Combine(basePath, path.Replace("/", ""));

            if(!File.Exists(fullPath)){
                Tools.Logger.Log("Requested file not found at path " + path, Tools.Logger.LogLevel.error);
                return null;
            }

            string mimeType = System.Web.MimeMapping.GetMimeMapping(Path.GetFileName(fullPath));
            MemoryStream content = null;
            try {
                content = new MemoryStream(File.ReadAllBytes(fullPath));
	        } catch (Exception ex) {
                Tools.Logger.Log("Failed to load file '" + fullPath + "' because: " + ex.Message, Tools.Logger.LogLevel.error);
                return null;    
	        }

            return new Tuple<string, MemoryStream>(mimeType, content);
        }
    }
}
