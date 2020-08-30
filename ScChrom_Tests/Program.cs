using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScChrom;
using ScChrom.Tools;
using ScChrom.Tools.Tests;
using ScChrom_Tests.BrowserJs;
using ScChrom_Tests.CommandLineArguments;
using ScChrom_Tests.JsController;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScChrom_Tests {
    class Program {
        static void Main(string[] args) {

            Logger.Init();

            Exception occuredException = null;

            try {
                executeTests();
            } catch (Exception ex) {
                occuredException = ex;
            }
            

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            if(occuredException != null) {
                if(occuredException is AssertFailedException) {
                    Console.WriteLine("A test failed: " + occuredException);
                } else {
                    Console.WriteLine("Exception thrown while executing tests: " + occuredException);
                }                
            } else {
                Console.WriteLine("All tests passed");
            }
            
        }

        static void executeTests() {
            
            // General tools test
            CommandlineTests.RunAllTests();


            // JS Controller tests
            BrowserJsControllerTest.RunAllTests();
            ArgumentsControllerTests.RunAllTests();
            WindowControllerTests.RunAllTests();
            InputControllerTests.RunAllTests();
            MediaRecordingControllerTests.RunAllTests();
            FilesystemControllerTests.RunAllTests();


            // CommandLineArguments tests
            RequestManipulationTests.RunAllTests();
            CustomHtmlTests.RunAllTests();
            JsDialogHandlingTests.RunAllTests();
            
            OnBeforeBrowseTests.RunAllTests();
            Base64EncodingTests.RunAllTests();
            CookieTests.RunAllTests();
            ResponseManipulationTests.RunAllTests();
            DownloadTests.RunAllTests();

            // commented out because test needs interaction, but demonstrates how to create custom contextmenus
            // ContextmenuTests.RunAllTests();
            
            PreventRedirectTests.RunAllTests();
            JsCrossOriginTests.RunAllTests();

        }

        /// <summary>
        /// Provides the directory of this project.
        /// </summary>
        /// <returns></returns>
        public static string GetProjectDirectory() {
            string startupPath = Directory.GetCurrentDirectory();
            string ret = Path.Combine(startupPath, "..", "..", "..", "ScChrom_Tests");
            ret = Path.GetFullPath(ret);
            return ret;
        }

        public static string GetScChrom_Test_html_file_url() {
            return Path.Combine(GetProjectDirectory(), "ScChrom_Test.html");
        }

        public static List<string> GetDefaultConfig(bool hideWindow = true) {
            List<string> ret = new List<string>() {
                "--window-width=300",
                "--window-height=300",
                "--max-runtime=30",
                "--window-preventstealingfocus=true",
                "--window-show_notifyicon=false",
                "--window-show_in_taskbar=true",
                "--window-hide_url_field=true",
                "--window-preventstealingfocus=true"
            };

            if (hideWindow) {
                ret.Add("--window-pos-x=10000");
                ret.Add("--window-pos-y=10000");
            }

            return ret;
        }

        public static void ShowBrowserBlocking(string[] args, int timeout_ms = 30000) {
            Arguments.ParseConfigArgs(args);

            
            ManualResetEvent waitTillClose = new ManualResetEvent(false);
            Action closeHandler = () => 
            { 
                waitTillClose.Set(); 
            };

            MainController.Instance.Closed += closeHandler;

            Task.Run(() => {
                if (MainController.Instance.IsRunning) {
                    MainController.Instance.Restart();
                } else {
                    MainController.Instance.Start();
                }
            });

            waitTillClose.WaitOne(timeout_ms);
             
            MainController.Instance.Closed -= closeHandler;
        }
                
    }
}
