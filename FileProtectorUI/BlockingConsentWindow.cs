using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileProtectorUI
{
    public partial class BlockingConsentWindow : Form
    {
        public BlockingConsentWindow(String processName, ulong processPid, String path)
        {
            InitializeComponent();
            AllowButton = allowButton;
            DenyButton = denyBtn;
            this.CenterToScreen();

            pidLabel.Text = processPid.ToString();
            processNameLabel.Text = processName;
            pathLabel.Text = path;
        }

        public Button AllowButton;
        public Button DenyButton;

        private void btnClose_MouseHover(object sender, EventArgs e)
        {
            btnClose.BackColor = Color.Red;
        }

        private void btnClose_MouseLeave(object sender, EventArgs e)
        {
            btnClose.BackColor = Color.FromArgb(0, 100, 150);
        }

        private void btnClose_MouseEnter(object sender, EventArgs e)
        {
            btnClose.BackColor = Color.Red;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
