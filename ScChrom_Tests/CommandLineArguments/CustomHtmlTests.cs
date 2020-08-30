using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScChrom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom_Tests.CommandLineArguments {
    public static class CustomHtmlTests {
        public static void RunAllTests() {
            UseCustomHtml();
        }

        public static void UseCustomHtml() {
           

            string testScript = @"
                --browser-js-allow_objects=WindowController
                --html=
                    <html>
                        <head></head>
                        <body>
                            Please wait, testing...
                            <script>
                                var executeTest = function() {
	                                ScChrom.log('html loaded');
                    
                                    ScChrom.WindowController.closeMainwindow();
                                };
                            </script>
                        </body>
                    </html>
                --injected-javascript=
                    executeTest();";

            
            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());


            Assert.AreEqual(1, lines.Count, "Unnecessary lines written");
            Assert.AreEqual("html loaded", lines[0], "Failed to load custom html");
        }
        
    }
}
