using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScChrom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom_Tests.CommandLineArguments {


    public static class OnBeforeBrowseTests {
        public static void RunAllTests() {
            SimpleInformTest();
            PreventBrowseTest();
        }

        public static void SimpleInformTest() {

            string url = "https://www.google.com/";

            string testScript = @"
                --on-before-browse=
                    write(targetUrl);
                    sleep(50);
                    WindowController.closeMainwindow();
                --url=" + url;

            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());


            Assert.AreEqual(1, lines.Count, "Unnecessary lines written");
            Assert.AreEqual(url, lines[0], "Invalid pagechange executed");
        }

        public static void PreventBrowseTest() {

            string testScript = @"
                --on-before-browse=
                                                            
                    if(currentUrl.includes('google')){                        
                        WindowController.closeMainwindow();
                    }
                    
                    if(!targetUrl.includes('google'))
                        return true;
                    
                --injected-javascript=
                    let currentUrl = document.location.href;
                    if(currentUrl.includes('google'))
                        await ScChrom.log('visited google');
                    if(currentUrl.includes('youtube')) 
                        await ScChrom.log('visited youtube'); // should not happen
                    

                    await new Promise(resolve => setTimeout(resolve, 50));
                    
                    // this pagechange should fail
                    document.location.href = 'https://youtube.com';
                    await new Promise(resolve => setTimeout(resolve, 100));
                    
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
            Assert.AreEqual("visited google", lines[0], "Failed to to use on-before-browse handler");
            

        }

    }
}
