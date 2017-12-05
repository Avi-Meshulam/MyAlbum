using MyAlbum.ContentDialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace MyAlbum
{
    public static class Utils
    {
        public const int MAX_FRAMEWORK_ALLOWED_DIALOG_COMMANDS = 3;

        private static Random _rand = new Random();

        /// <summary>
        /// Displays a content dialog with up to 3 buttons
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="primaryButtonText"></param>
        /// <param name="secondaryButtonText"></param>
        /// <param name="cancelButtonText"></param>
        /// <returns></returns>
        public static async Task<CustomContentDialogResult> ShowContentDialog(string title, string content,
            string primaryButtonText = null, string secondaryButtonText = null, string cancelButtonText = null)
        {
            var dialog = new CustomContentDialog
            {
                Title = title,
                Content = content,
                PrimaryButtonText = primaryButtonText,
                SecondaryButtonText = secondaryButtonText,
                CancelButtonText = cancelButtonText
            };

            try
            {
                await dialog.ShowAsync();
            }
            catch (Exception)
            {
                return (CustomContentDialogResult)await ShowMessageDialog(
                    title, content, primaryButtonText, secondaryButtonText, cancelButtonText);
            }

            return dialog.Result;
        }

        /// <summary>
        /// Displays a message dialog with variable number of commands (up to 3). 
        /// Returns the number of the selected command (counting from 1)
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="commands"></param>
        /// <returns></returns>
        public static async Task<int> ShowMessageDialog(
            string title = null, string content = null, params string[] commands)
        {
            var dialog = new MessageDialog(content, title);

            int counter = 1;
            foreach (var command in commands)
            {
                if (dialog.Commands.Count >= MAX_FRAMEWORK_ALLOWED_DIALOG_COMMANDS)
                    break;
                if(!string.IsNullOrWhiteSpace(command))
                    dialog.Commands.Add(new UICommand() { Id = counter++, Label = command });
            }

            if (dialog.Commands.Count == 0)
                dialog.Commands.Add(new UICommand() { Id = counter++, Label = "Close" });

            IUICommand res = await dialog.ShowAsync();

            return (int)res.Id;
        }

        public static Visibility BoolToVisibility(bool value)
        {
            return value ? Visibility.Visible : Visibility.Collapsed;
        }

        public static object VisibilityToBool(object value)
        {
            return (Visibility)value == Visibility.Visible ? true : false;
        }

        public static async Task<IReadOnlyList<StorageFile>> PickFiles(FileTypes groupType)
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();

            switch (groupType)
            {
                case FileTypes.Images:
                    fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
                    fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                    fileOpenPicker.FileTypeFilter.Add(".jpg");
                    fileOpenPicker.FileTypeFilter.Add(".jpeg");
                    fileOpenPicker.FileTypeFilter.Add(".png");
                    break;
                default:
                    break;
            }

            return await fileOpenPicker.PickMultipleFilesAsync();
        }

        #region Random Utils
        public static int GetRandom(int minValue, int maxValue)
        {
            if (maxValue == int.MaxValue)
                maxValue -= 1;
            return _rand.Next(minValue, maxValue + 1);
        }

        public static int GetRandom(int maxValue)
        {
            if (maxValue == int.MaxValue)
                maxValue -= 1;
            return GetRandom(0, maxValue + 1);
        }

        public static int GetRandom()
        {
            return GetRandom(0, int.MaxValue - 1);
        }

        public static DateTime GetRandomDate(int yearsBackwards)
        {
            return DateTime.Now.Add(new TimeSpan(-_rand.Next(365, 365 * yearsBackwards), 0, 0, 0)).Date;
        }

        public static string GetRandomName()
        {
            return $"{GetRandomFirstName()} {GetRandomLastName()}";
        }
        
        public static string GetRandomFirstName()
        {
            return _firstNames[_rand.Next(0, _firstNames.Count)];
        }

        public static string GetRandomLastName()
        {
            return _lastNames[_rand.Next(0, _lastNames.Count)];
        }
        #endregion // Random Utils

        #region Data Lists
        private static List<string> _firstNames = new List<string>
        {
            "James", "John", "Robert", "Michael", "William", "David", "Richard", "Charles", "Joseph", "Thomas", "Christopher", "Daniel", "Paul", "Mark", "Donald", "George", "Kenneth", "Steven", "Edward", "Brian", "Ronald", "Anthony", "Kevin", "Jason", "Matthew", "Gary", "Timothy", "Jose", "Larry", "Jeffrey", "Frank", "Scott", "Eric", "Stephen", "Andrew", "Raymond", "Gregory", "Joshua", "Jerry", "Dennis", "Walter", "Patrick", "Peter", "Harold", "Douglas", "Henry", "Carl", "Arthur", "Ryan", "Roger", "Mary", "Patricia", "Linda", "Barbara", "Elizabeth", "Jennifer", "Maria", "Susan", "Margaret", "Dorothy", "Lisa", "Nancy", "Karen", "Betty", "Helen", "Sandra", "Donna", "Carol", "Ruth", "Sharon", "Michelle", "Laura", "Sarah", "Kimberly", "Deborah", "Jessica", "Shirley", "Cynthia", "Angela", "Melissa", "Brenda", "Amy", "Anna", "Rebecca", "Virginia", "Kathleen", "Pamela", "Martha", "Debra", "Amanda", "Stephanie", "Carolyn", "Christine", "Marie", "Janet", "Catherine", "Frances", "Ann", "Joyce", "Diane"
        };

        private static List<string> _lastNames = new List<string>
        {
            "Smith", "Johnson", "Williams", "Jones", "Brown", "Davis", "Miller", "Wilson", "Moore", "Taylor", "Anderson", "Thomas", "Jackson", "White", "Harris", "Martin", "Thompson", "Garcia", "Martinez", "Robinson", "Clark", "Rodriguez", "Lewis", "Lee", "Walker", "Hall", "Allen", "Young", "Hernandez", "King", "Wright", "Lopez", "Hill", "Scott", "Green", "Adams", "Baker", "Gonzalez", "Nelson", "Carter", "Mitchell", "Perez", "Roberts", "Turner", "Phillips", "Campbell", "Parker", "Evans", "Edwards", "Collins", "Stewart", "Sanchez", "Morris", "Rogers", "Reed", "Cook", "Morgan", "Bell", "Murphy", "Bailey", "Rivera", "Cooper", "Richardson", "Cox", "Howard", "Ward", "Torres", "Peterson", "Gray", "Ramirez", "James", "Watson", "Brooks", "Kelly", "Sanders", "Price", "Bennett", "Wood", "Barnes", "Ross", "Henderson", "Coleman", "Jenkins", "Perry", "Powell", "Long", "Patterson", "Hughes", "Flores", "Washington", "Butler", "Simmons", "Foster", "Gonzales", "Bryant", "Alexander", "Russell", "Griffin", "Diaz", "Hayes"
        };
        #endregion // Data Lists
    }
}
