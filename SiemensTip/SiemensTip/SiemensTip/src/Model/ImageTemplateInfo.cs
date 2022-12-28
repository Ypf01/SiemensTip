using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiemensTip.src.Model
{
    public class ImageTemplateInfo : BindableBase
    {
        public string Addr { get; set; }
        public string Name { get; set; }
        public int ID { get; set; }
    }
}
