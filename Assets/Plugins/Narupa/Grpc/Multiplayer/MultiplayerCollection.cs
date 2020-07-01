using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Narupa.Core;

namespace Narupa.Grpc.Multiplayer
{
    public class MultiplayerCollection
    {
       
    }
    
    /// <summary>
    /// Represents a collection of related keys in the multiplayer shared state, all of
    /// which will be deserialized to <typeparam name="TType"></typeparam>
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    public class MultiplayerCollection<TType> : MultiplayerCollection, IDictionary<string, TType>
    {
        private string prefix;
        private IRemoteSharedState sharedState;
        private Dictionary<string, MultiplayerResource<TType>> resources = new Dictionary<string, MultiplayerResource<TType>>();
          
        public event Action<string> ItemRemoved;

        public event Action<string> ItemUpdated;

        public event Action<string> ItemCreated;
        
        internal MultiplayerCollection(IRemoteSharedState sharedState, string prefix)
        {
            this.sharedState = sharedState;
            this.prefix = prefix;
            sharedState.ResourceCreated += SharedStateOnResourceCreated;
            sharedState.SharedStateRemoteKeyUpdated += SessionOnSharedStateDictionaryKeyUpdated;
        }

        private void SharedStateOnResourceCreated(string key, MultiplayerResource resource)
        {
            if (key.StartsWith(prefix))
            {
                var res = resource as MultiplayerResource<TType>;
                res.ValueRemoved += () =>
                {
                    if (resources.ContainsKey(key))
                    {
                        ItemRemoved?.Invoke(key);
                        resources.Remove(key);
                    }
                };
                res.ValueUpdated += () =>
                {
                    if (!resources.ContainsKey(key))
                    {
                        resources[key] = res;
                        ItemCreated?.Invoke(key);
                    }
                    else
                    {
                        ItemUpdated?.Invoke(key);
                    }
                };
                if (res.HasValue)
                {
                    resources[key] = res;
                    ItemCreated?.Invoke(key);
                }
            }
        }

        private void SessionOnSharedStateDictionaryKeyUpdated(string key, object value)
        {
            if (key.StartsWith(prefix) && !resources.ContainsKey(key))
            {
                sharedState.GetSharedResource<TType>(key);
            }
        }
        
        public IEnumerator<KeyValuePair<string, TType>> GetEnumerator()
        {
            foreach (var (key, resource) in resources)
                yield return new KeyValuePair<string, TType>(key, resource.Value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, TType> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            var resources = this.resources.Values.ToArray();
            foreach(var resource in resources)
                resource.Remove();
        }

        public bool Contains(KeyValuePair<string, TType> item)
        {
            return ContainsKey(item.Key) && Equals(this[item.Key], item.Value);
        }

        public void CopyTo(KeyValuePair<string, TType>[] array, int arrayIndex)
        {
            var i = arrayIndex;
            foreach (var pair in this)
                array[i++] = pair;
        }

        public bool Remove(KeyValuePair<string, TType> item)
        {
            return Remove(item.Key);
        }

        public int Count => resources.Count;
        public bool IsReadOnly => false;

        public void Add(string key, TType value)
        {
            this[key] = value;
        }

        public bool ContainsKey(string key)
        {
            return resources.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            if (!ContainsKey(key))
                return false;
            GetReference(key).Remove();
            return true;
        }

        public bool TryGetValue(string key, out TType value)
        {
            value = default;
            if (!resources.ContainsKey(key))
                return false;
            value = this[key];
            return true;
        }

        public TType this[string key]
        {
            set
            {
                if (!resources.ContainsKey(key))
                    sharedState.GetSharedResource<TType>(key).SetLocalValue(value);
                else
                    resources[key].SetLocalValue(value);
            }
            get => resources[key].Value;
        }

        public IEnumerable<string> Keys => resources.Keys;

        ICollection<TType> IDictionary<string, TType>.Values => resources.Values.Select(res => res.Value).ToArray();

        ICollection<string> IDictionary<string, TType>.Keys => resources.Keys;

        public IEnumerable<TType> Values => Keys.Select(key => this[key]);

        public MultiplayerResource<TType> GetReference(string key)
        {
            return resources[key];
        }
    }
}