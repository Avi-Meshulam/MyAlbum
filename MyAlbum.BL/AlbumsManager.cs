using MyAlbum.BL.Models;
using MyLibrary.DAL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAlbum.BL
{
    public static class AlbumsManager
    {
        private static Collection<Album> _albums;

        private static ReadOnlyCollection<Album> _Albums;
        public static ReadOnlyCollection<Album> Albums
        {
            get
            {
                return _Albums ?? (_Albums = new ReadOnlyCollection<Album>(_albums = LoadAlbums()));
            }
        }

        private static Album MainAlbum
        {
            get { return _albums.SingleOrDefault(a => a.IsMain); }
        }

        #region Methods
        public static void AddAlbum(Album album)
        {
            if (_albums.Contains(album))
                throw new DuplicateObjectException();

            if (album.IsMain && MainAlbum != null)
                MainAlbum.IsMain = false;

            album.Save();
            _albums.Add(album);
        }

        public static void UpdateAlbum(Album album)
        {
            var existing = _albums.SingleOrDefault(a => a == album);

            if (existing != null && existing.Id != album.Id)
            {
                // Undo change => Restore album from storage
                album = DALManager.Read<Album>(album.Id, album.AncestorsIds);
                throw new DuplicateObjectException();
            }

            if (!album.IsMain && MainAlbum == null)
            {
                // Undo IsMain change
                album.IsMain = true;
                throw new RequiredConstraintViolationException();
            }

            if (album.IsMain && _albums.Where(a => a.IsMain).Count() > 1)
                UnsetPreviousMainAlbum(album);

            album.Save();
        }

        public static void DeleteAlbum(Album album)
        {
            if (album.IsMain)
                throw new RequiredConstraintViolationException("You cannot delete a 'Main' album");

            album.Delete();
            _albums.Remove(album);
        }
        #endregion // Class Methods

        #region Helper Methods
        private static Collection<Album> LoadAlbums()
        {
            return DALManager.ReadCollection<Album>();
        }

        private static void UnsetPreviousMainAlbum(Album newMainAlbum)
        {
            var mainAlbums = _albums.Where(a => a.IsMain);
            var previousMainAlbum = mainAlbums.FirstOrDefault(a => a.Id != newMainAlbum.Id);
            if (previousMainAlbum != null)
            {
                previousMainAlbum.IsMain = false;
                previousMainAlbum.Save();
            }
        }
        #endregion // Helper Methods
    }
}
