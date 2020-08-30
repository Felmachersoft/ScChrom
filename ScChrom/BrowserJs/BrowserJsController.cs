using CefSharp;
using CefSharp.WinForms;
using ScChrom.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom.BrowserJs {

    /// <summary>
    /// Provides all the bindings from .net objects to the browser javascript context
    /// </summary>
    public class BrowserJsController {
        
        private static readonly string _browserJsScript_template = @"
        var {{MainObjectName}} = 
        {
            'ready' : false,
            'waitingPostInitFuncs' : [],            
            'ensureThatReady' : function() {
                return new Promise(function(resolve, reject) {            
                    if({{MainObjectName}}.ready){
                        resolve();
                    } else {
                        // wait for init to finish
                        waitingPostInitFuncs.push(() => {
                            resolve();
                        });
                    }
                });
            }            
        };

        (async function(){
	        await CefSharp.BindObjectAsync('ScChromBaseJsController');
            {{MainObjectName}} = ScChromBaseJsController;
            {{MainObjectName}}['version'] = '" + MainController.Version + @"';
            {{MainObjectName}}['jintCallbacks'] = {};
            {{MainObjectName}}['addCallback'] = function(id, callback) {
                {{MainObjectName}}.jintCallbacks[id] = callback;
            };
            {{MainObjectName}}['removeCallback'] = function(id) {
                delete {{MainObjectName}}.jintCallbacks[id];
            };
            
            {{additionalJsCallables}};    

            (async function() {
                await ScChrom.ensureThatReady;
                {{postInitScript}};
            })();

            {{MainObjectName}}['ready'] = true;

            for(var i in {{MainObjectName}}.waitingPostInitFuncs){
                {{MainObjectName}}.waitingPostInitFuncs[i]();
            }
        })();";

        private const string _browserJsController_template = @"
            await CefSharp.BindObjectAsync('{{BindingObjectName}}');
            {{MainObjectName}}.{{BindingObjectName}} = {{BindingObjectName}};

        ";


        private Dictionary<string, Type> _availableTypes;
        private Dictionary<string, Type> _usedTypes;

        public string CurrentUrl { get; private set; }

        public bool AreBrowserJSControllerAllowed {
            get {
                return _usedTypes != null;
            }
        }

        public BrowserJsController(string allowedTypes = "false") {

            // get all available classes
            _availableTypes = Common.GetAllTypes(typeof(IBrowserContextCallable)); 
            
            _usedTypes = new Dictionary<string, Type>();

            if (allowedTypes == null || allowedTypes.ToLower().Trim() == "false") {               
                return;
            }


            if (allowedTypes.ToLower().Trim() == "true") {
                _usedTypes = new Dictionary<string, Type>(_availableTypes);
                return;
            }

            var allowedList = allowedTypes.Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries)
                                       .Select((s) => s.Trim()).ToArray();

            foreach(var atype in allowedList) {
                Type tempType = null;
                if(_availableTypes.TryGetValue(atype, out tempType)) {
                    if(!_usedTypes.ContainsKey(atype))
                        _usedTypes.Add(atype, tempType);
                } else {
                    Logger.Log("Unknown type '" + atype + "' given for parameter --browser-js-allow_objects , ignored", Logger.LogLevel.error);
                }
            }


        }

        /// <summary>
        /// Registers all allowed types to the ScChrom object in the given browsers javascript context
        /// </summary>
        /// <param name="browserComponent"></param>
        /// <param name="mainControllerName">Name of the </param>
        /// <param name="postInitScript"></param>
        public void InitBrowser(ChromiumWebBrowser browserComponent, string mainControllerName = "ScChrom", string postInitScript = null, bool forceInit = false) {

            bool firstTimeCall = !browserComponent.JavascriptObjectRepository.IsBound("ScChromBaseJsController"); //CurrentUrl == null;

            // only execute on a new page
            string curUrl = browserComponent.GetBrowser().MainFrame.Url;
            if (!forceInit) {                
                if (CurrentUrl == curUrl) {
                    // check if stil not initiated (happens on page change to the same url)
                    MainController.Instance.WindowInstance.IsBrowserMainJsControllerReady(mainControllerName, (inited) => {
                        if (inited)
                            return;
                        InitBrowser(browserComponent, mainControllerName, postInitScript, true);
                    });
                    return;
                }
            }
            CurrentUrl = curUrl;                        
            
            if (!browserComponent.JavascriptObjectRepository.IsBound("ScChromBaseJsController"))
                browserComponent.JavascriptObjectRepository.Register("ScChromBaseJsController", new ScChromBaseJsController(), true);

            //Console.WriteLine("first time call? " + firstTimeCall);

            StringBuilder sb_additionalCallables = new StringBuilder();
            
            // register all usedTypes and prepare the necessary javascript to be used in the next step
            foreach (var usedType in _usedTypes) {
                string name = usedType.Key;
                Type type = usedType.Value;

                
                object instance = null;
                try {
                    instance = type.GetConstructors().First().Invoke(new object[] { });
                } catch (Exception ex) {
                    // let it throw to inform about the problem with the callable
                    throw new Exception("Error occured while creating an instance for " + name + ". See InnerException for details.", ex);                    
                }
                
                if(firstTimeCall)
                    browserComponent.JavascriptObjectRepository.Register(name, instance, true);
                
                string jsContent = _browserJsController_template
                    .Replace("{{BindingObjectName}}", name)
                    .Replace("{{MainObjectName}}", mainControllerName);
                sb_additionalCallables.Append(jsContent);
            }

            


            StringBuilder sb = new StringBuilder();
            sb.Append(_browserJsScript_template)
                .Replace("{{MainObjectName}}", mainControllerName)
                .Replace("{{additionalJsCallables}}", sb_additionalCallables.ToString())
                .Replace("{{postInitScript}}", postInitScript);
            

            Tools.Logger.Log("executing custom script ...");
            
            browserComponent.GetMainFrame().ExecuteJavaScriptAsync(sb.ToString());
            Tools.Logger.Log("... custom script executed");                       

        }
    }
}
