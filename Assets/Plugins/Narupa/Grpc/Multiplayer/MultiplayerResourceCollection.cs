using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Narupa.Grpc.Multiplayer
{
    /// <summary>
    /// Represents a collection of related keys in the multiplayer shared state, all of
    /// which will be deserialized to <typeparam name="TType"></typeparam>
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    public class MultiplayerResourceCollection<TType> : IReadOnlyDictionary<string, VariableReference<TType>>, IDisposable
    {
        private string prefix;
        private MultiplayerSession session;

        public event Action<string> ItemRemoved;

        public event Action<string> ItemUpdated;

        public event Action<string> ItemCreated;

        private List<string> keys = new List<string>();

        public MultiplayerResourceCollection(MultiplayerSession session, string prefix)
        {
            this.session = session;
            this.prefix = prefix;
            session.SharedStateDictionaryKeyUpdated += SessionOnSharedStateDictionaryKeyUpdated;
            session.SharedStateDictionaryKeyRemoved += SessionOnSharedStateDictionaryKeyRemoved;
        }

        private void SessionOnSharedStateDictionaryKeyRemoved(string key)
        {
            if (key.StartsWith(prefix))
            {
                keys.Remove(key);
                ItemRemoved?.Invoke(key);
            }
        }

        private void SessionOnSharedStateDictionaryKeyUpdated(string key, object value)
        {
            if (key.StartsWith(prefix))
            {
                if(keys.Contains(key))
                    ItemUpdated?.Invoke(key);
                else
                    ItemCreated?.Invoke(key);
            }
        }
        
        public IEnumerator<KeyValuePair<string, VariableReference<TType>>> GetEnumerator()
        {
            foreach (var key in keys)
                yield return new KeyValuePair<string, VariableReference<TType>>(
                    key, session.GetReference<TType>(key));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => keys.Count;
        
        public bool ContainsKey(string key)
        {
            return keys.Contains(key);
        }

        public bool TryGetValue(string key, out VariableReference<TType> value)
        {
            value = default;
            if (!keys.Contains(key))
                return false;
            value = this[key];
            return true;
        }

        public VariableReference<TType> this[string key] => session.GetReference<TType>(key);

        public IEnumerable<string> Keys => keys;

        public IEnumerable<VariableReference<TType>> Values => Keys.Select(key => this[key]);

        public void Dispose()
        {
            session?.Dispose();
        }
    }
}