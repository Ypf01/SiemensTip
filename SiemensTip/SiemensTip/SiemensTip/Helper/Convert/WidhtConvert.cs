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
    public class WidhtConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null || parameter.ToString().Length == 0) return Visibility.Hidden;
            if (value is bool)
            {
                string targat = parameter.ToString();
                bool flag = (bool)value;
                Visibility visibility = Visibility.Hidden;
                switch (targat)
                {
                    case "auto":
                        visibility = flag ? Visibility.Hidden : Visibility.Visible;
                        break;
                    case "mauaul":
                        visibility = flag ? Visibility.Visible : Visibility.Hidden;
                        break;
                    default:
                        visibility = Visibility.Hidden;
                        break;
                }
                return visibility;
            }
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
