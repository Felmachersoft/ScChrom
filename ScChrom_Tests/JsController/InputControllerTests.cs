using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScChrom;
using System.Collections.Generic;

namespace ScChrom_Tests.JsController {

    public static class InputControllerTests {

        public static void RunAllTests() {
            DoubleclickTest();            
            ClickInWindowTest();
            PressCharTest();
            MouseDownTest();
            KeyTest();
        }
        
        public static void ClickInWindowTest() {
            
            string testScript = @"
                --browser-js-allow_objects=WindowController,InputController
                --injected-javascript=
	                document.addEventListener('click', function(e){
                        let x = e.clientX;
                        let y = e.clientY;

                        ScChrom.log(x);
                        ScChrom.log(y);
                        
                        ScChrom.WindowController.closeMainwindow();
                    });

                    setTimeout(() => {
                        ScChrom.InputController.clickInWindow(10, 25);
                    }, 100);
                    

                --url=" + Program.GetScChrom_Test_html_file_url();

            List<string> lines = new List<string>();            
            MainController.WrittenOut += (string content) => {                
                lines.Add(content);
            };
            
            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());

            Assert.AreEqual(2, lines.Count, "Unnecessary lines written");
            Assert.AreEqual("10", lines[0], "Invalid x value written");
            Assert.AreEqual("25", lines[1], "Invalid y value written");
        }

        public static void PressCharTest() {

            string testText = "hello";

            string testScript = @"
                --browser-js-allow_objects=InputController,WindowController
                --injected-javascript=	
                    document.addEventListener('keypress', function(e) {
                        ScChrom.log(String.fromCharCode(e.keyCode));
                    });

                    await ScChrom.InputController.pressCharButtons('" + testText + @"');
                    await ScChrom.InputController.pressCharButton('!');

                    ScChrom.WindowController.closeMainwindow();

                --url=" + Program.GetScChrom_Test_html_file_url();

            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());

            Assert.AreEqual(6, lines.Count, "Unnecessary lines written");
            string result = "";
            lines.ForEach((c) => result += c);

            Assert.AreEqual(testText + "!", result, "Invalid text written via button inputs");
        }

        public static void MouseDownTest() {
            
            string testScript = @"
                --browser-js-allow_objects=InputController,WindowController
                --injected-javascript=

                    await ScChrom.InputController.mouseDown(10, 10);    

                    await new Promise(resolve => setTimeout(resolve, 30));

                    await ScChrom.InputController.mouseMove(100, 10, 'left');

                    await new Promise(resolve => setTimeout(resolve, 30));

                    await ScChrom.InputController.mouseUp(100, 10);

                    await new Promise(resolve => setTimeout(resolve, 30));

                    let markedText = window.getSelection().toString();
                    await ScChrom.log(markedText);

                    setTimeout(ScChrom.WindowController.closeMainwindow, 100);

                --url=" + Program.GetScChrom_Test_html_file_url();

            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());

            Assert.AreEqual(1, lines.Count, "Unnecessary lines written");
            Assert.AreEqual("Testing... Plea", lines[0], "Invalid text marked");
        }

        public static void DoubleclickTest() {
            string testScript = @"
                --browser-js-allow_objects=InputController,WindowController
                --window-hide_url_field=true
                --injected-javascript=

                    var elem = document.createElement('div');
                    elem.style.cssText = 'position:absolute;width:100px;height:100px;z-index:100;background:#000;left:100px;top:100px;';
                    document.body.appendChild(elem);

                    elem.addEventListener('dblclick', async function () {
                      await ScChrom.log('doubleclicked');
                      setTimeout(ScChrom.WindowController.closeMainwindow, 100);
                    });

                    await new Promise(resolve => setTimeout(resolve, 500));
                    
                    ScChrom.InputController.doubleclickInWindow(150, 150);

                --url=" + Program.GetScChrom_Test_html_file_url();

            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());

           
            Assert.AreEqual(1, lines.Count, "Unnecessary lines written");
            Assert.AreEqual("doubleclicked", lines[0], "No double click done");
        }

        public static void KeyTest() {
            string testScript = @"
                --browser-js-allow_objects=InputController,WindowController
                --window-hide_url_field=true
                --injected-javascript=
                    
                    document.body.addEventListener('keydown', event => {
                        ScChrom.log('keydown: ' + event.keyCode);
                    });

                    document.body.addEventListener('keyup', event => {
                        ScChrom.log('keyup: ' + event.keyCode);
                        setTimeout(ScChrom.WindowController.closeMainwindow, 100);
                    });

                    await new Promise(resolve => setTimeout(resolve, 100));
                    // sends only a down event
                    ScChrom.InputController.sendKeyEvent(0, 65, 0, false);

                    await new Promise(resolve => setTimeout(resolve, 100));
                    // will be prevented from reaching the browser
                    ScChrom.InputController.sendKeyEvent(0, 32, 0, false);

                    await new Promise(resolve => setTimeout(resolve, 100));
                    // sends down and up event
                    ScChrom.InputController.pressKey(55);

                --on-before-key=
                    let ki = JSON.parse(keyEvent);
                    if(ki.winKeyCode < 50)
                        return true;
                    write('keycode: ' + ki.winKeyCode);

                --on-after-key=
                    let ki = JSON.parse(keyEvent);
                    if( ki.winKeyCode == 65)
                        write('after');
                                
                --url=" + Program.GetScChrom_Test_html_file_url();            
            
            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());
          

            Assert.AreEqual(7, lines.Count, "Unnecessary lines written");
            Assert.AreEqual("keycode: 65", lines[0], "on-before-key handler failed");
            Assert.AreEqual("keydown: 65", lines[1], "Failed to handle keys");
            Assert.AreEqual("after", lines[2], "on-after-key handler failed");
            Assert.AreEqual("keycode: 55", lines[3], "on-before-key handler failed");
            Assert.AreEqual("keycode: 55", lines[4], "on-before-key handler failed");
            Assert.AreEqual("keydown: 55", lines[5], "Failed to handle keys");
            Assert.AreEqual("keyup: 55", lines[6], "Failed to handle keys");
        }

    }
}