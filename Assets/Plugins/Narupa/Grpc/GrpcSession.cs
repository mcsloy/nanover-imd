using System;
using System.Collections.Generic;
using Narupa.Core.Async;

namespace Narupa.Grpc
{
    public abstract class GrpcSession<TClient> : IDisposable where TClient : GrpcClient
    {
        private TClient client;

        protected TClient Client => client;

        public ClientSharedState SharedState => client.SharedState;

        protected abstract TClient CreateClient(GrpcConnection connection);

        public virtual void OpenClient(GrpcConnection connection)
        {
            CloseClient();
            client = CreateClient(connection);
        }
        
        public virtual void CloseClient()
        {
            client?.CloseAndCancelAllSubscriptions();
            client?.Dispose();
            client = null;
        }
        
        /// <inheritdoc cref="IDisposable.Dispose" />
        public void Dispose()
        {
            CloseClient();
        }
        
        /// <summary>
        /// Is there an open client on this session?
        /// </summary>
        public bool IsOpen => Client != null;

        public void RunCommand(string name, Dictionary<string, object> arguments = null)
        {
            Client?.RunCommandAsync(name, arguments).AwaitInBackgroundIgnoreCancellation();
        }
    }
}