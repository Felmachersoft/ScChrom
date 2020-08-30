using System;
using ScChrom.Tools;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Threading;

namespace ScChrom {
    public class Program
    {        

        [STAThread]
        public static void Main() {
            
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            if (Arguments.GetArgument("version") != null) {                
                Console.WriteLine(MainController.Version);
                return;
            }
   

            // if an exception should be thrown and reach top level of excecution, log it(outside UI Thread)
            Application.ThreadException += new ThreadExceptionEventHandler((sender, ev) => {
                Logger.Log("Error occured: " + ev.Exception.StackTrace, Logger.LogLevel.error);
            });

            // if an exception should be thrown and reach top level of excecution, log it (inside UI Thread)           
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((sender, ev) => {
                Logger.Log("Error occured in gui thread: " + ((Exception)ev.ExceptionObject).StackTrace, Logger.LogLevel.error);
                Logger.Log("Error was: " + ((Exception)ev.ExceptionObject).Message, Logger.LogLevel.error);
            });
            
            string logLevel = Arguments.GetArgument("log-level");
            try {
                Logger.Init(logLevel, Arguments.GetArgument("logfile"));
            } catch (ArgumentException ex) {
                if (ex.ParamName == "logfilepath")
                    throw new ArgumentException("Command line argument 'logfile' had invalid value: " + Arguments.GetArgument("logfile"));                
            }
            Logger.Log("Logger started");                            

            Logger.Log("Starting program");


            if (logLevel == "debug") {
                Logger.Log("Started with following arguments:", Logger.LogLevel.debug);
                foreach(var kv in Arguments.AllArguments) {
                    Logger.Log(kv.Key, Logger.LogLevel.debug);
                    Logger.Log("=", Logger.LogLevel.debug);
                    Logger.Log(kv.Value, Logger.LogLevel.debug);
                }
                Logger.Log("", Logger.LogLevel.debug);
            }

            
            try {
                MainController.Instance.Start();                
	        } catch (FileNotFoundException ex) { // occurs if the dependencies are mssing, so installation starts
                Logger.Log("Missing dependency: " + ex.FileName, Logger.LogLevel.error);
                startSetup();
            }
            
        }

        private static void startSetup() {

            Logger.Log("Starting setup", Logger.LogLevel.info);
            // necessary cause from 15 june 2020 on nuget enforces tls 1.2
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            var dia = new View.MissingDependenciesForm();
            Application.Run(dia);
        }
        
    }
}
