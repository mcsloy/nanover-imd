using System;

namespace Narupa.Grpc.Multiplayer
{
    public class VariableReference
    {
    }

    public class VariableReference<TType> : VariableReference, IDisposable
    {
        private MultiplayerSession session;
        private string key;

        public VariableReference(MultiplayerSession session, string key)
        {
            this.key = key;
            this.session = session;
            session.SharedStateDictionaryKeyUpdated += OnMultiplayerKeyUpdated;
            session.SharedStateDictionaryKeyRemoved += OnMultiplayerKeyRemoved;
            Value = Deserialize(session.GetSharedState(key));
        }

        private void OnMultiplayerKeyUpdated(string key, object value)
        {
            if (key == this.key)
            {
                Value = Deserialize(value);
                Updated?.Invoke();
                Changed?.Invoke(Value);
            }
        }

        private void OnMultiplayerKeyRemoved(string key)
        {
            if (key == this.key)
            {
                Removed?.Invoke();
                Changed?.Invoke(default);
            }
        }

        public void Dispose()
        {
            session.SharedStateDictionaryKeyUpdated -= OnMultiplayerKeyUpdated;
            session.SharedStateDictionaryKeyRemoved -= OnMultiplayerKeyRemoved;
        }

        public TType Value { get; private set; }

        public event Action Removed;

        public event Action Updated;

        public event Action<TType> Changed;

        private static TType Deserialize(object value)
        {
            if (value == null)
                return default;
            return Serialization.Serialization.FromDataStructure<TType>(value);
        }
    }
}