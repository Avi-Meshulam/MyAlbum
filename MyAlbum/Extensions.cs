using MyAlbum.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MyAlbum
{
    public static class Extensions
    {
        public static void SubscribeToPropertyChangedEvent(this IList items,
            PropertyChangedEventHandler eventHandler)
        {
            if (items == null)
                return;

            foreach (var item in items.OfType<INotifyPropertyChanged>())
            {
                item.PropertyChanged += eventHandler;
            }
        }

        public static void UnsubscribeFromPropertyChangedEvent(this IList items,
            PropertyChangedEventHandler eventHandler)
        {
            if (items == null)
                return;

            foreach (var item in items.OfType<INotifyPropertyChanged>())
            {
                item.PropertyChanged -= eventHandler;
            }
        }

        public static void SubscribeToPropertyChangingEvent(this IList items,
            PropertyChangingEventHandler eventHandler)
        {
            if (items == null)
                return;

            foreach (var item in items.OfType<INotifyPropertyChanging>())
            {
                item.PropertyChanging += eventHandler;
            }
        }

        public static void UnsubscribeFromPropertyChangingEvent(this IList items,
            PropertyChangingEventHandler eventHandler)
        {
            if (items == null)
                return;

            foreach (var item in items.OfType<INotifyPropertyChanging>())
            {
                item.PropertyChanging -= eventHandler;
            }
        }

        public static object GetPropertyValue(this object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName).GetValue(obj, null);
        }

        public static bool In<T>(this T item, params T[] items)
        {
            return items?.Contains(item) ?? false;
        }

        public static string SplitCamelCase(this string input)
        {
            var regex = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

            string output = regex.Replace(input, " ");

            return $"{char.ToUpper(output[0])}{output.Substring(1)}";
        }

        public static void RemoveFromNavigationStackByParameter<T>(
            this Frame frame, T parameter, Predicate<T> predicate = null) where T : class
        {
            frame.BackStack.RemoveByParameter(parameter, predicate);
            frame.ForwardStack.RemoveByParameter(parameter, predicate);
        }

        public static void RemoveByParameter<T>(
            this IList<PageStackEntry> stack, T parameter, Predicate<T> predicate = null) where T : class
        {
            var entriesToDelete = stack.Where(entry => 
                entry.Parameter is T
                && (entry.Parameter as T).Equals(parameter));

            entriesToDelete?.ToList().ForEach(entry => stack.Remove(entry));
        }

        public static void RemoveFromNavigationStackByPredicate<T>(
            this Frame frame, Predicate<T> predicate) where T : class
        {
            frame.BackStack.RemoveByPredicate(predicate);
            frame.ForwardStack.RemoveByPredicate(predicate);
        }

        public static void RemoveByPredicate<T>(
            this IList<PageStackEntry> stack, Predicate<T> predicate) where T : class
        {
            var entriesToDelete = stack.Where(entry =>
                entry.Parameter is T
                && (predicate != null && predicate(entry.Parameter as T)));

            entriesToDelete?.ToList().ForEach(entry => stack.Remove(entry));
        }
    }
}
