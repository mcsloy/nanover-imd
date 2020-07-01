using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Narupa.Core;
using Narupa.Grpc.Multiplayer;

namespace Narupa.Grpc.Tests.Multiplayer
{
    /// <summary>
    /// A testing example of a shared state interface which does not require a server or gRPC
    /// connection, hence elinimating concerns about synchronizity when testing nuances of
    /// multiplayer resources & collections.
    /// </summary>
    public class SharedState : IRemoteSharedState
    {
        private Dictionary<string, object> remoteSharedState = new Dictionary<string, object>();

        private List<string> pendingRemovals = new List<string>();
        private Dictionary<string, object> pendingUpdates = new Dictionary<string, object>();
        
        public bool HasRemoteSharedStateValue(string key)
        {
            return remoteSharedState.ContainsKey(key);
        }

        public object GetRemoteSharedStateValue(string key)
        {
            return remoteSharedState[key];
        }

        public int LastReceivedIndex { get; private set; } = -1;
        public int NextUpdateIndex { get; private set; } = 0;

        public void ReplyToChanges()
        {
            LastReceivedIndex = NextUpdateIndex;
            NextUpdateIndex++;
            foreach (var key in pendingRemovals)
            {
                remoteSharedState.Remove(key);
                SharedStateRemoteKeyRemoved?.Invoke(key);
                if (resources.TryGetValue(key, out var resource))
                    resource.RemoteValueRemoved();
            }
            foreach (var (key, value) in pendingUpdates)
            {
                remoteSharedState[key] = value;
                SharedStateRemoteKeyUpdated?.Invoke(key, value);
                if (resources.TryGetValue(key, out var resource))
                    resource.RemoteValueUpdated(value);
            }
            pendingRemovals.Clear();
            pendingUpdates.Clear();
        }
        
        public event Action<string, object> SharedStateRemoteKeyUpdated;
        
        public event Action<string> SharedStateRemoteKeyRemoved;
        
        public void ScheduleSharedStateUpdate(string key, object value)
        {
            pendingUpdates[key] = value;
            pendingRemovals.Remove(key);
        }

        public void ScheduleSharedStateRemoval(string key)
        {
            pendingRemovals.Add(key);
            pendingUpdates.Remove(key);
        }

        public async Task<bool> LockResource(string key)
        {
            return true;
        }

        public async Task<bool> ReleaseResource(string key)
        {
            return true;
        }
        
        private readonly Dictionary<string, MultiplayerResource> resources = new Dictionary<string, MultiplayerResource>();

        public MultiplayerResource<TType> GetSharedResource<TType>(string key)
        {
            if (resources.TryGetValue(key, out var existing))
                return existing as MultiplayerResource<TType>;
            var added = new MultiplayerResource<TType>(this, key);
            resources[key] = added;
            ResourceCreated?.Invoke(key, added);
            return added;
        }

        public MultiplayerCollection<TType> GetSharedCollection<TType>(string prefix)
        {
            return new MultiplayerCollection<TType>(this, prefix);
        }

        public event Action<string, MultiplayerResource> ResourceCreated;

        public void SetRemoteValueAndSendChanges(string key, object value)
        {
            ScheduleSharedStateUpdate(key, value);
            ReplyToChanges();
        }
        
        public void RemoveRemoteValueAndSendChanges(string key)
        {
            ScheduleSharedStateRemoval(key);
            ReplyToChanges();
        }
    }
}