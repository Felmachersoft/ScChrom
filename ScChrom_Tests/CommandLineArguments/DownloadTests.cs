using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScChrom;
using ScChrom.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom_Tests.CommandLineArguments {
    public static class DownloadTests {
        public static void RunAllTests() {
            
            Reject();
            On_before_cancel();
            On_before_cancel_halfway();
            Download_complete();
        }

        public static void Reject() {
            string testScript = @"
                --url=https://www.google.com/chrome/
                --browser-js-allow_objects=WindowController                
                --injected-javascript=
                    setTimeout(function() {
                        let downloadElem = document.getElementById('js-download-hero');
                        if(!downloadElem){
                            ScChrom.log('Download element not found');
                            setTimeout(ScChrom.WindowController.closeMainwindow, 200);
                            return;    
                        }
                        downloadElem.click();                        
                        setTimeout(ScChrom.WindowController.closeMainwindow, 500);
                    }, 100);
                --reject-downloads=true
                --on-before-download=
                    // should not be called
                    write('should not be called');
                --on-progress-download=
                    // should not be called
                    write('should not be called');
            ";

            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(Arguments.GetScriptLines(testScript));

            Program.ShowBrowserBlocking(args.ToArray());


            Assert.AreEqual(0, lines.Count, "Unnecessary lines written");            
        }

        
        public static void On_before_cancel() {

            string testScript = @"
                --url=https://www.google.com/chrome/
                --browser-js-allow_objects=WindowController
                --injected-javascript=
                    setTimeout(function() {
                        document.getElementById('js-download-hero').click();                        
                    }, 100); 
                    ScChrom.addCallback('download', (val) => {
                        ScChrom.log(val);
                        setTimeout(ScChrom.WindowController.closeMainwindow, 100);
                    });
                --on-progress-download=
                    // this should not be called
                    write('progress');
                --on-before-download=
                    // this will be called second
                    BrowserController.callInBrowserCallback('download', 'canceled');
                    return true;
            ";

            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(Arguments.GetScriptLines(testScript));            

            Program.ShowBrowserBlocking(args.ToArray());
           

            Assert.AreEqual(1, lines.Count, "Unnecessary lines written");
            Assert.AreEqual(lines[0], "canceled", "Invalid value written");            
        }

        public static void On_before_cancel_halfway() {
                        
            string testScript = @"
                --url=https://www.google.com/chrome/
                --injected-javascript=
                    setTimeout(function() {
                        document.getElementById('js-download-hero').click();
                    }, 100);
                --on-before-download=                    
                    write('before download');
                    show_dialog = false;
                    return false;
                --on-progress-download=
                    if(percentage > 10){
                        write(fullpath);
                        write('canceled');
                        WindowController.closeMainwindow();
                        return true;
                    }                
            ";
            
            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };
            
            var args = Program.GetDefaultConfig(false);
            args.AddRange(Arguments.GetScriptLines(testScript));

            Program.ShowBrowserBlocking(args.ToArray());

            
            Assert.IsTrue(lines.Count >= 3, "Not all lines have been written");
            Assert.AreEqual(lines[0], "before download", "Missing before download handler");            
            Assert.AreEqual(lines[2], "canceled", "Missed cancelation");
        }

        public static void Download_complete() {            
            
            string filepath = Path.Combine(Program.GetProjectDirectory(), "test.file").Replace("\\", "\\\\");

            string testScript = @"
                --url=https://www.google.com/chrome/
                --browser-js-allow_objects=WindowController
                --injected-javascript=
                    setTimeout(function() {
                        document.getElementById('js-download-hero').click();
                    }, 100);
                --on-progress-download=                    
                    write(fullpath);
                    if(is_complete)
                        WindowController.closeMainwindow();
                --on-before-download=                    
                    write(mime_type);
                    write(total_bytes);
                    write('before download');
                    show_dialog = false;
                    fullpath = '" + filepath + @"';
                    return false;
            ";
            
            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };
            
            var args = Program.GetDefaultConfig(false);
            args.AddRange(Arguments.GetScriptLines(testScript));

            Program.ShowBrowserBlocking(args.ToArray());
            
            Assert.IsTrue(File.Exists(filepath), "Downloaded file not created!");
            File.Delete(filepath);

            Assert.IsTrue(lines.Count > 2, "Lines missing");
            Assert.AreEqual(lines[0], "application/octet-stream", "Wrong mimetype");
            Assert.IsTrue(int.Parse(lines[1]) > 1000, "Invalid total size");
            Assert.AreEqual(lines[2], "before download");            
            Assert.AreEqual(lines[3], filepath.Replace("\\\\", "\\"), "Invalid fullpath used");
        }

    }
}
