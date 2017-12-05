using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    public class Photo : Model<Photo>
    {
        public Photo(uint albumId, string imagePath)
        {
            StartBatchUpdate();
            {
                AlbumId = albumId;
                ImagePath = imagePath;
            }
            EndBatchUpdate(doSave: false);

            _characters = new Collection<Character>();
            Characters = new ReadOnlyCollection<Character>(_characters);
        }

        // Used by deserialization
        private Photo() : this(default(uint), default(string))
        { }

        protected override Photo This { get { return this; } }

        internal override string[] AncestorsIds
        {
            get { return new[] { AlbumId.ToString() }; }
        }

        private uint _albumId;

        [JsonProperty]
        public uint AlbumId
        {
            get { return _albumId; }
            set { SetProperty(ref _albumId, value); }
        }

        private string _title;

        [JsonProperty]
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private string _description;

        [JsonProperty]
        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }

        private string _location;

        [JsonProperty]
        public string Location
        {
            get { return _location; }
            set { SetProperty(ref _location, value); }
        }

        private string _photographer;

        [JsonProperty]
        public string Photographer
        {
            get { return _photographer; }
            set { SetProperty(ref _photographer, value); }
        }

        [JsonProperty]
        private long _captureDateSerialized;

        private DateTimeOffset? _captureDate;
        public DateTimeOffset? CaptureDate
        {
            get { return _captureDate; }
            set { SetProperty(ref _captureDate, value); }
        }

        private string _imagePath;

        [JsonProperty]
        public string ImagePath
        {
            get { return _imagePath; }
            set { SetProperty(ref _imagePath, value); }
        }

        [JsonProperty]
        private Collection<Character> _characters;
        public IReadOnlyCollection<Character> Characters { get; private set; }

        public override bool Equals(Photo other)
        {
            return other != null
                && AlbumId == other.AlbumId && ImagePath == other.ImagePath;
        }

        public void AddOrUpdateCharacter(Character character)
        {
            var ch = _characters.FirstOrDefault(c => c == character);

            if (ch != null)
                ch.Update(character);
            else
                _characters.Add(character);

            Save();
        }

        public void DeleteCharacter(Character character)
        {
            _characters.Remove(character);
            Save();
        }

        internal override void Delete()
        {
            File.Delete(ImagePath);
            base.Delete();
        }

        // Update all properties except for Keys
        internal void SetAllPropertiesExceptForKeys(Photo photo)
        {
            StartBatchUpdate();
            {
                Title = photo.Title;
                Description = photo.Description;
                Location = photo.Location;
                Photographer = photo.Photographer;
                CaptureDate = photo.CaptureDate;
                _characters.Clear();
                photo._characters.ToList().ForEach(c => _characters.Add(c));
            }
            EndBatchUpdate();
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            if (CaptureDate.HasValue)
                _captureDateSerialized = ((DateTimeOffset)CaptureDate).ToFileTime();
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (_captureDateSerialized != 0)
                CaptureDate = DateTimeOffset.FromFileTime(_captureDateSerialized);

            Characters = new ReadOnlyCollection<Character>(_characters);

            EndBatchUpdate(doSave: false);
        }
    }
}
