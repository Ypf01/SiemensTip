using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SiemensTip.Helper
{
    public class ToolTipConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "未拍照或算法处理中";
            switch (value.ToString())
            {
                case ConstHelper.OK:
                    return ConstHelper.OK;
                case ConstHelper.NG:
                    return ConstHelper.NG;
                case ConstHelper.Null:
                default:
                    return "未拍照或算法处理中";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
