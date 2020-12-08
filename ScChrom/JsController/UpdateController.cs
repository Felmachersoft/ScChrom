using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom.JsController {
    public class UpdateController: IJintContextCallable {

        public static List<JsControllerMethodInfo> MethodInfos {
            get {
                return new List<JsControllerMethodInfo>() {
                    new JsControllerMethodInfo(
                        "update",
                        "Updates ScChrom. Will overwrite the current ScChrom.exe with the latest version if the latest version is already used. Only usable in Jint context."
                    )
                };
            }
        }
        
        public void update() {
            Task.Run(() => Program.DownloadAndStartUpdate());
        }
        
    }
}
