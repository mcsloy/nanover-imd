// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using JetBrains.Annotations;
using Narupa.Core.Async;
using Narupa.Grpc.Stream;
using Narupa.Protocol.Command;
using Narupa.Protocol.State;

namespace Narupa.Grpc
{
    public abstract class GrpcClient : Cancellable, IAsyncClosable
    {
        // Chosen as an acceptable minimum rate that should ideally be 
        // explicitly increased.
        private const float DefaultUpdateInterval = 1f / 30f;

        /// <summary>
        /// The client used to access the Command service on the same port as this client.
        /// </summary>
        protected Command.CommandClient CommandClient { get; }
        
        protected State.StateClient StateClient { get; }
        
        public ClientSharedState SharedState { get; }

        protected GrpcClient([NotNull] GrpcConnection connection) : base(
            connection.GetCancellationToken())
        {
            if (connection.IsCancelled)
                throw new ArgumentException("Connection has already been shutdown.");

            CommandClient = new Command.CommandClient(connection.Channel);
            StateClient = new State.StateClient(connection.Channel);
            SharedState = new ClientSharedState(this);
        }
        
        
        /// <summary>
        /// Run a command on a gRPC service that uses the command service.
        /// </summary>
        /// <param name="command">The name of the command to run, which must be registered on the server.</param>
        /// <param name="arguments">Name/value arguments to provide to the command.</param>
        /// <returns>Dictionary of results produced by the command.</returns>
        public async Task<Dictionary<string, object>> RunCommandAsync(string command,
                                                                         Dictionary<string, object>
                                                                             arguments = null)
        {
            var message = new CommandMessage
            {
                Name = command,
                Arguments = arguments?.ToProtobufStruct()
            };
            return (await CommandClient.RunCommandAsync(message)).Result.ToDictionary();
        }

        /// <summary>
        /// Create an incoming stream from the definition of a gRPC call.
        /// </summary>
        protected IncomingStream<TResponse> GetIncomingStream<TRequest, TResponse>(
            ServerStreamingCall<TRequest, TResponse> call,
            TRequest request,
            CancellationToken externalToken = default)
        {
            if (IsCancelled)
                throw new InvalidOperationException("The client is closed.");

            return IncomingStream<TResponse>.CreateStreamFromServerCall(
                call,
                request,
                GetCancellationToken(), externalToken);
        }

        /// <summary>
        /// Create an outgoing stream from the definition of a gRPC call.
        /// </summary>
        protected OutgoingStream<TOutgoing, TResponse> GetOutgoingStream<TOutgoing, TResponse>(
            ClientStreamingCall<TOutgoing, TResponse> call,
            CancellationToken externalToken = default)
        {
            if (IsCancelled)
                throw new InvalidOperationException("The client is closed.");

            return OutgoingStream<TOutgoing, TResponse>.CreateStreamFromClientCall(
                call,
                GetCancellationToken(), externalToken);
        }

        /// <inheritdoc cref="IAsyncClosable.CloseAsync" />
        public Task CloseAsync()
        {
            SharedState.Close();
            Cancel();

            return Task.CompletedTask;
        }

        public void CloseAndCancelAllSubscriptions()
        {
            CloseAsync();
        }
        
        /// <summary>
        /// Starts an <see cref="IncomingStream{StateUpdate}" /> on 
        /// which the server provides updates to the shared key/value store at 
        /// the requested time interval (in seconds).
        /// </summary>
        /// <remarks>
        /// Corresponds to the SubscribeStateUpdates gRPC call.
        /// </remarks>
        public IncomingStream<StateUpdate> SubscribeStateUpdates(float updateInterval = DefaultUpdateInterval,
                                                                 CancellationToken externalToken = default)
        {
            var request = new SubscribeStateUpdatesRequest
            {
                UpdateInterval = updateInterval,
            };

            return GetIncomingStream(StateClient.SubscribeStateUpdates, request, externalToken);
        }
        
        public async Task<bool> UpdateState(string token, Dictionary<string, object> updates, List<string> removals)
        {
            var request = new UpdateStateRequest()
            {
                AccessToken = token,
                Update = CreateStateUpdate(updates, removals)
            };

            var response = await StateClient.UpdateStateAsync(request);

            return response.Success;
        }
        
        public async Task<bool> UpdateLocks(string token, IDictionary<string, float> toAcquire, IEnumerable<string> toRemove)
        {
            var request = new UpdateLocksRequest
            {
                AccessToken = token,
                LockKeys = CreateLockUpdate(toAcquire, toRemove)
            };

            var response = await StateClient.UpdateLocksAsync(request);

            return response.Success;
        }

        private Struct CreateLockUpdate(IDictionary<string, float> toAcquire, IEnumerable<string> toRelease)
        {
            var str = toAcquire.ToProtobufStruct();
            foreach(var releasedkey in toRelease)
                str.Fields[releasedkey] = Value.ForNull();
            return str;
        }

        private static StateUpdate CreateStateUpdate(IDictionary<string, object> updates, IEnumerable<string> removals)
        {
            var update = new StateUpdate();
            var updatesAsStruct = updates.ToProtobufStruct();
            update.ChangedKeys = updatesAsStruct;
            foreach (var removal in removals)
                update.ChangedKeys.Fields[removal] = Value.ForNull();
            return update;
        }
    }
    
    /// <summary>
    /// Base implementation of a C# wrapper around a gRPC client to a specific service.
    /// </summary>
    public abstract class GrpcClient<TClient> : GrpcClient where TClient : ClientBase<TClient>
    {
        /// <summary>
        /// gRPC Client that provides access to RPC calls.
        /// </summary>
        protected TClient Client { get; }

        /// <summary>
        /// Create a client to a server described by the provided
        /// <see cref="GrpcConnection" />.
        /// </summary>
        public GrpcClient([NotNull] GrpcConnection connection) : base(connection)
        {
            Client = (TClient) Activator.CreateInstance(typeof(TClient), connection.Channel);
        }
    }
}