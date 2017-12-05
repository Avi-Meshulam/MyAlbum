using MyAlbum.BL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyAlbum.ViewModels
{
    public abstract class ViewModelBase<T> : ValidatorChangeNotifier, IEquatable<ViewModelBase<T>>
        where T : Model<T>
    {
        public ViewModelBase(T model)
        {
            _model = model;
            Validate();
        }

        private readonly T _model;
        internal T Model { get { return _model; } }

        private static GUIUtils _guiUtils;
        public GUIUtils GUIUtils { get { return _guiUtils ?? (_guiUtils = new GUIUtils()); } }

        public bool IsBatchUpdate { get { return Model.IsBatchUpdate; } }

        /// <summary>
        /// Signifies not to save the model until EndBatchUpdate() is called
        /// </summary>
        public void StartBatchUpdate()
        {
            Model.StartBatchUpdate();
        }

        /// <summary>
        /// Ends batch update state and saves the model if doSave == true
        /// </summary>
        public void EndBatchUpdate(bool doSave = true)
        {
            Model.EndBatchUpdate(doSave);
        }

        public bool Equals(ViewModelBase<T> other)
        {
            return _model.Equals(other._model);
        }
    }
}