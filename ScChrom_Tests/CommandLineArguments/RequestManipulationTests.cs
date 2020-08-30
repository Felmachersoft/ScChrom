using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScChrom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom_Tests.CommandLineArguments {
    public static class RequestManipulationTests {
        public static void RunAllTests() {           

            UseWhitelistTest();
            OnRequestResponseUtf8();
            ExchangeUTF8Test();
            ExchangeUTF8_scriptTest();                            
            OnBeforeRequest_scriptTest();            
            OnBeforeRequest_redirect_scriptTest();
        }

        public static void UseWhitelistTest() {            
            
            string testScript = @"
                --browser-js-allow_objects=WindowController                          
                --request-whitelist=https://www.youtube.com/
                --injected-javascript=
	                // spf should not be loaded, because only https://www.youtube.com/ is whitelisted
                    if(typeof spf === 'undefined'){
                        ScChrom.log('success');
                    } else {
                        ScChrom.log('error, spf loaded');
                    }
                    
                    ScChrom.WindowController.closeMainwindow();
                --url=https://www.youtube.com/";


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

        public static void OnRequestResponseUtf8() {
            string testScript = @"
                --url=https://google.com/
                --on-request-response-utf8<https://www.google.com/>=
                    write(url);
                    write(response.length);
                    sleep(50);
                    WindowController.closeMainwindow();";


            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());


            Assert.AreEqual(2, lines.Count, "Unnecessary lines written");
            Assert.AreEqual("https://www.google.com/", lines[0], "Failed to execute on-before-request handler");
            Assert.IsTrue(long.Parse(lines[1]) > 0, "Failed to get valid response");
        }

        public static void ExchangeUTF8Test() {
        
            string testScript = @"
                --exchange-response-utf8<https://www.google.com*>=
                    <html>
                        <head></head>
                        <body>
                            Please wait, testing...
                            <script>
                                var executeTest = async function() {
                                    await ScChrom.log('this is a test');
                                    setTimeout(ScChrom.WindowController.closeMainwindow, 50);
                                };
                            </script>
                        </body>
                    </html>
                --injected-javascript=
                    executeTest();
                --browser-js-allow_objects=WindowController
                --url=https://www.google.com";

            //hier gucken warn ich korrekt
            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());


            Assert.AreEqual(1, lines.Count, "Unnecessary lines written");
            Assert.AreEqual("this is a test", lines[0], "exchanging content failed");
        }

        public static void ExchangeUTF8_scriptTest() {
            // response.text()
            string testScript = @"
                --exchange-response-utf8<""https://www.google.com/"">=
                    <html>
                        <head></head>
                        <body>
                            Please wait, testing...
                            <script>
                                var executeTest = function() {
                                    fetch('https://www.google.com/api/1').then(async (response) => {
                                        ScChrom.log(response.status);
                                        if(response.ok) {
                                            response.text().then((text) => {
                                                ScChrom.log(text);
                                                ScChrom.WindowController.closeMainwindow();
                                            });                                            
                                        } else {
                                            ScChrom.log('the exchange script failed');
                                            ScChrom.WindowController.closeMainwindow();
                                        }                                       
                                    });                                    
                                };
                            </script>
                        </body>
                    </html>
                --exchange-response-utf8_script<https://www.google.com/api/*>=
                    if(method == 'get' && url.endsWith('1'))
                        return 'success';
                    return 'no 1 at the end';
                --injected-javascript=
                    executeTest();
                --browser-js-allow_objects=WindowController
                --url=https://www.google.com/";


            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());


            Assert.AreEqual(2, lines.Count, "Unnecessary lines written");
            Assert.AreEqual("200", lines[0], "Request manipulation failed, errorcode is the given value");
            Assert.AreEqual("success", lines[1], "exchanging script failed");
            
        }

        public static void OnBeforeRequest_scriptTest() {
            string testScript = @"
                --url=https://www.youtube.com/
                --on-before-request<https://www.youtube.com/>=
                    write(url);
                    WindowController.closeMainwindow();
                ";


            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());


            Assert.AreEqual(1, lines.Count, "Unnecessary lines written");
            Assert.AreEqual("https://www.youtube.com/", lines[0], "Failed to execute on-before-request handler");
        }

        public static void OnBeforeRequest_redirect_scriptTest() {
            string testScript = @"                
                --on-before-request<https://www.youtube.com/>=
                    return 'https://www.google.com';                    
                --on-before-request<https://www.google.com*>=
                    write('success');
                    // wait for write
                    sleep(50);
                    WindowController.closeMainwindow();   
                --url=https://www.youtube.com/";


            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());


            Assert.AreEqual(1, lines.Count, "Unnecessary lines written");
            Assert.AreEqual("success", lines[0], "Failed to execute on-before-request handler");
        }
    }
}
