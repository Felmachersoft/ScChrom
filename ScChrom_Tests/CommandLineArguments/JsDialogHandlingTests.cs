using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScChrom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom_Tests.CommandLineArguments {


    public static class JsDialogHandlingTests {
        public static void RunAllTests() {
            BlockDialogsTest();
            ReactOnJsDialogTest();
            //InfoExample();
        }
        
        public static void BlockDialogsTest() {
            
            string testScript = @"
                --browser-js-allow_objects=WindowController
                --prevent-JS-dialog=true
                --injected-javascript=

	                alert('If you see this message, the test case failed');
                    confirm('If you see this message, the test case failed');
                    prompt('If you see this message, the test case failed');
                    
                    ScChrom.WindowController.closeMainwindow();

                --url=" + Program.GetScChrom_Test_html_file_url();


            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());


            Assert.AreEqual(0, lines.Count, "Unnecessary lines written");
        }

        public static void ReactOnJsDialogTest() {

            string testScript = @"
                --browser-js-allow_objects=WindowController                
                --on-js-dialog= 
                    switch(dialog_type){
                        case 'alert':                            
                            break;
                        case 'confirm':
                            if(dialog_messageText == 'yes'){
                                dialog_success = true;
                            } else {
                                dialog_success = false;
                            }
                            break; 
                        case 'prompt':
                            if(dialog_messageText == 'prompttest'){
                                dialog_success = true;
                                dialog_inputtext = 'inputtext';
                            } else {
                                dialog_success = false;
                                dialog_inputtext = 'failed';
                            }
                            break;
                    }
                    // for testing always skip dialog
                    return true;
                    
                --injected-javascript=

	                alert('If you see any message, the test case failed');
                    ScChrom.log('testline');

                    let confirmstate = confirm('yes');
                    ScChrom.log(confirmstate);

                    confirmstate = confirm('no');
                    ScChrom.log(confirmstate);

                    let enteredValue = prompt('prompttest');
                    ScChrom.log(enteredValue);
                    
                    ScChrom.WindowController.closeMainwindow();

                --url=" + Program.GetScChrom_Test_html_file_url();


            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());

            
            Assert.AreEqual(4, lines.Count, "Unnecessary lines written");
            Assert.AreEqual("testline", lines[0].ToLower());
            Assert.AreEqual("true", lines[1].ToLower());
            Assert.AreEqual("false", lines[2].ToLower());
            Assert.AreEqual("inputtext", lines[3].ToLower());

        }

        /// <summary>
        /// This is the test for the example in the ScChroms help.
        /// Will show an alert thus not automated and commented out.
        /// </summary>
        public static void InfoExample() {

            string testScript = @"
                --browser-js-allow_objects=WindowController                
                --on-js-dialog=               
                    switch(dialog_type){
                        case 'alert':
                            return false;
                        case 'confirm':                            
                            dialog_success = false;                            
                            return true;
                        case 'prompt':
                            if(dialog_messageText == 'hi'){
                                dialog_success = true;
                                dialog_inputtext = 'hello';
                            } else {
                                dialog_success = false;
                                dialog_inputtext = '';
                            }
                            return true;
                    }
                    
                --injected-javascript=
                    
	                alert('This is presented to the user');
                    ScChrom.log('testline');
                    
                    let confirmstate = confirm('this will be hidden and canceled');
                    ScChrom.log(confirmstate);

                    // will accept and return 'hello'
                    let enteredValue = prompt('hi');
                    ScChrom.log(enteredValue);
    
                    // will be canceled and thus not be written
                    enteredValue = prompt('something else');
                    ScChrom.log(enteredValue);
                    
                    ScChrom.WindowController.closeMainwindow();

                --url=" + Program.GetScChrom_Test_html_file_url();


            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());


            Assert.AreEqual(3, lines.Count, "Unnecessary lines written");
            Assert.AreEqual("testline", lines[0].ToLower());
            Assert.AreEqual("false", lines[1].ToLower());
            Assert.AreEqual("hello", lines[2].ToLower());

        }
    }
}
