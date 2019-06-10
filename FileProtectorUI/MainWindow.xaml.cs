using System;
using System.Windows;
using System.Threading;
using System.Windows.Input;
using System.Windows.Forms;
using Windows.UI.Notifications;
using System.Runtime.InteropServices;
using MahApps.Metro.Controls.Dialogs;
using static FileProtectorUI.CommonResources.Constants;

namespace FileProtectorUI
{
    public partial class MainWindow
    {
        [DllImport("FileProtectorCore.dll", CharSet = CharSet.Auto)]
        public
        static
        extern
        IntPtr
        function2();

        [DllImport("FileProtectorCore.dll", CharSet = CharSet.Unicode)]
        public
        static
        extern
        IntPtr
        ProtectFile(
            String Path,
            ushort Length
        );

        [DllImport("FileProtectorCore.dll", CharSet = CharSet.Unicode)]
        public
        static
        extern
        void
        GetNextNotification(
            out IntPtr Path,
            out ulong Pid
        );

        [DllImport("FileProtectorCore.dll", CharSet = CharSet.Unicode)]
        public
        static
        extern
        void
        FreeNotification(
             IntPtr Path
        );

        [DllImport("user32.dll")]
        public static extern IntPtr CreateDesktop(string lpszDesktop, IntPtr lpszDevice, IntPtr pDevmode, int dwFlags, uint dwDesiredAccess, IntPtr lpsa);

        [DllImport("user32.dll")]
        private static extern bool SwitchDesktop(IntPtr hDesktop);

        [DllImport("user32.dll")]
        public static extern bool CloseDesktop(IntPtr handle);

        [DllImport("user32.dll")]
        public static extern bool SetThreadDesktop(IntPtr hDesktop);

        [DllImport("user32.dll")]
        public static extern IntPtr GetThreadDesktop(int dwThreadId);

        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();

        enum DESKTOP_ACCESS : uint
        {
            DESKTOP_NONE = 0,
            DESKTOP_READOBJECTS = 0x0001,
            DESKTOP_CREATEWINDOW = 0x0002,
            DESKTOP_CREATEMENU = 0x0004,
            DESKTOP_HOOKCONTROL = 0x0008,
            DESKTOP_JOURNALRECORD = 0x0010,
            DESKTOP_JOURNALPLAYBACK = 0x0020,
            DESKTOP_ENUMERATE = 0x0040,
            DESKTOP_WRITEOBJECTS = 0x0080,
            DESKTOP_SWITCHDESKTOP = 0x0100,

            GENERIC_ALL = (DESKTOP_READOBJECTS | DESKTOP_CREATEWINDOW | DESKTOP_CREATEMENU |
                            DESKTOP_HOOKCONTROL | DESKTOP_JOURNALRECORD | DESKTOP_JOURNALPLAYBACK |
                            DESKTOP_ENUMERATE | DESKTOP_WRITEOBJECTS | DESKTOP_SWITCHDESKTOP),
        }

        public MainWindow()
        {
            var a = Marshal.PtrToStringAnsi(function2());
            InitializeComponent();
            PopulateFilesList();
            string path = null;
            ulong pid = 0;
            IntPtr ptr;
            GetNextNotification(out ptr, out pid);
            path = System.Runtime.InteropServices.Marshal.PtrToStringUni(ptr);
            FreeNotification(ptr);
            Console.WriteLine(path);
        }

        private void ReadProtectedPaths()
        {
            //Registry.GetValue
        }

        private void BrowseButtonClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();

            // Set initial directory    
            //openFileDlg.InitialDirectory = @"C:\Users\";

            // create _syncContext on application startup
            //var _syncContext = SynchronizationContext.Current;

            // whenever you do UI shit, schedule on syncContext
            //_syncContext.Send(o => { return; }, null);

            openFileDlg.Multiselect = true;
            openFileDlg.Filter = "All files (*.*)|*.*";

            Nullable<bool> result = openFileDlg.ShowDialog();
            if (result == true)
            {
                //add it in the table
            }
            ProtectedFiles.AddFile(openFileDlg.FileName);
            var path = "\\??\\" + openFileDlg.FileName;
            ProtectFile(path, (ushort)path.Length);
        }


        private async void RemoveButtonClick(object sender, RoutedEventArgs e)
        {
            FileDTO selectedFile = filesList.SelectedItem as FileDTO;
            if (selectedFile != null)
            {
                ProtectedFiles.RemoveFile(selectedFile);
            }
            else
            {
                await this.ShowMessageAsync("No file was selected", "Please go back and select a file to be removed");
            }
        }

        private void PopulateFilesList()
        {
            ProtectedFiles.GetProtectedFilesFromRegistryKey();
            filesList.ItemsSource = ProtectedFiles.files;
        }

        private void StackPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var panel = sender as System.Windows.Controls.StackPanel;
        }

        private void ShowToastNotification(object sender, RoutedEventArgs e)
        {
            var xml = @"<toast>
                <visual>
                    <binding template='ToastGeneric'>
                        <text>File Protector</text>
                        <text>Someone is trying to access your protected file: *fileName*.</text>
                    </binding>
                </visual>
                <actions>
                    <action arguments = 'Allow' content = 'Allow'/>
                    <action arguments = 'Deny' content = 'Deny'/>
                </actions>
            </toast>";
            var toastXml = new Windows.Data.Xml.Dom.XmlDocument();
            toastXml.LoadXml(xml);
            var toast = new ToastNotification(toastXml);
            toast.Activated += (notification, esf) =>
            {
                var arg = esf as ToastActivatedEventArgs;
                var args = arg.Arguments;

                switch(args)
                {
                    case "Deny":
                        break;
                    case "Allow":
                        var passw = InputPasswordMessageBoxLaunch();
                        break;
                    default:
                        break;
                }
                //TODO based on args, solve the buttons
                Console.WriteLine("a");
            };
            var t = ToastNotificationManager.CreateToastNotifier(APP_ID);
            t.Show(toast);
        }

        private string InputPasswordMessageBoxLaunch()
        {
            IntPtr hOldDesktop = GetThreadDesktop(GetCurrentThreadId());
            IntPtr pNewDesktop = CreateDesktop("NewDesktop", IntPtr.Zero, IntPtr.Zero, 0, (uint)DESKTOP_ACCESS.GENERIC_ALL, IntPtr.Zero);

            SwitchDesktop(pNewDesktop);

            string passwd = "";
            Thread t = new Thread(() => {
                SetThreadDesktop(pNewDesktop);

                Form loginWnd = new Form();
                TextBox passwordTextBox = new TextBox();
                passwordTextBox.Location = new System.Drawing.Point(10, 30);
                passwordTextBox.Width = 250;
                //passwordTextBox.Font = new Font("Arial", 20, FontStyle.Regular);

                loginWnd.Controls.Add(passwordTextBox);
                loginWnd.FormClosing += (sndr, evt) => { passwd = passwordTextBox.Text; };
                loginWnd.ShowDialog();
            });
            t.SetApartmentState(ApartmentState.STA);

            t.Start();
            t.Join();

            SwitchDesktop(hOldDesktop);
            SetThreadDesktop(hOldDesktop);
            CloseDesktop(pNewDesktop);
            return passwd;
        }

        private void StackPanelMyFilesMouseDown(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => mainTabControl.SelectedIndex = 1));
        }

        private void StackPanelHistoryMouseDown(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => mainTabControl.SelectedIndex = 2));
        }

        private void StackPanelStatisticsMouseDown(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => mainTabControl.SelectedIndex = 3));
        }

        private void StackPanelSettingsMouseDown(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => mainTabControl.SelectedIndex = 4));
        }

        private void StackPanelHelpMouseDown(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => mainTabControl.SelectedIndex = 5));
        }

        private void StackPanelCloseMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}
