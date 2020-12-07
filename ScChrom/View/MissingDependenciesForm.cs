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

        private Tools.DependencyInstaller installer;

        public bool IsUpdate {
            get; set;
        }

        public string InstallDirectory {
            get; set;
        }

        public MissingDependenciesForm() {
            InitializeComponent();

            installer = new DependencyInstaller(Program.Dependencies, true);
            installer.DownloadCanceled += Installer_DownloadCanceled;
            installer.DownloadStarted += Installer_DownloadStarted;
            installer.ExtractionStarted += Installer_ExtractionStarted;
            installer.ProgressChanged += Installer_ProgressChanged;
            installer.ErrorOccured += Installer_ErrorOccured;
            installer.InstallationFinished += Installer_InstallationFinished;
            
            tb_destination.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ScChrom");
        }

        private void startScChrom() {
            Process.Start(Path.Combine(installer.DestinationDirectory, Path.GetFileName(installer.OwnPath)));
            Logger.Log("ScChrom started");
        }

        #region installer events
        private void Installer_DownloadStarted() {
            if (InvokeRequired) {
                BeginInvoke(new Action(Installer_DownloadStarted));
                return;
            }

            btn_setup.Visible = false;
            btn_cancel.Visible = true;
            pg_progress.Visible = true;
            l_progress.Visible = true;
            btn_destination.Visible = false;
            tb_destination.Enabled = false;
        }

        private void Installer_DownloadCanceled(Exception ex) {

            if(InvokeRequired) {
                BeginInvoke(new Action(() => {
                    Installer_DownloadCanceled(ex);
                }));
                return;
            }

            if (ex != null) 
                MessageBox.Show(ex.Message, "Error while downloading");
            

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

        private void Installer_ExtractionStarted(string dependencyName) {
            if (InvokeRequired) {
                BeginInvoke(new Action(() => {
                    Installer_ExtractionStarted(dependencyName);
                }));
                return;
            }
            
            l_status.Text = "Extracting " + dependencyName;
        }

        private void Installer_ProgressChanged(DependencyInstaller.ProgressState state) {
            if(InvokeRequired) {
                BeginInvoke(new Action(() => Installer_ProgressChanged(state)));
                return;
            }

            if(state.Progress > -1) {
                pg_progress.Value = state.Progress;
            } else {
                l_status.Text = "Downloading (" + (state.FinishedCount + 1) + "/" + state.AllDepenciesCount + ") " + state.CurrentDepencyName;
            }
        }

        private void Installer_InstallationFinished() {
            if (InvokeRequired) {
                BeginInvoke(new Action(Installer_InstallationFinished));
                return;
            }
           
            Installer_DownloadCanceled(null);

            btn_setup.Text = "Start ScChrom";
            tb_destination.Enabled = false;
            btn_destination.Visible = false;
            cb_autostart.Visible = false;
            

            if (cb_autostart.Checked) {
                startScChrom();
                this.Close();
            }
        }

        private void Installer_ErrorOccured(Exception ex) {
            if (InvokeRequired) {
                BeginInvoke(new Action(() => Installer_ErrorOccured(ex)));
                return;
            }

            MessageBox.Show(ex.Message, "Error occured");
        }
        #endregion

        #region form events
        private void btn_setup_Click(object sender, EventArgs e) {
            if (!Directory.Exists(installer.DestinationDirectory)) {
                try {
                    Directory.CreateDirectory(installer.DestinationDirectory);
                } catch (Exception ex) {
                    MessageBox.Show("Could not create setup directory, error was: " + ex.Message, "Invalid setup path");
                    return;
                }                
            }

            
            if(installer.Finished) {
                startScChrom();
                return;
            }

            Logger.Init("info", Path.Combine(installer.DestinationDirectory, "setuplog.txt"));
            Logger.Log("Starting setup...");
            installer.DownloadDependencies();
        
        }        

        private void tb_destination_TextChanged(object sender, EventArgs e) {
            installer.DestinationDirectory = tb_destination.Text;
        }
        
        private void btn_destination_Click(object sender, EventArgs e) {
            FolderBrowserDialog dia = new FolderBrowserDialog();
            if(Directory.Exists(tb_destination.Text))
                dia.SelectedPath = tb_destination.Text;
            
            if (dia.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                tb_destination.Text = dia.SelectedPath;
            }
        }

        private void btn_cancel_Click(object sender, EventArgs e) {
            installer.CancelDownload();
        }

        private void MissingDependenciesForm_Load(object sender, EventArgs e) {
            if (InstallDirectory != null) 
                tb_destination.Text = InstallDirectory.Replace("\\\\", "\\");
            
            if (IsUpdate) {
                l_heading.Text = "ScChrom update";
                tb_destination.Enabled = false;
                btn_destination.Visible = false;
                this.Text = "Update to " + Application.ProductVersion;
                btn_setup_Click(null, null);
            }
            
        }
        #endregion

    }
}
