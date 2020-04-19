namespace Center_window
{
    partial class frmMain
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

                if (_capturing)
                    this.CaptureMouse(false);
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
            this.btnAply = new System.Windows.Forms.Button();
            this.txtHandle = new System.Windows.Forms.TextBox();
            this.txtCaption = new System.Windows.Forms.TextBox();
            this.txtClass = new System.Windows.Forms.TextBox();
            this._pictureBox = new System.Windows.Forms.PictureBox();
            this.lblFinder = new System.Windows.Forms.Label();
            this.lblInfo = new System.Windows.Forms.Label();
            this.lblHandle = new System.Windows.Forms.Label();
            this.lblCaption = new System.Windows.Forms.Label();
            this.lblClass = new System.Windows.Forms.Label();
            this.lblRectangle = new System.Windows.Forms.Label();
            this.txtRectangle = new System.Windows.Forms.TextBox();
            this.chkCenter = new System.Windows.Forms.CheckBox();
            this.trkTransparency = new System.Windows.Forms.TrackBar();
            this.btnClose = new System.Windows.Forms.Button();
            this.chkTransparency = new System.Windows.Forms.CheckBox();
            this.lblTransparencyValue = new System.Windows.Forms.Label();
            this.btnOptions = new System.Windows.Forms.Button();
            this.lstWindows = new System.Windows.Forms.ListView();
            this.btnApp = new System.Windows.Forms.Button();
            this.lblApplications = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this._pictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkTransparency)).BeginInit();
            this.SuspendLayout();
            // 
            // btnAply
            // 
            this.btnAply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAply.Location = new System.Drawing.Point(583, 703);
            this.btnAply.Margin = new System.Windows.Forms.Padding(4);
            this.btnAply.Name = "btnAply";
            this.btnAply.Size = new System.Drawing.Size(100, 28);
            this.btnAply.TabIndex = 11;
            this.btnAply.Text = "&Aply";
            this.btnAply.UseVisualStyleBackColor = true;
            this.btnAply.Click += new System.EventHandler(this.btnAply_Click);
            // 
            // txtHandle
            // 
            this.txtHandle.Location = new System.Drawing.Point(20, 229);
            this.txtHandle.Margin = new System.Windows.Forms.Padding(4);
            this.txtHandle.Name = "txtHandle";
            this.txtHandle.Size = new System.Drawing.Size(371, 23);
            this.txtHandle.TabIndex = 3;
            this.txtHandle.TextChanged += new System.EventHandler(this.txtHandle_TextChanged);
            // 
            // txtCaption
            // 
            this.txtCaption.Location = new System.Drawing.Point(20, 167);
            this.txtCaption.Margin = new System.Windows.Forms.Padding(4);
            this.txtCaption.Name = "txtCaption";
            this.txtCaption.Size = new System.Drawing.Size(371, 23);
            this.txtCaption.TabIndex = 1;
            this.txtCaption.TextChanged += new System.EventHandler(this.txtCaption_TextChanged);
            // 
            // txtClass
            // 
            this.txtClass.Location = new System.Drawing.Point(419, 167);
            this.txtClass.Margin = new System.Windows.Forms.Padding(4);
            this.txtClass.Name = "txtClass";
            this.txtClass.Size = new System.Drawing.Size(371, 23);
            this.txtClass.TabIndex = 2;
            this.txtClass.TextChanged += new System.EventHandler(this.txtClass_TextChanged);
            // 
            // _pictureBox
            // 
            this._pictureBox.Location = new System.Drawing.Point(99, 79);
            this._pictureBox.Margin = new System.Windows.Forms.Padding(4);
            this._pictureBox.Name = "_pictureBox";
            this._pictureBox.Size = new System.Drawing.Size(43, 39);
            this._pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this._pictureBox.TabIndex = 28;
            this._pictureBox.TabStop = false;
            // 
            // lblFinder
            // 
            this.lblFinder.AutoSize = true;
            this.lblFinder.Location = new System.Drawing.Point(16, 91);
            this.lblFinder.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFinder.Name = "lblFinder";
            this.lblFinder.Size = new System.Drawing.Size(75, 17);
            this.lblFinder.TabIndex = 29;
            this.lblFinder.Text = "Finder tool";
            // 
            // lblInfo
            // 
            this.lblInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInfo.Location = new System.Drawing.Point(16, 11);
            this.lblInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(780, 59);
            this.lblInfo.TabIndex = 30;
            // 
            // lblHandle
            // 
            this.lblHandle.AutoSize = true;
            this.lblHandle.Location = new System.Drawing.Point(16, 209);
            this.lblHandle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblHandle.Name = "lblHandle";
            this.lblHandle.Size = new System.Drawing.Size(104, 17);
            this.lblHandle.TabIndex = 31;
            this.lblHandle.Text = "Window handle";
            // 
            // lblCaption
            // 
            this.lblCaption.AutoSize = true;
            this.lblCaption.Location = new System.Drawing.Point(16, 148);
            this.lblCaption.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCaption.Name = "lblCaption";
            this.lblCaption.Size = new System.Drawing.Size(56, 17);
            this.lblCaption.TabIndex = 32;
            this.lblCaption.Text = "Caption";
            // 
            // lblClass
            // 
            this.lblClass.AutoSize = true;
            this.lblClass.Location = new System.Drawing.Point(415, 148);
            this.lblClass.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblClass.Name = "lblClass";
            this.lblClass.Size = new System.Drawing.Size(81, 17);
            this.lblClass.TabIndex = 33;
            this.lblClass.Text = "Class name";
            // 
            // lblRectangle
            // 
            this.lblRectangle.AutoSize = true;
            this.lblRectangle.Location = new System.Drawing.Point(415, 209);
            this.lblRectangle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblRectangle.Name = "lblRectangle";
            this.lblRectangle.Size = new System.Drawing.Size(72, 17);
            this.lblRectangle.TabIndex = 34;
            this.lblRectangle.Text = "Rectangle";
            // 
            // txtRectangle
            // 
            this.txtRectangle.Location = new System.Drawing.Point(419, 229);
            this.txtRectangle.Margin = new System.Windows.Forms.Padding(4);
            this.txtRectangle.Name = "txtRectangle";
            this.txtRectangle.Size = new System.Drawing.Size(371, 23);
            this.txtRectangle.TabIndex = 4;
            this.txtRectangle.TextChanged += new System.EventHandler(this.txtRectangle_TextChanged);
            // 
            // chkCenter
            // 
            this.chkCenter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkCenter.AutoSize = true;
            this.chkCenter.Checked = true;
            this.chkCenter.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCenter.Location = new System.Drawing.Point(20, 650);
            this.chkCenter.Margin = new System.Windows.Forms.Padding(4);
            this.chkCenter.Name = "chkCenter";
            this.chkCenter.Size = new System.Drawing.Size(118, 21);
            this.chkCenter.TabIndex = 8;
            this.chkCenter.Text = "Center window";
            this.chkCenter.UseVisualStyleBackColor = true;
            this.chkCenter.CheckedChanged += new System.EventHandler(this.chkCenter_CheckedChanged);
            // 
            // trkTransparency
            // 
            this.trkTransparency.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trkTransparency.Location = new System.Drawing.Point(189, 606);
            this.trkTransparency.Margin = new System.Windows.Forms.Padding(4);
            this.trkTransparency.Maximum = 255;
            this.trkTransparency.Name = "trkTransparency";
            this.trkTransparency.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.trkTransparency.Size = new System.Drawing.Size(601, 45);
            this.trkTransparency.TabIndex = 7;
            this.trkTransparency.TickFrequency = 5;
            this.trkTransparency.ValueChanged += new System.EventHandler(this.trkTransparency_Changed);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.btnClose.Location = new System.Drawing.Point(691, 703);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(100, 28);
            this.btnClose.TabIndex = 12;
            this.btnClose.Text = "&Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // chkTransparency
            // 
            this.chkTransparency.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkTransparency.AutoSize = true;
            this.chkTransparency.Location = new System.Drawing.Point(20, 618);
            this.chkTransparency.Margin = new System.Windows.Forms.Padding(4);
            this.chkTransparency.Name = "chkTransparency";
            this.chkTransparency.Size = new System.Drawing.Size(115, 21);
            this.chkTransparency.TabIndex = 6;
            this.chkTransparency.Text = "Transparency";
            this.chkTransparency.UseVisualStyleBackColor = true;
            this.chkTransparency.CheckedChanged += new System.EventHandler(this.chkTransparency_Changed);
            // 
            // lblTransparencyValue
            // 
            this.lblTransparencyValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblTransparencyValue.AutoSize = true;
            this.lblTransparencyValue.Location = new System.Drawing.Point(149, 619);
            this.lblTransparencyValue.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTransparencyValue.Name = "lblTransparencyValue";
            this.lblTransparencyValue.Size = new System.Drawing.Size(16, 17);
            this.lblTransparencyValue.TabIndex = 40;
            this.lblTransparencyValue.Text = "0";
            // 
            // btnOptions
            // 
            this.btnOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOptions.Location = new System.Drawing.Point(16, 703);
            this.btnOptions.Margin = new System.Windows.Forms.Padding(4);
            this.btnOptions.Name = "btnOptions";
            this.btnOptions.Size = new System.Drawing.Size(100, 28);
            this.btnOptions.TabIndex = 9;
            this.btnOptions.Text = "&Options...";
            this.btnOptions.UseVisualStyleBackColor = true;
            this.btnOptions.Click += new System.EventHandler(this.btnOptions_Click);
            // 
            // lstWindows
            // 
            this.lstWindows.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstWindows.CheckBoxes = true;
            this.lstWindows.FullRowSelect = true;
            this.lstWindows.Location = new System.Drawing.Point(20, 292);
            this.lstWindows.Margin = new System.Windows.Forms.Padding(4);
            this.lstWindows.Name = "lstWindows";
            this.lstWindows.Size = new System.Drawing.Size(769, 293);
            this.lstWindows.TabIndex = 5;
            this.lstWindows.UseCompatibleStateImageBehavior = false;
            // 
            // btnApp
            // 
            this.btnApp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApp.Location = new System.Drawing.Point(445, 703);
            this.btnApp.Margin = new System.Windows.Forms.Padding(4);
            this.btnApp.Name = "btnApp";
            this.btnApp.Size = new System.Drawing.Size(100, 28);
            this.btnApp.TabIndex = 10;
            this.btnApp.Text = "Applications";
            this.btnApp.UseVisualStyleBackColor = true;
            this.btnApp.Click += new System.EventHandler(this.btnApp_Click);
            // 
            // lblApplications
            // 
            this.lblApplications.AutoSize = true;
            this.lblApplications.Location = new System.Drawing.Point(16, 272);
            this.lblApplications.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblApplications.Name = "lblApplications";
            this.lblApplications.Size = new System.Drawing.Size(199, 17);
            this.lblApplications.TabIndex = 44;
            this.lblApplications.Text = "Applications running in the OS";
            // 
            // frmMain
            // 
            this.AcceptButton = this.btnAply;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(812, 747);
            this.Controls.Add(this.lblApplications);
            this.Controls.Add(this.btnApp);
            this.Controls.Add(this.lstWindows);
            this.Controls.Add(this.btnOptions);
            this.Controls.Add(this.lblTransparencyValue);
            this.Controls.Add(this.chkTransparency);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.trkTransparency);
            this.Controls.Add(this.chkCenter);
            this.Controls.Add(this.txtRectangle);
            this.Controls.Add(this.lblRectangle);
            this.Controls.Add(this.lblClass);
            this.Controls.Add(this.lblCaption);
            this.Controls.Add(this.lblHandle);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.lblFinder);
            this.Controls.Add(this._pictureBox);
            this.Controls.Add(this.txtClass);
            this.Controls.Add(this.txtCaption);
            this.Controls.Add(this.txtHandle);
            this.Controls.Add(this.btnAply);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(828, 782);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Center window";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.Shown += new System.EventHandler(this.frmMain_Shown);
            this.Resize += new System.EventHandler(this.frmMain_Resize);
            ((System.ComponentModel.ISupportInitialize)(this._pictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkTransparency)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnAply;
        private System.Windows.Forms.TextBox txtHandle;
        private System.Windows.Forms.TextBox txtCaption;
        private System.Windows.Forms.TextBox txtClass;
        private System.Windows.Forms.PictureBox _pictureBox;
        private System.Windows.Forms.Label lblFinder;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Label lblHandle;
        private System.Windows.Forms.Label lblCaption;
        private System.Windows.Forms.Label lblClass;
        private System.Windows.Forms.Label lblRectangle;
        private System.Windows.Forms.TextBox txtRectangle;
        private System.Windows.Forms.CheckBox chkCenter;
        private System.Windows.Forms.TrackBar trkTransparency;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.CheckBox chkTransparency;
        private System.Windows.Forms.Label lblTransparencyValue;
        private System.Windows.Forms.Button btnOptions;
        private System.Windows.Forms.ListView lstWindows;
        private System.Windows.Forms.Button btnApp;
        private System.Windows.Forms.Label lblApplications;
    }
}

