using ScChrom.BrowserJs;
using ScChrom.ConfigParams;
using ScChrom.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom.JsController {
    public class ArgumentsController : IBrowserContextCallable, IJintContextCallable {

        public static List<JsControllerMethodInfo> MethodInfos {
            get {
                return new List<JsControllerMethodInfo>() {
                    new JsControllerMethodInfo(
                        "getArgument",
                        "Get a command line argument. ",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "name",
                                "Name of the command line argument",
                                JsControllerMethodInfo.DataType.text
                            ),
                        },
                        new JsControllerMethodReturnValue(
                            "The command line arguments value. For stacked values this returns the first stacked value.",
                            JsControllerMethodInfo.DataType.text
                        )
                    ),
                    new JsControllerMethodInfo(
                        "getStackedArgument",
                        "Get a stacked command line argument as string by its name and stack key.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "name",
                                "Name of the command line argument",
                                JsControllerMethodInfo.DataType.text
                            ),
                            new JsControllerMethodParameter(
                                "stackKey",
                                "Stack key of the argument",
                                JsControllerMethodInfo.DataType.text
                            ),
                        },
                        new JsControllerMethodReturnValue(
                            "The command line arguments value.",
                            JsControllerMethodInfo.DataType.text
                        )
                    ),
                    new JsControllerMethodInfo(
                        "getStackedArguments",
                        "Get all values of a stacked command line argument.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "name",
                                "Name of the command line argument",
                                JsControllerMethodInfo.DataType.text
                            ),
                        },
                        new JsControllerMethodReturnValue(
                            "All stacked values. The keys are the stacked keys, the values are the associated values.",
                            JsControllerMethodInfo.DataType.dictionary
                        )
                    ),
                    new JsControllerMethodInfo(
                        "getArgumentHelpJson",
                        "Get infos about all available command line arguments.",
                        null,
                        new JsControllerMethodReturnValue(
                            "Json encoded infos about all available command line arguments.",
                            JsControllerMethodInfo.DataType.json
                        )
                    ),
                    new JsControllerMethodInfo(
                        "getJsControllerHelp",
                        "Get infos about all available JsControllers and their methods.",
                        null,
                        new JsControllerMethodReturnValue(
                            "Json encoded infos about all available JsControllers and their methods.",
                            JsControllerMethodInfo.DataType.json
                        )
                    )
                };
            }
        }


        public string getArgument(string name) {
            return Arguments.GetArgument(name);
        }

        public string getStackedArgument(string name, string stackKey) {
            var temp = getStackedArguments(name);
            string ret = null;
            temp.TryGetValue(stackKey, out ret);
            return ret;
        }

        public Dictionary<string, string> getStackedArguments(string name) {
            return Arguments.GetStackedArguments(name);
        }

        public string getArgumentHelpJson() {
            var allParams = ConfigParams.ConfigParamFactory.AllParameters;            
            return CefSharp.Web.JsonString.FromObject(allParams).Json;                        
        }

        public string getJsControllerHelp() {
            var allJsControllers = JsController.JsControllerInfo.GetAllControllerInfos();
            var jobj = Newtonsoft.Json.Linq.JObject.FromObject(allJsControllers);
            string ret = jobj.ToString();
            return ret;
        }
    }
}
