namespace PSVRToolbox
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            this.detectTimer = new System.Windows.Forms.Timer(this.components);
            this.lblStatus = new System.Windows.Forms.Label();
            this.grpFunctions = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.grpFunctions.SuspendLayout();
            this.SuspendLayout();
            // 
            // detectTimer
            // 
            this.detectTimer.Enabled = true;
            this.detectTimer.Interval = 1000;
            this.detectTimer.Tick += new System.EventHandler(this.detectTimer_Tick);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 9);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(102, 13);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "Waiting for PS VR...";
            // 
            // grpFunctions
            // 
            this.grpFunctions.Controls.Add(this.button7);
            this.grpFunctions.Controls.Add(this.button6);
            this.grpFunctions.Controls.Add(this.button5);
            this.grpFunctions.Controls.Add(this.button4);
            this.grpFunctions.Controls.Add(this.button3);
            this.grpFunctions.Controls.Add(this.button2);
            this.grpFunctions.Controls.Add(this.button1);
            this.grpFunctions.Enabled = false;
            this.grpFunctions.Location = new System.Drawing.Point(12, 44);
            this.grpFunctions.Name = "grpFunctions";
            this.grpFunctions.Size = new System.Drawing.Size(179, 227);
            this.grpFunctions.TabIndex = 1;
            this.grpFunctions.TabStop = false;
            this.grpFunctions.Text = "Functions";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 19);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(167, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Headset on";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(6, 48);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(167, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Headset off";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(6, 77);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(167, 23);
            this.button3.TabIndex = 2;
            this.button3.Text = "Enable tracking and VR";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(6, 106);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(167, 23);
            this.button4.TabIndex = 3;
            this.button4.Text = "Enable VR";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(6, 135);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(167, 23);
            this.button5.TabIndex = 4;
            this.button5.Text = "Enable Theater";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(6, 164);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(167, 23);
            this.button6.TabIndex = 5;
            this.button6.Text = "Recenter";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(6, 193);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(167, 23);
            this.button7.TabIndex = 6;
            this.button7.Text = "Shutdown box";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(203, 281);
            this.Controls.Add(this.grpFunctions);
            this.Controls.Add(this.lblStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.Text = "PSVRToolbox";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.grpFunctions.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer detectTimer;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.GroupBox grpFunctions;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
    }
}

