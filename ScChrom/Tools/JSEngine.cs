using CefSharp;
using ScChrom.JsController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom.Tools {


    /// <summary>
    /// Provides all additional functions to be used in jint (not the browser context!)
    /// </summary>
    public class JSEngine {
        
        private static JSEngine _instance = null;

        public static JSEngine Instance { 
            get {
                if (_instance == null)
                    _instance = new JSEngine();
                return _instance;
            } 
        }

        public Jint.Engine Engine { get; private set; }

        private JSEngine() {
            this.Engine = new Jint.Engine();

            var versionKey = new Jint.Key("getVersion");
            this.Engine.SetValue(ref versionKey, new Func<object>(() => {
                return MainController.Version;
            }));

            var logKey = new Jint.Key("log");
            this.Engine.SetValue(ref logKey, new Action<object>((obj) => {
                if (obj == null)
                    return;
                Logger.Log(obj.ToString(), Logger.LogLevel.info);
            }));

            var errorKey = new Jint.Key("error");
            this.Engine.SetValue(ref errorKey, new Action<object>((obj) => {
                if (obj == null)
                    return;                
                Logger.Log(obj.ToString(), Logger.LogLevel.error);
            }));

            var writeKey = new Jint.Key("write");
            this.Engine.SetValue(ref writeKey, new Action<object>((obj) => {
                if (obj == null)
                    return;
                MainController.Instance.WriteOut(obj.ToString());
            }));

            var writeLineKey = new Jint.Key("writeLine");
            this.Engine.SetValue(ref writeLineKey, new Action<object>((obj) => {
                if (obj == null)
                    return;
                MainController.Instance.WriteOut(obj.ToString() + "\n");
            }));

            var sleepKey = new Jint.Key("sleep");
            this.Engine.SetValue(ref sleepKey, new Action<object>((obj) => {
                if (obj == null)
                    return;
                int ms = -1;
                if (!int.TryParse(obj.ToString(), out ms) || ms < 0)
                    return;
                System.Threading.Thread.Sleep(ms);
            }));

            
            var allCallableTypes = Common.GetAllTypes(typeof(IJintContextCallable));
            foreach(var typeKV in allCallableTypes) {
                var tempController = new Jint.Key(typeKV.Key);
                this.Engine.SetValue(ref tempController, typeKV.Value.GetConstructors().First().Invoke(new object[] { }) );
            }
            

            // execute init scripts, if any
            var initialScripts = Tools.Arguments.GetStackedArguments("initial-script");
            if (initialScripts != null) {
                foreach (var script in initialScripts) {
                    string parameterName = "initial-script";
                    if (!string.IsNullOrWhiteSpace(script.Key)) 
                        parameterName += "<" + script.Key + ">";                    
                    Execute(script.Value, parameterName);
                }
            }
        }

        public void SetValue(string key, string value) { 
            Jint.Key k = new Jint.Key(key);
            Engine.SetValue(ref k, value);            
        }

        public string GetValue(string propertyName) {            
            var val = Engine.GetValue(propertyName);
            if (val == null)
                return null;
            return val.ToString();
        }


        public void Execute(string script, string parameterName = null) {
            try {
                Engine.Execute(script);
            } catch (Exception ex) {
                string errorString = "Error while executing script";
                if(!string.IsNullOrWhiteSpace(parameterName))
                    errorString += " for parameter --" + parameterName + " ";
                errorString += ": ";
                errorString +=  ex.Message;

                Logger.Log(errorString, Logger.LogLevel.error);
            }
        }

        public string ExecuteResult(string script, string exceptionInfo = null) {

            string functionScript = "function executeFunc() { " + script + " \n}";
            
            try {
                var eng = Engine.Execute(functionScript);
                return eng.Invoke("executeFunc").ToString();
            } catch (Exception ex) {
                string errorString = "Error while executing script ";
                if (!string.IsNullOrWhiteSpace(exceptionInfo))
                    errorString += exceptionInfo;
                errorString += ": ";
                errorString += ex.Message;

                Logger.Log(errorString, Logger.LogLevel.error);

                return null;
            }
        }
    }
}
