using ScChrom.BrowserJs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScChrom.JsController {
    public class NotificationController: IBrowserContextCallable, IJintContextCallable  {

        public static List<JsControllerMethodInfo> MethodInfos {
            get {
                return new List<JsControllerMethodInfo>() {
                    new JsControllerMethodInfo(
                        "showBaloonInfo",
                        @"Shows information via the windows build in notifications",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "title",
                                @"Title of the notification",
                                JsControllerMethodInfo.DataType.text
                            ),
                            new JsControllerMethodParameter(
                                "content",
                                @"Content of the notification",
                                JsControllerMethodInfo.DataType.text,
                                false
                            ),
                            new JsControllerMethodParameter(
                                "milliseconds",
                                "Duration in milliseconds the notification is visble. Defaults to 30000 (30 seconds). Set 0 to make it stay till the user clicks on it.",
                                JsControllerMethodInfo.DataType.text,
                                false
                            )
                        }
                    )
                };
            }
        }

        public void showBaloonInfo(string title, string content = "", int milliseconds = 30000) {          
            MainController.Instance.WindowInstance.BeginInvoke(new Action(() => {
                MainController.Instance.WindowInstance.ShowInfoBalloon(title, content, milliseconds);
            }));
        }
        
    }
}
