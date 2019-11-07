using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Narupa.Frame
{
    public class Selection : IReadOnlyList<int>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private ObservableCollection<int> indices = new ObservableCollection<int>();

        public Selection()
        {
            indices.CollectionChanged += (obj, args) => CollectionChanged?.Invoke(obj, args);
        }
        
        public IEnumerator<int> GetEnumerator()
        {
            return indices.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => indices.Count;

        public int this[int index] => indices[index];

        public void Add(int i)
        {
            if (!indices.Contains(i))
            {
                indices.Add(i);
            }
        }
        
        public void Remove(int i)
        {
            if (indices.Contains(i))
            {
                indices.Remove(i);
            }
        }

        public void Set(int i)
        {
            if (indices.Count != 1)
            {
                indices.Clear();
                indices.Add(i);
            }
            else if(indices[0] != i)
            {
                indices[0] = i;
            }
        }

        public void Clear()
        {
            if(indices.Count > 0)
                indices.Clear();;
        }
    }
}