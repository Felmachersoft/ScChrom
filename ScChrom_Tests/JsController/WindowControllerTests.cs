using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScChrom;
using ScChrom.Tools;
using ScChrom_Tests;
using System;
using System.Collections.Generic;
using System.IO;

namespace ScChrom_Tests.JsController {
    
    public static class WindowControllerTests {

        public static void RunAllTests() {
            WindowMovementTest();            
            ClickInWindowTest();           
        }

        public static void WindowMovementTest() {

            string testScript = @"
                --browser-js-allow_objects=WindowController
                --injected-javascript=
                    
                    let oldPos = await ScChrom.WindowController.getWindowPosition();
                    let oldSize = await ScChrom.WindowController.getWindowSize();
                    await ScChrom.log('oldPos:' + oldPos.x + ',' + oldPos.y);
                    await ScChrom.log('oldSize:' + oldSize.width + ',' + oldSize.height);
                        
                    await new Promise(resolve => setTimeout(resolve, 50));


                    await ScChrom.WindowController.setWindowPosition(oldPos.x + 100, oldPos.y + 150);
                    await ScChrom.WindowController.setWindowSize(oldSize.width + 200, oldSize.height + 250);
                    await new Promise(resolve => setTimeout(resolve, 50));
                    
                    let newPos = await ScChrom.WindowController.getWindowPosition();
                    let newSize = await ScChrom.WindowController.getWindowSize();
	                await ScChrom.log('newPos:' + newPos.x + ',' + newPos.y);
                    await ScChrom.log('newSize:' + newSize.width + ',' + newSize.height);
                    
                    await new Promise(resolve => setTimeout(resolve, 50));
                    
                    ScChrom.WindowController.closeMainwindow();                   

                --url=" + Program.GetScChrom_Test_html_file_url();

            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());

            Assert.AreEqual(4, lines.Count, "Unnecessary lines written");
            Assert.AreEqual("oldPos:400,400", lines[0], "Invalid x value written");
            Assert.AreEqual("oldSize:300,300", lines[1], "Invalid y value written");
            Assert.AreEqual("newPos:500,550", lines[2], "Invalid x value written");
            Assert.AreEqual("newSize:500,550", lines[3], "Invalid y value written");
        }

        public static void ClickInWindowTest() {
            
            string testScript = @"
                --browser-js-allow_objects=WindowController,InputController
                --injected-javascript=
	                document.addEventListener('click', async function(e){
                        let x = e.clientX;
                        let y = e.clientY;

                        await ScChrom.log(x);
                        await ScChrom.log(y);

                        await new Promise(resolve => setTimeout(resolve, 30));

                        ScChrom.WindowController.closeMainwindow();
                    });
                    
                    await ScChrom.InputController.clickInWindow(10, 25);                    

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
    
    }
}