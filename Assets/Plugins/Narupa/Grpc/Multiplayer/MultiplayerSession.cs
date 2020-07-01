﻿// Copyright (c) Intangible Realities Lab. All rights reserved.
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
    public sealed class MultiplayerSession : IDisposable
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
        /// Dictionary of the currently known shared state.
        /// </summary>
        public Dictionary<string, object> SharedStateDictionary { get; } =
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

        public event Action<string, object> SharedStateDictionaryKeyUpdated;

        public event Action<string> SharedStateDictionaryKeyRemoved;

        public event Action MultiplayerJoined;

        /// <summary>
        /// The index of the next update that we will send to the server. A key
        /// `update.index.{player_id}` will be inserted with this value. By getting this value
        /// when you've scheduled something to be done to the dictionary, you can then determine
        /// when a returned update has incorporated your change.
        /// </summary>
        public int NextUpdateIndex => nextUpdateIndex;

        /// <summary>
        /// The index of the latest changes we sent to the server which have been received by us.
        /// </summary>
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

        /// <summary>
        /// Set the given key in the shared state dictionary, which will be
        /// sent to the server according in the future according to the publish 
        /// interval.
        /// </summary>
        public void SetSharedState(string key, object value)
        {
            pendingValues[key] = value.ToProtobufValue();
            pendingRemovals.Remove(key);
        }

        /// <summary>
        /// Remove the given key from the shared state dictionary, which will be
        /// sent to the server according in the future according to the publish 
        /// interval.
        /// </summary>
        public void RemoveSharedStateKey(string key)
        {
            pendingValues.Remove(key);
            pendingRemovals.Add(key);
        }

        /// <summary>
        /// Does the shared state contain an item with the given key?
        /// </summary>
        public bool HasSharedState(string key)
        {
            return SharedStateDictionary.ContainsKey(key);
        }

        /// <summary>
        /// Get a key in the shared state dictionary.
        /// </summary>
        public object GetSharedState(string key)
        {
            return SharedStateDictionary.TryGetValue(key, out var value) ? value : null;
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
            var keys = SharedStateDictionary.Keys.ToList();
            SharedStateDictionary.Clear();

            foreach (var key in keys)
            {
                try
                {
                    SharedStateDictionaryKeyRemoved?.Invoke(key);
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
            SharedStateDictionary.Remove(key);
            SharedStateDictionaryKeyRemoved?.Invoke(key);

            if (multiplayerResources.TryGetValue(key, out var weakRef) &&
                weakRef.TryGetTarget(out var resource))
            {
                resource.RemoteValueRemoved();
            }
        }

        public void UpdateRemoteValue(string key, object value)
        {
            SharedStateDictionary[key] = value;
            SharedStateDictionaryKeyUpdated?.Invoke(key, value);
            
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
            return added;
        }
    }
}