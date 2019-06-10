using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace FileProtectorUI
{
    class ProtectedFiles
    {
        public static ObservableCollection<FileDTO> files = new ObservableCollection<FileDTO>();
        private const string registryPath = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\fpf";
        //public ProtectedFiles()
        //{
        //    files = new List<FileDTO>();
        //}

        public static ObservableCollection<FileDTO> GetProtectedFilesFromRegistryKey()
        {
            String[] registryKeyValue = (String[])Registry.GetValue(registryPath, "ProtectedPaths", "");

            foreach (String entry in registryKeyValue)
            {
                files.Add(new FileDTO(Path.GetFileName(entry), entry));
            }

            return files;
        }

        public static void AddFile(String path)
        {
            files.Add(new FileDTO(Path.GetFileName(path), path));
            String[] paths = new String[files.Count];
            var index = 0;
            foreach(FileDTO file in files)
            {
                paths[index] = file.Path;
                index++;
            }

            Registry.SetValue(registryPath, "ProtectedPaths", paths);
        }

        public static void RemoveFile(FileDTO file)
        {
            files.Remove(file);
            String[] paths = new String[files.Count];
            var index = 0;
            foreach (FileDTO f in files)
            {
                paths[index] = f.Path;
                index++;
            }
            Registry.SetValue(registryPath, "ProtectedPaths", paths);
        }
    }
}
