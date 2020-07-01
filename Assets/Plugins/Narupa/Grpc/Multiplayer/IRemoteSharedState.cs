using System;
using System.Threading.Tasks;

namespace Narupa.Grpc.Multiplayer
{
    /// <summary>
    /// An API for communication with a remote shared state, where a dictionary of arbitrary data
    /// is stored on a remote server and synced to the client. The client can override keys
    /// locally using <see cref="MultiplayerResource"/> or a <see cref="MultiplayerCollection"/>,
    /// which allows a local value to be used up until a more recent update from the server is
    /// received.
    /// </summary>
    public interface IRemoteSharedState
    {
        /// <summary>
        /// Does the current copy of the remote shared state have the given key. This does not
        /// account for local changes or removals.
        /// </summary>
        bool HasRemoteSharedStateValue(string key);
        
        /// <summary>
        /// Get the value associated with a given key in the current copy of the remote shared
        /// state. This does not account for local changes or removals.
        /// </summary>
        object GetRemoteSharedStateValue(string key);
        
        /// <summary>
        /// The index of the last update that the client has sent to the server that was
        /// incorporated into the last received update from the server.
        /// </summary>
        int LastReceivedIndex { get; }
        
        /// <summary>
        /// The index of the next update that will sent to the server. A key
        /// `update.index.{player_id}` will be inserted with this value. By obtaining the current
        /// value of this index before sending an item, you can compare with
        /// <see cref="LastReceivedIndex"/> to determine if an update from the server has included
        /// your sent value.
        /// </summary>
        int NextUpdateIndex { get; }

        /// <summary>
        /// Invoked when an update is received from the remote source, indicating a given key has
        /// been updated. This does not account for local changes or removals.
        /// </summary>
        event Action<string, object> SharedStateRemoteKeyUpdated;

        /// <summary>
        /// Invoked when an update is received from the remote source, indicating a given key has
        /// been removed. This does not account for local changes or removals.
        /// </summary>
        event Action<string> SharedStateRemoteKeyRemoved;

        /// <summary>
        /// Add a pending update to the shared state dictionary, which will be sent to the server
        /// at the next publish interval.
        /// </summary>
        void ScheduleSharedStateUpdate(string key, object value);
        
        /// <summary>
        /// Add a pending removal to the shared state dictionary, which will be sent to the server
        /// at the next publish interval.
        /// </summary>
        void ScheduleSharedStateRemoval(string key);
        
        Task<bool> LockResource(string key);
        
        Task<bool> ReleaseResource(string key);
        
        /// <summary>
        /// Get a view of a single key, which is automatically converted to the given type. This
        /// view allows local changes and removals to be made. It is guaranteed that all
        /// <see cref="MultiplayerResource{TValue}"/>'s for a given key are the same object.
        /// </summary>
        MultiplayerResource<TType> GetSharedResource<TType>(string key);

        /// <summary>
        /// Get a view of a set of keys with the same prefix, with their values automatically
        /// converted to the given type.
        /// </summary>
        MultiplayerCollection<TType> GetSharedCollection<TType>(string prefix);
        
        /// <summary>
        /// Invoked when a <see cref="MultiplayerResource"/> is created for the first time. This
        /// allows collections to be aware when resources are created which should belong to them.
        /// </summary>
        event Action<string, MultiplayerResource> ResourceCreated;
    }
}