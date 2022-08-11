using System;
using System.Windows.Forms;

namespace MinishCapTools.Elements.SplitWindow
{
    public partial class HelpWindow : Form
    {
        public HelpWindow()
        {
            InitializeComponent();
        }

        public void Show(string text, IWin32Window parent)
        {
            Text1.Text = text;
            ShowDialog(parent);
        }

        private void OK_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}