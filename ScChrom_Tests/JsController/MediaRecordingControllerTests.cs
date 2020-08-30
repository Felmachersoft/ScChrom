using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScChrom;
using ScChrom.Tools;
using ScChrom_Tests;
using System;
using System.Collections.Generic;
using System.IO;

namespace ScChrom_Tests.JsController {
    
    public static class MediaRecordingControllerTests {

        public static void RunAllTests() {            
            screenshotTest();
            createPDFTest();            
            createGIFTest();
        }
        
        public static void screenshotTest() {
            
            string testScript = @"
                --browser-js-allow_objects=WindowController,MediaRecordingController                
                --injected-javascript=
                    // creates a single pixel border inside the windows borders to see it in the screenshot
                    let body = document.getElementsByTagName('body')[0];
                    body.style.border = '1px solid black';
                    body.style.width = '100%';
                    body.style.height = '100vh';
                    body.style.margin = '0';
                    body.style.padding = '0';
                    body.style.display = 'block';
                    body.style.boxSizing = 'border-box';

                    // ensure window is visible
                    ScChrom.WindowController.activateMainwindow();

                    // wait till style is applied
                    setTimeout(() => {
                        ScChrom.MediaRecordingController.screenshot('test.jpg');
                        setTimeout(ScChrom.WindowController.closeMainwindow, 100);
                    }, 100);                                       

                --url=" + Program.GetScChrom_Test_html_file_url();
                      
            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());
            

            FileInfo fi = new FileInfo("test.jpg");
            Assert.IsTrue(fi.Exists, "Image not created");
            // 4484 Bytes in tests
            Assert.IsTrue(fi.Length > 4000 && fi.Length < 6000, "Wrong image saved");

            // cleanup
            fi.Delete();            
        }

        public static void createPDFTest() {

            string testScript = @"
                --browser-js-allow_objects=WindowController,MediaRecordingController
                --injected-javascript=
                    // creates a single pixel border inside the windows borders to see it in the screenshot
                    let body = document.getElementsByTagName('body')[0];
                    body.style.border = '1px solid black';
                    body.style.width = '100%';
                    body.style.height = '800px';
                    body.style.margin = '0';
                    body.style.padding = '0';
                    body.style.display = 'block';
                    body.style.boxSizing = 'border-box';

                    await ScChrom.MediaRecordingController.createPDF('test.pdf');
                    setTimeout(ScChrom.WindowController.closeMainwindow, 300);

                --url=" + Program.GetScChrom_Test_html_file_url();

            

            // create pdf while outside of desktop bounds
            var args = Program.GetDefaultConfig(true);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());

            
            FileInfo fi = new FileInfo("test.pdf");
            Assert.IsTrue(fi.Exists, "pdf not created");
            // 17210 Bytes in tests
            Assert.IsTrue(fi.Length > 16000 && fi.Length < 18000, "Wrong pdf saved");


            // cleanup
            fi.Delete();
        }

        public static void createGIFTest() {

            string testScript = @"
                --browser-js-allow_objects=WindowController,MediaRecordingController
                --injected-javascript=
                    // creates a single pixel border inside the windows borders to see it in the screenshot
                    let body = document.getElementsByTagName('body')[0];
                    body.text = 'Please wait';

                    // ensure window is visible
                    ScChrom.WindowController.activateMainwindow();

                    // start gif creation (takes two seconds)
                    ScChrom.MediaRecordingController.createGif('test.gif', 2000);

                    // animate something (for two seconds)
                    for(let i = 0; i < 60; i++){
                        body.innerHTML += (i + ' ');
                         await new Promise(resolve => setTimeout(resolve, 33));
                    }

                    // wait for save
                    setTimeout(ScChrom.WindowController.closeMainwindow, 500);

                --url=" + Program.GetScChrom_Test_html_file_url();

            // create pdf while outside of desktop bounds
            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());


            FileInfo fi = new FileInfo("test.gif");
            Assert.IsTrue(fi.Exists, "gif not created");
            // 217265 Bytes in tests
            Assert.IsTrue(fi.Length > 200000 && fi.Length < 240000, "Wrong gif saved");

            // cleanup
            fi.Delete();
        }
    }


}