using ScChrom.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScChrom.View {
    
    public partial class MissingDependenciesForm : Form {

        public class OnlineDependency {
            public string Name { get; set; }

            public string URL { set; get; }

            public long TotalBytes { set; get; }

            public long DownloadedBytes { get; set; }

            public string SourceDirectory { get; set; }

            public bool IsFinished {
                get {
                    return DownloadedBytes == TotalBytes;
                }
            }
        }

        private OnlineDependency _currentDependency;
        private List<OnlineDependency> _allDependencies;
        private bool _downloadCanceled;

        public string OwnPath {
            get {                
                return System.Reflection.Assembly.GetEntryAssembly().Location;
            }
        }

        public string BaseDirectory {
            get {
                return System.IO.Path.GetDirectoryName(OwnPath);
            }
        }

        public string TempDirectory {
            get {
                return System.IO.Path.Combine(BaseDirectory, "temp_ScChrom");
            }
        }

        public string DestinationDirectory {
            get; set;
        }

        public int ProgressPercentage {
            get {
                long allBytes = 0;
                long downloadedBytes = 0;

                foreach (var dep in _allDependencies) {
                    allBytes += dep.TotalBytes;
                    downloadedBytes += dep.DownloadedBytes;
                }

                if (downloadedBytes == allBytes)
                    return 100;

                return (int)(((double)downloadedBytes / (double)allBytes) * 100.0f);
            }
        }

        private static System.Net.WebClient _wc;

        public static List<OnlineDependency> Dependencies {
            get {
                string baseUrl = "https://www.nuget.org/api/v2/package/";

                var ret = new List<OnlineDependency>() { 
                    new OnlineDependency() {
                        Name = "CefSharp.Common",
                        TotalBytes = 9685269, 
                        URL = baseUrl + "CefSharp.Common/85.3.130",
                        SourceDirectory = Path.Combine("CefSharp", "x" + (Environment.Is64BitOperatingSystem ? "64" : "32"))
                    },
                    new OnlineDependency() {
                        Name = "CefSharp.WinForms",
                        TotalBytes = 106193,
                        URL = baseUrl + "CefSharp.WinForms/85.3.130",
                        SourceDirectory = Path.Combine("CefSharp", "x" + (Environment.Is64BitOperatingSystem ? "64" : "32"))
                    },
                    new OnlineDependency() {
                        Name = "Jint",
                        TotalBytes = 428983,
                        URL = baseUrl + "jint/3.0.0-beta-1632",
                        SourceDirectory = Path.Combine("lib", "net45")
                    },
                    new OnlineDependency() {
                        Name = "Esprima",
                        TotalBytes = 204800,
                        URL = baseUrl + "esprima/1.0.1251",
                        SourceDirectory = Path.Combine("lib", "net45")
                    },
                    new OnlineDependency() {
                        Name = "Esprima",
                        TotalBytes = 2596051,
                        URL = baseUrl + "Newtonsoft.Json/12.0.3",
                        SourceDirectory = Path.Combine("lib", "net45")
                    },
                };

                if (Environment.Is64BitOperatingSystem) {
                    ret.Add(new OnlineDependency() {
                        Name = "Redis64",
                        TotalBytes = 80754395,
                        URL =  baseUrl + "cef.redist.x64/85.3.13",
                        SourceDirectory = "CEF"
                    });
                } else {
                    ret.Add(new OnlineDependency() {
                        Name = "Redis32",
                        TotalBytes = 76714841,
                        URL = baseUrl + "cef.redist.x86/85.3.13",
                        SourceDirectory = "CEF"
                    });
                }
                
                return ret;
            }
        }

        public MissingDependenciesForm() {
            InitializeComponent();            

            tb_destination.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ScChrom");
        }

        private void btn_setup_Click(object sender, EventArgs e) {
            if (!Directory.Exists(DestinationDirectory)) {
                try {
                    Directory.CreateDirectory(DestinationDirectory);
                } catch (Exception ex) {
                    MessageBox.Show("Could not create setup directory, error was: " + ex.Message, "Invalid setup path");
                    return;
                }                
            }

            Logger.Init("info", Path.Combine(DestinationDirectory, "setuplog.txt"));
            Logger.Log("Starting setup...");
            downloadDependencies();

            string destinationFile = Path.Combine(DestinationDirectory, Path.GetFileName(OwnPath));

            if(Path.GetFullPath(destinationFile) != Path.GetFullPath(OwnPath)) {
                try {
                    File.Copy(OwnPath, destinationFile, true);
                } catch (Exception ex) {
                    MessageBox.Show("Could not copy ScChrom main executable, error was: " + ex.Message, "Error while copying");
                    return;
                }
            }
            

            btn_setup.Visible = false;
            btn_cancel.Visible = true;
            pg_progress.Visible = true;
            l_progress.Visible = true;
            btn_destination.Visible = false;
            tb_destination.Enabled = false;
        }

        public void downloadDependencies() {            

            _allDependencies = Dependencies;            

            if (_wc == null) {
                _wc = new System.Net.WebClient();

                _wc.DownloadProgressChanged += _wc_DownloadProgressChanged;                
                _wc.DownloadFileCompleted += _wc_DownloadFileCompleted;                
            }
            
            downloadDependency(_allDependencies.First());
           
        }

        public void downloadDependency(OnlineDependency dependency) {

            _downloadCanceled = false;

            int dependencyIndex = _allDependencies.IndexOf(dependency);

            if (System.IO.Directory.Exists(TempDirectory)) {
                Directory.Delete(TempDirectory, true);
            }
            System.IO.Directory.CreateDirectory(TempDirectory);

            l_status.Text = "Downloading (" + dependencyIndex + "/" + _allDependencies.Count + ") " + dependency.Name;

            _currentDependency = dependency;

            Logger.Log("Start downloading dependency " + dependency.Name);

            _wc.DownloadFileAsync(new Uri(dependency.URL), System.IO.Path.Combine(TempDirectory, dependency.Name));
        }

        private void _wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e) {

            bool errorOccured = false;

            this.Invoke(new Action(() => {
                if (e.Error != null) {

                    errorOccured = true;

                    string text = "Error occured while downloading dependency " + _currentDependency.Name + ": \n" + e.Error.Message;

                    Logger.Log("Error while doanloading dependency: " + _currentDependency.Name, Logger.LogLevel.error);
                    Logger.Log("Error was: " + e.Error.Message, Logger.LogLevel.error);

                    if (e.Error is System.Net.WebException) {
                        if(_downloadCanceled) {
                            text = null;
                        } else {
                            cancelDownload();
                        }
                    }

                    if(text != null) {                    
                        MessageBox.Show(text);
                    }

                    cleanup();
                    
                    return;
                }

                Logger.Log("Download complete: " + _currentDependency.Name);
                l_status.Text = "Extracting " + _currentDependency.Name;
            }));

            if (_downloadCanceled || errorOccured) {
                return;
            }

            Task.Run(() => {
                Logger.Log("Extracting dependency: " + _currentDependency.Name);

                System.IO.Compression.ZipFile.ExtractToDirectory(System.IO.Path.Combine(TempDirectory, _currentDependency.Name), TempDirectory);
                string sourceFolder = "";
                if(_currentDependency.SourceDirectory != null){
                    
                    sourceFolder = Path.Combine(TempDirectory, _currentDependency.SourceDirectory);
                    
                    // copy to correct position
                    var files = Directory.GetFiles(sourceFolder);
                    foreach (var file in files) {
                        if (file.ToLower().EndsWith(".pdb"))
                            continue;
                        File.Copy(file, Path.Combine(DestinationDirectory, Path.GetFileName(file)), true);
                    }

                    var dirs = Directory.GetDirectories(sourceFolder);
                    foreach (var dir in dirs) {
                        string dirName = new DirectoryInfo(dir).Name;
                        CopyFolder(dir, Path.Combine(DestinationDirectory, dirName));
                    }

                    cleanup();
                }

                BeginInvoke(new Action(() => {
                    if (_currentDependency != _allDependencies.Last()) {
                        int index = _allDependencies.IndexOf(_currentDependency);
                        var curDep = _allDependencies[index+1];
                        downloadDependency(curDep);
                    } else {
                        copyNecessaryDlls();
                        installationFinished();
                    }
                }));

            });
                            
        }

        private void cleanup() {
            Logger.Log("Doing cleanup, removing temp direcctory");
            try {
                if (Directory.Exists(TempDirectory))
                    Directory.Delete(TempDirectory, true);
            } catch (Exception ex) {
                Logger.Log("Error while removing temp directory: " + ex.Message, Logger.LogLevel.error);
                Invoke(new Action(() => {
                    MessageBox.Show("Error occured while removing temp folder: " + ex.Message, "Failed to remove temp folder");
                }));
            }
        }

        private void installationFinished() {
            Logger.Log("Setup finished");
            MessageBox.Show("Setup finished, will start now");
            Process.Start(Path.Combine(DestinationDirectory, Path.GetFileName(OwnPath)));
            Logger.Log("ScChrom started");
            this.Close();
        }

        private void copyNecessaryDlls() {
            File.WriteAllBytes(Path.Combine(DestinationDirectory, "msvcp140.dll"), Properties.Resources.msvcp140_64);
            File.WriteAllBytes(Path.Combine(DestinationDirectory, "vcruntime140.dll"), Properties.Resources.vcruntime140_64);
        }

        private void cancelDownload() {
            _downloadCanceled = true;
            if (_wc != null && _wc.IsBusy ) {
                _wc.CancelAsync();
            }

            Logger.Log("Download canceled");

            btn_setup.Visible = true;
            btn_cancel.Visible = false;
            pg_progress.Visible = false;
            l_progress.Visible = false;
            l_progress.Text = "";
            l_status.Text = "";
            btn_destination.Visible = true;
            tb_destination.Enabled = true;
            pg_progress.Value = 0;
        }

        private void _wc_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e) {
            if (!this.IsHandleCreated)
                return;

            int oldProgress = ProgressPercentage;
            _currentDependency.DownloadedBytes = e.BytesReceived;
            int newProgress = ProgressPercentage;

            if (oldProgress < newProgress) {
                this.BeginInvoke(new Action(() => {
                    pg_progress.Value = newProgress;
                }));
            }
        }

        private void tb_destination_TextChanged(object sender, EventArgs e) {
            DestinationDirectory = tb_destination.Text;
        }


        static public void CopyFolder(string sourceFolder, string destFolder) {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);
            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files) {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);
                File.Copy(file, dest, true);
            }
            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders) {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyFolder(folder, dest);
            }
        }

        private void b_destination_Click(object sender, EventArgs e) {
            FolderBrowserDialog dia = new FolderBrowserDialog();
            if(Directory.Exists(tb_destination.Text))
                dia.SelectedPath = tb_destination.Text;
            
            if (dia.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                tb_destination.Text = dia.SelectedPath;
            }
        }

        private void btn_cancel_Click(object sender, EventArgs e) {
            cancelDownload();
        }
    }
}
