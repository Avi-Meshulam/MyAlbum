using MyLibrary.DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyAlbum.BL.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Album : Model<Album>
    {
        public Album(string title, bool isMain = false)
        {
            SaveMode = ModelSaveMode.Explicit;

            Title = title;
            IsMain = isMain;

            _photos = new Collection<Photo>();
            Photos = new ReadOnlyCollection<Photo>(_photos);
        }

        // Used by deserialization
        public Album() : this(default(string))
        { }

        protected override Album This { get { return this; } }

        private Collection<Photo> _photos;
        public IReadOnlyCollection<Photo> Photos { get; private set; }

        private StorageFolder _imagesFolder;
        internal async Task<StorageFolder> GetImagesFolder()
        {
            return _imagesFolder ?? (_imagesFolder = 
                await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync(
                    Id.ToString(), CreationCollisionOption.OpenIfExists));
        }

        private bool _isMain;

        [JsonProperty]
        public bool IsMain
        {
            get { return _isMain; }
            set { SetProperty(ref _isMain, value); }
        }

        private string _title;

        [JsonProperty]
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        [JsonProperty]
        private long _dateCreatedSerialized;

        public DateTimeOffset? DateCreated { get; private set; } = DateTimeOffset.Now;

        [JsonProperty]
        private long _lastEditedSerialized;

        private DateTimeOffset? _lastEdited = DateTimeOffset.Now;
        public DateTimeOffset? LastEdited
        {
            get { return _lastEdited; }
            set { SetProperty(ref _lastEdited, value); }
        }

        public override bool Equals(Album other)
        {
            return other != null && Title == other.Title;
        }

        internal override void Save()
        {
            LastEdited = DateTimeOffset.Now;
            base.Save();
        }

        public async Task<Photo> AddPhoto(Photo photo,
            NameCollisionOption option = NameCollisionOption.FailIfExists)
        {
            Photo newPhoto = await AddPhoto(photo.ImagePath);
            newPhoto.SetAllPropertiesExceptForKeys(photo);
            return newPhoto;
        }

        public async Task<Photo> AddPhoto(string imagePath, 
            NameCollisionOption option = NameCollisionOption.FailIfExists)
        {
            var imageFile = await StorageFile.GetFileFromPathAsync(imagePath);
            return await AddPhoto(imageFile, option);
        }

        public async Task<Photo> AddPhoto(StorageFile imageFile,
            NameCollisionOption option = NameCollisionOption.FailIfExists)
        {
            var albumFolder = await GetImagesFolder();
            var targetPath = Path.Combine(albumFolder.Path, imageFile.Name);

            var photo = new Photo(Id, targetPath);

            if (Photos.Contains(photo))
                throw new DuplicateObjectException();

            var fileCopy = await imageFile.CopyAsync(albumFolder, imageFile.Name, option);

            photo.Save();

            _photos.Add(photo);

            return photo;
        }

        public void DeletePhoto(Photo photo)
        {
            photo.Delete();
            _photos.Remove(photo);
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            if (DateCreated.HasValue)
                _dateCreatedSerialized = ((DateTimeOffset)DateCreated).ToFileTime();
            if (LastEdited.HasValue)
                _lastEditedSerialized = ((DateTimeOffset)LastEdited).ToFileTime();
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (_dateCreatedSerialized != 0)
                DateCreated = DateTimeOffset.FromFileTime(_dateCreatedSerialized);
            if (_lastEditedSerialized != 0)
                LastEdited = DateTimeOffset.FromFileTime(_lastEditedSerialized);

            _photos = new Collection<Photo>(DALManager.ReadCollection<Photo>(Id.ToString()));
            Photos = new ReadOnlyCollection<Photo>(_photos);

            EndBatchUpdate(doSave: false);
        }
    }
}
