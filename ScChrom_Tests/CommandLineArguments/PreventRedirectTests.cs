using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScChrom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom_Tests.CommandLineArguments {


    public static class PreventRedirectTests {
        public static void RunAllTests() {
            PreventAllChangesTest();
            PreventCertainChangesTest();
        }

        public static void PreventAllChangesTest() {
            
            string testScript = @"
                --browser-js-allow_objects=WindowController
                --prevent-pagechange=true
                --injected-javascript=

	                // this pagechange should fail
                    document.location.href = 'google.com';
                    
                    setTimeout(
                        () => {
                            ScChrom.log('success');
                            ScChrom.WindowController.closeMainwindow();                            
                        },
                        200
                    );

                --url=" + Program.GetScChrom_Test_html_file_url();


            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());


            Assert.AreEqual(1, lines.Count, "Unnecessary lines written");
            Assert.AreEqual("success", lines[0], "Invalid pagechange executed");
        }

        public static void PreventCertainChangesTest() {

            string testScript = @"
                --browser-js-allow_objects=WindowController
                --allowed-pagechange_urls=*google.com*
                --injected-javascript=

                    let url = document.location.href;
                    
                    if(url.includes('google')){
                        ScChrom.log(url);
                        ScChrom.WindowController.closeMainwindow();                      
                        return;
                    }

                    // this pagechange should fail
                    document.location.href = 'https://youtube.com';

                    await new Promise(resolve => setTimeout(resolve, 200));

	                // this pagechange should work
                    document.location.href = 'https://google.com';                    

                --url=" + Program.GetScChrom_Test_html_file_url();


            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());

            Assert.AreEqual(1, lines.Count, "Unnecessary lines written");            
            Assert.AreEqual("https://www.google.com/", lines[0], "Failed to inject javascript after page change");
            

        }

    }
}
