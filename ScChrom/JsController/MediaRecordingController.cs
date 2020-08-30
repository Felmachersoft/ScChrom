using CefSharp;
using ScChrom.BrowserJs;
using ScChrom.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom.JsController {
    public class MediaRecordingController: IBrowserContextCallable, IJintContextCallable {

        public static List<JsControllerMethodInfo> MethodInfos {
            get {
                return new List<JsControllerMethodInfo>() {
                    new JsControllerMethodInfo(
                        "screenshot",
                        @"Makes a screenshot of the content of the browser (including the scrollbars!) and saves it to the given filename.<br>
                         <b>BEWARE:</b> This will only screenshot the visible area currently on screen, in another window is in front of the browser the other windows content will be in the screenshot.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "filename",
                                @"The destination of the screenshot. Supported image formates are png, bmp, jpg, gif.<br> 
                                  Use %desktop% to get the current desktop path.<br>
                                  If no path given the image will be saved in the working directory (Most likely the directory that contains the ScChrom.exe).",
                                JsControllerMethodInfo.DataType.text
                            )
                        }
                    ),
                    new JsControllerMethodInfo(
                        "createPDF",
                        "Makes a PDF of the content of the page (not just the area visible on screen) and saves it to the given filename.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "filename",
                                @"The destination of the pdf.<br> 
                                  Use %desktop% to get the current desktop path.<br>
                                  If no path given the pdf will be saved in the working directory (Most likely the directory that contains the ScChrom.exe).",
                                JsControllerMethodInfo.DataType.text
                            )
                        }
                    ),
                    new JsControllerMethodInfo(
                        "createGif",
                        @"Creates a animated gif.<br>
                           <b>BEWARE</b>: Only whats visible on the screen will be in the recorded in the gif.
                        ",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "filename",
                                @"The destinantion of the gif.<br> 
                                  Use %desktop% get to the current desktop path.<br>
                                  If no path given the gif will be saved in the working directory (Most likely the directory that contains the ScChrom.exe).",
                                JsControllerMethodInfo.DataType.text
                            ),
                            new JsControllerMethodParameter(
                                "durationMilliseconds",
                                @"The duration of the recorded gif in milliseconds. Defaults to 5000 (5 seconds).",
                                JsControllerMethodInfo.DataType.integer,
                                false
                            ),
                            new JsControllerMethodParameter(
                                "msFrameDelay",
                                @"The milliseconds between the single captured frames. Defualts to 33 (33 ms is about 30 frames per second)",
                                JsControllerMethodInfo.DataType.integer,
                                false
                            ),
                            new JsControllerMethodParameter(
                                "loop",
                                @"True to make the gif loop.",
                                JsControllerMethodInfo.DataType.boolean,
                                false
                            ),
                        }
                    )
                };
            }
        }

        public void screenshot(string filename) {

            MainController.Instance.WindowInstance.BeginInvoke(new Action(() => {
             
                var form = MainController.Instance.WindowInstance;               

                var img = WindowCapture.CaptureWindow(form.ChromeBrowserInstance.Handle);

                ImageFormat imgFormat = ImageFormat.Png;
                if(filename.Length > 4 && filename[filename.Length - 4] == '.') {
                    string fileextension = filename.ToLower().Substring(filename.Length - 3, 3);
                    switch(fileextension) {
                        case "png":
                            imgFormat = ImageFormat.Png;
                            break;
                        case "bmp":
                            imgFormat = ImageFormat.Bmp;
                            break;
                        case "jpg":
                            imgFormat = ImageFormat.Jpeg;
                            break;
                        case "gif":
                            imgFormat = ImageFormat.Gif;
                            break;
                    }
                } else { // no fileextension specified, use default png 
                    filename += ".png";
                }

                filename.Replace("%desktop%", System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                

                img.Save(filename, ImageFormat.Jpeg);                
                
            }));
        }

        public void createPDF(string filename) {
            filename = filename.Replace("%desktop%", System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

            MainController.Instance.WindowInstance.BeginInvoke(new Action(() => {
                
                var mainWindow = MainController.Instance.WindowInstance;

                if (!filename.ToLower().EndsWith(".pdf"))
                    filename += ".pdf";

                mainWindow.ChromeBrowserInstance.PrintToPdfAsync(filename).Wait();
               
            }));
        }
                
        public void createGif(string filename, int durationMilliseconds = 5000, int msFrameDelay = 33, bool loop = false) {
            // 33 milliseconds for around 30 fps

            filename = filename.Replace("%desktop%", System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

            List<Image> images = new List<Image>();
            var form = MainController.Instance.WindowInstance;
           
            var temp = AnimatedGif.AnimatedGif.Create(filename, msFrameDelay, loop ? 0 : 1);            
            while (durationMilliseconds > 0) {
                
                System.Threading.Thread.Sleep(msFrameDelay);                
                durationMilliseconds -= msFrameDelay;

                form.Invoke(new Action(() => {
                    var img = WindowCapture.CaptureWindow(form.ChromeBrowserInstance.Handle);
                    images.Add(img);
                }));
                
            }

            foreach(var img in images)
                temp.AddFrame(img);

            temp.Dispose();
        }
    }
  

}
