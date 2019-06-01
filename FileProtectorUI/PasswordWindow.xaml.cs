using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System;
using System.Windows;
using System.Drawing;
using System.Windows.Input;
using Windows.UI.Notifications;
using System.Runtime.InteropServices;
using MahApps.Metro.Controls.Dialogs;
using static FileProtectorUI.CommonResources.Constants;
using System.Threading;

namespace FileProtectorUI
{
    /// <summary>
    /// Interaction logic for PasswordWindow.xaml
    /// </summary>
    public partial class PasswordWindow : Window
    {

        public PasswordWindow(IntPtr pNewDesktop, IntPtr hOldDesktop)
        {
            InitializeComponent();
            this.pNewDesktop = pNewDesktop;
            this.hOldDesktop = hOldDesktop;
            this.signalEvent = new ManualResetEvent(false);
        }

        private IntPtr pNewDesktop;
        private IntPtr hOldDesktop;
        public ManualResetEvent signalEvent;

        [DllImport("user32.dll")]
        private static extern bool SwitchDesktop(IntPtr hDesktop);

        [DllImport("user32.dll")]
        public static extern bool CloseDesktop(IntPtr handle);

        private void OkHandler(object sender, RoutedEventArgs e)
        {
            //SwitchDesktop(hOldDesktop);
            //CloseDesktop(pNewDesktop);
            this.signalEvent.Set();
            this.Close();
        }
    }
}
