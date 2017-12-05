using MyLibrary.DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MyAlbum.BL.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Model<T> : Equatable<T> where T : Model<T>
    {
        private static uint _nextId = 1;

        protected abstract T This { get; }

        // Hierarchy of ancestors identifiers (used for storage)
        internal virtual string[] AncestorsIds { get; }

        [JsonProperty]
        public uint Id { get; protected set; }

        internal virtual void Save()
        {
            if (Id == default(uint))
                Id = _nextId++;

            DALManager.Write(This, Id, AncestorsIds);
        }

        internal virtual void Delete()
        {
            DALManager.Delete<T>(Id, AncestorsIds);
        }

        internal ModelSaveMode SaveMode { get; set; } = ModelSaveMode.Implicit;

        private bool _isBatchUpdate;
        public bool IsBatchUpdate { get { return _isBatchUpdate; } }

        /// <summary>
        /// Signifies not to save the model until EndBatchUpdate() is called
        /// </summary>
        public void StartBatchUpdate()
        {
            _isBatchUpdate = true;
        }

        /// <summary>
        /// Ends batch update state and saves the model if doSave == true
        /// </summary>
        public void EndBatchUpdate(bool doSave = true)
        {
            if (doSave && SaveMode == ModelSaveMode.Implicit)
                Save();
            _isBatchUpdate = false;
        }

        public override bool Equals(T other)
        {
            return other != null && Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        // Compares field's current and new values, and if they're different - 
        // sets the new value and raises a PropertyChanged event, allowing subscribers to react.
        protected void SetProperty<propType>(ref propType currentValue, propType newValue,
            [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<propType>.Default.Equals(currentValue, newValue))
                return;

            currentValue = newValue;

            if (SaveMode == ModelSaveMode.Implicit && !_isBatchUpdate)
                Save();
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            StartBatchUpdate();
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (Id >= _nextId)
                _nextId = Id + 1;
        }
    }
}