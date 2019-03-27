using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProtectorUI
{
    class ProtectedFiles
    {
        private List<FileDTO> files;

        public ProtectedFiles()
        {
            files = new List<FileDTO>();
            files.Add(new FileDTO("file1", "D:/file1", System.DateTime.Now));
            files.Add(new FileDTO("file2", "D:/file2", System.DateTime.Now));
            files.Add(new FileDTO("file3", "D:/file3", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
            files.Add(new FileDTO("file4", "D:/file4", System.DateTime.Now));
        }

        public List<FileDTO> GetProtectedFiles()
        {
            return files;
        }
    }
}
