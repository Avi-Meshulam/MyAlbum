using MyAlbum.BL;
using MyAlbum.BL.Models;
using MyAlbum.ContentDialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using System.Collections;

namespace MyAlbum.ViewModels
{
    public class AlbumViewModel : ViewModelBase<Album>
    {
        private const double PHOTO_DETAILS_OPEN_PANE_LENGTH = 364;
        private const double PHOTO_DETAILS_COMPACT_PANE_LENGTH = 32;

        public event EventHandler AlbumDeleted;
        public event EventHandler AlbumEdited;

        public ICommand SetViewTypeCommand;

        private bool _isTapHandled;

        public AlbumViewModel() : this(new Album())
        { }

        public AlbumViewModel(string title, bool isMain = false) : this(new Album(title, isMain))
        { }

        public AlbumViewModel(Album album) : base(album)
        {
            _photos = new ObservableCollection<PhotoViewModel>(
                Model.Photos.Select(p => new PhotoViewModel(p)));

            _photos.CollectionChanged += Photos_CollectionChanged;
            _photos.ToList().ForEach(p => SubscribeToPhotoEvents(p));

            Photos = new ReadOnlyObservableCollection<PhotoViewModel>(_photos);

            SetViewTypeCommand = new CommandHandler(SetViewType, canExecute => true);
        }

        private ObservableCollection<PhotoViewModel> _photos;
        public INotifyCollectionChanged Photos { get; private set; }

        public List<PhotoViewModel> SelectedPhotos
        {
            get { return _photos.Where(p => p.IsSelected == true).ToList(); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        private PhotoViewModel _selectedPhoto;
        public PhotoViewModel SelectedPhoto
        {
            get { return _selectedPhoto; }
            set
            {
                SetProperty(ref _selectedPhoto, value);
                if ((_selectedPhoto != null && !IsPhotoDetailsPaneOpen && ViewType != ViewType.FlipView) ||
                    (_selectedPhoto == null && IsPhotoDetailsPaneOpen && !(bool)IsPhotoDetailsPanePinned))
                    TogglePhotoDetailsPane();
            }
        }

        private PhotoViewModel _manipulatedPhoto;
        public PhotoViewModel ManipulatedPhoto
        {
            get { return _manipulatedPhoto; }
            set { SetProperty(ref _manipulatedPhoto, value); }
        }

        public bool IsSinglePhotoSelected { get { return SelectedPhotos?.Count == 1; } }

        public bool IsAnyPhotoSelected { get { return SelectedPhotos?.Count > 0; } }

        private double _photoDetailsOpenPaneLength = PHOTO_DETAILS_OPEN_PANE_LENGTH;
        public double PhotoDetailsOpenPaneLength
        {
            get { return _photoDetailsOpenPaneLength; }
            set { SetProperty(ref _photoDetailsOpenPaneLength, value); }
        }

        public double PhotoDetailsCompactPaneLength
        {
            get { return PHOTO_DETAILS_COMPACT_PANE_LENGTH; }
        }

        private bool _isPhotoDetailsPaneOpen;
        public bool IsPhotoDetailsPaneOpen
        {
            get { return _isPhotoDetailsPaneOpen; }
            set { SetProperty(ref _isPhotoDetailsPaneOpen, value); }
        }

        private bool _isPhotoDetailsPanePinned;
        public bool? IsPhotoDetailsPanePinned
        {
            get { return _isPhotoDetailsPanePinned; }
            set { SetProperty(ref _isPhotoDetailsPanePinned, value ?? false); }
        }

        private bool _isProgressRingVisible;
        public bool IsProgressRingVisible
        {
            get { return _isProgressRingVisible; }
            set { SetProperty(ref _isProgressRingVisible, value); }
        }

        private ViewType _viewType = ViewType.GridView;
        public ViewType ViewType
        {
            get { return _viewType; }
            set { SetProperty(ref _viewType, value, () => ViewTypeChanged()); }
        }

        public bool IsDeleted { get; private set; }

        #region Class Utilities
        public async void AddPhotosFromStorage()
        {
            IsProgressRingVisible = true;
            var files = await Utils.PickFiles(FileTypes.Images);
            IsProgressRingVisible = false;

            foreach (StorageFile file in files)
            {
                await AddPhoto(file);
            }
        }

        public async void Capture()
        {
            var cam = new CameraCaptureUI();
            cam.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Png;
            cam.PhotoSettings.MaxResolution = CameraCaptureUIMaxPhotoResolution.HighestAvailable;
            //cam.PhotoSettings.CroppedSizeInPixels = new Size(200, 200);

            StorageFile file = await cam.CaptureFileAsync(CameraCaptureUIMode.Photo);

            if (file != null)
                await AddPhoto(file);
        }

        public async void MoveSelectedPhotos()
        {
            var movePhotosDialog =
                new MovePhotosContentDialog(AlbumsVMManager.Albums.Where(a => a != this));

            ContentDialogResult result = await movePhotosDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var targetAlbum = movePhotosDialog.SelectedAlbum;

                if (ViewType == ViewType.GridView)
                    SelectedPhotos.ForEach(async photo => await MovePhoto(photo, targetAlbum));
                else if(ViewType == ViewType.FlipView)
                    await MovePhoto(SelectedPhoto, targetAlbum);
            }
        }

        public async void DeleteSelectedPhotos()
        {
            var result = await Utils.ShowContentDialog("Delete",
                "Are you sure you want to delete all selected photo(s)?", "Yes", "No");

            if (result == CustomContentDialogResult.Primary)
            {
                if (ViewType == ViewType.GridView)
                    SelectedPhotos.ForEach(photo => DeletePhoto(photo));
                else if (ViewType == ViewType.FlipView)
                    DeletePhoto(SelectedPhoto);
            }
        }

        public async void EditAlbum()
        {
            AlbumContentDialog albumDialog = new AlbumContentDialog(this);

            ShowAlbumDialog:

            ContentDialogResult result = await albumDialog.ShowAsync();
            if (result == ContentDialogResult.Secondary)
            {
                // Revert changes
                AlbumsVMManager.SetViewModelFromModel(this);
                return;
            }

            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    AlbumsVMManager.UpdateAlbum(this);
                }
                catch (DuplicateObjectException)
                {
                    await Utils.ShowContentDialog(
                        "Edit Album", "Name already exists. Please chose a different one.");
                    goto ShowAlbumDialog;
                }
                catch (RequiredConstraintViolationException)
                {
                    await Utils.ShowContentDialog(
                        "Edit Album", "You must set another album as 'Main', before unsetting 'Main' from this one.");
                    goto ShowAlbumDialog;
                }
                catch (Exception ex)
                {
                    await Utils.ShowContentDialog("Edit Album", ex.Message);
                    goto ShowAlbumDialog;
                }

                AlbumEdited?.Invoke(this, EventArgs.Empty);
            }
        }

        public async Task<bool> DeleteAlbum(object sender)
        {
            try
            {
                AlbumsVMManager.DeleteAlbum(this);
                _photos.ToList().ForEach(p => UnsubscribeFromPhotoEvents(p));
                IsDeleted = true;
                AlbumDeleted?.Invoke(this, EventArgs.Empty);
                return true;
            }
            catch (Exception ex)
            {
                await Utils.ShowContentDialog("Delete Album", ex.Message);
                return false;
            }
        }
        #endregion // Class Utilities

        #region Data Events Handlers
        private void Photos_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (var item in e.OldItems)
                {
                    UnsubscribeFromPhotoEvents(item as PhotoViewModel);
                }

            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                {
                    SubscribeToPhotoEvents(item as PhotoViewModel);
                }
        }

        private void Photo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PhotoViewModel.IsSelected))
                SetSelectedPhoto();
        }

        private void Photo_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            ManipulatedPhoto = sender as PhotoViewModel;
        }
        #endregion // Data Events Handlers

        #region UI Events Handlers
        public void PhotosGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var photo = e.ClickedItem as PhotoViewModel;
            if ((bool)(photo.IsSelected))
            {
                if (IsSinglePhotoSelected && !IsPhotoDetailsPaneOpen)
                    TogglePhotoDetailsPane();
                _isTapHandled = true;
            }
        }

        public void PhotosGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.RemovedItems.ToList()
                .ForEach(p => (p as PhotoViewModel).IsSelected = false);

            e.AddedItems.ToList()
                .ForEach(p => (p as PhotoViewModel).IsSelected = true);

            SetSelectedPhoto();

            _isTapHandled = IsAnyPhotoSelected;

            // Raise selected PropertyChanged events in order to update bindings
            OnPropertyChanged(nameof(IsAnyPhotoSelected));
            OnPropertyChanged(nameof(IsSinglePhotoSelected));
        }

        public void PhotosGridView_Tapped()
        {
            if (_isTapHandled)
            {
                _isTapHandled = false;
                return;
            }

            if (IsPhotoDetailsPaneOpen && !(bool)IsPhotoDetailsPanePinned)
                TogglePhotoDetailsPane();
        }

        public async void PhotosGridView_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                IsProgressRingVisible = true;
                var files = await e.DataView.GetStorageItemsAsync();
                IsProgressRingVisible = false;

                foreach (StorageFile file in files)
                {
                    if (file.ContentType.Contains("image"))
                        await AddPhoto(file);
                }
            }
        }

        public void PhotosGridView_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        public void TogglePhotoDetailsPane()
        {
            IsPhotoDetailsPaneOpen = !IsPhotoDetailsPaneOpen;
            if (IsPhotoDetailsPaneOpen)
                PhotoDetailsOpenPaneLength =
                    PHOTO_DETAILS_OPEN_PANE_LENGTH - PHOTO_DETAILS_COMPACT_PANE_LENGTH;
        }

        // This method is called when the pane is closed by UI
        public void PhotoDetailsPaneClosed()
        {
            if (!IsPhotoDetailsPaneOpen)
            {
                PhotoDetailsOpenPaneLength = PHOTO_DETAILS_OPEN_PANE_LENGTH;
                IsPhotoDetailsPanePinned = false;
            }
        }

        public void ShowSelectedPhotoDetails()
        {
            if (!IsPhotoDetailsPaneOpen)
                TogglePhotoDetailsPane();
        }

        public void PhotosFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewType == ViewType.FlipView)
            {
                // Set SelectedPhoto only when selection was changed by 
                // the user (and not when the control is loaded)
                if (e.AddedItems.Count == e.RemovedItems.Count)
                    SelectedPhoto = (sender as FlipView).SelectedItem as PhotoViewModel;
                else if ((sender as FlipView).SelectedItem == null)
                    (sender as FlipView).SelectedItem = SelectedPhoto;
            }
        }

        public void PhotosFlipView_Tapped()
        {
            if (IsPhotoDetailsPaneOpen && !(bool)IsPhotoDetailsPanePinned)
                TogglePhotoDetailsPane();
        }
        #endregion // UI Events Handlers

        #region Model Properties Proxies
        private bool? _isMain;

        public bool? IsMain
        {
            get { return _isMain ?? Model.IsMain; }
            internal set { SetProperty(ref _isMain, value ?? false); }
        }

        private string _title;

        [Required]
        [MaxLength(20, ErrorMessage = "Max. length is 20 characters")]
        public string Title
        {
            get { return _title ?? Model.Title; }
            set { SetProperty(ref _title, value); }
        }

        private DateTimeOffset? _dateCreated;

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTimeOffset? DateCreated
        {
            get { return _dateCreated ?? Model.DateCreated; }
            set { SetProperty(ref _dateCreated, value); }
        }

        private DateTimeOffset? _lastEdited;

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTimeOffset? LastEdited
        {
            get { return _lastEdited ?? Model.LastEdited; }
            set { SetProperty(ref _lastEdited, value); }
        }
        #endregion // Model Properties Proxies

        #region Model Methods Proxies
        public async Task<PhotoViewModel> AddPhoto(PhotoViewModel photo)
        {
            PhotoViewModel newPhoto = await AddPhoto(photo.ImagePath);
            newPhoto.SetAllPropertiesExceptForKeys(photo);
            return newPhoto;
        }

        public async Task<PhotoViewModel> AddPhoto(string imagePath)
        {
            StorageFile imageFile = await StorageFile.GetFileFromPathAsync(imagePath);
            return await AddPhoto(imageFile);
        }

        public async Task<PhotoViewModel> AddPhoto(StorageFile imageFile)
        {
            if (!imageFile.ContentType.Contains("image"))
                throw new ArgumentException("Input file is not an image");

            Photo photo = null;

            try
            {
                photo = await Model.AddPhoto(imageFile);
            }
            catch (DuplicateObjectException)
            {
                var result = await Utils.ShowContentDialog(
                    "Add Photo",
                    $"Image {imageFile.Name} already exists in album.\n" +
                    "What would you like to do?",
                    "Override", "Make a copy", "Cancel");

                switch (result)
                {
                    case CustomContentDialogResult.Primary:
                        photo = await Model.AddPhoto(imageFile, NameCollisionOption.ReplaceExisting);
                        break;
                    case CustomContentDialogResult.Secondary:
                        photo = await Model.AddPhoto(imageFile, NameCollisionOption.GenerateUniqueName);
                        break;
                    default:
                        return null;
                }
            }

            var newPhoto = new PhotoViewModel(photo);

            SubscribeToPhotoEvents(newPhoto);

            _photos.Add(newPhoto);

            return newPhoto;
        }

        public async Task MovePhoto(PhotoViewModel photo, AlbumViewModel targetAlbum)
        {
            await targetAlbum.AddPhoto(photo);
            DeletePhoto(photo);
        }

        internal void DeletePhoto(PhotoViewModel photo)
        {
            Model.DeletePhoto(photo.Model);
            UnsubscribeFromPhotoEvents(photo);
            _photos.Remove(photo);
        }
        #endregion // Model Methods Proxies

        #region Helper Methods
        private void ViewTypeChanged()
        {
            SetSelectedPhoto();

            if (IsPhotoDetailsPaneOpen && !(bool)IsPhotoDetailsPanePinned)
                TogglePhotoDetailsPane();
        }

        private void SetSelectedPhoto()
        {
            switch (ViewType)
            {
                case ViewType.GridView:
                    SelectedPhoto = IsSinglePhotoSelected ?
                        _photos.Single(p => p.IsSelected == true) : null;
                    break;
                case ViewType.FlipView:
                    if (SelectedPhoto == null)
                        SelectedPhoto = _photos.FirstOrDefault();
                    break;
                default:
                    break;
            }
        }

        private void SetViewType(object parameter)
        {
            if (Enum.IsDefined(typeof(ViewType), parameter))
                ViewType = (ViewType)parameter;
        }

        private void SubscribeToPhotoEvents(PhotoViewModel photo)
        {
            photo.PropertyChanging += Photo_PropertyChanging;
            photo.PropertyChanged += Photo_PropertyChanged;
        }

        private void UnsubscribeFromPhotoEvents(PhotoViewModel photo)
        {
            photo.PropertyChanging -= Photo_PropertyChanging;
            photo.PropertyChanged -= Photo_PropertyChanged;
        }
        #endregion // Helper Methods
    }
}
