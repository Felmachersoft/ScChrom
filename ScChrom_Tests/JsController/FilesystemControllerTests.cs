using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScChrom;
using ScChrom.Tools;
using ScChrom_Tests;
using System;
using System.Collections.Generic;
using System.IO;

namespace ScChrom_Tests.JsController {
    
    public static class FilesystemControllerTests {

        public static void RunAllTests() {
            ComplexFilehandling();            
            GetDrives();            
            GetDirectoryContent();
        }        

        public static void GetDirectoryContent() {

            string testScript = @"
                --browser-js-allow_objects=FilesystemController,WindowController               
                --injected-javascript=                    
                    let json = await ScChrom.FilesystemController.getDirectoryContent('.');
                    let content = JSON.parse(json);
                    for(let f of content){
                        let line = f.isFile == 'false' ? 'DIR' : 'FIL';
                        line += '|' + f.path;
                        if(f.size)
                            line += '|' + f.size;
                        await ScChrom.log(line);
                    }
                    setTimeout(ScChrom.WindowController.closeMainwindow, 50);
                --url=" + Program.GetScChrom_Test_html_file_url();
                      
            List<string> lines = new List<string>();            
            MainController.WrittenOut += (string content) => {                
                lines.Add(content);
            };
            
            var args = Program.GetDefaultConfig(false);
            args.AddRange(Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());



            // check if necessary fiels/folders are listed correctly
            List<string> necessaryFiles = new List<string>() {
                "scchrom.exe",
                "jint.dll",
                "libcef.dll"
            };

            List<string> necessaryFolders = new List<string>() {
                "swiftshader"
            };

            foreach(var line in lines) {
                var parts = line.Split('|');
                string pathname = parts[1].ToLower();

                if (parts[0].ToLower() == "dir") {
                    int indexToRemove = -1;
                    for (int i = 0; i < necessaryFolders.Count; i++) {
                        var fPath = necessaryFolders[i];
                        if(pathname.EndsWith(fPath)) {
                            indexToRemove = i;
                            break;
                        }
                    }
                    if (indexToRemove >= 0)
                        necessaryFolders.RemoveAt(indexToRemove);
                }

                if (parts[0].ToLower() == "fil") {
                    int indexToRemove = -1;
                    for (int i = 0; i < necessaryFiles.Count; i++) {
                        var fPath = necessaryFiles[i];
                        if (pathname.EndsWith(fPath)) {
                            indexToRemove = i;
                            break;
                        }
                    }
                    if (indexToRemove >= 0)
                        necessaryFiles.RemoveAt(indexToRemove);
                }
            }

            Assert.IsTrue(lines.Count > 10, "Not all lines written");
            Assert.AreEqual(0, necessaryFolders.Count, "Not all necessary folders found");
            Assert.AreEqual(0, necessaryFiles.Count, "Not all necessary files found");            
        }

        public static void GetDrives() {

            string testScript = @"
                --browser-js-allow_objects=FilesystemController,WindowController               
                --injected-javascript=
                    let json = await ScChrom.FilesystemController.getDrives();
                    let content = JSON.parse(json);
                    for(let d of content){                        
                        await ScChrom.log(d.path + ' - ' + d.label);
                    }
                    setTimeout(ScChrom.WindowController.closeMainwindow, 50);
                --url=" + Program.GetScChrom_Test_html_file_url();

            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());
            

            Assert.IsTrue(lines.Count > 0, "Not all lines written");
            Assert.AreNotEqual("false",lines[0], "Failed to get drives");
        }

        public static void ComplexFilehandling() {

            string testScript = @"
                --browser-js-allow_objects=FilesystemController,WindowController               
                --injected-javascript=
                    let writtenSuccess = await ScChrom.FilesystemController.writeToFile('./test.txt', 'thisisatest');
                    if(writtenSuccess != 'true'){
                        setTimeout(ScChrom.WindowController.closeMainwindow, 50);
                        return;
                    }
                    
                    let fileCreated = await ScChrom.FilesystemController.fileExists('./test.txt');
                    if(!fileCreated){
                        setTimeout(ScChrom.WindowController.closeMainwindow, 50);
                        return;
                    }

                    let fileContent = await ScChrom.FilesystemController.getFileContent('./test.txt');
                    await ScChrom.log(fileContent);
                    
                    let deleted = await ScChrom.FilesystemController.deleteFile('./test.txt');
                    await ScChrom.log(deleted);                    
                
                    setTimeout(ScChrom.WindowController.closeMainwindow, 50);
                --url=" + Program.GetScChrom_Test_html_file_url();

            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());


            Assert.AreEqual(2, lines.Count, "Failed to write file");
            Assert.AreEqual("thisisatest", lines[0], "Failed read written file");
            Assert.AreEqual("true", lines[1], "Failed to delete file");
        }

    }
}