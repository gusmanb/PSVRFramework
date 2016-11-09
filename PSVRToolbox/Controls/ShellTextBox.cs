using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Text;
using System.Diagnostics;

namespace PSVRToolbox
{
    
	internal class ShellTextBox : TextBox
	{
		private string prompt = "Report>";
	        private CommandHistory commandHistory = new CommandHistory();
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		internal ShellTextBox()
		{
        	    // This call is required by the Windows.Forms Form Designer.
	            InitializeComponent();
	            printPrompt();
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

                // Overridden to protect against deletion of contents
                // cutting the text and deleting it from the context menu
		protected override void WndProc(ref Message m)
		{
			switch(m.Msg)
			{
				case 0x0302: //WM_PASTE
				case 0x0300: //WM_CUT
				case 0x000C: //WM_SETTEXT
					if (!IsCaretAtWritablePosition())
						MoveCaretToEndOfText();
					break;
				case 0x0303: //WM_CLEAR
					return;
			}
			base.WndProc(ref m);
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// shellTextBox
			// 
			this.BackColor = System.Drawing.Color.Black;
			this.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ForeColor = System.Drawing.Color.LawnGreen;
			this.Location = new System.Drawing.Point(0, 0);
                        this.MaxLength  = 0;
			this.Multiline = true;
			this.Name = "shellTextBox";
		        this.AcceptsTab = true;
		        this.AcceptsReturn = true;
		        this.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.Size = new System.Drawing.Size(400, 176);
			this.TabIndex = 0;
			this.Text = "";
			this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.shellTextBox_KeyPress);
			this.KeyDown += new KeyEventHandler(ShellControl_KeyDown);
			// 
			// ShellControl
			// 
			this.Name = "ShellTextBox";
			this.Size = new System.Drawing.Size(400, 176);
			this.ResumeLayout(false);

		}
		#endregion


		private void printPrompt()
		{
	            string currentText = this.Text;
	            if (currentText.Length != 0 && currentText[currentText.Length - 1] != '\n')
 		    printLine();
                    this.AddText(prompt);
		}

		private void printLine()
		{
			this.AddText(System.Environment.NewLine);
		}

		
	        // Handle Backspace and Enter keys in KeyPress. A bug in .NET 1.1
                // prevents the e.Handled = true from having the desired effect in KeyDown
                private void shellTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			// Handle backspace
			if (e.KeyChar == (char) 8 && IsCaretJustBeforePrompt())
			{
				e.Handled = true;
				return;
			}

			if (IsTerminatorKey(e.KeyChar))
			{
				e.Handled = true;
				string currentCommand = GetTextAtPrompt();
				if (currentCommand.Length != 0)
				{
					printLine();
					((ShellControl)this.Parent).FireCommandEntered(currentCommand);
					commandHistory.Add(currentCommand);
				}
				printPrompt();
			}
		}

		private void ShellControl_KeyDown(object sender, KeyEventArgs e)
		{
			// If the caret is anywhere else, set it back when a key is pressed.
			if (!IsCaretAtWritablePosition() && !(e.Control || IsTerminatorKey(e.KeyCode)))
			{
				MoveCaretToEndOfText();
			}
			
			// Prevent caret from moving before the prompt
			if (e.KeyCode == Keys.Left && IsCaretJustBeforePrompt())
			{
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Down)
			{
				if (commandHistory.DoesNextCommandExist())
				{
					ReplaceTextAtPrompt(commandHistory.GetNextCommand());
				}
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Up )
			{
				if (commandHistory.DoesPreviousCommandExist())
				{
					ReplaceTextAtPrompt(commandHistory.GetPreviousCommand());
				}
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Right)
			{
				// Performs command completion
				string currentTextAtPrompt = GetTextAtPrompt();
				string lastCommand = commandHistory.LastCommand;

				if (lastCommand != null && (currentTextAtPrompt.Length == 0 || lastCommand.StartsWith(currentTextAtPrompt)))
				{
					if (lastCommand.Length > currentTextAtPrompt.Length)
					{
						this.AddText(lastCommand[currentTextAtPrompt.Length].ToString());
					}
				}
			}
		}


		private string GetCurrentLine()
		{
			if (this.Lines.Length >0)
				return (string)this.Lines.GetValue(this.Lines.GetLength(0) - 1);
			else
				return "";
		}

	        private string GetTextAtPrompt()
		{
	            return GetCurrentLine().Substring(prompt.Length);
        	}

		private void ReplaceTextAtPrompt(string text)
		{
			string currentLine = GetCurrentLine();
			int charactersAfterPrompt = currentLine.Length - prompt.Length;
			
			if (charactersAfterPrompt == 0)
				this.AddText(text);
			else
			{
				this.Select(this.TextLength - charactersAfterPrompt, charactersAfterPrompt);
				this.SelectedText = text;
			}
		}

	        private bool IsCaretAtCurrentLine()
        	{
	            return this.TextLength - this.SelectionStart <= GetCurrentLine().Length ;
        	}

	        private void MoveCaretToEndOfText()
        	{
	            this.SelectionStart = this.TextLength;
        	    this.ScrollToCaret();
	        }

		private bool IsCaretJustBeforePrompt()
	        {
        	    return IsCaretAtCurrentLine() && GetCurrentCaretColumnPosition() == prompt.Length;
        	}

	        private int GetCurrentCaretColumnPosition()
        	{
	            string currentLine = GetCurrentLine();
        	    int currentCaretPosition = this.SelectionStart;
	            return (currentCaretPosition - this.TextLength + currentLine.Length);
        	}

	        private bool IsCaretAtWritablePosition()
        	{
	            return IsCaretAtCurrentLine() && GetCurrentCaretColumnPosition() >= prompt.Length;
        	}

	        private void SetPromptText(string val)
        	{
	            string currentLine = GetCurrentLine();
        	    this.Select(0, prompt.Length);
	            this.SelectedText = val;

	            prompt = val;
        	}

	        public string Prompt
        	{
	            get { return prompt; }
        	    set { SetPromptText(value); }
	        }

	        public string[] GetCommandHistory()
        	{
	            return commandHistory.GetCommandHistory();
        	}

	        public void WriteText(string text)
       		{
	            this.AddText(text);
       		}


	        private bool IsTerminatorKey(Keys key)
        	{
	            return key == Keys.Enter;
        	}

		private bool IsTerminatorKey(char keyChar)
		{
			return ((int)keyChar) == 13;
		}
 
		private void AddText(string text)
		{
			this.Text += text;
			MoveCaretToEndOfText();
		}

	}
}
