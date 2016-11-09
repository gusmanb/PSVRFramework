using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PSVRToolbox.Controls
{
    public class HextTextBox : TextBox
    {
        bool skip = false;

        protected override void OnTextChanged(EventArgs e)
        {

            if (skip || string.IsNullOrWhiteSpace(Text) || SelectionStart == 0)
                return;

            var lCount = Lines.Length;
            var lNum = SelectionStart;
            var line = GetLineFromCharIndex(SelectionStart);
            string clearText = JointLines;
            int pos = SelectionStart - (line * 2 + 1);
            string chr = clearText.Substring(pos, 1);

            byte n;
            if (!byte.TryParse(chr, System.Globalization.NumberStyles.HexNumber, System.Globalization.NumberFormatInfo.CurrentInfo, out n) &&
              Text != String.Empty)
                clearText = clearText.Remove(pos, 1);

            List<string> lines = new List<string>();

            while (clearText.Length > 32)
            {
                lines.Add(clearText.Substring(0, 32));
                clearText = clearText.Remove(0, 32);
            }

            if (!string.IsNullOrWhiteSpace(clearText))
                lines.Add(clearText);

            skip = true;
            Lines = lines.ToArray();
            skip = false;

            SelectionStart = lNum + ((Lines.Length - lCount) * 2);

            base.OnTextChanged(e);
            
        }
        
        public string JointLines { get { return string.Concat(Lines).Replace("\r\n", ""); } }
    }
}
