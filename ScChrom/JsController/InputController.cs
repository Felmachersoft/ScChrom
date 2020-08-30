using CefSharp;
using ScChrom.BrowserJs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom.JsController {
    public class InputController: IBrowserContextCallable, IJintContextCallable {

        public static List<JsControllerMethodInfo> MethodInfos {
            get {
                return new List<JsControllerMethodInfo>() {
                    new JsControllerMethodInfo(
                        "pressCharButton",
                        "Sends a key pressed event to the browser. (Only use this to write text into focused forms.)",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "buttonChar",
                                "The single character that should be send to the browser.",
                                JsControllerMethodInfo.DataType.text
                            ),
                        }
                    ),
                    new JsControllerMethodInfo(
                        "pressCharButtons",
                        "Sends multiple key pressed events to the browser (Only use this to write text into focused forms).",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "text",
                                "The string of characters that will be send to the browser.",
                                JsControllerMethodInfo.DataType.text
                            ),
                        }
                    ),                    
                    new JsControllerMethodInfo(
                        "sendKeyEvent",
                        "Sends a specific key event (Use this to inject keyboard presses when no text is entered). You can get the necessary value by using the on-before-key or on-after-key event.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "keyEventType",
                                @"The type of event. Valid values are:
                                    <ul style=""margin:0;"">
                                        <li>0 == RawKeyDown (prefer over KeyDown)</li>
                                        <li>1 == KeyDown</li>
                                        <li>2 == KeyUp</li>
                                        <li>3 == Char (for text)</li>
                                    </ul>",
                                JsControllerMethodInfo.DataType.integer                                
                            ),
                            new JsControllerMethodParameter(
                                "winKeycode",
                                "The windowsKeyCode, see WebCore/platform/chromium/KeyboardCodes.h for the list of values.",
                                JsControllerMethodInfo.DataType.integer
                            ),
                            new JsControllerMethodParameter(
                                "modifiers",
                                @"The modifier can be one or more (via logical or) of the available <a style=""color:white;"" href=""external:https://cefsharp.github.io/api/71.0.0/html/T_CefSharp_CefEventFlags.htm"">flags</a>",
                                JsControllerMethodInfo.DataType.integer,
                                false
                            ),
                            new JsControllerMethodParameter(
                                "isSystemKey",
                                @"Indicate if the key is a system key",
                                JsControllerMethodInfo.DataType.boolean,
                                false
                            ),
                        }
                    ),
                    new JsControllerMethodInfo(
                        "pressKey", 
                        "Sends a key pressed and released event (Use this to inject keyboard presses when no text is entered). You can get the windowsKeyCode by using the on-before-key or on-after-key event.",
                        new List<JsControllerMethodParameter>() {                            
                            new JsControllerMethodParameter(
                                "winKeycode",
                                "The windowsKeyCode, see WebCore/platform/chromium/KeyboardCodes.h for the list of values.",
                                JsControllerMethodInfo.DataType.integer
                            ),
                            new JsControllerMethodParameter(
                                "modifiers",
                                @"The modifier can be one or more (via logical or) of the available <a style=""color:white"" href=""external:https://cefsharp.github.io/api/71.0.0/html/T_CefSharp_CefEventFlags.htm"">flags</a>",
                                JsControllerMethodInfo.DataType.integer,
                                false
                            ),
                        }
                    ),
                    new JsControllerMethodInfo(
                        "clickInWindow",
                        "Sends a left click event to the browser at the given position.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "x",
                                "Horizontal position starting with 0 on the left.",
                                JsControllerMethodInfo.DataType.integer
                            ),
                            new JsControllerMethodParameter(
                                "y",
                                "Vertical position starting with 0 at the top.",
                                JsControllerMethodInfo.DataType.integer
                            )
                        }
                    ),
                    new JsControllerMethodInfo(
                        "doubleclickInWindow",
                        "Sends a left double click event to the browser at the given position.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "x",
                                "Horizontal position starting with 0 on the left.",
                                JsControllerMethodInfo.DataType.integer
                            ),
                            new JsControllerMethodParameter(
                                "y",
                                "Vertical position starting with 0 at the top.",
                                JsControllerMethodInfo.DataType.integer
                            )
                        }
                    ),                    
                    new JsControllerMethodInfo(
                        "mouseDown",
                        "Sends a button down event to the browser at the given position. The mousebutton stays pressed until using the 'mouseUp' method.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "x",
                                "Horizontal position starting with 0 on the left.",
                                JsControllerMethodInfo.DataType.integer
                            ),
                            new JsControllerMethodParameter(
                                "y",
                                "Vertical position starting with 0 at the top.",
                                JsControllerMethodInfo.DataType.integer
                            ),
                            new JsControllerMethodParameter(
                                "mouseButtonName",
                                "Kind of mouse button, possible values: 'left', 'right', 'middle'. If not given 'left' is used.",
                                JsControllerMethodInfo.DataType.text
                            )
                        }
                    ),
                    new JsControllerMethodInfo(
                        "mouseUp",
                        "Sends a button up event to the browser at the given position.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "x",
                                "Horizontal position starting with 0 on the left.",
                                JsControllerMethodInfo.DataType.integer
                            ),
                            new JsControllerMethodParameter(
                                "y",
                                "Vertical position starting with 0 at the top.",
                                JsControllerMethodInfo.DataType.integer
                            ),
                            new JsControllerMethodParameter(
                                "mouseButtonName",
                                "Kind of mouse button, possible values: 'left', 'right', 'middle'. If not given 'left' is used.",
                                JsControllerMethodInfo.DataType.text
                            )
                        }
                    ),
                    new JsControllerMethodInfo(
                        "mouseMove",
                        "Sends a mouse move event to the browser from the old position to the new one.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "target_x",
                                "Horizontal position starting with 0 on the left.",
                                JsControllerMethodInfo.DataType.integer
                            ),
                            new JsControllerMethodParameter(
                                "target_y",
                                "Vertical position starting with 0 at the top.",
                                JsControllerMethodInfo.DataType.integer
                            ),
                            new JsControllerMethodParameter(
                                "modifierName",
                                "Kind of mouse button that is pressed while moving, possible values: 'none', 'left', 'right', 'middle'. If not given 'none' is used.",
                                JsControllerMethodInfo.DataType.text
                            )
                        }
                    ),
                    new JsControllerMethodInfo(
                        "mouseScroll",
                        "Sends a mouse scroll event to the browser.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "start_x",
                                "Horizontal start position starting with 0 on the left.",
                                JsControllerMethodInfo.DataType.integer
                            ),
                            new JsControllerMethodParameter(
                                "start_y",
                                "Vertical start position starting with 0 at the top.",
                                JsControllerMethodInfo.DataType.integer
                            ),
                            new JsControllerMethodParameter(
                                "delta_x",
                                "Horizontal delta of scroll.",
                                JsControllerMethodInfo.DataType.integer
                            ),
                            new JsControllerMethodParameter(
                                "delta_y",
                                "Vertical delta of scroll.",
                                JsControllerMethodInfo.DataType.integer
                            ),
                            new JsControllerMethodParameter(
                                "modifierName",
                                "Kind of mouse button that is pressed while scrolling, possible values: 'none', 'left', 'right', 'middle'. If not given 'none' is used.",
                                JsControllerMethodInfo.DataType.integer
                            )
                        }
                    )
                };
            }
        }

        public void pressCharButton(string buttonChar) {
            var host = MainController.Instance.WindowInstance.ChromeBrowserInstance.GetBrowser().GetHost();
            var keyEvent = new KeyEvent() {
                Type = KeyEventType.Char,
                WindowsKeyCode = ((int)(buttonChar[0]))
            };
            
            host.SendKeyEvent(keyEvent);
        }

        public void pressCharButtons(string text) {
            foreach(char c in text) {
                pressCharButton(c + "");
            }
        }
                    
        public void sendKeyEvent(int keyEventType, int winKeycode, int modifiers = 0, bool isSystemKey = false) {
            var host = MainController.Instance.WindowInstance.ChromeBrowserInstance.GetBrowser().GetHost();

            host.SendKeyEvent(new KeyEvent() {
                WindowsKeyCode = winKeycode,
                NativeKeyCode = 0, // NOTE: NativeKeyCode is always 0 for injected buttons
                Modifiers = (CefEventFlags)modifiers,
                IsSystemKey = isSystemKey,
                Type = (KeyEventType) keyEventType
            });
        }
        
        public void pressKey(int winKeyCode, int modifiers = 0) {
            sendKeyEvent(0, winKeyCode, modifiers);            
            sendKeyEvent(2, winKeyCode, modifiers);           
        }

        public void clickInWindow(int x, int y) {
            mouseDown(x, y);            
            mouseUp(x, y);
        }        
        
        public void doubleclickInWindow(int x, int y) {
            var host = MainController.Instance.WindowInstance.ChromeBrowserInstance.GetBrowser().GetHost();

            // first click            
            host.SendMouseClickEvent(
                x, y, MouseButtonType.Left, false, 1, CefEventFlags.LeftMouseButton
            );

            host.SendMouseClickEvent(
                x, y, MouseButtonType.Left, true, 1, CefEventFlags.None
            );

            // second click
            host.SendMouseClickEvent( 
                x, y, MouseButtonType.Left, false, 2, CefEventFlags.LeftMouseButton
            );

            host.SendMouseClickEvent(
                x, y, MouseButtonType.Left, true, 1, CefEventFlags.None
            );            
        }

        public void mouseDown(int x, int y, string mouseButtonName = "") {
            if (string.IsNullOrWhiteSpace(mouseButtonName))
                mouseButtonName = "left";

            MouseButtonType mouseButton = (MouseButtonType)Enum.Parse(typeof(MouseButtonType), mouseButtonName, true);

            Tools.Logger.Log("Mousebutton down of button " + mouseButtonName, Tools.Logger.LogLevel.debug);
            var host = MainController.Instance.WindowInstance.ChromeBrowserInstance.GetBrowser().GetHost();
            host.SendMouseClickEvent(x, y, mouseButton, false, 1, CefEventFlags.None);
        }

        public void mouseUp(int x, int y, string mouseButtonName = "") {
            if (string.IsNullOrWhiteSpace(mouseButtonName))
                mouseButtonName = "left";

            MouseButtonType mouseButton = (MouseButtonType)Enum.Parse(typeof(MouseButtonType), mouseButtonName, true);

            Tools.Logger.Log("Mousebutton up of button " + mouseButtonName, Tools.Logger.LogLevel.debug);
            var host = MainController.Instance.WindowInstance.ChromeBrowserInstance.GetBrowser().GetHost();
            host.SendMouseClickEvent(x, y, mouseButton, true, 1, CefEventFlags.None);            
        }

        public void mouseMove(int target_x, int target_y, string modifierName = "") {

            CefEventFlags modifier = CefEventFlags.None;
            switch(modifierName.ToLower()) {
                case "left":
                case "leftmousebutton":
                    modifier = CefEventFlags.LeftMouseButton;
                    break;
                case "right":
                case "rightmousebutton":
                    modifier = CefEventFlags.RightMouseButton;
                    break;
                case "middle":
                case "middlemousebutton":
                    modifier = CefEventFlags.MiddleMouseButton;
                    break;
                default:
                    modifier = CefEventFlags.None;
                    break;
            }

            var host = MainController.Instance.WindowInstance.ChromeBrowserInstance.GetBrowser().GetHost();
            host.SendMouseMoveEvent(target_x, target_y, false, modifier);
        }

        public void mouseScroll(int start_x, int start_y, int delta_x, int delta_y, string modifierName = "") {
            CefEventFlags modifier = CefEventFlags.None;
            switch (modifierName.ToLower()) {
                case "left":
                case "leftmousebutton":
                    modifier = CefEventFlags.LeftMouseButton;
                    break;
                case "right":
                case "rightmousebutton":
                    modifier = CefEventFlags.RightMouseButton;
                    break;
                case "middle":
                case "middlemousebutton":
                    modifier = CefEventFlags.MiddleMouseButton;
                    break;
                default:
                    modifier = CefEventFlags.None;
                    break;
            }

            var host = MainController.Instance.WindowInstance.ChromeBrowserInstance.GetBrowser().GetHost();
            host.SendMouseWheelEvent(start_x, start_y, delta_x, delta_y, modifier);
        }
               
    }
}
