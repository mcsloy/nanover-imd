using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Narupa.Core.Async;
using Narupa.Grpc.Multiplayer;
using Narupa.Grpc.Stream;
using Narupa.Protocol.Multiplayer;
using Narupa.Protocol.State;

namespace Narupa.Grpc
{
    public class ClientSharedState
    {
        private string Token { get; }
        
        private GrpcClient client;

        /// <inheritdoc cref="CurrentSharedState"/>
        private Dictionary<string, object> sharedState { get; } =
            new Dictionary<string, object>();

        /// <summary>
        /// Dictionary of the currently known shared state.
        /// </summary>
        public IReadOnlyDictionary<string, object> CurrentSharedState => sharedState;

        /// <summary>
        /// How many milliseconds to put between sending our requested value
        /// changes.
        /// </summary>
        public int ValuePublishInterval { get; set; } = 1000 / 30;

        private IncomingStream<StateUpdate> IncomingValueUpdates { get; set; }

        private Dictionary<string, object> pendingValues
            = new Dictionary<string, object>();

        private List<string> pendingRemovals
            = new List<string>();

        private Task valueFlushingTask;

        public ClientSharedState(GrpcClient grpcClient)
        {
            client = grpcClient;
            Token = Guid.NewGuid().ToString();
            StartTasks();
        }

        /// <summary>
        /// Set the given key in the shared state dictionary, which will be
        /// sent to the server according in the future according to the publish 
        /// interval.
        /// </summary>
        public void SetValue(string key, object value)
        {
            pendingValues[key] = value.ToProtobufValue();
            pendingRemovals.Remove(key);
        }

        /// <summary>
        /// Remove the given key from the shared state dictionary, which will be
        /// sent to the server according in the future according to the publish 
        /// interval.
        /// </summary>
        public void RemoveKey(string key)
        {
            pendingValues.Remove(key);
            pendingRemovals.Add(key);
        }


        /// <summary>
        /// Get a key in the shared state dictionary.
        /// </summary>
        public object GetValue(string key)
        {
            return sharedState.TryGetValue(key, out var value) ? value : null;
        }


        public void StartTasks()
        {
            if (valueFlushingTask == null)
            {
                valueFlushingTask = CallbackInterval(FlushValues, ValuePublishInterval);
                valueFlushingTask.AwaitInBackground();
            }

            IncomingValueUpdates = client.SubscribeStateUpdates();
            IncomingValueUpdates.MessageReceived += OnStateUpdate;
            IncomingValueUpdates.StartReceiving().AwaitInBackgroundIgnoreCancellation();
        }


        /// <summary>
        /// Attempt to gain exclusive write access to the shared value of the given key.
        /// </summary>
        public async Task<bool> LockKey(string id, float time)
        {
            return await client.UpdateLocks(Token, new Dictionary<string, float>
                                            {
                                                [id] = 1f
                                            },
                                            new string[0]);
        }

        /// <summary>
        /// Release the lock on the given object of a given key.
        /// </summary>
        public async Task<bool> ReleaseKey(string id)
        {
            return await client.UpdateLocks(Token, new Dictionary<string, float>(), new string[]
            {
                id
            });
        }


        public void Close()
        {
            FlushValues();
            pendingValues.Clear();
            pendingRemovals.Clear();
        }

        public SharedStateResource<T> GetResource<T>(string key, Converter<object, T> objectToValue = null, Converter<T, object> valueToObject = null)
        {
            return new SharedStateResource<T>(this, key, objectToValue, valueToObject);
        }
        
        public SharedStateResource<object> GetResource(string key)
        {
            return new SharedStateResource<object>(this, key);
        }

        public event Action<string, object> KeyUpdated;

        public event Action<string> KeyRemoved;

        private void ClearSharedState()
        {
            var keys = sharedState.Keys.ToList();
            sharedState.Clear();

            foreach (var key in keys)
            {
                KeyRemoved?.Invoke(key);
            }
        }

        private void OnStateUpdate(StateUpdate update)
        {
            foreach (var pair in update.ChangedKeys.Fields)
            {
                var value = pair.Value.ToObject();
                if (value == null)
                {
                    KeyRemoved?.Invoke(pair.Key);
                }
                else
                {
                    sharedState[pair.Key] = value;
                    KeyUpdated?.Invoke(pair.Key, value);
                }
            }
        }

        public void FlushValues()
        {
            client.UpdateState(Token, pendingValues, pendingRemovals)
                  .AwaitInBackgroundIgnoreCancellation();

            pendingValues.Clear();
            pendingRemovals.Clear();
        }

        private static async Task CallbackInterval(Action callback, int interval)
        {
            while (true)
            {
                callback();
                await Task.Delay(interval);
            }
        }
    }
}