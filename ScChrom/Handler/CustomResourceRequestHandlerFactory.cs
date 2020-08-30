using CefSharp;
using CefSharp.Internals;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScChrom.Handler {    

    /// <summary>
    /// Custom implementation of the ResourceRequestHandlerFactory that supports custom IResourceRequestHandler per 
    /// request. 
    /// 
    /// Per request only a single RessourceRequestHandler is executed.
    /// </summary>
    public class CustomResourceRequestHandlerFactory : IResourceRequestHandlerFactory {

        private object _locker = new object();

        
        private List<Tuple<RequestIdentifier, IResourceRequestHandler>> _ressourceRequestHandlers;

        /// <summary>
        /// List of RessourceRequestHandler together with the identifier when to use them
        /// </summary>
        public Tuple<RequestIdentifier, IResourceRequestHandler>[] RessourceRequestHandlers {
            get {
                lock (_locker)
                    return _ressourceRequestHandlers.ToArray();
            }
        }


        public bool HasHandlers {
            get { return _ressourceRequestHandlers.Count > 0; }
        }

        public CustomResourceRequestHandlerFactory(List<Tuple<RequestIdentifier, IResourceRequestHandler>> rrh = null) {
            _ressourceRequestHandlers = rrh != null ? rrh : new List<Tuple<RequestIdentifier, IResourceRequestHandler>>();
        }

        public void AddRessourceRequestHandler(Tuple<RequestIdentifier, IResourceRequestHandler> rrh) {
            lock (_locker)
                _ressourceRequestHandlers.Add(rrh);
        }

        public bool RemoveRessourceRequestHandler(RequestIdentifier ri) {
            lock (_locker) {
                int indexToRemove = -1;
                
                for(int i = 0; i < _ressourceRequestHandlers.Count; i++) {
                    var rrh = _ressourceRequestHandlers[i];
                    if (rrh.Equals(ri)) {
                        indexToRemove = i;
                        break;
                    }
                }

                if (indexToRemove >= 0) {
                    _ressourceRequestHandlers.RemoveAt(indexToRemove);
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Called before a resource is loaded. To specify a handler for the resource return a <see cref="ResourceHandler"/> object
        /// </summary>
        /// <param name="browserControl">The browser UI control</param>
        /// <param name="browser">the browser object</param>
        /// <param name="frame">the frame object</param>
        /// <param name="request">the request object - cannot be modified in this callback</param>
        /// <returns>To allow the resource to load normally return NULL otherwise return an instance of ResourceHandler with a valid stream</returns>
        IResourceRequestHandler IResourceRequestHandlerFactory.GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling) {
            
            //Console.WriteLine(request.Method + ": " + request.Url);

            string url = request.Url;
            lock (_locker) {
                foreach (var kv in _ressourceRequestHandlers) {
                    if (kv.Item1.Match(request.Url, request.Method, isDownload, isNavigation)) {
                        return kv.Item2;
                    }
                }
            }
            return null;
         
        }
                
    }
}
     