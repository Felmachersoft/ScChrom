using ScChrom.BrowserJs;
using ScChrom.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom.JsController {
    public class ExecutableController: IBrowserContextCallable, IJintContextCallable {

        public static List<JsControllerMethodInfo> MethodInfos {
            get {
                return new List<JsControllerMethodInfo>() {
                    new JsControllerMethodInfo(
                        "startProgram",
                        "Starts the program at the given executable path.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "executablePath",
                                "Path to the executable.",
                                JsControllerMethodInfo.DataType.text
                            ),
                            new JsControllerMethodParameter(
                                "arguments",
                                "The command line arguments to start the program with.",
                                JsControllerMethodInfo.DataType.text,
                                false
                            ),
                            new JsControllerMethodParameter(
                                "exit_callbackId",
                                "The id of the callback (registered in the browser context via ScChrom.registerCallback(id)) called when the program closes and is used by the 'killProgram' method. The value will be the exit code.",
                                JsControllerMethodInfo.DataType.text,
                                false
                            ),
                            new JsControllerMethodParameter(
                                "stdOut_callbackId",
                                "The id of the callback (registered in the browser context via ScChrom.registerCallback(id)) called when the program writes to the standard output.",
                                JsControllerMethodInfo.DataType.text,
                                false
                            ),
                            new JsControllerMethodParameter(
                                "errOut_callbackId",
                                "The id of the callback (registered in the browser context via ScChrom.registerCallback(id)) called when the program writes to the error output.",
                                JsControllerMethodInfo.DataType.text,
                                false
                            ),
                            new JsControllerMethodParameter(
                                "hideWindow",
                                "Set to true to prevent opening a console window for the new program (Only if program is a console program).",
                                JsControllerMethodInfo.DataType.boolean,
                                false
                            ),
                        }
                    ),
                    new JsControllerMethodInfo(
                        "killProgram",
                        "Closes a previously started program.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "exit_callbackId",
                                "The exit_callbackId used when starting the program.",
                                JsControllerMethodInfo.DataType.text
                            ),
                        }
                    ),                    
                    new JsControllerMethodInfo(
                        "callInBrowserCallback",
                        "This calls the callback that was registered via 'ScChrom.addCallback(id)' in the browser context.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "exitcallback_id",
                                "The exitcallback_id used to start the program via the 'startProgram' method.",
                                JsControllerMethodInfo.DataType.text
                            )
                        }
                    ),
                    new JsControllerMethodInfo(
                        "startNewScChrom",
                        "Shorthand function to start a new ScChrom instance.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "config",
                                "The config, structured like the contents of a config file, to start the new ScChrom instance with.",
                                JsControllerMethodInfo.DataType.text
                            ),
                            new JsControllerMethodParameter(
                                "exitcallback_id",
                                "The exitcallback_id used to start the program via the 'startProgram' method.",
                                JsControllerMethodInfo.DataType.text,
                                false
                            ),
                            new JsControllerMethodParameter(
                                "stdOut_callbackId",
                                "The id of the callback (registered in the browser context via ScChrom.registerCallback(id)) called when the new ScChrom instance writes to the standard output.",
                                JsControllerMethodInfo.DataType.text,
                                false
                            ),
                            new JsControllerMethodParameter(
                                "errOut_callbackId",
                                "The id of the callback (registered in the browser context via ScChrom.registerCallback(id)) called when the new ScChrom instance writes to the error output.",
                                JsControllerMethodInfo.DataType.text,
                                false
                            )
                        }
                    )
                };
            }
        }

        private static Dictionary<string, Process> _runningProcesses = new Dictionary<string, Process>();

        public void startProgram(string executablePath, string arguments = "", string exit_callbackId = "", string stdOut_callbackId = "", string errOut_callbackId = "", bool hideWindow = false) {
            
            try {
                ProcessStartInfo psi = new ProcessStartInfo(executablePath, arguments);
                
                if(hideWindow){
                    psi.CreateNoWindow = true;
                    psi.WindowStyle = ProcessWindowStyle.Hidden;
                }

                if (!string.IsNullOrWhiteSpace(stdOut_callbackId)) {
                    psi.RedirectStandardOutput = true;
                    psi.UseShellExecute = false;
                }
                if (!string.IsNullOrWhiteSpace(errOut_callbackId)) {
                    psi.RedirectStandardError = true;
                    psi.UseShellExecute = false;                    
                }

                Process proc = Process.Start(psi);

                if (!string.IsNullOrWhiteSpace(stdOut_callbackId)) {                    
                    proc.OutputDataReceived += (object sender, DataReceivedEventArgs e) => {
                        if (e.Data == null)
                            return;
                        MainController.Instance.WindowInstance.CallInBrowserCallback(stdOut_callbackId, e.Data);
                    };
                    proc.BeginOutputReadLine();
                }

                if (!string.IsNullOrWhiteSpace(errOut_callbackId)) {
                    proc.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => {
                        if (e.Data == null)
                            return;
                        MainController.Instance.WindowInstance.CallInBrowserCallback(errOut_callbackId, e.Data);
                    };
                    proc.BeginErrorReadLine();
                }

                if(!string.IsNullOrWhiteSpace(exit_callbackId)) {
                    proc.EnableRaisingEvents = true;
                    proc.Exited += (object sender, EventArgs e) => {
                        _runningProcesses.Remove(exit_callbackId);                        
                        MainController.Instance.WindowInstance.CallInBrowserCallback(exit_callbackId, proc.ExitCode + "");                        
                    };
                    _runningProcesses[exit_callbackId] = proc;
                }

            } catch (Exception ex) {
                MainController.Instance.WriteErrorOut("Error occured while starting program " + executablePath + " : " + ex.Message);
            }
        }
        
        public void killProgram(string exitcallback_id) {
            Process runningProc = null;
            if(_runningProcesses.TryGetValue(exitcallback_id, out runningProc)) {
                runningProc.Kill();
                _runningProcesses.Remove(exitcallback_id);
            }
        }

        public void startNewScChrom(string config, string exit_callback = "", string stdOut_callbackId = "", string errOut_callbackId = "") {
            string ownExe = System.Reflection.Assembly.GetExecutingAssembly().Location;
            
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(config);
            string base64Script = System.Convert.ToBase64String(plainTextBytes);
 
            startProgram(ownExe, "--config-base64=" + base64Script, exit_callback, stdOut_callbackId, errOut_callbackId);
        }
    }
}
