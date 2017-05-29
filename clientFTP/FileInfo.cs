using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace clientFTP
{
   public  class FileInfo
    {
        public int size { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string date { get; set; }

        public FileInfo() { }

        public FileInfo(int _size, string _type, string _name, string _date)
        {
            size = _size;
            type = _type;
            name = _name;
            date = _date;
        }
    }
}
