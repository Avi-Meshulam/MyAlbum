using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MyAlbum.ViewModels;
using MyAlbum.ContentDialogs;
using MyAlbum.Converters;
using System.Reflection;
using System.Text;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MyAlbum.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AlbumPage : Page
    {
        public AlbumPage()
        {
            InitializeComponent();
        }

        public AlbumViewModel AlbumViewModel { get; set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            AlbumViewModel = e.Parameter as AlbumViewModel;
            DataContext = AlbumViewModel;
            BindPhotoPropertyToValidationMessageConverter();
            base.OnNavigatedTo(e);
        }

        private void BindPhotoPropertyToValidationMessageConverter()
        {
            object converter;
            if (Resources.TryGetValue("PhotoPropertyToValidationMessageConverter", out converter))
            {
                BindingOperations.SetBinding(
                    converter as PropertyToValidationMessageConverter,
                    PropertyToValidationMessageConverter.ValidatorProperty,
                    new Binding
                    {
                        Source = AlbumViewModel,
                        Path = new PropertyPath(nameof(AlbumViewModel.ManipulatedPhoto))
                    });
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            AlbumViewModel?.SelectedPhotos?.ForEach(p =>
            {
                if (p.IsSelected ?? false)
                    photosGridView.SelectedItems.Add(p);
            });
        }
    }
}
