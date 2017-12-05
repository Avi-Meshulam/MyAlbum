using MyAlbum.BL.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MyAlbum.ViewModels
{
    public class PhotoViewModel : ViewModelBase<Photo>
    {
        // Google Maps API template
        private string _locationTemplate =
            "http://maps.googleapis.com/maps/api/geocode/xml?latlng={0},{1}&sensor=false";

        public ICommand SelectionChangedCommand;

        public PhotoViewModel(Photo photo) : base(photo)
        {
            _characters = new ObservableCollection<CharacterViewModel>(
                Model.Characters.Select(ch => new CharacterViewModel(ch)));

            _characters.CollectionChanged += Characters_CollectionChanged;
            _characters.ToList().ForEach(c => SubscribeToCharacterEvents(c));

            Characters = new ReadOnlyObservableCollection<CharacterViewModel>(_characters);

            SelectionChangedCommand = new CommandHandler(SelectionChanged, canExecute => true);
        }

        public PhotoViewModel(uint albumId, string imagePath)
            : this(new Photo(albumId, imagePath))
        { }

        private ObservableCollection<CharacterViewModel> _characters;
        public INotifyCollectionChanged Characters { get; private set; }

        private bool _isSelected;
        public bool? IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value ?? false); }
        }

        #region Class Utilities
        public async void SetCurrentLocation()
        {
            var checkAccess = await Geolocator.RequestAccessAsync();
            if (checkAccess == GeolocationAccessStatus.Denied)
            {
                await Utils.ShowContentDialog("Set Location",
                    "You need to enable location on your device in order to use this feature.\n" +
                    "See: Settings -> Privacy");
                return;
            }

            var geoLocator = new Geolocator();
            geoLocator.DesiredAccuracy = PositionAccuracy.High;
            geoLocator.DesiredAccuracyInMeters = 5;

            Geoposition position = await geoLocator.GetGeopositionAsync();
            BasicGeoposition point = position.Coordinate.Point.Position;

            Location = await GetLocationName(point.Latitude.ToString(), point.Longitude.ToString());
        }

        public CharacterViewModel AddCharacter()
        {
            var character = new CharacterViewModel();
            SubscribeToCharacterEvents(character);
            _characters.Add(character);
            return character;
        }

        public void AddNamedCharacter(string name)
        {
            var character = AddCharacter();
            character.Name = name;
        }
        #endregion // Class Utilities

        #region Data Events Handlers
        private void Characters_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (var item in e.OldItems)
                {
                    UnsubscribeFromCharacterEvents(item as CharacterViewModel);
                }

            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                {
                    SubscribeToCharacterEvents(item as CharacterViewModel);
                }
        }

        private void Character_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var character = sender as CharacterViewModel;

            if (!character.HasErrors)
                Model.AddOrUpdateCharacter(character.Model);
        }

        private void Character_EditCompleted(object sender, EventArgs e)
        {
            var character = sender as CharacterViewModel;

            // Add/Remove changed character in order to refresh characters collection's in UI
            int index = _characters.IndexOf(character);
            _characters.RemoveAt(index);
            _characters.Insert(index, character);
        }

        private void Character_CharacterDeleted(object sender, EventArgs e)
        {
            DeleteCharacter(sender as CharacterViewModel);
        }
        #endregion // Data Events Handlers

        #region UI Events Handlers
        public void SelectionChanged(object parameter = null)
        {
            var gridView = parameter as GridView;
            var items = gridView.Items;
            var selectedItems = gridView.SelectedItems;

            foreach (PhotoViewModel photo in items)
            {
                if (photo._isSelected)
                {
                    if (!selectedItems.Contains(photo))
                        selectedItems.Add(photo);
                }
                else
                {
                    if (selectedItems.Contains(photo))
                        selectedItems.Remove(photo);
                }
            }
        }
        #endregion // UI Events Handlers

        #region Model Properties Proxies

        private string _title;

        [MaxLength(30, ErrorMessage = "Max. length is 30 characters")]
        public string Title
        {
            get { return PropertyHasErrors() ? _title : Model.Title; }
            set { SetProperty(ref _title, value, () => Model.Title = value); }
        }

        private string _description;

        [MaxLength(500, ErrorMessage = "Max. length is 500 characters")]
        public string Description
        {
            get { return PropertyHasErrors() ? _description : Model.Description; }
            set { SetProperty(ref _description, value, () => Model.Description = value); }
        }

        private string _location;

        [MaxLength(40, ErrorMessage = "Max. length is 40 characters")]
        public string Location
        {
            get { return PropertyHasErrors() ? _location : Model.Location; }
            set { SetProperty(ref _location, value, () => Model.Location = value); }
        }

        private string _photographer;

        [MaxLength(20, ErrorMessage = "Max. length is 20 characters")]
        public string Photographer
        {
            get { return PropertyHasErrors() ? _photographer : Model.Photographer; }
            set { SetProperty(ref _photographer, value, () => Model.Photographer = value); }
        }

        private DateTimeOffset? _captureDate;

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTimeOffset? CaptureDate
        {
            get { return PropertyHasErrors() ? _captureDate : Model.CaptureDate; }
            set { SetProperty(ref _captureDate, value, () => Model.CaptureDate = value); }
        }

        private string _imagePath;

        [Key]
        [Required(ErrorMessage = "Image path cannot be empty")]
        public string ImagePath
        {
            get { return Model.ImagePath; }
            set { SetProperty(ref _imagePath, value, () => Model.ImagePath = value); }
        }
        #endregion // Model Properties Proxies

        #region Model Methods Proxies
        public void DeleteCharacter(CharacterViewModel character)
        {
            Model.DeleteCharacter(character.Model);
            UnsubscribeFromCharacterEvents(character);
            _characters.Remove(character);
        }
        #endregion // Model Methods Proxies

        #region Helper Methdos
        private void SubscribeToCharacterEvents(CharacterViewModel character)
        {
            character.CharacterDeleted += Character_CharacterDeleted;
            character.PropertyChanged += Character_PropertyChanged;
            character.EditCompleted += Character_EditCompleted;
        }

        private void UnsubscribeFromCharacterEvents(CharacterViewModel character)
        {
            character.CharacterDeleted -= Character_CharacterDeleted;
            character.PropertyChanged -= Character_PropertyChanged;
            character.EditCompleted -= Character_EditCompleted;
        }

        private async Task<string> GetLocationName(string latitude, string longitude)
        {
            string location = default(string);

            string requestUri = string.Format(_locationTemplate, latitude, longitude);

            using (HttpClient client = new HttpClient())
            {
                // Use Google API web service to get location
                string response = await client.GetStringAsync(requestUri);

                // Parse response
                XElement xResponse = XElement.Parse(response);
                XElement xStatus = xResponse.Descendants().FirstOrDefault(elm => elm.Name == "status");

                if (xStatus.Value.ToLower() == "ok")
                    location = xResponse.Descendants().FirstOrDefault(
                        elm => elm.Name == "formatted_address")?.Value;
            }

            return location;
        }

        internal void PopulateWithRandomData(string title)
        {
            StartBatchUpdate();
            {
                Title = title;
                CaptureDate = Utils.GetRandomDate(5);
                Photographer = Utils.GetRandomName();
                int numberOfCharacters = Utils.GetRandom(1, 5);
                for (int i = 0; i < numberOfCharacters; i++)
                {
                    if (Utils.GetRandom() % 2 == 0)
                        AddNamedCharacter(Utils.GetRandomName());
                    else
                        AddNamedCharacter(Utils.GetRandomFirstName());
                }
            }
            EndBatchUpdate();
        }

        // Update all properties except for Keys
        internal void SetAllPropertiesExceptForKeys(PhotoViewModel photo)
        {
            StartBatchUpdate();
            {
                Title = photo.Title;
                Description = photo.Description;
                Location = photo.Location;
                Photographer = photo.Photographer;
                CaptureDate = photo.CaptureDate;
                _characters.Clear();
                photo._characters.ToList().ForEach(c => AddNamedCharacter(c.Name));
            }
            EndBatchUpdate();
        }
        #endregion // Helper Methdos
    }
}
