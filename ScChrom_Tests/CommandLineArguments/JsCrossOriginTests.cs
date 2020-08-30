using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScChrom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom_Tests.CommandLineArguments {

    
    public static class JsCrossOriginTests {
        public static void RunAllTests() {
            SameOriginFilesTest();
        }
       
        public static void SameOriginFilesTest() {

            string testScript = @"
                --browser-js-allow_objects=WindowController
                --injected-javascript=
	                fetch('https://google.com')
                    .then(() => {
                        ScChrom.log('ok');
                        // give time to write
                        setTimeout(ScChrom.WindowController.closeMainwindow, 500);
                    })
                    .catch(() => {
                        ScChrom.log('failed');
                        // give time to write
                        setTimeout(ScChrom.WindowController.closeMainwindow, 500);                        
                    });
                --url=" + Program.GetScChrom_Test_html_file_url();

            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());

            Assert.AreEqual(1, lines.Count, "Unnecessary lines written");
            Assert.AreEqual("failed", lines[0], "Same-Origin policy was not respected");


            // check with ignored same-origin policy
            lines.Clear();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };
            args.Add("--ignore-crossorigin-from-files=true");
            Program.ShowBrowserBlocking(args.ToArray());
            Assert.AreEqual(1, lines.Count, "Unnecessary lines written");
            Assert.AreEqual("ok", lines[0], "Same-Origin policy was not ignored as intended");

        }
        
    }
}
