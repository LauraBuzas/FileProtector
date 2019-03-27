using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProtectorUI
{
    class FileDTO
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public DateTime DateAdded { get; set; }

        public FileDTO(string Name, string Path, DateTime DateAdded)
        {
            this.Name = Name;
            this.Path = Path;
            this.DateAdded = DateAdded;
        }
    }
}
