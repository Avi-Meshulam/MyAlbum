using MyAlbum.BL;
using MyAlbum.ContentDialogs;
using MyAlbum.Converters;
using MyAlbum.Pages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MyAlbum.ViewModels
{
    public class MainViewModel : ValidatorChangeNotifier
    {
        private Frame _mainFrame;

        public MainViewModel()
        {
            if (AlbumsVMManager.Albums.Count() > 0)
                InitAlbumsCollections();
        }

        private AlbumViewModel MainAlbum
        {
            get { return _albums?.SingleOrDefault(album => album.IsMain ?? false); }
        }

        private ObservableCollection<AlbumViewModel> _albums;

        public MenuFlyout Menu { get; private set; }

        private AlbumViewModel _selectedAlbum;
        public AlbumViewModel SelectedAlbum
        {
            get { return _selectedAlbum ?? MainAlbum; }
            set { SetProperty(ref _selectedAlbum, value, HandleAlbumSelection); }
        }

        private ElementTheme _currentElementTheme = ElementTheme.Dark;
        public ElementTheme CurrentElementTheme
        {
            get { return _currentElementTheme; }
            set { SetProperty(ref _currentElementTheme, value); }
        }

        private bool _isProgressRingVisible;
        public bool IsProgressRingVisible
        {
            get { return _isProgressRingVisible; }
            set { SetProperty(ref _isProgressRingVisible, value); }
        }
        
        public void ToggleElementTheme()
        {
            CurrentElementTheme = CurrentElementTheme == ElementTheme.Dark ?
                ElementTheme.Light : ElementTheme.Dark;
        }

        #region Data Events Handlers
        private void Albums_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (var item in e.OldItems)
                {
                    UnsubscribeFromAlbumEvents(item as AlbumViewModel);
                }

            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                {
                    SubscribeToAlbumEvents(item as AlbumViewModel);
                }

            // No need to call SetMenu() here as long as it's 
            // called after Add/Delete album (via MainFrame_Navigated)
            //SetMenu();
        }

        private void Album_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var album = sender as AlbumViewModel;

            switch (e.PropertyName)
            {
                case nameof(AlbumViewModel.IsSinglePhotoSelected):
                    var photoDetailsMenu = GetMenuSubItem(MenuItems.Main.Photo).Items.FirstOrDefault(
                        i => (i as MenuFlyoutItem)?.Text == MenuItems.Photo.Details) as MenuFlyoutItem;
                    photoDetailsMenu.IsEnabled = SelectedAlbum.IsSinglePhotoSelected;
                    break;
                case nameof(AlbumViewModel.IsAnyPhotoSelected):
                    GetMenuSubItem(MenuItems.Main.Photo).IsEnabled = SelectedAlbum.IsAnyPhotoSelected;
                    break;
                case nameof(AlbumViewModel.ViewType):
                    var photoMenu = GetMenuSubItem(MenuItems.Main.Photo);
                    photoDetailsMenu = GetMenuSubItem(MenuItems.Main.Photo).Items.FirstOrDefault(
                        i => (i as MenuFlyoutItem)?.Text == MenuItems.Photo.Details) as MenuFlyoutItem;
                    if (SelectedAlbum.ViewType == ViewType.GridView)
                    {
                        photoMenu.IsEnabled = SelectedAlbum.IsAnyPhotoSelected;
                        photoDetailsMenu.IsEnabled = SelectedAlbum.IsSinglePhotoSelected;
                    }
                    else if (SelectedAlbum.ViewType == ViewType.FlipView)
                    {
                        photoMenu.IsEnabled = true;
                        photoDetailsMenu.IsEnabled = true;
                    }
                    break;
                default:
                    break;
            }
        }

        private void Album_AlbumDeleted(object sender, EventArgs e)
        {
            var album = sender as AlbumViewModel;

            UnsubscribeFromAlbumEvents(album);
            _albums.Remove(album);

            if (_mainFrame.CanGoBack)
                _mainFrame.GoBack();
            else
                NavigateTo(MainAlbum);
        }

        private void Album_AlbumEdited(object sender, EventArgs e)
        {
            SetMenu();
        }
        #endregion // Data Events Handlers

        #region UI Events Handlers
        public async void MainFrame_Loaded(object sender, RoutedEventArgs e)
        {
            // Save for future navigations
            _mainFrame = sender as Frame;

            if (_albums == null || _albums.Count() == 0)
            {
                var result = await Utils.ShowContentDialog(
                    "Welcome to 'My Album'!",
                    "Would you like the application to load some demo data?",
                    "Yes, Please!", "No, Thank You!");

                if (result == CustomContentDialogResult.Primary)
                {
                    IsProgressRingVisible = true;
                    await AlbumsVMManager.LoadDemoData();
                    IsProgressRingVisible = false;
                }
                else
                    AlbumsVMManager.CreateMainAlbum();

                InitAlbumsCollections();
            }

            NavigateTo(MainAlbum);
        }

        public void MainFrame_Navigated()
        {
            _mainFrame.RemoveFromNavigationStackByPredicate<AlbumViewModel>(a => a.IsDeleted);
            SetMenu();
        }

        public void GoBack()
        {
            if (_mainFrame.CanGoBack)
            {
                _mainFrame.GoBack();
                SelectedAlbum = (_mainFrame.Content as AlbumPage).AlbumViewModel;
            }
        }

        public void GoForward()
        {
            if (_mainFrame.CanGoForward)
            {
                _mainFrame.GoForward();
                SelectedAlbum = (_mainFrame.Content as AlbumPage).AlbumViewModel;
            }
        }
        #endregion // UI Events Handlers

        #region Menu Events Handlers
        private void Menu_Albums_Album_Click(object sender, RoutedEventArgs e)
        {
            var selectedMenuItem = sender as ToggleMenuFlyoutItem;

            var selectedAlbum = _albums.SingleOrDefault(album => album.Title == selectedMenuItem.Text);

            if (selectedAlbum == SelectedAlbum)
            {
                selectedMenuItem.IsChecked = true;
                return;
            }

            NavigateTo(selectedAlbum);
        }

        private void Menu_Albums_Add_Click(object sender, RoutedEventArgs e)
        {
            AddAlbum();
        }

        private void Menu_AddPhotos_Click(object sender, RoutedEventArgs e)
        {
            SelectedAlbum.AddPhotosFromStorage();
        }

        private void Menu_Capture_Click(object sender, RoutedEventArgs e)
        {
            SelectedAlbum.Capture();
        }

        private void Menu_Photo_Details_Click(object sender, RoutedEventArgs e)
        {
            SelectedAlbum.ShowSelectedPhotoDetails();
        }

        private void Menu_Photo_Move_Click(object sender, RoutedEventArgs e)
        {
            SelectedAlbum.MoveSelectedPhotos();
        }

        private void Menu_Photo_Delete_Click(object sender, RoutedEventArgs e)
        {
            SelectedAlbum.DeleteSelectedPhotos();
        }

        private void Menu_Exit_Click(object sender, RoutedEventArgs e)
        {
            Exit();
        }
        #endregion // Menu Events Handlers

        #region Menu Helper Methods
        private void SetMenu()
        {
            if (Menu != null)
                UnsubscribeFromAllMenuEvents();

            Menu = new MenuFlyout();

            // Albums
            AppendAlbumsMenu();

            Menu.Items.Add(new MenuFlyoutSeparator());

            // Add Photos
            AppendAddPhotosMenu();

            // Capture
            AppendCaptureMenu();

            // Photo
            AppendPhotoMenu();

            Menu.Items.Add(new MenuFlyoutSeparator());

            // Exit
            AppendExitMenu();

            OnPropertyChanged(nameof(Menu));
        }

        private void AppendAlbumsMenu()
        {
            var albumsMenu = new MenuFlyoutSubItem { Text = MenuItems.Main.Albums };

            // Add 'Main' Album
            if (MainAlbum != null)
                albumsMenu.Items.Add(AlbumViewModelToMenuFlyoutItem(MainAlbum));

            // Add Other Albums
            _albums.Where(album => !album.IsMain ?? false)
                .Select(album => AlbumViewModelToMenuFlyoutItem(album)).ToList()
                .ForEach(albumMenuItem => albumsMenu.Items.Add(albumMenuItem));

            albumsMenu.Items.Add(new MenuFlyoutSeparator());

            var menuItem = new MenuFlyoutItem { Text = MenuItems.Albums.NewAlbum };
            menuItem.Click += Menu_Albums_Add_Click;
            albumsMenu.Items.Add(menuItem);

            Menu.Items.Add(albumsMenu);
        }

        private MenuFlyoutItemBase AlbumViewModelToMenuFlyoutItem(AlbumViewModel album)
        {
            var albumMenuItem = new ToggleMenuFlyoutItem();

            albumMenuItem.Text = album.Title;
            albumMenuItem.IsChecked = album.IsSelected;
            albumMenuItem.FontWeight = (album.IsMain ?? false) ? FontWeights.Bold : FontWeights.Normal;

            albumMenuItem.Click += Menu_Albums_Album_Click;

            return albumMenuItem as MenuFlyoutItemBase;
        }

        private void AppendAddPhotosMenu()
        {
            var menuItem = new MenuFlyoutItem { Text = MenuItems.Album.AddPhotos };
            menuItem.Click += Menu_AddPhotos_Click;
            Menu.Items.Add(menuItem);
        }

        private void AppendCaptureMenu()
        {
            var menuItem = new MenuFlyoutItem { Text = MenuItems.Album.CapturePhoto };
            menuItem.Click += Menu_Capture_Click;
            Menu.Items.Add(menuItem);
        }

        private void AppendPhotoMenu()
        {
            // Photo > 
            var photoMenuItem = new MenuFlyoutSubItem { Text = MenuItems.Main.Photo };
            photoMenuItem.IsEnabled = false;

            // Photo > Details
            var menuItem = new MenuFlyoutItem { Text = MenuItems.Photo.Details };
            menuItem.Click += Menu_Photo_Details_Click;
            photoMenuItem.Items.Add(menuItem);

            // Photo > Move
            menuItem = new MenuFlyoutItem { Text = MenuItems.Photo.Move };
            menuItem.IsEnabled = _albums.Count() > 1 ? true : false;
            menuItem.Click += Menu_Photo_Move_Click;
            photoMenuItem.Items.Add(menuItem);

            // Photo > Delete
            menuItem = new MenuFlyoutItem { Text = MenuItems.Photo.Delete };
            menuItem.Click += Menu_Photo_Delete_Click;
            photoMenuItem.Items.Add(menuItem);

            Menu.Items.Add(photoMenuItem);
        }

        private void AppendExitMenu()
        {
            var menuItem = new MenuFlyoutItem { Text = MenuItems.Main.Exit };
            menuItem.Click += Menu_Exit_Click;
            Menu.Items.Add(menuItem);
        }

        private void UnsubscribeFromAllMenuEvents()
        {
            UnsubscribeFromAlbumsMenuEvents();

            UnsubscribeFromSingleMenuEvent(MenuItems.Album.AddPhotos);
            UnsubscribeFromSingleMenuEvent(MenuItems.Album.CapturePhoto);
            UnsubscribeFromSingleMenuEvent(MenuItems.Photo.Details);
            UnsubscribeFromSingleMenuEvent(MenuItems.Photo.Move);
            UnsubscribeFromSingleMenuEvent(MenuItems.Photo.Delete);
            UnsubscribeFromSingleMenuEvent(MenuItems.Main.Exit);
        }

        private void UnsubscribeFromAlbumsMenuEvents()
        {
            // Unsubscribe from album items click events
            Menu.Items?.Where(i => i.GetType() == typeof(ToggleMenuFlyoutItem))?.ToList()
                .ForEach(i => (i as ToggleMenuFlyoutItem).Click -= Menu_Albums_Album_Click);

            // Unsubscribe from "New Album" click event
            var newAlbumMenuItem = Menu.Items?.SingleOrDefault(i => i.GetType() == typeof(MenuFlyoutItem)
                && (i as MenuFlyoutItem).Text == MenuItems.Albums.NewAlbum) as MenuFlyoutItem;
            if (newAlbumMenuItem != null)
                newAlbumMenuItem.Click -= Menu_Albums_Add_Click;
        }

        private void UnsubscribeFromSingleMenuEvent(string menuText)
        {
            var menuItem = Menu.Items?.Where(i => i.GetType() == typeof(MenuFlyoutItem)
                && (i as MenuFlyoutItem).Text == menuText) as MenuFlyoutItem;
            if (menuItem != null)
                switch (menuText)
                {
                    case MenuItems.Album.AddPhotos:
                        menuItem.Click -= Menu_AddPhotos_Click;
                        break;
                    case MenuItems.Album.CapturePhoto:
                        menuItem.Click -= Menu_Capture_Click;
                        break;
                    case MenuItems.Photo.Details:
                        menuItem.Click -= Menu_Photo_Details_Click;
                        break;
                    case MenuItems.Photo.Move:
                        menuItem.Click -= Menu_Photo_Move_Click;
                        break;
                    case MenuItems.Photo.Delete:
                        menuItem.Click -= Menu_Photo_Delete_Click;
                        break;
                    case MenuItems.Main.Exit:
                        menuItem.Click -= Menu_Exit_Click;
                        break;
                    default:
                        break;
                }
        }

        private void UpdateAlbumsMenu()
        {
            var albumsMenu = GetMenuSubItem(MenuItems.Main.Albums);

            if (albumsMenu == null)
                return;

            var selectedAlbumMenuItem =
                GetSubMenuItem(SelectedAlbum.Title, albumsMenu) as ToggleMenuFlyoutItem;

            // Uncheck all Album menu items
            albumsMenu?.Items.ToList().ForEach(i =>
            {
                if (i.GetType() == typeof(ToggleMenuFlyoutItem))
                    (i as ToggleMenuFlyoutItem).IsChecked = false;
            });

            // Check the selected menu item
            selectedAlbumMenuItem.IsChecked = true;
        }

        private MenuFlyoutSubItem GetMenuSubItem(string menuText, MenuFlyout parentMenu = null)
        {
            return (parentMenu ?? Menu)?.Items.FirstOrDefault(i =>
                (i as MenuFlyoutSubItem)?.Text == menuText) as MenuFlyoutSubItem;
        }

        private MenuFlyoutItem GetMenuItem(string menuText, MenuFlyout parentMenu = null)
        {
            return (parentMenu ?? Menu)?.Items.FirstOrDefault(i =>
                (i as MenuFlyoutItem)?.Text == menuText) as MenuFlyoutItem;
        }

        private MenuFlyoutItem GetSubMenuItem(string menuText, MenuFlyoutSubItem parentMenu = null)
        {
            return parentMenu?.Items.FirstOrDefault(i =>
                (i as MenuFlyoutItem)?.Text == menuText) as MenuFlyoutItem;
        }
        #endregion // Menu Helper Methods

        #region Helper Methods
        private void InitAlbumsCollections()
        {
            _albums = new ObservableCollection<AlbumViewModel>(AlbumsVMManager.Albums);
            _albums.CollectionChanged += Albums_CollectionChanged;
            _albums.ToList().ForEach(a => SubscribeToAlbumEvents(a));
        }

        private void HandleAlbumSelection()
        {
            // Set IsSelected = false for all albums, except selected album
            _albums.ToList().ForEach(album =>
                album.IsSelected = (album == SelectedAlbum) ? true : false);

            UpdateAlbumsMenu();
        }

        private async void AddAlbum()
        {
            AlbumContentDialog albumDialog = new AlbumContentDialog();

        ShowNewAlbumDialog:

            ContentDialogResult result = await albumDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var newAlbum = albumDialog.AlbumViewModel;

                if (newAlbum.HasErrors)
                    return;

                try
                {
                    AlbumsVMManager.AddAlbum(newAlbum);
                }
                catch (DuplicateObjectException)
                {
                    await Utils.ShowContentDialog(
                        "Add Album", "Name already exists. Please chose a different one.");
                    goto ShowNewAlbumDialog;
                }
                catch (Exception ex)
                {
                    await Utils.ShowContentDialog("Add Album", ex.Message);
                    goto ShowNewAlbumDialog;
                }

                _albums.Add(newAlbum);

                NavigateTo(newAlbum);
            }
        }

        private async void Exit()
        {
            var result = await Utils.ShowContentDialog(
                "Exit", "Are you sure you want to exit?", "Yes", "No");

            if (result == CustomContentDialogResult.Primary)
                Application.Current.Exit();
        }

        private void SubscribeToAlbumEvents(AlbumViewModel album)
        {
            album.AlbumEdited += Album_AlbumEdited;
            album.AlbumDeleted += Album_AlbumDeleted;
            album.PropertyChanged += Album_PropertyChanged;
        }

        private void UnsubscribeFromAlbumEvents(AlbumViewModel album)
        {
            album.AlbumEdited -= Album_AlbumEdited;
            album.AlbumDeleted -= Album_AlbumDeleted;
            album.PropertyChanged -= Album_PropertyChanged;
        }

        private void NavigateTo(AlbumViewModel album)
        {
            ViewType viewType = ViewType.GridView;

            if (SelectedAlbum != null)
                viewType = SelectedAlbum.ViewType;

            _mainFrame.Navigate(typeof(AlbumPage), album);
            SelectedAlbum = (_mainFrame.Content as AlbumPage).AlbumViewModel;

            SelectedAlbum.ViewType = viewType;
        }
        #endregion Helper Methods
    }
}
