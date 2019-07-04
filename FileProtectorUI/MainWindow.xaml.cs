using System;
using System.Windows;
using System.Threading;
using System.Windows.Input;
using System.Windows.Forms;
using Windows.UI.Notifications;
using System.Runtime.InteropServices;
using MahApps.Metro.Controls.Dialogs;
using static FileProtectorUI.CommonResources.Constants;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using FileProtectorUI.Utils;
using LiveCharts;
using LiveCharts.Wpf;
using System.Linq;
using Microsoft.Win32;
using System.Collections.Generic;
using FileProtectorUI.CommonResources;
using System.Drawing;
using System.Drawing.Imaging;

namespace FileProtectorUI
{
    public partial class MainWindow
    {
        public readonly SynchronizationContext syncCtx;
        private Thread worker;
        private Dictionary<ulong, List<Tuple<string, Int32, bool>>> cache = new Dictionary<ulong, List<Tuple<string, int, bool>>>();
        private NotifyIcon Icon;


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
        public SeriesCollection Series;
        public bool isMonitoringActive = true;
        public bool isDefaultBlockingActive = true;
        public bool isInitState = true;

        public MainWindow()
        {
            this.Closed += (sender, e) => { Environment.Exit(0); };
            InitializeComponent();
            PopulateFilesList();

            isMonitoringActive = readFromRegistry("isMonitoring");
            ComboMonitoring.SelectedIndex = isMonitoringActive ? 0 : 1;
            isDefaultBlockingActive = readFromRegistry("isDefaultBlocking");
            ComboBlock.SelectedIndex = isDefaultBlockingActive ? 0 : 1;
            showIcons();
            isInitState = false;

            fp = new FileProtectorContext();
            PopulateHistoryList();
            syncCtx = SynchronizationContext.Current;
            worker = new Thread(NotificationWorker);
            worker.SetApartmentState(ApartmentState.STA);
            worker.Start();
            InitializeLabels();
            createTray();
            Closing += OnWindowClosing;
            this.ResizeMode = ResizeMode.CanMinimize;

        }

        void createTray()
        {
            Icon = new NotifyIcon
            {
                Icon = new System.Drawing.Icon("lock1.ico"),
                Visible = true
            };

            Icon.Text = "File Protector";
            var contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.AutoSize = true;
            contextMenuStrip.ShowCheckMargin = true;
            var item = contextMenuStrip.Items.Add("Exit");
            item.Click += (s, e) =>
            {
                Closing -= OnWindowClosing; Environment.Exit(0);
            };
            var contextMenu = new System.Windows.Forms.ContextMenu();
            Icon.ContextMenuStrip = contextMenuStrip;

            Icon.DoubleClick += (s, args) =>
            {
                if (IsVisible)
                {
                    Activate();
                }
                else
                {
                    Show();
                }

                if (this.WindowState == WindowState.Minimized)
                {
                    this.WindowState = WindowState.Normal;
                }
            };
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        protected override void OnClosed(EventArgs e)
        {
            Icon.Dispose();
        }

        private void showIcons()
        {
            if (isMonitoringActive)
            {
                TextBlockMonitoringRed.Visibility = Visibility.Hidden;
                TextBlockMonitoringGreen.Visibility = Visibility.Visible;
            }
            else
            {
                TextBlockMonitoringRed.Visibility = Visibility.Visible;
                TextBlockMonitoringGreen.Visibility = Visibility.Hidden;
            }

            if (isDefaultBlockingActive)
            {
                TextBlockBlockGreen.Visibility = Visibility.Visible;
                TextBlockBlockRed.Visibility = Visibility.Hidden;
            }
            else
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
            var monthLabels = new[]{
            ((Month)((currentTime.Month - 5 + 12) % 12 + 1)).ToString(),
            ((Month)((currentTime.Month - 4 + 12) % 12 + 1)).ToString(),
            ((Month)((currentTime.Month - 3 + 12) % 12 + 1)).ToString(),
            ((Month)((currentTime.Month - 2 + 12) % 12 + 1)).ToString(),
            ((Month)currentTime.Month).ToString()
            };


            this.StatsChart.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "allowed",
                    Values = new ChartValues<double> {
                        getHistoryEntries((currentTime.Month - 5 + 12) % 12 + 1, true),
                        getHistoryEntries((currentTime.Month - 4 + 12) % 12 + 1, true),
                        getHistoryEntries((currentTime.Month - 3 + 12) % 12 + 1, true),
                        getHistoryEntries((currentTime.Month - 2 + 12) % 12 + 1, true),
                        getHistoryEntries(currentTime.Month, true)}
                }
            };

            //adding series will update and animate the chart automatically
            this.StatsChart.Series.Add(new ColumnSeries
            {
                Title = "denied",
                Values = new ChartValues<double> {
                        getHistoryEntries((currentTime.Month - 5 + 12) % 12 + 1, false),
                        getHistoryEntries((currentTime.Month - 4 + 12) % 12 + 1, false),
                        getHistoryEntries((currentTime.Month - 3 + 12) % 12 + 1, false),
                        getHistoryEntries((currentTime.Month - 2 + 12) % 12 + 1, false),
                        getHistoryEntries(currentTime.Month, false)}
            });

            this.StatsChart.AxisX.Add(new Axis
            {
                Title = "Months",
                Labels = monthLabels
            });

            this.StatsChart.AxisY.Add(new Axis
            {
                Title = "Number of files",
                LabelFormatter = value => value.ToString("N")
            });
        }

        private double getHistoryEntries(int month, bool status)
        {
            var historyEntries = (from entry in fp.HistoryEntries
                                  where entry.Allowed == status
                                      && entry.TimeAccessed.Month == month
                                  select entry).Count();

            return historyEntries;
        }



        private void AskAndCache(string path, ulong pid, UInt64 messageId)
        {
            var allow = ShowToastNotification(path, pid);

            List<Tuple<string, Int32, bool>> val;
            if (cache.TryGetValue(pid, out val))
            {
                val.Add(new Tuple<string, Int32, bool>(path, Environment.TickCount, allow));
            }
            else
            {
                List<Tuple<string, Int32, bool>> newVal = new List<Tuple<string, Int32, bool>>();
                newVal.Add(new Tuple<string, Int32, bool>(path, Environment.TickCount, allow));
                cache.Add(pid, newVal);
            }
            BlockAccess(messageId, !allow);
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

                    if (isDefaultBlockingActive)
                    {
                        BlockAccess(messageId, true);
                        FreeNotification(ptr);
                        continue;
                    }

                    path = System.Runtime.InteropServices.Marshal.PtrToStringUni(ptr);


                    List<Tuple<string, Int32, bool>> val;
                    if (cache.TryGetValue(pid, out val))
                    {
                        var item = val.Find(t => t.Item1 == path);
                        if (item == null)
                        {
                            AskAndCache(path, pid, messageId);
                            FreeNotification(ptr);
                            continue;
                        }

                        val.Remove(item);
                        if (Environment.TickCount - item.Item2 < 5000)
                        {
                            var newItem = new Tuple<string, Int32, bool>(item.Item1, Environment.TickCount, item.Item3);
                            val.Insert(0, newItem);
                            BlockAccess(messageId, !item.Item3);
                            continue;
                        }

                        if (val.Count == 0)
                        {
                            cache.Remove(pid);
                        }
                    }

                    AskAndCache(path, pid, messageId);
                    FreeNotification(ptr);
                }
                else
                {
                    GetNextNotification(out ptr, out pid, out messageId);
                    BlockAccess(messageId, false);
                    FreeNotification(ptr);
                }
            }
        }

        private void BrowseButtonClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();

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
            foreach (var f in ProtectedFiles.files)
            {
                f.Path = "\\??\\" + f.Path;
                ProtectFile(f.Path, (ushort)f.Path.Length);
            }
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

                var currentTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

                this.StatsChart.Series = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "allowed",
                        Values = new ChartValues<double> {
                            getHistoryEntries((currentTime.Month - 5 + 12) % 12 + 1, true),
                            getHistoryEntries((currentTime.Month - 4 + 12) % 12 + 1, true),
                            getHistoryEntries((currentTime.Month - 3 + 12) % 12 + 1, true),
                            getHistoryEntries((currentTime.Month - 2 + 12) % 12 + 1, true),
                            getHistoryEntries(currentTime.Month, true)}
                    }
                };

                //adding series will update and animate the chart automatically
                this.StatsChart.Series.Add(new ColumnSeries
                {
                    Title = "denied",
                    Values = new ChartValues<double> {
                        getHistoryEntries((currentTime.Month - 5 + 12) % 12 + 1, false),
                        getHistoryEntries((currentTime.Month - 4 + 12) % 12 + 1, false),
                        getHistoryEntries((currentTime.Month - 3 + 12) % 12 + 1, false),
                        getHistoryEntries((currentTime.Month - 2 + 12) % 12 + 1, false),
                        getHistoryEntries(currentTime.Month, false)}
                });


            });


            return allowed;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
        private static readonly int MAX_PATH = 260;
        private static readonly int SPI_GETDESKWALLPAPER = 0x73;
        private static readonly int SPI_SETDESKWALLPAPER = 0x14;
        private static readonly int SPIF_UPDATEINIFILE = 0x01;
        private static readonly int SPIF_SENDWININICHANGE = 0x02;

        private bool InputPasswordMessageBoxLaunch(string path, ulong pid)
        {
            IntPtr hOldDesktop = GetThreadDesktop(GetCurrentThreadId());
            IntPtr pNewDesktop = CreateDesktop("NewDesktop", IntPtr.Zero, IntPtr.Zero, 0, (uint)DESKTOP_ACCESS.GENERIC_ALL, IntPtr.Zero);
            //Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            //bmp.Save("secure_desktop.jpg", ImageFormat.Jpeg);

            //string wallpaper = new string('\0', MAX_PATH);
            //SystemParametersInfo(SPI_GETDESKWALLPAPER, (int)wallpaper.Length, wallpaper, 0);
            //wallpaper.Substring(0, wallpaper.IndexOf('\0'));

            SwitchDesktop(pNewDesktop);
            //int ret = 0;

            bool allow = false;
            Thread t = new Thread(() =>
            {
                SetThreadDesktop(pNewDesktop);
                //ret = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, wallpaper, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
                //ret = SystemParametersInfo(SPI_GETDESKWALLPAPER, (int)wallpaper.Length, wallpaper, 0);
                //wallpaper.Substring(0, wallpaper.IndexOf('\0'));
                BlockingConsentWindow blockWindow = new BlockingConsentWindow(Process.GetProcessById((int)pid).ProcessName, pid, path);
                blockWindow.AllowButton.Click += (e, args) =>
                {
                    allow = true;
                    blockWindow.Close();
                };

                blockWindow.DenyButton.Click += (e, args) =>
                {
                    allow = false;
                    blockWindow.Close();
                };


                blockWindow.ShowDialog();
                //loginWnd.ShowDialog();
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
                Registry.SetValue(Constants.registryPath, "isMonitoring", isMonitoringActive.ToString());
            }
        }

        private void BlockingSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!isInitState)
            {
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
                Registry.SetValue(Constants.registryPath, "isDefaultBlocking", isDefaultBlockingActive.ToString());
            }
        }
    }
}
