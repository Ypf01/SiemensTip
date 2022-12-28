using SiemensTip.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiemensTip.src
{
    public class Adjustment
    {
        public static List<ImageDisplayViewModel> PosAdjustment(List<ImageDisplayViewModel> viewModels, int source = 1, int targat = 2)
        {
            if (viewModels == null || viewModels.Count < 2)
                return null;
            ImageDisplayViewModel temp = viewModels[targat];
            viewModels[targat] = viewModels[source];
            viewModels[source] = temp;
            return viewModels;
        }
    }
}
