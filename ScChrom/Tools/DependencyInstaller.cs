using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ScChrom.Tools {
    public class DependencyInstaller {

        public class OnlineDependency {
            public string Name { get; set; }

            public string URL { set; get; }

            public long TotalBytes { set; get; }

            public long DownloadedBytes { get; set; }

            public string[] SourceDirectories { get; set; }
            
            public bool IsFinished {
                get {
                    return DownloadedBytes == TotalBytes;
                }
            }
        }

        public struct ProgressState {
            public int AllDepenciesCount;
            public int FinishedCount;
            public int Progress;
            public bool Extracting;
            public string CurrentDepencyName; 
        }

        private OnlineDependency _currentDependency;
        private List<OnlineDependency> _allDependencies;
        private bool _downloadCanceled;
        private System.Net.WebClient _wc;
        private bool _copyOwnExecutable;

        public event Action DownloadStarted;
        public event Action<Exception> DownloadCanceled;
        public event Action<string> ExtractionStarted;
        public event Action<Exception> ErrorOccured;
        public event Action<ProgressState> ProgressChanged;
        public event Action InstallationFinished;


        public string OwnPath {
            get {
                return System.Reflection.Assembly.GetEntryAssembly().Location;
            }
        }

        public string BaseDirectory {
            get {
                return Path.GetDirectoryName(OwnPath);
            }
        }

        public string TempDirectory {
            get {
                return Path.Combine(BaseDirectory, "temp_ScChrom");
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

        public bool Finished {
            get {
                return ProgressPercentage == 100;
            }
        }
       
        public Version InstalledVersion {
            get {
                Version ret = null;
                try {
                    ret = Version.Parse(CefSharp.AssemblyInfo.AssemblyVersion);
                } catch (Exception) {
                    // not available
                    return null;
                }
                
                return ret;
            }
        }


        public DependencyInstaller(List<OnlineDependency> allDependencies, bool copyOwnExecutable) {
            // necessary cause from 15 june 2020 on nuget enforces tls 1.2
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            _copyOwnExecutable = copyOwnExecutable;

            _allDependencies = allDependencies;
        }

        public void DownloadDependencies() {

            if (DownloadStarted != null)
                Task.Run(DownloadStarted);

            if (_wc == null) {
                _wc = new System.Net.WebClient();

                _wc.DownloadProgressChanged += _wc_DownloadProgressChanged;
                _wc.DownloadFileCompleted += _wc_DownloadFileCompleted;
            }

            downloadDependency(_allDependencies.First());

        }

        public void CancelDownload() {
            _downloadCanceled = true;
            if (_wc != null && _wc.IsBusy) 
                _wc.CancelAsync();
            

            Logger.Log("Download canceled");

            if (DownloadCanceled != null)
                Task.Run(() => DownloadCanceled.Invoke(null));

        }


        private void downloadDependency(OnlineDependency dependency) {

            _downloadCanceled = false;

            int dependencyIndex = _allDependencies.IndexOf(dependency);

            if (Directory.Exists(TempDirectory)) {
                Directory.Delete(TempDirectory, true);
            }
            Directory.CreateDirectory(TempDirectory);

            informAboutProgressChange(dependencyIndex, dependency.Name);

            _currentDependency = dependency;

            Logger.Log("Start downloading dependency " + dependency.Name);

            _wc.DownloadFileAsync(new Uri(dependency.URL), Path.Combine(TempDirectory, dependency.Name));
        }

        private void _wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e) {

            bool errorOccured = false;

           
            if (e.Error != null) {

                errorOccured = true;

                string text = "Error occured while downloading dependency " + _currentDependency.Name + ": \n" + e.Error.Message;

                if(!_downloadCanceled) {
                    Logger.Log("Error while doanloading dependency: " + _currentDependency.Name, Logger.LogLevel.error);
                    Logger.Log("Error was: " + e.Error.Message, Logger.LogLevel.error);
                }

                if (e.Error is System.Net.WebException) {
                    if (_downloadCanceled) {
                        text = null;
                    } else {
                        CancelDownload();
                    }
                }


                Exception ex = null;
                if (text != null)
                    ex = new Exception(text);

                if (DownloadCanceled != null)
                    Task.Run(() => DownloadCanceled.Invoke(ex));
                
                cleanup();

                return;
            }

            Logger.Log("Download complete: " + _currentDependency.Name);
            

            if (_downloadCanceled || errorOccured) {
                return;
            }
            
            Task.Run(() => {
                Logger.Log("Extracting dependency: " + _currentDependency.Name);

                if(ExtractionStarted != null) 
                    Task.Run(() => ExtractionStarted(_currentDependency.Name));

                try {
                    extractDependency();                    
                } catch (Exception ex) {
                    Logger.Log("Error while removing temp directory: " + ex.Message, Logger.LogLevel.error);

                    if (ErrorOccured != null)
                        Task.Run(() => ErrorOccured.Invoke(ex));
                }
            });
        }

        private void _wc_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e) {
            
            int oldProgress = ProgressPercentage;
            _currentDependency.DownloadedBytes = e.BytesReceived;
            int newProgress = ProgressPercentage;

            if (oldProgress < newProgress) 
                informAboutProgressChange(0, "", newProgress);               
            
        }

        private void informAboutProgressChange(int depencyIndex, string depencyName, int progress = -1) {
            if (ProgressChanged == null)
                return;

            ProgressState ps = new ProgressState() {
                Progress = progress
            };

            if (progress < 0) {
                ps.AllDepenciesCount = _allDependencies.Count;
                ps.CurrentDepencyName = depencyName;
                ps.Extracting = false;
                ps.FinishedCount = depencyIndex;
            }

            Task.Run(() => ProgressChanged.Invoke(ps));
        }

        /// <summary>
        /// Adds necessary dlls missing on some windows installations
        /// </summary>
        private void copyNecessaryDlls() {
            File.WriteAllBytes(Path.Combine(DestinationDirectory, "msvcp140.dll"), Properties.Resources.msvcp140_64);
            File.WriteAllBytes(Path.Combine(DestinationDirectory, "vcruntime140.dll"), Properties.Resources.vcruntime140_64);
        }
      
        /// <summary>
        /// Extracts the current Dependency and informs if it's the last one
        /// </summary>
        private void extractDependency() {
            // extract nuget packages
            System.IO.Compression.ZipFile.ExtractToDirectory(Path.Combine(TempDirectory, _currentDependency.Name), TempDirectory);
            
            // copy files to destination
            foreach(var sourceDirectory in _currentDependency.SourceDirectories) {

                string sourceFolder = Path.Combine(TempDirectory, sourceDirectory);
                
                if (!Directory.Exists(DestinationDirectory))
                    Directory.CreateDirectory(DestinationDirectory);

                // copy to correct position
                var files = Directory.GetFiles(sourceFolder);
                foreach (var file in files) {

                    if (file.ToLower().EndsWith(".pdb") || 
                        file.ToLower().EndsWith(".xml") ||
                        file.ToLower().EndsWith(".txt"))
                        continue;
                    File.Copy(file, Path.Combine(DestinationDirectory, Path.GetFileName(file)), true);
                }

                var dirs = Directory.GetDirectories(sourceFolder);                
                foreach (var dir in dirs) {
                    string dirName = new DirectoryInfo(dir).Name;
                    Common.CopyFolder(dir, Path.Combine(DestinationDirectory, dirName));
                }                
            }

            cleanup();

            if (_currentDependency != _allDependencies.Last()) {
                // download next dependency
                int index = _allDependencies.IndexOf(_currentDependency);
                var curDep = _allDependencies[index + 1];
                downloadDependency(curDep);
            } else {                
                // finalize setup

                copyNecessaryDlls();

                string destinationFile = Path.Combine(DestinationDirectory, Path.GetFileName(OwnPath));

                if (_copyOwnExecutable) {
                    if (Path.GetFullPath(destinationFile) != Path.GetFullPath(OwnPath)) {
                        try {
                            File.Copy(OwnPath, destinationFile, true);
                        } catch (Exception ex) {
                            if (ErrorOccured != null)
                                ErrorOccured.Invoke(new Exception("Could not copy main executable, error was: " + ex.Message));
                            return;
                        }
                    }
                }

                Logger.Log("Setup finished");

                if (InstallationFinished != null)
                    Task.Run(InstallationFinished);
            }
        }

        private void cleanup() {
            Logger.Log("Doing cleanup, removing temp direcctory");
            try {
                if (Directory.Exists(TempDirectory))
                    Directory.Delete(TempDirectory, true);
            } catch (Exception ex) {
                Logger.Log("Error while removing temp directory: " + ex.Message, Logger.LogLevel.error);

                if (ErrorOccured != null)
                    Task.Run(() => ErrorOccured.Invoke(ex));
            }
        }

    }
}
