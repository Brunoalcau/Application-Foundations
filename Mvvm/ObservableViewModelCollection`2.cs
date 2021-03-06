﻿namespace Tvl.UI.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Diagnostics.Contracts;
    using System.Linq;

    public class ObservableViewModelCollection<TViewModel, TModel> : ObservableCollection<TViewModel>
    {
        private readonly INotifyCollectionChanged _source;
        private readonly Func<TModel, TViewModel> _viewModelFactory;

        public ObservableViewModelCollection(ReadOnlyObservableCollection<TModel> source, Func<TModel, TViewModel> viewModelFactory)
            : this((INotifyCollectionChanged)source, viewModelFactory)
        {
            Contract.Requires(source != null);
            Contract.Requires(viewModelFactory != null);
        }

        public ObservableViewModelCollection(ObservableCollection<TModel> source, Func<TModel, TViewModel> viewModelFactory)
            : this((INotifyCollectionChanged)source, viewModelFactory)
        {
            Contract.Requires(source != null);
            Contract.Requires(viewModelFactory != null);
        }

        public ObservableViewModelCollection(INotifyCollectionChanged source, Func<TModel, TViewModel> viewModelFactory)
            : base(((IEnumerable<TModel>)source).Select(model => viewModelFactory(model)))
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (viewModelFactory == null)
                throw new ArgumentNullException("viewModelFactory");
            Contract.EndContractBlock();

            this._source = source;
            this._viewModelFactory = viewModelFactory;
            this._source.CollectionChanged += OnSourceCollectionChanged;
        }

        protected virtual TViewModel CreateViewModel(TModel model)
        {
            return _viewModelFactory(model);
        }

        private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
            case NotifyCollectionChangedAction.Add:
                for (int i = 0; i < e.NewItems.Count; i++)
                {
                    this.Insert(e.NewStartingIndex + i, CreateViewModel((TModel)e.NewItems[i]));
                }
                break;

            case NotifyCollectionChangedAction.Move:
                if (e.OldItems.Count == 1)
                {
                    this.Move(e.OldStartingIndex, e.NewStartingIndex);
                }
                else
                {
                    List<TViewModel> items = this.Skip(e.OldStartingIndex).Take(e.OldItems.Count).ToList();
                    for (int i = 0; i < e.OldItems.Count; i++)
                        this.RemoveAt(e.OldStartingIndex);

                    for (int i = 0; i < items.Count; i++)
                        this.Insert(e.NewStartingIndex + i, items[i]);
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                for (int i = 0; i < e.OldItems.Count; i++)
                    this.RemoveAt(e.OldStartingIndex);
                break;

            case NotifyCollectionChangedAction.Replace:
                // remove
                for (int i = 0; i < e.OldItems.Count; i++)
                    this.RemoveAt(e.OldStartingIndex);

                // add
                goto case NotifyCollectionChangedAction.Add;

            case NotifyCollectionChangedAction.Reset:
                Clear();
                for (int i = 0; i < e.NewItems.Count; i++)
                    this.Add(CreateViewModel((TModel)e.NewItems[i]));
                break;

            default:
                break;
            }
        }
    }
}
