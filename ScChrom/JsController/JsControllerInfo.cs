using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ScChrom.BrowserJs;
using ScChrom.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom.JsController {

    public class JsControllerMethodReturnValue {
        
        /// <summary>
        /// HTML formatted description of the return value
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// DataType of the parameter
        /// </summary>
        /// 
        [JsonConverter(typeof(StringEnumConverter))]
        public JsControllerMethodInfo.DataType DataType { get; private set; }

        public JsControllerMethodReturnValue(string description, JsControllerMethodInfo.DataType datatype) {
            Description = description;
            DataType = datatype;
        }
    }

    public class JsControllerMethodParameter {        

        public string Parametername { get; private set; }
        
        /// <summary>
        /// HTML formatted description of the parameter
        /// </summary>
        public string Description { get; private set; }


        [JsonConverter(typeof(StringEnumConverter))]
        public JsControllerMethodInfo.DataType DataType { get; private set; }

        public bool IsNecessary { get; private set; }

        public JsControllerMethodParameter(string paramtername, string description, JsControllerMethodInfo.DataType dataType, bool isNecessary = true) {
            Parametername = paramtername;
            Description = description;
            DataType = dataType;
            IsNecessary = isNecessary;
        }
    }

    public class JsControllerMethodInfo {
        
        public enum DataType {
            text,
            array,
            dictionary,
            boolean,
            integer,
            number,
            json,
            other
        }

        /// <summary>
        /// The method name
        /// </summary>
        public string Methodname { get; private set; }

        /// <summary>
        /// HTML formatted description of the method
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// The parameters of the method
        /// </summary>
        public List<JsControllerMethodParameter> Parameters { get; private set; }

        /// <summary>
        /// The return value of the method, null if the method returns nothing (void)
        /// </summary>
        public JsControllerMethodReturnValue ReturnValue { get; private set; }

        public JsControllerMethodInfo(string methodname, string description, List<JsControllerMethodParameter> parameters = null, JsControllerMethodReturnValue returnValue = null) {
            Methodname = methodname;
            Description = description;
            if (parameters == null)
                parameters = new List<JsControllerMethodParameter>();
            Parameters = parameters;            
            ReturnValue = returnValue;
        }
    }

    public class JsControllerInfo {

        private static Dictionary<string, JsControllerInfo> _allControllerInfos = null;
        private static List<string> _defaultMethodNames = null;

        public static Dictionary<string, JsControllerInfo> GetAllControllerInfos() {
            if (_allControllerInfos != null)
                return _allControllerInfos;

            var ret = new Dictionary<string, JsControllerInfo>();

            var jintClasses = Tools.Common.GetAllTypes(typeof(IJintContextCallable));
            var browserClasses = Tools.Common.GetAllTypes(typeof(IBrowserContextCallable));

            var allTypes = jintClasses.Values.ToList();
            foreach (var type in browserClasses.Values)
                if (!allTypes.Contains(type))
                    allTypes.Add(type);
            
            
            foreach(var type in allTypes) {                
                var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                bool browserContext = type.GetInterfaces().Contains(typeof(IBrowserContextCallable));
                bool jintContext = type.GetInterfaces().Contains(typeof(IJintContextCallable));
                

                List<JsControllerMethodInfo> methodInfos = null;
                foreach (var prop in properties) {
                    if (prop.Name == "MethodInfos")
                        methodInfos = prop.GetValue(null) as List<JsControllerMethodInfo>;
                }
                
                if (methodInfos == null)                     
                    methodInfos = GetGenericJsControllerInfo(type);                

                if(ret.ContainsKey(type.Name)) {
                    Logger.Log("Found multiple JsControllers with the same class name, following JsController have been ignored: " + type.FullName, Logger.LogLevel.error);
                    continue;
                }

                var newControllerInfo = new JsControllerInfo(type.Name, browserContext, jintContext, methodInfos);
                ret.Add(type.Name, newControllerInfo);
            }


            _allControllerInfos = ret;
            return ret;
        }

        public static List<JsControllerMethodInfo> GetGenericJsControllerInfo(Type jsController) {
            Logger.Log("Creating generic JsController info for " + jsController.Name, Logger.LogLevel.debug);

            bool browserContext = jsController.GetInterfaces().Contains(typeof(IBrowserContextCallable));
            bool jintContext = jsController.GetInterfaces().Contains(typeof(IJintContextCallable));

            if (!browserContext && !jintContext) 
                throw new ArgumentException("Given type must inherit either IBrowserContextCallable and/or IJintContextCallable", "jsController");
            
            var methodInfos = new List<JsControllerMethodInfo>();
            var methods = jsController.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var method in methods) {
                // ingore default methods
                if(_defaultMethodNames == null) {
                    _defaultMethodNames = new List<string>();
                    foreach(var m in typeof(object).GetMethods()) 
                        _defaultMethodNames.Add(m.Name);                    
                }
                if (_defaultMethodNames.Contains(method.Name))
                    continue;

                var parameters = method.GetParameters();
                var newParameters = new List<JsControllerMethodParameter>();
                
                foreach (var parameter in parameters) {
                    JsControllerMethodInfo.DataType paramType = JsControllerMethodInfo.DataType.text;
                    bool isBoolean = parameter.ParameterType == typeof(bool);
                    if (isBoolean)
                        paramType = JsControllerMethodInfo.DataType.boolean;

                    bool isNumber = parameter.ParameterType == typeof(Int32) ||
                        parameter.ParameterType == typeof(Int64) ||
                        parameter.ParameterType == typeof(Int16) ||
                        parameter.ParameterType == typeof(float) ||
                        parameter.ParameterType == typeof(double);
                    if (isNumber)
                        paramType = JsControllerMethodInfo.DataType.number;

                    if(parameter.ParameterType == typeof(Array) || (parameter.ParameterType.IsGenericType && (parameter.ParameterType.GetGenericTypeDefinition() == typeof(List<>))))
                        paramType = JsControllerMethodInfo.DataType.array;                    

                    newParameters.Add(new JsControllerMethodParameter(
                        parameter.Name,
                        null,
                        paramType,
                        !parameter.IsOptional
                    ));
                }

                JsControllerMethodReturnValue returnValue = null;
                if(method.ReturnType != typeof(void)) {
                    bool isDict = method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Dictionary<,>);
                    if (isDict)
                        returnValue = new JsControllerMethodReturnValue("", JsControllerMethodInfo.DataType.dictionary);

                    bool isBoolean = method.ReturnType == typeof(bool);
                    if(isBoolean)
                        returnValue = new JsControllerMethodReturnValue("", JsControllerMethodInfo.DataType.boolean);

                    bool isNumber = method.ReturnType == typeof(Int32) ||
                        method.ReturnType == typeof(Int64) ||
                        method.ReturnType == typeof(Int16) ||
                        method.ReturnType == typeof(float) ||
                        method.ReturnType == typeof(double);
                    if (isNumber)
                        returnValue = new JsControllerMethodReturnValue("", JsControllerMethodInfo.DataType.number);

                    if (method.ReturnType == typeof(Array) || (method.ReturnType.IsGenericType && (method.ReturnType.GetGenericTypeDefinition() == typeof(List<>))))
                        returnValue = new JsControllerMethodReturnValue("", JsControllerMethodInfo.DataType.array);

                    // none of the above? => generic text
                    if (returnValue == null)
                        returnValue = new JsControllerMethodReturnValue("", JsControllerMethodInfo.DataType.text);
                    
                }

                methodInfos.Add(new JsControllerMethodInfo(method.Name, null, newParameters, returnValue));
            }

            return methodInfos;
        }

        public string Name { get; private set; }

        public bool AvailableInJint { get; private set; }

        public bool AvailableInBrowser { get; private set; }

        public List<JsControllerMethodInfo> MethodInfos { get; private set; }

        public JsControllerInfo(string name, bool availableInBrowser, bool availableInJint, List<JsControllerMethodInfo> methodInfos = null) {
            Name = name;
            MethodInfos = methodInfos;
            AvailableInJint = availableInJint;
            AvailableInBrowser = availableInBrowser;
        }
    }


}
