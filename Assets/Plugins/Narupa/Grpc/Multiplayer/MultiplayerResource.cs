using System;
using System.Threading.Tasks;
using Narupa.Core.Async;

namespace Narupa.Grpc.Multiplayer
{
    /// <summary>
    /// Represents a reference to an item in the multiplayer shared state. They can only be created
    /// by the <see cref="MultiplayerSession"/>, which ensures for a given shared state there is
    /// exactly one <see cref="MultiplayerResource"/> for each key.
    /// </summary>
    public abstract class MultiplayerResource
    {
        /// <summary>
        /// Does this multiplayer resource have a value?
        /// </summary>
        public abstract bool HasValue { get; }

        internal abstract void RemoteValueUpdated(object value);

        internal abstract void RemoteValueRemoved();
    }

    /// <summary>
    /// Represents a reference to an item in the multiplayer shared state, which is serialized
    /// and deserialized into type <typeparamref name="TValue"/>. They can only be created by
    /// the <see cref="MultiplayerSession"/>, which ensures for a given shared state there is
    /// exactly one <see cref="MultiplayerResource"/> for each key.
    /// </summary>
    /// <typeparam name="TValue">The type to interpret the value of this key as.</typeparam>
    public class MultiplayerResource<TValue> : MultiplayerResource
    {
        /// <summary>
        /// The multiplayer session that hosts this resource.
        /// </summary>
        private IRemoteSharedState SharedState { get; }

        /// <summary>
        /// The full key that this resource will be found in the shared state.
        /// </summary>
        public string ResourceKey { get; }


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

        private TValue RemoteValue { get; set; }

        private TValue LocalValue { get; set; }

        private bool HasPendingLocalValue { get; set; }
        
        private bool HasPendingLocalRemoval { get; set; }

        public TValue Value
        {
            get
            {
                if (HasPendingLocalValue)
                    return LocalValue;
                return RemoteValue;
            }
        }

        private bool HasRemoteValue { get; set; }

        /// <inheritdoc cref="MultiplayerResource.HasValue"/>
        public override bool HasValue
        {
            get
            {
                if (HasPendingLocalRemoval)
                    return false;
                if (HasPendingLocalValue)
                    return true;
                return HasRemoteValue;
            }
        }

        /// <summary>
        /// The current state of the lock on this resource.
        /// </summary>
        public MultiplayerResourceLockState LockState { get; private set; }

        private int sentUpdateIndex = -1;

        /// <summary>
        /// Create a multiplayer resource.
        /// </summary>
        /// <param name="sharedState">The multiplayer session that will provide this value.</param>
        /// <param name="key">The key that identifies this resource in the dictionary.</param>
        /// <param name="objectToValue">
        /// An optional converter for converting the value in
        /// the dictionary to an appropriate value.
        /// </param>
        /// <param name="valueToObject">
        /// An optional converter for converting the value
        /// provided to one suitable for serialisation to protobuf.
        /// </param>
        internal MultiplayerResource(IRemoteSharedState sharedState, string key)
        {
            ResourceKey = key;
            SharedState = sharedState;
            HasRemoteValue = sharedState.HasRemoteSharedStateValue(key);
            if (HasRemoteValue)
                RemoteValue = Deserialize(sharedState.GetRemoteSharedStateValue(key));
            else
                RemoteValue = default;
            LockState = MultiplayerResourceLockState.Unlocked;
        }

        /// <summary>
        /// Invoked when the value of this resource is updated or removed from the shared state.
        /// </summary>
        public event Action RemoteValueChanged;

        /// <summary>
        /// Invoked when the value of this resource is updated or removed, either remotely or
        /// locally.
        /// </summary>
        public event Action ValueChanged;

        /// <summary>
        /// Invoked when the value of this resource is updated or removed, either remotely or
        /// locally.
        /// </summary>
        public event Action ValueRemoved;

        /// <summary>
        /// Invoked when the value of this resource is updated or removed, either remotely or
        /// locally.
        /// </summary>
        public event Action ValueUpdated;

        internal override void RemoteValueUpdated(object value)
        {
            RemoteValue = Deserialize(value);
            HasRemoteValue = true;
            
            // If we are up to date with the server, remove our pending value
            if (sentUpdateIndex <= SharedState.LastReceivedIndex)
            {
                LocalValue = default;
                HasPendingLocalValue = false;
                HasPendingLocalRemoval = false;
            }

            if (!HasPendingLocalValue)
            {
                ValueUpdated?.Invoke();
                ValueChanged?.Invoke();
            }

            RemoteValueChanged?.Invoke();
        }

        internal override void RemoteValueRemoved()
        {
            RemoteValue = default;
            HasRemoteValue = false;
            HasPendingLocalRemoval = false;
            
            if (!HasPendingLocalValue)
            {
                ValueRemoved?.Invoke();
                ValueChanged?.Invoke();
            }

            RemoteValueChanged?.Invoke();
        }

        protected TValue Deserialize(object value)
        {
            if (value == null)
                return default;
            return Serialization.Serialization.FromDataStructure<TValue>(value);
        }

        /// <summary>
        /// Copy the locally set version of this value to the multiplayer service.
        /// </summary>
        private void SendLocalValueToRemote()
        {
            sentUpdateIndex = SharedState.NextUpdateIndex;
            SharedState.ScheduleSharedStateUpdate(ResourceKey, Serialize(Value));
        }
        
        /// <summary>
        /// Inform the multiplayer service that an item has been removed.
        /// </summary>
        private void SendLocalRemovalToRemote()
        {
            sentUpdateIndex = SharedState.NextUpdateIndex;
            SharedState.ScheduleSharedStateRemoval(ResourceKey);
        }

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
            ReleaseLockAsync().AwaitInBackground();
        }

        private async Task ObtainLockAsync()
        {
            LockState = MultiplayerResourceLockState.Pending;
            var success = await SharedState.LockResource(ResourceKey);
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
            if (HasPendingLocalValue)
                SendLocalValueToRemote();
            LockAccepted?.Invoke();
        }

        private void OnLockRejected()
        {
            LocalValue = default;
            HasPendingLocalValue = false;
            HasPendingLocalRemoval = false;
            sentUpdateIndex = -1;
            LockRejected?.Invoke();
        }

        private async Task ReleaseLockAsync()
        {
            if (LockState != MultiplayerResourceLockState.Unlocked)
            {
                LockState = MultiplayerResourceLockState.Unlocked;
                LockReleased?.Invoke();
                await SharedState.ReleaseResource(ResourceKey);
            }
        }

        /// <summary>
        /// Set the local value of this key, and try to lock this resource to send it to
        /// everyone.
        /// If it is rejected, the value will revert to the default.
        /// </summary>
        public void UpdateValueWithLock(TValue value)
        {
            LocalValue = value;
            HasPendingLocalValue = true;
            HasPendingLocalRemoval = false;
            
            ValueUpdated?.Invoke();
            ValueChanged?.Invoke();

            switch (LockState)
            {
                case MultiplayerResourceLockState.Unlocked:
                    ObtainLock();
                    return;
                case MultiplayerResourceLockState.Pending:
                    return;
                case MultiplayerResourceLockState.Locked:
                    SendLocalValueToRemote();
                    return;
            }
        }

        protected object Serialize(TValue value)
        {
            return Serialization.Serialization.ToDataStructure(value);
        }

        public void SetLocalValue(TValue value)
        {
            LocalValue = value;
            HasPendingLocalValue = true;
            HasPendingLocalRemoval = false;
            
            SendLocalValueToRemote();

            ValueUpdated?.Invoke();
            ValueChanged?.Invoke();

        }

        public void Remove()
        {
            LocalValue = default;
            HasPendingLocalValue = false;
            HasPendingLocalRemoval = true;
            
            SendLocalRemovalToRemote();
            
            ValueRemoved?.Invoke();
            ValueChanged?.Invoke();
        }
    }
}