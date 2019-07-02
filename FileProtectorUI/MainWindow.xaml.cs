using System;
using System.Windows;
using System.Threading;
using System.Windows.Input;
using System.Windows.Forms;
using Windows.UI.Notifications;
using System.Runtime.InteropServices;
using MahApps.Metro.Controls.Dialogs;
using static FileProtectorUI.CommonResources.Constants;
using System.IO;
using System.Collections;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using FileProtectorUI.Utils;
using System.Collections.ObjectModel;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using Microsoft.Win32;

namespace FileProtectorUI
{
    public partial class MainWindow
    {
        public readonly SynchronizationContext syncCtx;
        private Thread worker;

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
            out ulong Pid,
            out UInt64 MessageId
        );

        [DllImport("FileProtectorCore.dll", CharSet = CharSet.Unicode)]
        public
        static
        extern
        void
        BlockAccess(
            UInt64 MessageId,
            bool Block
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

        private FileProtectorContext fp;
        public string[] Labels { get; set; }
        public SeriesCollection Series;
        public bool isMonitoringActive = true;
        public bool isDefaultBlockingActive = true;
        public bool isInitState = true;

        public MainWindow()
        {
            this.Closed += (sender, e) => { Environment.Exit(0); };
            var a = Marshal.PtrToStringAnsi(function2());
            InitializeComponent();
            PopulateFilesList();

            isMonitoringActive = readFromRegistry("isMonitoring");
            ComboMonitoring.SelectedIndex =  isMonitoringActive ? 0 : 1;
            isDefaultBlockingActive = readFromRegistry("isDefaultBlocking");
            ComboBlock.SelectedIndex = isDefaultBlockingActive ? 0 : 1;
            showIcons();
            isInitState = false;
           
            fp = new FileProtectorContext();
            PopulateHistoryList();
            syncCtx = SynchronizationContext.Current;
            worker = new Thread(NotificationWorker);
            worker.SetApartmentState(ApartmentState.STA);
            //worker.Start();
            InitializeLabels();

        }

        private void showIcons()
        {
            if(isMonitoringActive)
            {
                TextBlockMonitoringRed.Visibility = Visibility.Hidden;
                TextBlockMonitoringGreen.Visibility = Visibility.Visible;
            } else
            {
                TextBlockMonitoringRed.Visibility = Visibility.Visible;
                TextBlockMonitoringGreen.Visibility = Visibility.Hidden;
            }

            if(isDefaultBlockingActive)
            {
                TextBlockBlockGreen.Visibility = Visibility.Visible;
                TextBlockBlockRed.Visibility = Visibility.Hidden;
            } else
            {
                TextBlockBlockGreen.Visibility = Visibility.Hidden;
                TextBlockBlockRed.Visibility = Visibility.Visible;
            }
        }

        private bool readFromRegistry(String key)
        {
            string registryPath = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\fpf";
            String registryKeyValue = (String)Registry.GetValue(registryPath, key, "");
            if (registryKeyValue == "True")
            {
                return true;
            }
            else if (registryKeyValue == "False")
            {
                return false;
            }
            else
            {
                Registry.SetValue(registryPath, key, false.ToString());
                return false;
            }
        }

        private void InitializeLabels()
        {
            var currentTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            Labels = new[]{
            ((Month)((currentTime.Month - 5 + 12) % 12 + 1)).ToString(),
            ((Month)((currentTime.Month - 4 + 12) % 12 + 1)).ToString(),
            ((Month)((currentTime.Month - 3 + 12) % 12 + 1)).ToString(),
            ((Month)((currentTime.Month - 2 + 12) % 12 + 1)).ToString(),
            ((Month)currentTime.Month).ToString()
            };

            Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Values = new ChartValues<HistoryEntry>(fp.HistoryEntries.Local.ToList())
                }

            };
        }

        private void NotificationWorker()
        {
            while (true)
            {
                string path = null;
                ulong pid = 0;
                UInt64 messageId;
                IntPtr ptr;
                if (isMonitoringActive)
                {
                    GetNextNotification(out ptr, out pid, out messageId);
                    if (ptr == IntPtr.Zero)
                    {
                        continue;
                    }
                    path = System.Runtime.InteropServices.Marshal.PtrToStringUni(ptr);
                    bool allow = false;
                    if (isDefaultBlockingActive)
                    {
                        allow = false;
                    }
                    else
                    {
                        allow = ShowToastNotification(path, pid);
                    }
                    BlockAccess(messageId, !allow);
                    FreeNotification(ptr);
                }
                else
                {
                    GetNextNotification(out ptr, out pid, out messageId);
                    if (ptr == IntPtr.Zero)
                    {
                        continue;
                    }
                    bool allow = true;
                    BlockAccess(messageId, !allow);
                    FreeNotification(ptr);
                }
            }
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
                ProtectedFiles.AddFile(openFileDlg.FileName);
                var path = "\\??\\" + openFileDlg.FileName;
                ProtectFile(path, (ushort)path.Length);
            }
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

        private void PopulateHistoryList()
        {
            fp.HistoryEntries.Load();
            historyList.ItemsSource = fp.HistoryEntries.Local.ToBindingList();
        }

        private void StackPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var panel = sender as System.Windows.Controls.StackPanel;
        }

        private bool ShowToastNotification(string path, ulong pid)
        {
            var xml = $@"<toast>
                <visual>
                    <binding template='ToastGeneric'>
                        <text>File Protector</text>
                        <text>{Process.GetProcessById((int)pid).ProcessName} is trying to access your protected file: {path}.</text>
                    </binding>
                </visual>
                <actions>
                    <action arguments = 'More' content = 'More'/>
                    <action arguments = 'Deny' content = 'Deny'/>
                </actions>
            </toast>";
            var toastXml = new Windows.Data.Xml.Dom.XmlDocument();
            toastXml.LoadXml(xml);
            var toast = new ToastNotification(toastXml);
            bool allowed = false;
            ManualResetEvent evt = new ManualResetEvent(false);
            toast.Activated += (notification, esf) =>
            {
                var arg = esf as ToastActivatedEventArgs;
                var args = arg.Arguments;

                switch (args)
                {
                    case "Deny":
                        break;
                    case "More":
                        allowed = InputPasswordMessageBoxLaunch(path, pid);
                        break;
                    default:
                        break;
                }
                evt.Set();
            };
            toast.Failed += (notification, args) =>
            {
                allowed = false;
                evt.Set();
            };
            toast.Dismissed += (notification, args) =>
            {
                allowed = false;
                evt.Set();
            };
            var t = ToastNotificationManager.CreateToastNotifier(APP_ID);
            t.Show(toast);
            evt.WaitOne();

            App.Current.Dispatcher.Invoke((Action)delegate
            {
                fp.Add(new HistoryEntry
                {
                    ProcessId = pid.ToString(),
                    Path = path,
                    Allowed = allowed,
                    ProcessName = Process.GetProcessById((int)pid).ProcessName,
                    TimeAccessed = DateTime.Now
                });
                fp.SaveChanges();
            });


            return allowed;
        }

        private bool InputPasswordMessageBoxLaunch(string path, ulong pid)
        {
            IntPtr hOldDesktop = GetThreadDesktop(GetCurrentThreadId());
            IntPtr pNewDesktop = CreateDesktop("NewDesktop", IntPtr.Zero, IntPtr.Zero, 0, (uint)DESKTOP_ACCESS.GENERIC_ALL, IntPtr.Zero);

            SwitchDesktop(pNewDesktop);

            bool allow = false;
            Thread t = new Thread(() =>
            {
                SetThreadDesktop(pNewDesktop);

                Form loginWnd = new Form();
                loginWnd.AutoSize = true;
                Button allowButton = new Button();
                allowButton.Text = "Allow";
                allowButton.Click += (e, args) =>
                {
                    allow = true;
                    loginWnd.Close();
                };
                allowButton.Location = new System.Drawing.Point(100, 100);

                Button denyButton = new Button();
                denyButton.Text = "Deny";
                denyButton.Click += (e, args) =>
                {
                    allow = false;
                    loginWnd.Close();
                };
                denyButton.Location = new System.Drawing.Point(0, 100);

                Label message = new Label();
                message.Location = new System.Drawing.Point(0, 0);
                message.Text = $"Process {Process.GetProcessById((int)pid).ProcessName} with PID: {pid} is trying to access your file: {path}. Allow Access?";
                message.AutoSize = true;

                loginWnd.Controls.Add(message);
                loginWnd.Controls.Add(allowButton);
                loginWnd.Controls.Add(denyButton);

                loginWnd.ShowDialog();
            });
            t.SetApartmentState(ApartmentState.STA);

            t.Start();
            t.Join();

            SwitchDesktop(hOldDesktop);
            SetThreadDesktop(hOldDesktop);
            CloseDesktop(pNewDesktop);
            return allow;
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

        private void ClearHistory(object sender, RoutedEventArgs e)
        {
            foreach (var entry in fp.HistoryEntries)
            {
                fp.Remove(entry);
            }
            fp.SaveChanges();
        }

        private void MonitoringSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!isInitState)
            {
                string registryPath = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\fpf";
                if (ComboMonitoring.Text.Equals("Active"))
                {
                    isMonitoringActive = false;
                    TextBlockMonitoringRed.Visibility = Visibility.Visible;
                    TextBlockMonitoringGreen.Visibility = Visibility.Hidden;
                }
                else
                {
                    isMonitoringActive = true;
                    TextBlockMonitoringRed.Visibility = Visibility.Hidden;
                    TextBlockMonitoringGreen.Visibility = Visibility.Visible;
                }
                Registry.SetValue(registryPath, "isMonitoring", isMonitoringActive.ToString());
            }
        }

        private void BlockingSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!isInitState)
            {
                string registryPath = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\fpf";
                if (ComboBlock.Text.Equals("Yes"))
                {
                    isDefaultBlockingActive = false;
                    TextBlockBlockGreen.Visibility = Visibility.Hidden;
                    TextBlockBlockRed.Visibility = Visibility.Visible;
                }
                else
                {
                    isDefaultBlockingActive = true;
                    TextBlockBlockGreen.Visibility = Visibility.Visible;
                    TextBlockBlockRed.Visibility = Visibility.Hidden;
                }
                Registry.SetValue(registryPath, "isDefaultBlocking", isDefaultBlockingActive.ToString());
            }
        }
    }
}
