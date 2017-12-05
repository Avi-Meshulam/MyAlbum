using MyAlbum.BL.Models;
using MyAlbum.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class MovePhotosContentDialog : ContentDialog
    {
        private IEnumerable<AlbumViewModel> _albums;

        public MovePhotosContentDialog(IEnumerable<AlbumViewModel> albums)
        {
            if (albums == null)
                throw new ArgumentNullException();

            _albums = albums;
            InitializeComponent();
        }

        public AlbumViewModel SelectedAlbum { get; private set; }

        private void MovePhotosContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            SelectedAlbum = null;
            cboAlbums.SelectedIndex = _albums.Count() == 1 ? 0 : -1;
        }

        private void cboAlbums_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IsPrimaryButtonEnabled = (cboAlbums.SelectedIndex == -1) ? false : true;
        }

        private void MovePhotosContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            SelectedAlbum = cboAlbums.SelectedItem as AlbumViewModel;
        }
    }
}
