// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Narupa.Core;
using Narupa.Core.Async;
using Narupa.Core.Math;
using Narupa.Grpc.Stream;
using Narupa.Grpc.Trajectory;
using Narupa.Protocol.State;
using UnityEngine;

namespace Narupa.Grpc.Multiplayer
{
    /// <summary>
    /// Manages the state of a single user engaging in multiplayer. Tracks
    /// local and remote avatars and maintains a copy of the latest values in
    /// the shared key/value store.
    /// </summary>
    public sealed class MultiplayerSession : IRemoteSharedState, IDisposable
    {
        public const string SimulationPoseKey = "scene";

        public string AccessToken { get; set; }

        public MultiplayerAvatars Avatars { get; }

        public MultiplayerSession()
        {
            Avatars = new MultiplayerAvatars(this);

            SimulationPose =
                new MultiplayerResource<Transformation>(this, SimulationPoseKey);
        }

        /// <summary>
        /// The transformation of the simulation box.
        /// </summary>
        public readonly MultiplayerResource<Transformation> SimulationPose;

        /// <summary>
        /// Dictionary of the shared state as received directly from the server. Local modifications
        /// are not made to this dictionary.
        /// </summary>
        public Dictionary<string, object> RemoteSharedStateDictionary { get; } =
            new Dictionary<string, object>();

        /// <summary>
        /// Is there an open client on this session?
        /// </summary>
        public bool IsOpen => client != null;

        /// <summary>
        /// How many milliseconds to put between sending our requested value
        /// changes.
        /// </summary>
        public int ValuePublishInterval { get; set; } = 1000 / 30;

        private MultiplayerClient client;

        private IncomingStream<StateUpdate> IncomingValueUpdates { get; set; }

        private Dictionary<string, object> pendingValues
            = new Dictionary<string, object>();

        private List<string> pendingRemovals
            = new List<string>();

        private Task valueFlushingTask;

        public event Action<string, object> SharedStateRemoteKeyUpdated;

        public event Action<string> SharedStateRemoteKeyRemoved;

        public event Action MultiplayerJoined;

        /// <inheritdoc cref="IRemoteSharedState.NextUpdateIndex"/>
        public int NextUpdateIndex => nextUpdateIndex;

        /// <inheritdoc cref="IRemoteSharedState.LastReceivedIndex"/>
        public int LastReceivedIndex => lastReceivedIndex;

        private int nextUpdateIndex = 0;

        private int lastReceivedIndex = -1;

        private string UpdateIndexKey => $"update.index.{AccessToken}";

        /// <summary>
        /// Connect to a Multiplayer service over the given connection. 
        /// Closes any existing client.
        /// </summary>
        public void OpenClient(GrpcConnection connection)
        {
            CloseClient();

            client = new MultiplayerClient(connection);
            AccessToken = Guid.NewGuid().ToString();

            if (valueFlushingTask == null)
            {
                valueFlushingTask = CallbackInterval(FlushValues, ValuePublishInterval);
                valueFlushingTask.AwaitInBackground();
            }

            IncomingValueUpdates = client.SubscribeStateUpdates();
            BackgroundIncomingStreamReceiver<StateUpdate>.Start(IncomingValueUpdates,
                                                                OnResourceValuesUpdateReceived,
                                                                MergeResourceUpdates);

            void MergeResourceUpdates(StateUpdate dest, StateUpdate src)
            {
                foreach (var (key, value) in src.ChangedKeys.Fields)
                    dest.ChangedKeys.Fields[key] = value;
            }

            MultiplayerJoined?.Invoke();
        }

        /// <summary>
        /// Close the current Multiplayer client and dispose all streams.
        /// </summary>
        public void CloseClient()
        {
            Avatars.CloseClient();
            FlushValues();

            client?.CloseAndCancelAllSubscriptions();
            client?.Dispose();
            client = null;

            AccessToken = null;

            ClearSharedState();
            pendingValues.Clear();
            pendingRemovals.Clear();
        }

        /// <inheritdoc cref="ScheduleSharedStateUpdate"/>
        public void ScheduleSharedStateUpdate(string key, object value)
        {
            pendingValues[key] = value.ToProtobufValue();
            pendingRemovals.Remove(key);
        }

        /// <inheritdoc cref="ScheduleSharedStateRemoval"/>
        public void ScheduleSharedStateRemoval(string key)
        {
            pendingValues.Remove(key);
            pendingRemovals.Add(key);
        }

        /// <summary>
        /// Does the shared state contain an item with the given key?
        /// </summary>
        public bool HasRemoteSharedStateValue(string key)
        {
            return RemoteSharedStateDictionary.ContainsKey(key);
        }

        /// <summary>
        /// Get a key in the shared state dictionary.
        /// </summary>
        public object GetRemoteSharedStateValue(string key)
        {
            return RemoteSharedStateDictionary.TryGetValue(key, out var value) ? value : null;
        }

        /// <summary>
        /// Attempt to gain exclusive write access to the shared value of the given key.
        /// </summary>
        public async Task<bool> LockResource(string id)
        {
            return await client.UpdateLocks(AccessToken, new Dictionary<string, float>
                                            {
                                                [id] = 1f
                                            },
                                            new string[0]);
        }

        /// <summary>
        /// Release the lock on the given object of a given key.
        /// </summary>
        public async Task<bool> ReleaseResource(string id)
        {
            return await client.UpdateLocks(AccessToken, new Dictionary<string, float>(),
                                            new string[]
                                            {
                                                id
                                            });
        }

        /// <inheritdoc cref="IDisposable.Dispose" />
        public void Dispose()
        {
            FlushValues();

            CloseClient();
        }

        private void ClearSharedState()
        {
            var keys = RemoteSharedStateDictionary.Keys.ToList();
            RemoteSharedStateDictionary.Clear();

            foreach (var key in keys)
            {
                try
                {
                    SharedStateRemoteKeyRemoved?.Invoke(key);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private void OnResourceValuesUpdateReceived(StateUpdate update)
        {
            if (update.ChangedKeys.Fields.ContainsKey(UpdateIndexKey))
            {
                lastReceivedIndex = (int) update.ChangedKeys
                                                .Fields[UpdateIndexKey]
                                                .NumberValue;
            }

            foreach (var (key, value1) in update.ChangedKeys.Fields)
            {
                var value = value1.ToObject();
                if (value == null)
                {
                    RemoveRemoteValue(key);
                }
                else
                {
                    UpdateRemoteValue(key, value);
                }
            }
        }

        private void FlushValues()
        {
            if (!IsOpen)
                return;

            if (pendingValues.Any() || pendingRemovals.Any())
            {
                pendingValues[UpdateIndexKey] = nextUpdateIndex;

                client.UpdateState(AccessToken, pendingValues, pendingRemovals)
                      .AwaitInBackgroundIgnoreCancellation();

                pendingValues.Clear();
                pendingRemovals.Clear();

                nextUpdateIndex++;
            }
        }

        private static async Task CallbackInterval(Action callback, int interval)
        {
            while (true)
            {
                callback();
                await Task.Delay(interval);
            }
        }

        public void RemoveRemoteValue(string key)
        {
            RemoteSharedStateDictionary.Remove(key);
            SharedStateRemoteKeyRemoved?.Invoke(key);

            if (multiplayerResources.TryGetValue(key, out var weakRef) &&
                weakRef.TryGetTarget(out var resource))
            {
                resource.RemoteValueRemoved();
            }
        }

        public void UpdateRemoteValue(string key, object value)
        {
            RemoteSharedStateDictionary[key] = value;
            SharedStateRemoteKeyUpdated?.Invoke(key, value);

            if (multiplayerResources.TryGetValue(key, out var weakRef) &&
                weakRef.TryGetTarget(out var resource))
            {
                resource.RemoteValueUpdated(value);
            }
        }


        internal Dictionary<string, WeakReference<MultiplayerResource>> multiplayerResources =
            new Dictionary<string, WeakReference<MultiplayerResource>>();

        /// <summary>
        /// Get a <see cref="MultiplayerResource"/>, which is a typed wrapper around a value in
        /// the shared state dictionary that is automatically kept up to date with the remote
        /// shared state. It also allows its value to be set, and returns this local value until
        /// the server replies with a more up to date one. The session keeps a weak reference to
        /// each resource, so a resource with a given key is reused by repeated calls to this
        /// function. However, when everyone stops listening to the resource it will be deleted.
        /// </summary>
        public MultiplayerResource<TType> GetSharedResource<TType>(string key)
        {
            if (multiplayerResources.TryGetValue(key, out var existingWeakRef))
            {
                if (existingWeakRef.TryGetTarget(out var existing))
                {
                    if (existing is MultiplayerResource<TType> correctExisting)
                        return correctExisting;
                    throw new KeyNotFoundException("Tried getting multiplayer resource with "
                                                 + $"key {key} and type {typeof(MultiplayerResource<TType>)}, but found"
                                                 + $"existing incompatible type {existing.GetType()}");
                }
            }

            var added = new MultiplayerResource<TType>(this, key);
            multiplayerResources[key] = new WeakReference<MultiplayerResource>(added);
            ResourceCreated?.Invoke(key, added);
            return added;
        }

        private Dictionary<string, MultiplayerCollection> collections =
            new Dictionary<string, MultiplayerCollection>();

        public MultiplayerCollection<TType> GetSharedCollection<TType>(string prefix)
        {
            if (collections.TryGetValue(prefix, out var existing))
                return existing as MultiplayerCollection<TType>;
            foreach (var existingKey in collections.Keys)
                if (existingKey.StartsWith(prefix) || prefix.StartsWith(existingKey))
                    throw new ArgumentException(
                        $"Conflicting collections with keys {prefix} and {existingKey}");
            var added = new MultiplayerCollection<TType>(this, prefix);
            collections[prefix] = added;
            return added;
        }

        public event Action<string, MultiplayerResource> ResourceCreated;
    }
}