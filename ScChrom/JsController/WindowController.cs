using CefSharp;
using ScChrom.BrowserJs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScChrom.JsController {
    public class WindowController: IBrowserContextCallable, IJintContextCallable {

        public static List<JsControllerMethodInfo> MethodInfos {
            get {
                return new List<JsControllerMethodInfo>() {
                    new JsControllerMethodInfo(
                        "setWindowState",
                        "Sets the mainwindows state like minimized, maximized and normal.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "state",
                                "The new state of the window. One of: 'minimized','maximized', 'normal'",
                                JsControllerMethodInfo.DataType.text
                            )
                        }
                    ),
                    new JsControllerMethodInfo(
                        "setWindowPosition",
                        @"Sets the mainwindows position.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "x",
                                "The new horizontal position. Starting on the left with 0. Offset from the right with negative values.",
                                JsControllerMethodInfo.DataType.integer
                            ),
                            new JsControllerMethodParameter(
                                "y",
                                "The new vertical position. Starting on top with 0. Offset from the bottom with negative values.",
                                JsControllerMethodInfo.DataType.integer
                            )
                        }
                    ),
                    new JsControllerMethodInfo(
                        "getWindowPosition",
                        "Gets the mainwindows position.",
                        null,
                        new JsControllerMethodReturnValue(
                            "An objects with <b>x</b> for the horizontal and <b>y</b> for the vertical offset.",
                            JsControllerMethodInfo.DataType.dictionary
                        )
                    ),
                    new JsControllerMethodInfo(
                        "setWindowSize",
                        "Sets the size of the main window.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "width",
                                "Horizontal size of the window.",
                                JsControllerMethodInfo.DataType.integer
                            ),
                            new JsControllerMethodParameter(
                                "height",
                                "Vertical size of the window.",
                                JsControllerMethodInfo.DataType.integer
                            )
                        }
                    ),
                    new JsControllerMethodInfo(
                        "getWindowSize",
                        "Gets the size of the main window.",
                        null,
                        new JsControllerMethodReturnValue(
                            "An objects with <b>width</b> for the horizontal and <b>height</b> for the vertical offset.",
                            JsControllerMethodInfo.DataType.dictionary
                        )
                    ),
                    new JsControllerMethodInfo(
                        "closeMainwindow",
                        "Closes the mainwindow and with it ScChrom."                        
                    ),
                    new JsControllerMethodInfo(
                        "activateMainwindow",
                        "Activates the mainwindow, making it the focused one."
                    ),
                    new JsControllerMethodInfo(
                        "isMainwindowTopmost",
                        "Gets if the mainwindow is topmost, so always in front of other windows even when not focused.",
                        null,
                        new JsControllerMethodReturnValue("True if it is topmost, otherwise false.", JsControllerMethodInfo.DataType.boolean)
                    ),
                    new JsControllerMethodInfo(
                        "setMainwindowTopmost",
                        "Sets if the mainwindow is topmost, so always in front of other windows even when not focused.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "topmost",
                                "True to make the mainwindow topmost.",
                                JsControllerMethodInfo.DataType.boolean
                            )
                        }
                    )                    
                };
            }
        }        

        public void setWindowState(string state) {
            MainController.Instance.WindowInstance.BeginInvoke(new Action(() => {
                switch (state.ToLower()) {
                    case "normal":
                        MainController.Instance.WindowInstance.WindowState = System.Windows.Forms.FormWindowState.Normal;
                        break;
                    case "minimized":
                        MainController.Instance.WindowInstance.WindowState = System.Windows.Forms.FormWindowState.Minimized;
                        break;
                    case "maximized":
                        MainController.Instance.WindowInstance.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                        break;
                }
            }));
        }

        public void setWindowSize(int width, int height) {
            var mainWindow = MainController.Instance.WindowInstance;
            mainWindow.BeginInvoke(new Action(() => {                
                mainWindow.Size = new System.Drawing.Size(width, height);
            }));
        }

        public Dictionary<string, int> getWindowSize() {
            return new Dictionary<string, int>() {
                { "width", MainController.Instance.WindowInstance.Width},
                { "height", MainController.Instance.WindowInstance.Height}
            };
        }

        public void setWindowPosition(int x, int y) {
            var mainWindow = MainController.Instance.WindowInstance;
            mainWindow.BeginInvoke(new Action(() => {
                if (x < 0)
                    x = Screen.GetWorkingArea(mainWindow).Width - mainWindow.Width + x;                
                if (y < 0)
                    y = Screen.GetWorkingArea(mainWindow).Height - mainWindow.Height + y;

                mainWindow.Location = new System.Drawing.Point(x, y);
            }));
        }

        public Dictionary<string, int> getWindowPosition() {
            return new Dictionary<string, int>() {
                { "x", MainController.Instance.WindowInstance.Location.X},
                { "y", MainController.Instance.WindowInstance.Location.Y}
            };
        }

        public void closeMainwindow() {
            var windowInstance = MainController.Instance.WindowInstance;
            if (!windowInstance.IsHandleCreated)
                return;

            windowInstance.BeginInvoke(new Action(() => {
                MainController.Instance.WindowInstance.Close();
            }));
        }

        public void activateMainwindow() {
            var windowInstance = MainController.Instance.WindowInstance;
            if (!windowInstance.IsHandleCreated)
                return;

            windowInstance.BeginInvoke(new Action(() => {
                MainController.Instance.WindowInstance.Activate();
            }));
        }

        public bool isMainwindowTopmost() {
            var windowInstance = MainController.Instance.WindowInstance;
            if (!windowInstance.IsHandleCreated)
                return false;

            return windowInstance.IsTopMost;
        }

        public void setMainwindowTopmost(bool topmost) {
            var windowInstance = MainController.Instance.WindowInstance;
            if (!windowInstance.IsHandleCreated)
                return ;

            windowInstance.BeginInvoke(new Action(() => {
                MainController.Instance.WindowInstance.IsTopMost = topmost;
            }));
        }
    }
}
