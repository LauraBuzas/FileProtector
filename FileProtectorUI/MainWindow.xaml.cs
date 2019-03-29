using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.Controls;

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
            //filesDataGrid.ItemsSource = protectedFiles.GetProtectedFiles();
        }

        private ProtectedFiles protectedFiles;

        private void BrowseButtonClick(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();

            // Set initial directory    
            //openFileDlg.InitialDirectory = @"C:\Users\";

            // Set filter for file extension and default file extension  
            // Multiple selection with all file types    
            openFileDlg.Multiselect = true;
            openFileDlg.Filter = "All files (*.*)|*.*";

            // Launch OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = openFileDlg.ShowDialog();
            // Get the selected file name and display in a TextBox.
            // Load content of file in a TextBlock
            if (result == true)
            {
                //add it in the table
            }
        }

        private async void RemoveButtonClick(object sender, RoutedEventArgs e)
        {
            FileDTO selectedFile = filesList.SelectedItem as FileDTO;
            if(selectedFile != null)
            {
                //TODO remove file from db/list
            } else
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
    }
}
