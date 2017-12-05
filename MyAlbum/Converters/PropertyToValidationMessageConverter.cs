using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace MyAlbum.Converters
{
    public class PropertyToValidationMessageConverter : DependencyObject, IValueConverter
    {
        public static readonly DependencyProperty ValidatorProperty =
            DependencyProperty.Register("Validator",
                                        typeof(INotifyDataErrorInfo),
                                        typeof(PropertyToValidationMessageConverter),
                                        new PropertyMetadata(null));

        public INotifyDataErrorInfo Validator
        {
            get { return (INotifyDataErrorInfo)GetValue(ValidatorProperty); }
            set { SetValue(ValidatorProperty, value); }
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter == null)
                return null;

            var errors = Validator?.GetErrors(parameter.ToString())?.Cast<string>();

            if (errors == null || errors.Count() == 0)
                return null;

            return errors.Aggregate((a, b) => $"{a}\n{b}");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
