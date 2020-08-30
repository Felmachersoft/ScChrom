using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScChrom;
using ScChrom.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom_Tests.CommandLineArguments {
    public static class Base64EncodingTests {
        public static void RunAllTests() {
            UseBase64EncodedParams();
        }

        public static string Base64Encode(string plainText) {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static void UseBase64EncodedParams() {
            // this test is the same as starting ScChrom like
            // ScChrom.exe --config-base64=AgICAgICAgICAgIDUwMDANCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICk7DQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH07DQogICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zY3JpcHQ+DQogICAgICAgICAgICAgICAgICAgICAgICA8L2JvZHk+DQogICAgICAgICAgICAgICAgICAgIDwvaHRtbD4NCi0taW5qZWN0ZWQtamF2YXNjcmlwdD0NCiAgICAgICAgICAgICAgICAgICAgZXhlY3V0ZVRlc3QoKTsNCg==

            string testScript = @"
                --browser-js-allow_objects=WindowController
                --html=
                    <html>
                        <head></head>
                        <body>
                            Please wait, testing...
                            <script>
                                var executeTest = async function() {
	                                await ScChrom.log('html loaded');
                                    setTimeout(ScChrom.WindowController.closeMainwindow, 100);
                                    
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
            args.AddRange(Arguments.GetScriptLines(testScript));

            StringBuilder sb = new StringBuilder();
            foreach(string arg in args){
                sb.AppendLine(arg);
            }

            string rebuildConfig = sb.ToString();            

            args.AddRange(Arguments.DecodeBase64String(Base64Encode(rebuildConfig)));

            Program.ShowBrowserBlocking(args.ToArray());

           

            Assert.AreEqual(1, lines.Count, "Unnecessary lines written");
            Assert.AreEqual("html loaded", lines[0], "base64 decoding failed");
        }

    }
}
