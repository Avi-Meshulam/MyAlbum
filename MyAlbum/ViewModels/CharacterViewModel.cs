using MyAlbum.BL.Models;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace MyAlbum.ViewModels
{
    public class CharacterViewModel : ValidatorChangeNotifier, IVariableDisplayProperty
    {
        internal event EventHandler CharacterDeleted;
        internal event EventHandler<PropertyChangedEventArgs> EditCompleted;

        public CharacterViewModel() : this(new Character())
        { }

        public CharacterViewModel(string name) : this(new Character(name))
        { }

        public CharacterViewModel(Character character)
        {
            Model = character ?? new Character();
            Validate();
        }

        public Character Model { get; set; }

        private string _errorsForNameProperty;
        public string ErrorsForNameProperty
        {
            get { return _errorsForNameProperty = GetAggregatedErrors(nameof(Name)); }
            set { SetProperty(ref _errorsForNameProperty, value); }
        }

        PropertyInfo _variableDisplayPropertyInfo;
        PropertyInfo IVariableDisplayProperty.VariableDisplayPropertyInfo
        {
            get
            {
                return _variableDisplayPropertyInfo ??
                    (_variableDisplayPropertyInfo = GetType().GetProperty(nameof(Name)));
            }
        }

        public void NameEditCompleted()
        {
            EditCompleted?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        }

        #region Class Utilities
        public void Delete()
        {
            CharacterDeleted?.Invoke(this, EventArgs.Empty);
        }
        #endregion // Class Utilities

        #region Model Properties Proxies
        private string _name;

        [Required(ErrorMessage = "Name cannot be empty")]
        [MinLength(1, ErrorMessage = "Name cannot be empty")]
        [MaxLength(20, ErrorMessage = "Max. length is 20 characters")]
        public string Name
        {
            get { return PropertyHasErrors() ? _name : Model.Name; }
            set
            {
                SetProperty(ref _name, value, () => Model.Name = value);
                ErrorsForNameProperty = GetAggregatedErrors(nameof(Name));
            }
        }
        #endregion // Model Properties Proxies
    }
}