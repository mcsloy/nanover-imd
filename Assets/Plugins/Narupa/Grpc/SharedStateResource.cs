using System;
using System.Threading.Tasks;
using Narupa.Core.Async;
using Narupa.Session;

namespace Narupa.Grpc.Multiplayer
{
    /// <summary>
    /// Represents a multiplayer resource that is shared across the multiplayer
    /// service.
    /// </summary>
    /// <typeparam name="TValue">The type to interpret the value of this key as.</typeparam>
    public class SharedStateResource<TValue>
    {
        /// <summary>
        /// Is the current value a local value that is pending being sent to the server.
        /// </summary>
        private bool localValuePending = false;
        
        /// <summary>
        /// Create a multiplayer resource.
        /// </summary>
        /// <param name="session">The multiplayer session that will provide this value.</param>
        /// <param name="key">The key that identifies this resource in the dictionary.</param>
        /// <param name="objectToValue">
        /// An optional converter for converting the value in
        /// the dictionary to an appropriate value.
        /// </param>
        /// <param name="valueToObject">
        /// An optional converter for converting the value
        /// provided to one suitable for serialisation to protobuf.
        /// </param>
        public SharedStateResource(ClientSharedState sharedState,
                                   string key)
        {
            this.sharedState = sharedState;
            ResourceKey = key;
            LockState = MultiplayerResourceLockState.Unlocked;
            sharedState.KeyUpdated += SharedStateDictionaryKeyUpdated;
            sharedState.KeyRemoved += SharedStateOnKeyRemoved;
        }

        public Converter<object, TValue> ObjectToValue { get; set; }

        public Converter<TValue, object> ValueToObject { get; set; }

        public TValue DefaultValue { get; set; } = default;

        /// <summary>
        /// Convert the value provided to one suitable for serialisation to protobuf.
        /// </summary>
        private object ConvertValueToObject(TValue value)
        {
            return ValueToObject != null ? ValueToObject(value) : value;
        }

        /// <summary>
        /// Convert the value in the dictionary to an appropriate value.
        /// </summary>
        private TValue ConvertObjectToValue(object obj)
        {
            if (ObjectToValue != null)
                return ObjectToValue(obj);
            if (obj is TValue v)
                return v;
            return default;
        }

        /// <summary>
        /// Callback for when the shared value is changed.
        /// </summary>
        public event Action RemoteValueChanged;

        /// <summary>
        /// Callback for when the value is changed, either remotely or locally.
        /// </summary>
        public event Action ValueChanged;

        /// <summary>
        /// Callback for when a lock request is accepted.
        /// </summary>
        public event Action LockAccepted;

        /// <summary>
        /// Callback for when a lock request is rejected.
        /// </summary>
        public event Action LockRejected;

        /// <summary>
        /// Callback for when a lock is released.
        /// </summary>
        public event Action LockReleased;

        private void SharedStateDictionaryKeyUpdated(string key, object value)
        {
            if (key == ResourceKey)
            {
                CopyRemoteValueToLocal();
                RemoteValueChanged?.Invoke();
            }
        }
        
        
        private void SharedStateOnKeyRemoved(string key)
        {
            if (key == ResourceKey)
            {
                CopyRemoteValueToLocal();
                RemoteValueChanged?.Invoke();
            }
        }

        private TValue GetRemoteValue()
        {
            if(sharedState.TryGetValue(ResourceKey, out var value))
                return ConvertObjectToValue(value);
            return GetDefaultValue();
        }

        private TValue GetDefaultValue()
        {
            return DefaultValue;
        }

        /// <summary>
        /// Copy the locally set version of this value to the multiplayer service.
        /// </summary>
        private void CopyLocalValueToRemote()
        {
            sharedState.SetValue(ResourceKey, ConvertValueToObject(value));
        }
        
        private TValue value;

        /// <summary>
        /// The key which identifies this resource.
        /// </summary>
        public readonly string ResourceKey;

        /// <summary>
        /// The current state of the lock on this resource.
        /// </summary>
        public MultiplayerResourceLockState LockState { get; private set; }

        private ClientSharedState sharedState;

        /// <summary>
        /// Value of this resource. Mirrors the value in the remote dictionary, unless a
        /// local change is in progress.
        /// </summary>
        public TValue Value => value;

        /// <summary>
        /// Obtain a lock on this resource.
        /// </summary>
        public void ObtainLock()
        {
            ObtainLockAsync().AwaitInBackground();
        }

        /// <summary>
        /// Release the lock on this resource.
        /// </summary>
        public void ReleaseLock()
        {
            CopyRemoteValueToLocal();
            ReleaseLockAsync().AwaitInBackground();
        }

        private async Task ObtainLockAsync()
        {
            LockState = MultiplayerResourceLockState.Pending;
            var success = await sharedState.LockKey(ResourceKey, 1f);
            LockState = success
                            ? MultiplayerResourceLockState.Locked
                            : MultiplayerResourceLockState.Unlocked;
            if (success)
                OnLockAccepted();
            else
                OnLockRejected();
        }

        private void OnLockAccepted()
        {
            if (localValuePending)
            {
                localValuePending = false;
                CopyLocalValueToRemote();
            }
            LockAccepted?.Invoke();
        }

        private void OnLockRejected()
        {
            localValuePending = false;
            CopyRemoteValueToLocal();
            LockRejected?.Invoke();
        }

        private async Task ReleaseLockAsync()
        {
            if (LockState != MultiplayerResourceLockState.Unlocked)
            {
                localValuePending = false;
                LockState = MultiplayerResourceLockState.Unlocked;
                CopyRemoteValueToLocal();
                LockReleased?.Invoke();
                await sharedState.ReleaseKey(ResourceKey);
            }
        }

        /// <summary>
        /// Set the local value of this key, and try to lock this resource to send it to
        /// everyone.
        /// If it is rejected, the value will revert to the default.
        /// </summary>
        public void UpdateValueWithLock(TValue value)
        {
            SetLocalValue(value);
            switch (LockState)
            {
                case MultiplayerResourceLockState.Unlocked:
                    ObtainLock();
                    return;
                case MultiplayerResourceLockState.Pending:
                    return;
                case MultiplayerResourceLockState.Locked:
                    CopyLocalValueToRemote();
                    return;
            }
        }

        private void SetLocalValue(TValue value)
        {
            this.value = value;
            localValuePending = true;
            ValueChanged?.Invoke();
        }
        
        /// <summary>
        /// Copy the remote value to this value.
        /// </summary>
        private void CopyRemoteValueToLocal()
        {
            if (!localValuePending)
            {
                value = GetRemoteValue();
                ValueChanged?.Invoke();
            }
        }

    }
}