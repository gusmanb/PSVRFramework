using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace PSVRToolbox
{
	/// <summary>
	/// Summary description for ShellControl.
	/// </summary>
	public class ShellControl : System.Windows.Forms.UserControl
	{
		private ShellTextBox shellTextBox;
		public event EventCommandEntered CommandEntered;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

        public override void ResetText()
        {
            shellTextBox.Text = "";
            WritePrompt();
        }

        public ShellControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
			

			// TODO: Add any initialization after the InitializeComponent call
		}

		internal void FireCommandEntered(string command)
		{
			OnCommandEntered(command);
		}

		protected virtual void OnCommandEntered(string command)
		{
			if (CommandEntered != null)
				CommandEntered(command, new CommandEnteredEventArgs(command));
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		public Color ShellTextForeColor
		{
			get { return shellTextBox != null ? shellTextBox.ForeColor : Color.Green; }
			set 
			{
				if (shellTextBox != null)
					shellTextBox.ForeColor = value;
			}
		}

		public Color ShellTextBackColor
		{
			get { return shellTextBox != null ? shellTextBox.BackColor:Color.Black; }
			set 
			{ 
				if (shellTextBox != null)
					shellTextBox.BackColor = value; 
			}
		}

		public Font ShellTextFont
		{
			get { return shellTextBox != null ? shellTextBox.Font: new Font("Tahoma", 8); }
			set 
			{
				if (shellTextBox != null)
					shellTextBox.Font = value; 
			}
		}

		public void Clear()
		{
			shellTextBox.Clear();
		}

		public void WriteText(string text)
		{
			shellTextBox.WriteText(text);
		}

        public void WriteLine(string text)
        {
         shellTextBox.WriteText(text + "\r\n");
        }

        public void WritePrompt()
        {
            shellTextBox.WriteText(Prompt);
        }

        public string[] GetCommandHistory()
		{
			return shellTextBox.GetCommandHistory();
		}


		public string Prompt
		{
			get { return shellTextBox.Prompt; }
			set { shellTextBox.Prompt = value; }
		}


		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.shellTextBox = new ShellTextBox();
			this.SuspendLayout();
			// 
			// shellTextBox
			// 
			this.shellTextBox.AcceptsReturn = true;
			this.shellTextBox.AcceptsTab = true;
			this.shellTextBox.BackColor = System.Drawing.Color.Black;
			this.shellTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.shellTextBox.ForeColor = System.Drawing.Color.LawnGreen;
			this.shellTextBox.Location = new System.Drawing.Point(0, 0);
			this.shellTextBox.Multiline = true;
			this.shellTextBox.Name = "shellTextBox";
			this.shellTextBox.Prompt = "Report>";
			this.shellTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.shellTextBox.BackColor = System.Drawing.Color.Black;
			this.shellTextBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.shellTextBox.ForeColor = System.Drawing.Color.LawnGreen;
			this.shellTextBox.Size = new System.Drawing.Size(232, 216);
			this.shellTextBox.TabIndex = 0;
			this.shellTextBox.Text = "";
			// 
			// ShellControl
			// 
			this.Controls.Add(this.shellTextBox);
			this.Name = "ShellControl";
			this.Size = new System.Drawing.Size(232, 216);
			this.ResumeLayout(false);

		}
		#endregion
	}

	public class CommandEnteredEventArgs : EventArgs
	{
		string command;
		public CommandEnteredEventArgs(string command)
		{
			this.command = command;
		}

		public string Command
		{
			get { return command; }
		}
	}

	public delegate void EventCommandEntered(object sender, CommandEnteredEventArgs e);

}
