using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace SiemensTip.src.Model
{
    public class RectangleArea
    {
        private Rectangle reg;
        private List<Rectangle> listRegs;

        public int ParentID { get; set; }

        public Rectangle Reg
        {
            get { return reg; }
            set { reg = value; }
        }

        public List<Rectangle> ListRegs
        {
            get => listRegs ?? (listRegs = new List<Rectangle>());
            set { listRegs = value; }
        }
    }
}
