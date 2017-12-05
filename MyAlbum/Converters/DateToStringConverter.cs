using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace MyAlbum.Converters
{
    /// <summary>
    /// Converts from System.DateTime or System.DateTimeOffset to string 
    /// using StringFormat property or "parameter" paramater
    /// </summary>
    public class DateToStringConverter : IValueConverter
    {
        public string StringFormat { get; set; } = "{0:dd/MM/yyyy}";

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value?.GetType().In(typeof(DateTime), typeof(DateTimeOffset)) ?? false)
                || targetType != typeof(string))
                return value;

            string format = (string.IsNullOrEmpty(parameter as string)) ? StringFormat : parameter as string;

            if (string.IsNullOrEmpty(format))
                return value;

            string result;
            try
            {
                result = string.Format(format, value);
            }
            catch (Exception)
            {
                return value;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value?.GetType() != typeof(string) ||
                !(targetType?.In(typeof(DateTime), typeof(DateTimeOffset)) ?? false))
                return value;

            return TypeDescriptor.GetConverter(targetType).ConvertFrom(value.ToString());
        }
    }
}
