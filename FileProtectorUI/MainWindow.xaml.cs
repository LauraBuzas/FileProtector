using System;
using System.Windows;
using System.Windows.Input;
using Windows.UI.Notifications;
using System.Runtime.InteropServices;
using MahApps.Metro.Controls.Dialogs;
using static FileProtectorUI.CommonResources.Constants;

namespace FileProtectorUI
{
    public partial class MainWindow
    {

        [DllImport("FileProtectorCore.dll")]
        static extern int function();

        [DllImport("FileProtectorCore.dll", CharSet = CharSet.Auto)]
        public
        static
        extern
        IntPtr
        function2();

        public MainWindow()
        {
            var a = Marshal.PtrToStringAnsi(function2());
            InitializeComponent();
            protectedFiles = new ProtectedFiles();
            PopulateFilesList();
        }

        private ProtectedFiles protectedFiles;

        private void BrowseButtonClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();

            // Set initial directory    
            //openFileDlg.InitialDirectory = @"C:\Users\";

            openFileDlg.Multiselect = true;
            openFileDlg.Filter = "All files (*.*)|*.*";

            Nullable<bool> result = openFileDlg.ShowDialog();
            if (result == true)
            {
                //add it in the table
            }
        }

        private async void RemoveButtonClick(object sender, RoutedEventArgs e)
        {
            FileDTO selectedFile = filesList.SelectedItem as FileDTO;
            if (selectedFile != null)
            {
                //TODO remove file from db/list
            }
            else
            {
                await this.ShowMessageAsync("No file was selected", "Please go back and select a file to be removed");
            }
        }

        private void PopulateFilesList()
        {
            filesList.ItemsSource = protectedFiles.GetProtectedFiles();
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
                    <action arguments = 'Allow' content = 'Allow' />
                    <action arguments = 'Deny' content = 'Deny' />
                </actions>
            </toast>";
            var toastXml = new Windows.Data.Xml.Dom.XmlDocument();
            toastXml.LoadXml(xml);
            var toast = new ToastNotification(toastXml);
            toast.Activated += (notification, esf) =>
            {
                //TODO based on args, solve the buttons
                Console.WriteLine("a");
            };
            var t = ToastNotificationManager.CreateToastNotifier(APP_ID);
            t.Show(toast);
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

        private void StackPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            Console.WriteLine();
        }
    }
}
