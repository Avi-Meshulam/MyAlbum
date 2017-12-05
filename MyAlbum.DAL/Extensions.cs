using Inflector;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLibrary.DAL
{
    public static class Extensions
    {
        static Extensions()
        {
            Inflector.Inflector.SetDefaultCultureFunc = () => CultureInfo.CurrentUICulture;
        }

        public static string Pluralize(this string name)
        {
            return InflectorExtensions.Pluralize(name);
        }

        public static string Singularize(this string name)
        {
            return InflectorExtensions.Singularize(name);
        }

        public static bool IsNumeric(this string input)
        {
            float output;
            return float.TryParse(input, out output);
        }
    }
}
