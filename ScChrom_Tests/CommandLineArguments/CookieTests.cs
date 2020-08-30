using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScChrom;
using ScChrom.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom_Tests.CommandLineArguments {
    public static class CookieTests {
        public static void RunAllTests() {
            SetCookie();
        }
        

        public static void SetCookie() {
            
            string testScript = @"
                --browser-js-allow_objects=WindowController
                --url=https://www.w3schools.com/js/js_cookies.asp
                --injected-javascript=
                    setTimeout(function() {
                        ScChrom.log(document.cookie);
                        setTimeout(ScChrom.WindowController.closeMainwindow, 200);
                    }, 100);
                --cookies<https://www.w3schools.com>=
# This is a comment and will be ignored	
www.w3schools.com	FALSE	/js	FALSE	0	abcdef	123456
            ";

            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(Arguments.GetScriptLines(testScript));            

            Program.ShowBrowserBlocking(args.ToArray());
           

            Assert.AreEqual(1, lines.Count, "Unnecessary lines written");
            bool containsCookie = lines[0].Contains("abcdef=123456");
            Assert.IsTrue(containsCookie, "invalid cookie set");            
        }

    }
}
