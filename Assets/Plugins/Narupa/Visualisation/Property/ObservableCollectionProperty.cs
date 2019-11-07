using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Narupa.Visualisation.Property
{
    /// <summary>
    /// Wraps a collection which provides notifications that the collection has updated, and exposes it as an array for visualisation purposes.
    /// </summary>
    public class ObservableArrayProperty<TValue, TArray> : IReadOnlyProperty<TValue[]>
        where TArray : IEnumerable<TValue>, INotifyCollectionChanged
    {
        public ObservableArrayProperty(TArray value)
        {
            collection = value;
            collection.CollectionChanged += (obj, args) => UpdateArray();
        }

        private void UpdateArray()
        {
            Value = collection.ToArray();
            ValueChanged?.Invoke();
        }

        private readonly TArray collection;

        public TValue[] Value { get; private set; } = new TValue[0];

        public bool HasValue => true;
        
        public event Action ValueChanged;
    }

    public static class ObservableArrayExtensions
    {
        public static ObservableArrayProperty<TValue, TArray> AsVisualisationProperty<TValue, TArray>(this TArray value)
            where TArray : INotifyCollectionChanged, IEnumerable<TValue>
        {
            return new ObservableArrayProperty<TValue, TArray>(value);
        }
    }
}