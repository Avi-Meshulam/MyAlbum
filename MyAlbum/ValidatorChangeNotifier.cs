using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace MyAlbum
{
    public abstract class ValidatorChangeNotifier : INotifyPropertyChanging, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private ConcurrentDictionary<string, ICollection<string>> _validationErrors =
            new ConcurrentDictionary<string, ICollection<string>>();

        #region INotifyPropertyChanging members
        public event PropertyChangingEventHandler PropertyChanging;
        protected virtual void OnPropertyChanging(string propertyName)
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }
        #endregion // INotifyPropertyChanging members

        #region INotifyPropertyChanged members
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion // INotifyPropertyChanged members

        #region INotifyDataErrorInfo members
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        protected void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            ICollection<string> errorsForName = null;
            _validationErrors.TryGetValue(propertyName, out errorsForName);
            return errorsForName;
        }

        private bool _hasErrors;
        public bool HasErrors
        {
            get { return _hasErrors = _validationErrors.Any(kv => kv.Value != null && kv.Value.Count > 0); }
        }
        #endregion // INotifyDataErrorInfo members

        #region INotifyDataErrorInfo Extensions
        public string GetAggregatedErrors(string propertyName)
        {
            return GetErrors(propertyName)?.Cast<string>().Aggregate((a, b) => $"{a}\n{b}");
        }

        public bool PropertyHasErrors([CallerMemberName] string propertyName = null)
        {
            return _validationErrors.Any(kv => kv.Key == propertyName && kv.Value != null && kv.Value?.Count > 0);
        }
        #endregion INotifyDataErrorInfo Extensions

        // Data-annotations-based validation
        protected void Validate()
        {
            _validationErrors.Clear();

            ICollection<ValidationResult> validationResults = new List<ValidationResult>();

            ValidationContext validationContext = new ValidationContext(this, null, null);

            bool isValid = Validator.TryValidateObject(this, validationContext, validationResults, true);

            foreach (ValidationResult validationResult in validationResults)
            {
                string propertyName = validationResult.MemberNames.ElementAt(0);

                _validationErrors.AddOrUpdate(propertyName, new List<string> { validationResult.ErrorMessage },
                    (prop, propErrors) =>
                    {
                        propErrors.Add(validationResult.ErrorMessage);
                        return propErrors;
                    });

                RaiseErrorsChanged(propertyName);
            }

            if (_hasErrors == isValid)  // _hasErrors != (!isValid)
            {
                _hasErrors = isValid;
                OnPropertyChanged(nameof(HasErrors));
            }
        }

        // Data-annotations-based validation
        protected void ValidateProperty(object propertyValue, string propertyName)
        {
            ICollection<string> propErrors;
            _validationErrors.TryRemove(propertyName, out propErrors);

            ICollection<ValidationResult> validationResults = new List<ValidationResult>();

            ValidationContext validationContext =
                new ValidationContext(this, null, null) { MemberName = propertyName };

            bool isValid = Validator.TryValidateProperty(propertyValue, validationContext, validationResults);

            if (!isValid)
            {
                _validationErrors.TryAdd(propertyName, validationResults.Select(r => r.ErrorMessage).ToList());
            }

            RaiseErrorsChanged(propertyName);
        }

        // Compares field's current and new values.
        // If values are equal => Returns.
        // Otherwise => Validates field agaisnt its validation attributes and if 
        // validation succeeds => Calls successCallback().
        // Raises a PropertyChanged event, even if validation fails.
        protected void SetProperty<T>(ref T currentValue, T newValue,
            Action validationSuccessCallback = null, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(currentValue, newValue))
                return;

            OnPropertyChanging(propertyName);
            
            currentValue = newValue;

            ValidateProperty(newValue, propertyName);

            if (!PropertyHasErrors(propertyName))
                validationSuccessCallback?.Invoke();

            OnPropertyChanged(propertyName);
        }
    }
}
