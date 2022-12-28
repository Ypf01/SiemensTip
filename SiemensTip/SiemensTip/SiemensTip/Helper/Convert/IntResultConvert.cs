using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SiemensTip.Helper.Convert
{
    public class IntResultConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null || parameter.ToString().Length == 0) return ConstHelper.GetResult();
            if (value is string && ((string)value).Contains('|'))
            {
                string[] vs = value.ToString().Split('|');
                if (vs.Count() >= 2 && int.TryParse(vs[0], out int result) && int.TryParse(vs[1], out int endPos))
                {
                    //string[] bindResult = ConstHelper.IntToStringArray(result, endPos);
                    return null;
                }
                else
                    return ConstHelper.GetResult();
            }
            else
                return ConstHelper.GetResult();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
