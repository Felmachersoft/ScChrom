using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScChrom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom_Tests.CommandLineArguments {
    public static class ResponseManipulationTests {
        public static void RunAllTests() {
                        
            AddJavaScript_once();
            UsePattern();            
            Attach_to_mainpage();
           
        }
        
        public static void AddJavaScript_once() {

            string testScript = @"
                --on-js-dialog=
                    write(dialog_messageText);
                    WindowController.closeMainwindow();
                    return true;
                --on-response=
                    return response + ""<script>alert('success')</script>"";
                --url=https://google.com";


            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());


            Assert.AreEqual(1, lines.Count, "Unnecessary lines written");
            Assert.AreEqual("success", lines[0], "Whitelist failed");
        }

        public static void UsePattern() {
            
            string testScript = @"
                --browser-js-allow_objects=WindowController
                --injected-javascript=
                    ScChrom.WindowController.closeMainwindow();                    
                --on-js-dialog=
                    write(dialog_messageText);
                    WindowController.closeMainwindow();
                    return true;
                --max-runtime=0
                --on-response<*.js>=
                    write(url);
                --url=https://twitter.com";

           
            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());


            Assert.AreNotEqual(0, lines.Count, "Not all .js files have been received");
            foreach(string line in lines) {
                Assert.IsTrue(line.EndsWith(".js"), "Invalid url listed: " + line);
            }
            
        }

        public static void Attach_to_mainpage() {
                      
            string testScript = @"
                --append-to-mainpage=
                    <style>
                        body{
                           background-color:rgb(1, 1, 1);
                        }
                    </style>
                --window-hide_url_field=false
                --browser-js-allow_objects=WindowController
                --injected-javascript=
                    // Gets actual style (defined by css) instead of inlined style
                    let color = window.getComputedStyle( document.body ,null).getPropertyValue('background-color');                    
                    ScChrom.log(color);
                    ScChrom.WindowController.closeMainwindow();                
                --url=https://www.google.com";

           
            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());


            Assert.AreEqual(1, lines.Count, "Unnecessary lines written");
            Assert.AreEqual("rgb(1, 1, 1)", lines[0], "failed to inject css");
        }
      
    }
}
