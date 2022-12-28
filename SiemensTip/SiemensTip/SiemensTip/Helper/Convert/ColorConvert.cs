using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SiemensTip.Helper
{
    public class ColorConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Brushes.LightGray;
            switch (value.ToString())
            {
                case ConstHelper.OK:
                    return  new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00ae9d"));
                case ConstHelper.NG:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ef5b9c"));
                case ConstHelper.Null:
                default:
                    return Brushes.LightGray;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
