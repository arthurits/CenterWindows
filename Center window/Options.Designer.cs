namespace Center_window
{
    partial class Options
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
            this.BtnOk = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.updWidth = new System.Windows.Forms.NumericUpDown();
            this.lblWidth = new System.Windows.Forms.Label();
            this.chkParent = new System.Windows.Forms.CheckBox();
            this.lblParent = new System.Windows.Forms.Label();
            this.officeColorPicker = new OfficePickers.ColorPicker.OfficeColorPicker();
            this.lblColor = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.updWidth)).BeginInit();
            this.SuspendLayout();
            // 
            // BtnOk
            // 
            this.BtnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.BtnOk.Location = new System.Drawing.Point(13, 333);
            this.BtnOk.Margin = new System.Windows.Forms.Padding(4);
            this.BtnOk.Name = "BtnOk";
            this.BtnOk.Size = new System.Drawing.Size(100, 28);
            this.BtnOk.TabIndex = 0;
            this.BtnOk.Text = "&OK";
            this.BtnOk.UseVisualStyleBackColor = true;
            this.BtnOk.Click += new System.EventHandler(this.BtnOk_Click);
            // 
            // BtnCancel
            // 
            this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BtnCancel.Location = new System.Drawing.Point(121, 333);
            this.BtnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(100, 28);
            this.BtnCancel.TabIndex = 1;
            this.BtnCancel.Text = "&Cancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            // 
            // updWidth
            // 
            this.updWidth.Location = new System.Drawing.Point(148, 75);
            this.updWidth.Margin = new System.Windows.Forms.Padding(4);
            this.updWidth.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.updWidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.updWidth.Name = "updWidth";
            this.updWidth.Size = new System.Drawing.Size(51, 22);
            this.updWidth.TabIndex = 2;
            this.updWidth.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblWidth
            // 
            this.lblWidth.AutoSize = true;
            this.lblWidth.Location = new System.Drawing.Point(28, 78);
            this.lblWidth.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblWidth.Name = "lblWidth";
            this.lblWidth.Size = new System.Drawing.Size(102, 16);
            this.lblWidth.TabIndex = 3;
            this.lblWidth.Text = "Rectangle width";
            // 
            // chkParent
            // 
            this.chkParent.AutoSize = true;
            this.chkParent.Location = new System.Drawing.Point(148, 32);
            this.chkParent.Margin = new System.Windows.Forms.Padding(4);
            this.chkParent.Name = "chkParent";
            this.chkParent.Size = new System.Drawing.Size(15, 14);
            this.chkParent.TabIndex = 4;
            this.chkParent.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkParent.UseVisualStyleBackColor = true;
            // 
            // lblParent
            // 
            this.lblParent.AutoSize = true;
            this.lblParent.Location = new System.Drawing.Point(28, 32);
            this.lblParent.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblParent.Name = "lblParent";
            this.lblParent.Size = new System.Drawing.Size(75, 16);
            this.lblParent.TabIndex = 5;
            this.lblParent.Text = "Only parent";
            // 
            // officeColorPicker
            // 
            this.officeColorPicker.AutoSize = true;
            this.officeColorPicker.BackColor = System.Drawing.SystemColors.Control;
            this.officeColorPicker.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.officeColorPicker.Color = System.Drawing.Color.Black;
            this.officeColorPicker.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.officeColorPicker.Location = new System.Drawing.Point(41, 151);
            this.officeColorPicker.Margin = new System.Windows.Forms.Padding(4);
            this.officeColorPicker.Name = "officeColorPicker";
            this.officeColorPicker.Size = new System.Drawing.Size(147, 139);
            this.officeColorPicker.TabIndex = 6;
            // 
            // lblColor
            // 
            this.lblColor.AutoSize = true;
            this.lblColor.Location = new System.Drawing.Point(28, 119);
            this.lblColor.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblColor.Name = "lblColor";
            this.lblColor.Size = new System.Drawing.Size(102, 16);
            this.lblColor.TabIndex = 7;
            this.lblColor.Text = "Rectangle color";
            // 
            // Options
            // 
            this.AcceptButton = this.BtnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.BtnCancel;
            this.ClientSize = new System.Drawing.Size(235, 374);
            this.Controls.Add(this.lblColor);
            this.Controls.Add(this.lblParent);
            this.Controls.Add(this.chkParent);
            this.Controls.Add(this.lblWidth);
            this.Controls.Add(this.updWidth);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOk);
            this.Controls.Add(this.officeColorPicker);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Options";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Options";
            ((System.ComponentModel.ISupportInitialize)(this.updWidth)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnOk;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.NumericUpDown updWidth;
        private System.Windows.Forms.Label lblWidth;
        private System.Windows.Forms.CheckBox chkParent;
        private System.Windows.Forms.Label lblParent;
        private OfficePickers.ColorPicker.OfficeColorPicker officeColorPicker;
        private System.Windows.Forms.Label lblColor;
    }
}