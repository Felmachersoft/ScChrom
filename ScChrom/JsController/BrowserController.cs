using ScChrom.BrowserJs;
using ScChrom.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom.JsController {
    public class BrowserController: IBrowserContextCallable, IJintContextCallable {

        public static List<JsControllerMethodInfo> MethodInfos {
            get {
                return new List<JsControllerMethodInfo>() {
                    new JsControllerMethodInfo(
                        "loadUrl",
                        "Loads the page for the given URL.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "url",
                                "The URL of the page, can also be a local path.",
                                JsControllerMethodInfo.DataType.text
                            ),
                        }
                    ),
                    new JsControllerMethodInfo(
                        "refresh",
                        "Refreshes the current page."
                    ),
                    new JsControllerMethodInfo(
                        "callInBrowserCallback",
                        "This calls the callback that was registered via 'ScChrom.addCallback(id)' in the browser context.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "id",
                                "The id used to identify the callback",
                                JsControllerMethodInfo.DataType.text
                            ),
                            new JsControllerMethodParameter(
                                "value",
                                "The value of this callbacks call",
                                JsControllerMethodInfo.DataType.text
                            )
                        }
                    )
                };
            }
        }

        public void loadUrl(string url) {
            MainController.Instance.WindowInstance.BeginInvoke(new Action(() => {
                MainController.Instance.WindowInstance.LoadUrl(url);
            }));
        }

        public void refresh() {
            MainController.Instance.WindowInstance.BeginInvoke(new Action(() => {
                MainController.Instance.WindowInstance.BrowserReload();
            }));
        }

        public void callInBrowserCallback(string id, string value) {
            MainController.Instance.WindowInstance.CallInBrowserCallback(id, value);
        }
    }
}
