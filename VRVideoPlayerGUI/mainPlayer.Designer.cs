namespace VRVideoPlayerGUI
{
    partial class mainPlayer
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(mainPlayer));
            this.button1 = new System.Windows.Forms.Button();
            this.cbVideoMode = new System.Windows.Forms.ComboBox();
            this.cbFrameMode = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.cbMonitor = new System.Windows.Forms.ComboBox();
            this.cbVFOV = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(15, 122);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(236, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Open video";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // cbVideoMode
            // 
            this.cbVideoMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbVideoMode.FormattingEnabled = true;
            this.cbVideoMode.Items.AddRange(new object[] {
            "180 Mono",
            "180 Stereo",
            "360 Mono",
            "360 Stereo"});
            this.cbVideoMode.Location = new System.Drawing.Point(89, 12);
            this.cbVideoMode.Name = "cbVideoMode";
            this.cbVideoMode.Size = new System.Drawing.Size(162, 21);
            this.cbVideoMode.TabIndex = 1;
            // 
            // cbFrameMode
            // 
            this.cbFrameMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbFrameMode.FormattingEnabled = true;
            this.cbFrameMode.Items.AddRange(new object[] {
            "Side by side",
            "Up to down"});
            this.cbFrameMode.Location = new System.Drawing.Point(89, 41);
            this.cbFrameMode.Name = "cbFrameMode";
            this.cbFrameMode.Size = new System.Drawing.Size(162, 21);
            this.cbFrameMode.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Video mode";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Frame mode";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 98);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(42, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Monitor";
            // 
            // cbMonitor
            // 
            this.cbMonitor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbMonitor.FormattingEnabled = true;
            this.cbMonitor.Location = new System.Drawing.Point(89, 95);
            this.cbMonitor.Name = "cbMonitor";
            this.cbMonitor.Size = new System.Drawing.Size(162, 21);
            this.cbMonitor.TabIndex = 9;
            // 
            // cbVFOV
            // 
            this.cbVFOV.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbVFOV.FormattingEnabled = true;
            this.cbVFOV.Items.AddRange(new object[] {
            "360",
            "180",
            "160"});
            this.cbVFOV.Location = new System.Drawing.Point(89, 68);
            this.cbVFOV.Name = "cbVFOV";
            this.cbVFOV.Size = new System.Drawing.Size(162, 21);
            this.cbVFOV.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 71);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(66, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Vertical FOV";
            // 
            // mainPlayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(263, 161);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cbVFOV);
            this.Controls.Add(this.cbMonitor);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbFrameMode);
            this.Controls.Add(this.cbVideoMode);
            this.Controls.Add(this.button1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "mainPlayer";
            this.Text = "PSVR Player";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox cbVideoMode;
        private System.Windows.Forms.ComboBox cbFrameMode;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cbMonitor;
        private System.Windows.Forms.ComboBox cbVFOV;
        private System.Windows.Forms.Label label5;
    }
}

