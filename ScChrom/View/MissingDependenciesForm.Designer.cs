namespace ScChrom.View {
    partial class MissingDependenciesForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MissingDependenciesForm));
            this.l_heading = new System.Windows.Forms.Label();
            this.btn_setup = new System.Windows.Forms.Button();
            this.pg_progress = new System.Windows.Forms.ProgressBar();
            this.l_status = new System.Windows.Forms.Label();
            this.btn_destination = new System.Windows.Forms.Button();
            this.tb_destination = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.l_progress = new System.Windows.Forms.Label();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.cb_autostart = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // l_heading
            // 
            this.l_heading.AutoSize = true;
            this.l_heading.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.l_heading.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.l_heading.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.l_heading.Location = new System.Drawing.Point(75, 25);
            this.l_heading.Name = "l_heading";
            this.l_heading.Size = new System.Drawing.Size(257, 39);
            this.l_heading.TabIndex = 0;
            this.l_heading.Text = "ScChrom setup";
            // 
            // btn_setup
            // 
            this.btn_setup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_setup.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.btn_setup.Location = new System.Drawing.Point(140, 124);
            this.btn_setup.Name = "btn_setup";
            this.btn_setup.Size = new System.Drawing.Size(131, 23);
            this.btn_setup.TabIndex = 1;
            this.btn_setup.Text = "start download";
            this.btn_setup.UseVisualStyleBackColor = true;
            this.btn_setup.Click += new System.EventHandler(this.btn_setup_Click);
            // 
            // pg_progress
            // 
            this.pg_progress.ForeColor = System.Drawing.Color.Gray;
            this.pg_progress.Location = new System.Drawing.Point(29, 205);
            this.pg_progress.Name = "pg_progress";
            this.pg_progress.Size = new System.Drawing.Size(354, 23);
            this.pg_progress.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.pg_progress.TabIndex = 2;
            this.pg_progress.Visible = false;
            // 
            // l_status
            // 
            this.l_status.AutoSize = true;
            this.l_status.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.l_status.Location = new System.Drawing.Point(89, 180);
            this.l_status.Name = "l_status";
            this.l_status.Size = new System.Drawing.Size(0, 13);
            this.l_status.TabIndex = 3;
            // 
            // btn_destination
            // 
            this.btn_destination.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.btn_destination.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_destination.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.btn_destination.Location = new System.Drawing.Point(352, 98);
            this.btn_destination.Margin = new System.Windows.Forms.Padding(0);
            this.btn_destination.Name = "btn_destination";
            this.btn_destination.Size = new System.Drawing.Size(31, 22);
            this.btn_destination.TabIndex = 4;
            this.btn_destination.Text = "...";
            this.btn_destination.UseVisualStyleBackColor = false;
            this.btn_destination.Click += new System.EventHandler(this.btn_destination_Click);
            // 
            // tb_destination
            // 
            this.tb_destination.BackColor = System.Drawing.Color.Gray;
            this.tb_destination.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tb_destination.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.tb_destination.Location = new System.Drawing.Point(29, 98);
            this.tb_destination.Name = "tb_destination";
            this.tb_destination.Size = new System.Drawing.Size(320, 20);
            this.tb_destination.TabIndex = 5;
            this.tb_destination.TextChanged += new System.EventHandler(this.tb_destination_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.label2.Location = new System.Drawing.Point(26, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Destination:";
            // 
            // l_progress
            // 
            this.l_progress.AutoSize = true;
            this.l_progress.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.l_progress.Location = new System.Drawing.Point(26, 180);
            this.l_progress.Name = "l_progress";
            this.l_progress.Size = new System.Drawing.Size(51, 13);
            this.l_progress.TabIndex = 7;
            this.l_progress.Text = "Progress:";
            this.l_progress.Visible = false;
            // 
            // btn_cancel
            // 
            this.btn_cancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_cancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.btn_cancel.Location = new System.Drawing.Point(140, 249);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(131, 23);
            this.btn_cancel.TabIndex = 8;
            this.btn_cancel.Text = "cancel";
            this.btn_cancel.UseVisualStyleBackColor = true;
            this.btn_cancel.Visible = false;
            this.btn_cancel.Click += new System.EventHandler(this.btn_cancel_Click);
            // 
            // cb_autostart
            // 
            this.cb_autostart.AutoSize = true;
            this.cb_autostart.Checked = true;
            this.cb_autostart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_autostart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cb_autostart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cb_autostart.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.cb_autostart.Location = new System.Drawing.Point(113, 153);
            this.cb_autostart.Name = "cb_autostart";
            this.cb_autostart.Size = new System.Drawing.Size(179, 17);
            this.cb_autostart.TabIndex = 9;
            this.cb_autostart.Text = "Autostart ScChrom when finished";
            this.cb_autostart.UseVisualStyleBackColor = true;
            // 
            // MissingDependenciesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.ClientSize = new System.Drawing.Size(412, 284);
            this.Controls.Add(this.cb_autostart);
            this.Controls.Add(this.btn_cancel);
            this.Controls.Add(this.l_progress);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tb_destination);
            this.Controls.Add(this.btn_destination);
            this.Controls.Add(this.l_status);
            this.Controls.Add(this.pg_progress);
            this.Controls.Add(this.btn_setup);
            this.Controls.Add(this.l_heading);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MissingDependenciesForm";
            this.Text = "Setup";
            this.Load += new System.EventHandler(this.MissingDependenciesForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label l_heading;
        private System.Windows.Forms.Button btn_setup;
        private System.Windows.Forms.ProgressBar pg_progress;
        private System.Windows.Forms.Label l_status;
        private System.Windows.Forms.Button btn_destination;
        private System.Windows.Forms.TextBox tb_destination;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label l_progress;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.CheckBox cb_autostart;
    }
}