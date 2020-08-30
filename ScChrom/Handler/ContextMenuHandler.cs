using CefSharp;
using Newtonsoft.Json.Linq;
using ScChrom.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace ScChrom.Handler {
    
    public class ContextmenuHandler: IContextMenuHandler {

        private Dictionary<RequestIdentifier, string> _beforeContextScripts = null;
        private Dictionary<RequestIdentifier, string> _onContextScripts = null;
        
        public ContextmenuHandler(Dictionary<RequestIdentifier, string> beforeContextScriptHandlers, Dictionary<RequestIdentifier, string> onContextScriptHandlers) {
            _beforeContextScripts = beforeContextScriptHandlers;
            _onContextScripts = onContextScriptHandlers;
        }

        /// <summary>
        /// Adds all necessary infos from the given parameters to the JsEngine.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="parameterName"></param>
        private void setParametersInJsEngine(ref IContextMenuParams parameters, string parameterName) {
            var key_url = new Jint.Key("url");
            JSEngine.Instance.Engine.SetValue(ref key_url, parameters.PageUrl);

            var key_mediaurl = new Jint.Key("mediasource_url");
            JSEngine.Instance.Engine.SetValue(ref key_mediaurl, parameters.SourceUrl);

            var key_sourceurl = new Jint.Key("link_url");
            JSEngine.Instance.Engine.SetValue(ref key_sourceurl, parameters.LinkUrl);

            var key_mediatype = new Jint.Key("mediatype");
            JSEngine.Instance.Engine.SetValue(ref key_mediatype, Enum.GetName(typeof(ContextMenuMediaType), parameters.MediaType).ToLower());

            var key_seletedText = new Jint.Key("selected_text");
            JSEngine.Instance.Engine.SetValue(ref key_seletedText, parameters.SelectionText);

            var key_positionX = new Jint.Key("position_x");
            JSEngine.Instance.Engine.SetValue(ref key_positionX, parameters.XCoord);

            var key_positionY = new Jint.Key("position_y");
            JSEngine.Instance.Engine.SetValue(ref key_positionY, parameters.YCoord);

            var selected_types = new List<string>();
            var menutypes = Enum.GetValues(typeof(ContextMenuType));

            string selectedTypesString = "[";
            foreach (var flagname in parameters.TypeFlags.GetFlaggedNames(true))
                selectedTypesString += "\"" + flagname + "\",";
            // remove tailing comma
            selectedTypesString = selectedTypesString.Substring(0, selectedTypesString.Length - 1);
            selectedTypesString += "]";
            var key_seletedType = new Jint.Key("selected_types");
            JSEngine.Instance.Engine.SetValue(ref key_seletedType, selectedTypesString);
        }

        /// <summary>
        /// Applies the JSON formatted result of an on-before-contextmenu script and fails silently (Error only visible in debug log).
        /// </summary>
        /// <param name="model"></param>
        /// <param name="result"></param>
        /// <param name="parameterName"></param>
        private void applyResultToModel(ref IMenuModel model, JObject result, string parameterName) {
            if (result.ContainsKey("clear") && result["clear"].ToObject<bool>()) {
                model.Clear();
            }

            if (!result.ContainsKey("entries") || result["entries"] == null)
                return;

            JArray entries = result["entries"] as JArray;
            if (entries == null) {
                Tools.Logger.Log("Error while getting entries of result from parameter " + parameterName, Tools.Logger.LogLevel.error);
                return;
            }

            foreach (var entry in entries) {
                int id = 0;
                string text = "missing text";
                if (entry["id"] != null) {
                    try {
                        id = entry["id"].ToObject<int>();
                    } catch (Exception) {
                        Tools.Logger.Log("Invalid 'id' for entry of result from parameter " + parameterName, Tools.Logger.LogLevel.error);
                    }
                    if(id > 1998) {
                        Tools.Logger.Log("id for entry of result is too high, only 1998 ids allowd from parameter " + parameterName, Tools.Logger.LogLevel.error);
                        id = 0;
                    }
                }
                id = ((int)CefMenuCommand.UserFirst) + id;
                if (entry["text"] != null) {
                    try {
                        text = entry["text"].ToObject<string>();
                    } catch (Exception) {
                        Tools.Logger.Log("Invalid 'text' for entry of result from parameter " + parameterName, Tools.Logger.LogLevel.error);
                    }
                }

                string type = "label";
                if (entry["type"] != null) {
                    try {
                        type = entry["type"].ToObject<string>();
                    } catch (Exception) {
                        Tools.Logger.Log("Invalid 'type' for entry of result from parameter " + parameterName, Tools.Logger.LogLevel.error);
                    }
                }

                switch (type) { 
                    case "separator":
                        model.AddSeparator();
                        break;
                    case "checkbox":
                        model.AddCheckItem((CefMenuCommand)id, text);
                        bool isChecked = false;
                        if (entry["checked"] != null) {
                            try {
                                isChecked = entry["checked"].ToObject<bool>();
                            } catch (Exception) {
                                Tools.Logger.Log("Invalid value for 'checked' for entry of result from parameter " + parameterName, Tools.Logger.LogLevel.error);
                            }
                            model.SetCheckedAt(model.Count-1, isChecked);
                        }
                        break;
                    default:
                        model.AddItem((CefMenuCommand)id, text);
                        break;
                }
            }
        }

        /// <summary>
        /// Called before a context menu is displayed. The model can be cleared to show no context menu or
        /// modified to show a custom menu.
        /// </summary>
        /// <param name="chromiumWebBrowser">the ChromiumWebBrowser control</param>
        /// <param name="browser">the browser object</param>
        /// <param name="frame">The frame the request is coming from</param>
        /// <param name="parameters">provides information about the context menu state</param>
        /// <param name="model">initially contains the default context menu</param>
        public void OnBeforeContextMenu(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model) {
            // excuted script to get all entries, clear if necessary
            if (_beforeContextScripts == null)
                return;
            
            foreach (var rs in _beforeContextScripts) {
                if (!rs.Key.Match(frame.Url))
                    continue;

                string parameterName = "on-before-contextmenu";
                if (!rs.Key.MatchesEverything) {
                    if (rs.Key.AddressPattern != null) {
                        parameterName += "<" + rs.Key.AddressPattern + ">";
                    } else {
                        parameterName += "<" + rs.Key.ExactAddress + ">";
                    }
                }

                setParametersInJsEngine(ref parameters, parameterName);

                Tools.Logger.Log("Executing custom script for " + parameterName + " handler", Logger.LogLevel.debug);
                string result_string = JSEngine.Instance.ExecuteResult(rs.Value, parameterName);
                if (string.IsNullOrWhiteSpace(result_string) || result_string == "undefined") {
                    Tools.Logger.Log("Ignored empty result from " + parameterName, Tools.Logger.LogLevel.info);
                    continue;
                }
                
                JObject result = null;
                try {
                    result = JObject.Parse(result_string);
                } catch (Exception ex) {                    
                    Tools.Logger.Log("Error while parsing json result from parameter "  + parameterName + " :" + ex.Message, Tools.Logger.LogLevel.error);
                    continue;
                }
                
                applyResultToModel(ref model, result, parameterName);                
            }
        }

        /// <summary>
        /// Called to execute a command selected from the context menu. See
        /// cef_menu_id_t for the command ids that have default implementations. All
        /// user-defined command ids should be between MENU_ID_USER_FIRST and
        /// MENU_ID_USER_LAST.
        /// </summary>
        /// <param name="chromiumWebBrowser">the ChromiumWebBrowser control</param>
        /// <param name="browser">the browser object</param>
        /// <param name="frame">The frame the request is coming from</param>
        /// <param name="parameters">will have the same values as what was passed to</param>
        /// <param name="commandId">menu command id</param>
        /// <param name="eventFlags">event flags</param>
        /// <returns>Return true if the command was handled or false for the default implementation.</returns>
        public bool OnContextMenuCommand(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags) {
            
            if (_onContextScripts == null)
                return false;

            int commandInt = (int)commandId;
            if (commandInt > ((int)CefMenuCommand.UserLast) || commandInt < ((int)CefMenuCommand.UserFirst))
                return false;

            foreach (var rs in _onContextScripts) {
                if (!rs.Key.Match(frame.Url))
                    continue;

                string parameterName = "on-click-contextmenu";
                if (!rs.Key.MatchesEverything) {
                    if (rs.Key.AddressPattern != null) {
                        parameterName += "<" + rs.Key.AddressPattern + ">";
                    } else {
                        parameterName += "<" + rs.Key.ExactAddress + ">";
                    }
                }

                setParametersInJsEngine(ref parameters, parameterName);

                commandInt -= ((int)CefMenuCommand.UserFirst);

                var key_id = new Jint.Key("clicked_id");
                JSEngine.Instance.Engine.SetValue(ref key_id, commandInt);

                Tools.Logger.Log("Executing custom script for " + parameterName + " handler", Logger.LogLevel.debug);
                JSEngine.Instance.ExecuteResult(rs.Value, parameterName);

            }

            return true;            
        }

        /// <summary>
        /// Called when the context menu is dismissed irregardless of whether the menu
        /// was empty or a command was selected.
        /// </summary>
        /// <param name="chromiumWebBrowser">the ChromiumWebBrowser control</param>
        /// <param name="browser">the browser object</param>
        /// <param name="frame">The frame the request is coming from</param>
        public void OnContextMenuDismissed(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame) {
            // nothing to do
        }

        /// <summary>
        /// Called to allow custom display of the context menu.
        /// For custom display return true and execute callback either synchronously or asynchronously with the selected command Id.
        /// For default display return false. Do not keep references to parameters or model outside of this callback. 
        /// </summary>
        /// <param name="chromiumWebBrowser">the ChromiumWebBrowser control</param>
        /// <param name="browser">the browser object</param>
        /// <param name="frame">The frame the request is coming from</param>
        /// <param name="parameters">provides information about the context menu state</param>
        /// <param name="model">contains the context menu model resulting from OnBeforeContextMenu</param>
        /// <param name="callback">the callback to execute for custom display</param>
        /// <returns>For custom display return true and execute callback either synchronously or asynchronously with the selected command ID.</returns>
        public bool RunContextMenu(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback) {
            return false;
        }
    }
}
