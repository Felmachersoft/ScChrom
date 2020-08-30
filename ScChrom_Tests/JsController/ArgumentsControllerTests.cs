using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScChrom;
using ScChrom.Tools;
using ScChrom_Tests;
using System;
using System.Collections.Generic;
using System.IO;

namespace ScChrom_Tests.JsController {
    
    public static class ArgumentsControllerTests {

        public static void RunAllTests() {
            
            GetArgumentTest();
            GetArgumentStackedTest();            
            GetJsControllerInfoTest();
        }        

        public static void GetArgumentTest() {

            string testScript = @"
                --test=testtext123
                --injected-javascript=
	                console.log('test');
                --on-console-message<test>=
	                if(content == 'test'){
		                write(ArgumentsController.getArgument('test'));
                        WindowController.closeMainwindow();
	                }

                --url=" + Program.GetScChrom_Test_html_file_url();

            List<string> lines = new List<string>();            
            MainController.WrittenOut += (string content) => {                
                lines.Add(content);
            };
            
            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());

            Assert.AreEqual(1, lines.Count, "Unnecessary lines written");
            Assert.AreEqual("testtext123", lines[0], "Invalid value for parameter test");
        }

        public static void GetArgumentStackedTest() {

            string testScript = @"
                --test<first>=testtextfirst
                --test<second>=testtextsecond
                --injected-javascript=
                (function() {
	                console.log('test');
                })();
                --on-console-message<test>=
                (function(){
	                if(content == 'test'){
		                write(ArgumentsController.getArgument('test'));
                        write(ArgumentsController.getStackedArgument('test', 'first'));
                        write(ArgumentsController.getStackedArgument('test', 'second'));
        
                        var args = ArgumentsController.getStackedArguments('test');
                        write(args['first']);
                        write(args['second']);

                        // wait for output
                        sleep(250);
                        WindowController.closeMainwindow();
	                }
                })();
                --url=" + Program.GetScChrom_Test_html_file_url();

            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());

            Assert.AreEqual(4, lines.Count, "Invalid number of lines written");
            Assert.AreEqual("testtextfirst", lines[0], "Invalid value for stacked parameter test");
            Assert.AreEqual("testtextsecond", lines[1], "Invalid value for stacked parameter test");
            Assert.AreEqual("testtextfirst", lines[2], "Invalid value for stacked parameter test");
            Assert.AreEqual("testtextsecond", lines[3], "Invalid value for stacked parameter test");
        }

        public static void GetJsControllerInfoTest() {
            string testScript = @"
                --browser-js-allow_objects=ArgumentsController,WindowController       
                --injected-javascript=
                    let json = await ArgumentsController.getJsControllerHelp();                    
                    let content = JSON.parse(json);      
              
                    if(content['ArgumentsController'])
                        await ScChrom.log('success');
                    else
                        await ScChrom.log('Failed to get ArgumentsControllerInfo');
                    setTimeout(WindowController.closeMainwindow, 50);
                --url=" + Program.GetScChrom_Test_html_file_url();

            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };
            
            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());

            Assert.AreEqual(1, lines.Count, "Invalid number of lines written");
            Assert.AreEqual("success", lines[0], "Failed to get ArgumentControllerInfo");
        }
    }
}