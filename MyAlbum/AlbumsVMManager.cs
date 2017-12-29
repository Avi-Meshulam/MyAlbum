using MyAlbum.BL;
using MyAlbum.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Search;

namespace MyAlbum
{
    public static class AlbumsVMManager
    {
        private const string FIRST_ALBUM_TITLE = "My Album";
        private const string DEMO_MAIN_ALBUM = "Elements";

        private static Collection<AlbumViewModel> _albums;

        private static ReadOnlyCollection<AlbumViewModel> _Albums;
        public static ReadOnlyCollection<AlbumViewModel> Albums
        {
            get { return _Albums ?? (_Albums = GetAlbums()); }
        }

        private static AlbumViewModel MainAlbum
        {
            get { return _albums.SingleOrDefault(a => a.IsMain ?? false); }
        }

        #region Methods
        public static void AddAlbum(AlbumViewModel album)
        {
            if (album.HasErrors)
                throw new ArgumentException("Album contains invalid data");

            if (_albums.Contains(album))
                throw new DuplicateObjectException();

            SetModelFromViewModel(album);
            AlbumsManager.AddAlbum(album.Model);

            if ((album.IsMain ?? false) && MainAlbum != null)
                MainAlbum.IsMain = false;

            _albums.Add(album);
        }

        public static void UpdateAlbum(AlbumViewModel album)
        {
            if (album.HasErrors)
                throw new ArgumentException("Album contains invalid data");

            var existing = _albums.SingleOrDefault(a => a == album);

            if (existing != null && existing.Model.Id != album.Model.Id)
            {
                // Revert to previous values
                SetViewModelFromModel(album);
                throw new DuplicateObjectException();
            }

            if (!(album.IsMain ?? false) && MainAlbum == null)
            {
                // Undo IsMain change
                album.IsMain = true;
                throw new RequiredConstraintViolationException(
                    "You have to set another album as 'Main' before unsetting 'Main'");
            }

            if ((album.IsMain ?? false) && _albums.Where(a => a.IsMain ?? false).Count() > 1)
                UnsetPreviousMainAlbum(album);

            SetModelFromViewModel(album);
            AlbumsManager.UpdateAlbum(album.Model);
        }

        public static void DeleteAlbum(AlbumViewModel album)
        {
            if (album.IsMain ?? false)
                throw new RequiredConstraintViolationException("You cannot delete a 'Main' album");

            AlbumsManager.DeleteAlbum(album.Model);
            _albums.Remove(album);
        }

        public async static Task LoadDemoData()
        {
            StorageFolder appInstalledFolder = Package.Current.InstalledLocation;
            StorageFolder albumsFolder = await appInstalledFolder.GetFolderAsync(@"Assets\Albums");

            var fileTypesFilter = new List<string>
            {
                ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tiff", ".jxr", ".ico" //, ".svg"
                // *.svg files are supported in Windows 10, starting from version 1703
            };

            QueryOptions queryOptions = new QueryOptions(CommonFileQuery.OrderByName, fileTypesFilter);

            foreach (var folder in await albumsFolder.GetFoldersAsync())
            {
                var album = new AlbumViewModel { Title = folder.Name };

                if (folder.Name == DEMO_MAIN_ALBUM && MainAlbum == null)
                    album.IsMain = true;

                AddAlbum(album);

                var files = await folder.CreateFileQueryWithOptions(queryOptions).GetFilesAsync();

                foreach (var file in files)
                {
                    try
                    {
                        var photo = await album.AddPhoto(file);
                        photo.PopulateWithRandomData(file.DisplayName.SplitCamelCase());
                    }
                    catch (Exception ex)
                    {
                        await Utils.ShowContentDialog("Add Photo", ex.Message);
                    }
                }
            }
        }

        public static void CreateMainAlbum()
        {
            AddAlbum(new AlbumViewModel { Title = FIRST_ALBUM_TITLE, IsMain = true });
        }

        internal static void SetModelFromViewModel(AlbumViewModel album)
        {
            album.Model.IsMain = album.IsMain ?? false;
            album.Model.Title = album.Title;
            album.Model.LastEdited = album.LastEdited;
        }

        internal static void SetViewModelFromModel(AlbumViewModel album)
        {
            album.IsMain = album.Model.IsMain;
            album.Title = album.Model.Title;
            album.LastEdited = album.Model.LastEdited;
        }
        #endregion // Class Methods

        #region Helper Methods
        private static ReadOnlyCollection<AlbumViewModel> GetAlbums()
        {
            _albums = new Collection<AlbumViewModel>(
                AlbumsManager.Albums.Select(album => new AlbumViewModel(album)).ToList());

            return new ReadOnlyCollection<AlbumViewModel>(_albums);
        }

        private static void UnsetPreviousMainAlbum(AlbumViewModel newMainAlbum)
        {
            var mainAlbum = _albums.SingleOrDefault(
                a => (a.IsMain ?? false) && a.Model.Id != newMainAlbum.Model.Id);
            mainAlbum.IsMain = false;
        }
        #endregion // Helper Methods
    }
}
