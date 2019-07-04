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

        public FileDTO(string Name, string Path)
        {
            this.Name = Name;
            this.Path = Path;
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                return (Path == ((FileDTO)obj).Path);
            }
        }
    }
}
