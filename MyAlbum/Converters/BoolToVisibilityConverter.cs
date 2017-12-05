using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MyAlbum.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Utils.BoolToVisibility((bool)(value ?? false));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Utils.VisibilityToBool(value);
        }
    }
}
