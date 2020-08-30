using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScChrom;
using ScChrom.Tools;
using ScChrom_Tests;
using System;
using System.Collections.Generic;
using System.IO;

namespace ScChrom_Tests.BrowserJs {

    public static class BrowserJsControllerTest {

        public static void RunAllTests() {
            BasicTest();
            MatchTest();
        }

        public static void BasicTest() {

            string testText = "testtext123";

            string testScript = @"
                --test=" + testText + @"
                --browser-js-allow_objects=WindowController,ArgumentsController
                --injected-javascript=
                    let testtext = await ScChrom.ArgumentsController.getArgument('test');
	                ScChrom.log(testtext);
                    ScChrom.WindowController.closeMainwindow();

                --url=" + Program.GetScChrom_Test_html_file_url();

            List<string> lines = new List<string>();            
            MainController.WrittenOut += (string content) => {                
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());

            Assert.AreEqual(1, lines.Count, "Unnecessary lines written");
            Assert.AreEqual(testText, lines[0], "Invalid value for parameter test");
        }

        public static void MatchTest() {
            
            string testPattern = "www.test.com/*";

            string testScript = @"
                --testPattern=" + testPattern + @"
                --browser-js-allow_objects=WindowController,ArgumentsController
                --injected-javascript=
                    let testpattern = await ScChrom.ArgumentsController.getArgument('testPattern');
                    let firstResult = await ScChrom.matchText('www.test.com/123', testpattern);                    
                    let secondResult = await ScChrom.matchText('www.abc.com/abc', testpattern);
                    
	                await ScChrom.log(firstResult);
                    await ScChrom.log(secondResult);
                    
                    await new Promise(resolve => setTimeout(resolve, 100));

                    ScChrom.WindowController.closeMainwindow();

                --url=" + Program.GetScChrom_Test_html_file_url();

            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());

            Assert.AreEqual(2, lines.Count, "Unnecessary lines written");
            Assert.AreEqual("true", lines[0].ToLower(), "Invalid value for parameter test");
            Assert.AreEqual("false", lines[1].ToLower(), "Invalid value for parameter test");
        }
    }
}