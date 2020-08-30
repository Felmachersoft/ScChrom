using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using ScChrom.Tools;

namespace ScChrom.Handler {

    public class KeyboardEventInfo {

        public static KeyboardEventInfo FromJson(string json) {
            var obj = Newtonsoft.Json.Linq.JObject.Parse(json);

            return new KeyboardEventInfo(
                (KeyType)Enum.Parse(typeof(KeyType), obj["type"].ToObject<string>()),
                obj["winKeyCode"].ToObject<int>(),
                obj["natKeyCode"].ToObject<int>(),
                (CefEventFlags)Enum.Parse(typeof(CefEventFlags), obj["modifiers"].ToObject<string>()),
                obj["isSysKey"].ToObject<bool>()
            );
        }

        public KeyType Keytype { get; private set; }
        public int WindowsKeyCode { get; private set; }
        public int NativeKeyCode { get; private set; }
        public CefEventFlags Modifiers { get; private set; }
        public bool IsSystemKey { get; private set; }

        public KeyboardEventInfo(KeyType keytype, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey) {
            Keytype = keytype;
            WindowsKeyCode = windowsKeyCode;
            NativeKeyCode = nativeKeyCode;
            Modifiers = modifiers;
            IsSystemKey = isSystemKey;
        }

        public string ToJson() {            
            string ret = string.Format(@"{{""type"":{0},""winKeyCode"":{1},""natKeyCode"":{2},""modifiers"":{3},""isSysKey"":{4}}}", 
                (int)Keytype,
                WindowsKeyCode,
                NativeKeyCode,
                (int)Modifiers,
                IsSystemKey ? 1 : 0);

            return ret;
        }
        
    }

    public class KeyboardHandler: CefSharp.IKeyboardHandler {        

        private string _onPreButtonEvent_script = null;
        private string _onAfterButtonEvent_script = null;


        public KeyboardHandler(string onBeforeButton_script = null, string onAfterButtonEvent_script = null) {
            _onPreButtonEvent_script = onBeforeButton_script;
            _onAfterButtonEvent_script = onAfterButtonEvent_script;
        }

        public bool OnPreKeyEvent(IWebBrowser chromiumWebBrowser, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey, ref bool isKeyboardShortcut) {
            if(_onPreButtonEvent_script == null)
                return false;

            var ki = new KeyboardEventInfo(type, windowsKeyCode, nativeKeyCode, modifiers, isSystemKey);
            
            JSEngine.Instance.SetValue("keyEvent", ki.ToJson());
            string result = JSEngine.Instance.ExecuteResult(_onPreButtonEvent_script, "on-before-key");

            if (result == null)
                return false;

            return result.ToLower() == "true";
        }

        public bool OnKeyEvent(IWebBrowser chromiumWebBrowser, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey) {
            if (_onAfterButtonEvent_script == null)
                return false;

            var ki = new KeyboardEventInfo(type, windowsKeyCode, nativeKeyCode, modifiers, isSystemKey);

            JSEngine.Instance.SetValue("keyEvent", ki.ToJson());
            JSEngine.Instance.Execute(_onAfterButtonEvent_script, "on-after-key");

            return false;
        }


    }
}
