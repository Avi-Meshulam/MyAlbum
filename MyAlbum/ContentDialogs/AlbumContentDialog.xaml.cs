using MyAlbum.Converters;
using MyAlbum.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MyAlbum.ContentDialogs
{
    public sealed partial class AlbumContentDialog : ContentDialog
    {
        private bool _isMainChangeHandled;

        public AlbumContentDialog(AlbumViewModel album = null)
        {
            InitializeComponent();
            AlbumViewModel = album ?? new AlbumViewModel();
            AlbumViewModel.PropertyChanged += Album_PropertyChanged;
            DataContext = AlbumViewModel;
            BindPropertyToValidationMessageConverter();
            InitUI(album);
        }

        public AlbumViewModel AlbumViewModel { get; private set; }

        // Hide base.ShowAsync() in order to support keyboard control
        public new IAsyncOperation<ContentDialogResult> ShowAsync()
        {
            var tcs = new TaskCompletionSource<ContentDialogResult>();

            Window.Current.CoreWindow.KeyDown += (sender, args) =>
            {
                if (args.VirtualKey == VirtualKey.Enter && IsPrimaryButtonEnabled)
                {
                    tcs.TrySetResult(ContentDialogResult.Primary);
                    Hide();
                    args.Handled = true;
                }
            };

            var asyncOperation = base.ShowAsync();
            asyncOperation.AsTask().ContinueWith(task => tcs.TrySetResult(task.Result));
            return tcs.Task.AsAsyncOperation();
        }

        #region Data Events Handlers
        private void Album_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (AlbumViewModel.HasErrors)
                IsPrimaryButtonEnabled = false;
            else
                IsPrimaryButtonEnabled = true;
        }
        #endregion // Data Events Handlers

        #region UI Events Handlers
        private void DeleteAlbumButton_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
        }

        private async void DeleteAlbumConfirmationButton_Click(object sender, RoutedEventArgs e)
        {
            if (await AlbumViewModel.DeleteAlbum(this))
            {
                DeleteAlbumFlyout.Hide();
                this.Hide();
            }
        }

        private void DeleteAlbumCancelButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteAlbumFlyout.Hide();
        }

        private void IsMainCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked == true)
                FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
        }

        private void IsMainConfirmationButton_Click(object sender, RoutedEventArgs e)
        {
            AlbumViewModel.IsMain = true;
            _isMainChangeHandled = true;
            IsMainConfirmationFlyout.Hide();
        }

        private void IsMainCancelButton_Click(object sender, RoutedEventArgs e)
        {
            AlbumViewModel.IsMain = false;
            _isMainChangeHandled = true;
            IsMainConfirmationFlyout.Hide();
        }

        private void IsMainConfirmationFlyout_Closed(object sender, object e)
        {
            if (!_isMainChangeHandled)
                AlbumViewModel.IsMain = false;
            else
                _isMainChangeHandled = false;
        }
        #endregion // UI Events Handlers

        #region Helper Methods
        private void BindPropertyToValidationMessageConverter()
        {
            object converter;
            if (Resources.TryGetValue(nameof(PropertyToValidationMessageConverter), out converter))
            {
                BindingOperations.SetBinding(
                    converter as PropertyToValidationMessageConverter,
                    PropertyToValidationMessageConverter.ValidatorProperty,
                    new Binding { Source = AlbumViewModel });
            }
        }

        private void InitUI(AlbumViewModel album)
        {
            if (album == null)
            {
                Title = "New Album";
                DeleteAlbumButton.Visibility = Visibility.Collapsed;
                DatesGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                Title = "Edit Album";
            }
        }
        #endregion // Helper Methods
    }
}
