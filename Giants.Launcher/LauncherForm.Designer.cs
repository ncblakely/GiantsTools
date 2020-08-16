namespace Giants.Launcher
{
    partial class LauncherForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LauncherForm));
            this.btnExit = new Giants.Launcher.ImageButton();
            this.btnOptions = new Giants.Launcher.ImageButton();
            this.btnPlay = new Giants.Launcher.ImageButton();
            this.updateProgressBar = new System.Windows.Forms.ProgressBar();
            this.txtProgress = new System.Windows.Forms.Label();
            this.CommunityLabel = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.btnExit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnOptions)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnPlay)).BeginInit();
            this.SuspendLayout();
            // 
            // btnExit
            // 
            this.btnExit.BackColor = System.Drawing.Color.Transparent;
            this.btnExit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnExit.DownImage = global::Giants.Launcher.Resources.exitpush;
            this.btnExit.HoverImage = global::Giants.Launcher.Resources.exithover;
            this.btnExit.Location = new System.Drawing.Point(618, 451);
            this.btnExit.Name = "btnExit";
            this.btnExit.NormalImage = global::Giants.Launcher.Resources.exit;
            this.btnExit.Size = new System.Drawing.Size(100, 50);
            this.btnExit.TabIndex = 8;
            this.btnExit.TabStop = false;
            this.btnExit.Visible = false;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // btnOptions
            // 
            this.btnOptions.BackColor = System.Drawing.Color.Transparent;
            this.btnOptions.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOptions.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btnOptions.DownImage = global::Giants.Launcher.Resources.optionspush;
            this.btnOptions.HoverImage = global::Giants.Launcher.Resources.optionshover;
            this.btnOptions.Location = new System.Drawing.Point(618, 395);
            this.btnOptions.Name = "btnOptions";
            this.btnOptions.NormalImage = global::Giants.Launcher.Resources.options;
            this.btnOptions.Size = new System.Drawing.Size(118, 50);
            this.btnOptions.TabIndex = 7;
            this.btnOptions.TabStop = false;
            this.btnOptions.Visible = false;
            this.btnOptions.Click += new System.EventHandler(this.btnOptions_Click);
            // 
            // btnPlay
            // 
            this.btnPlay.BackColor = System.Drawing.Color.Transparent;
            this.btnPlay.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPlay.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btnPlay.DownImage = global::Giants.Launcher.Resources.playpush;
            this.btnPlay.HoverImage = global::Giants.Launcher.Resources.playhover;
            this.btnPlay.Location = new System.Drawing.Point(618, 339);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.NormalImage = ((System.Drawing.Image)(resources.GetObject("btnPlay.NormalImage")));
            this.btnPlay.Size = new System.Drawing.Size(100, 50);
            this.btnPlay.TabIndex = 6;
            this.btnPlay.TabStop = false;
            this.btnPlay.Visible = false;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // updateProgressBar
            // 
            this.updateProgressBar.Location = new System.Drawing.Point(64, 465);
            this.updateProgressBar.Name = "updateProgressBar";
            this.updateProgressBar.Size = new System.Drawing.Size(534, 23);
            this.updateProgressBar.TabIndex = 9;
            this.updateProgressBar.Visible = false;
            // 
            // txtProgress
            // 
            this.txtProgress.AutoSize = true;
            this.txtProgress.BackColor = System.Drawing.Color.Transparent;
            this.txtProgress.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.txtProgress.Location = new System.Drawing.Point(65, 449);
            this.txtProgress.Name = "txtProgress";
            this.txtProgress.Size = new System.Drawing.Size(69, 13);
            this.txtProgress.TabIndex = 10;
            this.txtProgress.Text = "ProgressText";
            this.txtProgress.Visible = false;
            // 
            // CommunityLabel
            // 
            this.CommunityLabel.ActiveLinkColor = System.Drawing.Color.DodgerBlue;
            this.CommunityLabel.AutoSize = true;
            this.CommunityLabel.BackColor = System.Drawing.Color.Transparent;
            this.CommunityLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CommunityLabel.LinkColor = System.Drawing.Color.DodgerBlue;
            this.CommunityLabel.Location = new System.Drawing.Point(12, 9);
            this.CommunityLabel.Name = "CommunityLabel";
            this.CommunityLabel.Size = new System.Drawing.Size(112, 17);
            this.CommunityLabel.TabIndex = 12;
            this.CommunityLabel.TabStop = true;
            this.CommunityLabel.Text = "CommunityLabel";
            this.CommunityLabel.Visible = false;
            this.CommunityLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.DiscordLabel_LinkClicked);
            // 
            // LauncherForm
            // 
            this.AcceptButton = this.btnPlay;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.CancelButton = this.btnExit;
            this.ClientSize = new System.Drawing.Size(800, 500);
            this.ControlBox = false;
            this.Controls.Add(this.CommunityLabel);
            this.Controls.Add(this.txtProgress);
            this.Controls.Add(this.updateProgressBar);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnOptions);
            this.Controls.Add(this.btnPlay);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LauncherForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Launcher";
            this.Load += new System.EventHandler(this.LauncherForm_Load);
            this.Shown += new System.EventHandler(this.LauncherForm_Shown);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LauncherForm_MouseDown);
            ((System.ComponentModel.ISupportInitialize)(this.btnExit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnOptions)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnPlay)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ImageButton btnPlay;
        private ImageButton btnOptions;
        private ImageButton btnExit;
        private System.Windows.Forms.ProgressBar updateProgressBar;
        private System.Windows.Forms.Label txtProgress;
        private System.Windows.Forms.LinkLabel CommunityLabel;
    }
}

