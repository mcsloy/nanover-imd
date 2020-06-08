// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using Narupa.Session;
using System.Threading.Tasks;
using Narupa.Core.Async;
using Narupa.Grpc;
using Narupa.Grpc.Trajectory;
using UnityEngine;
using System.Collections.Generic;
using Essd;
using Newtonsoft.Json.Linq;
using System.Linq;
using Narupa.Grpc.Multiplayer;

namespace NarupaXR
{
    /// <summary>
    /// Manages trajectory and IMD sessions for the NarupaXR application.
    /// </summary>
    public sealed class NarupaXRSessionManager
    {
        private const string TrajectoryServiceName = "trajectory";
        private const string ImdServiceName = "imd";
        private const string MultiplayerServiceName = "multiplayer";

        public TrajectorySession Trajectory { get; } = new TrajectorySession();
        public ImdSession Imd { get; } = new ImdSession();
        public MultiplayerSession Multiplayer { get; } = new MultiplayerSession();

        private Dictionary<string, GrpcConnection> channels
            = new Dictionary<string, GrpcConnection>();

        /// <summary>
        /// Connect to the host address and attempt to open clients for the
        /// trajectory and IMD services.
        /// </summary>
        public async Task Connect(string address, 
                                  int? trajectoryPort, 
                                  int? imdPort = null,
                                  int? multiplayerPort = null)
        {
            await CloseAsync();

            if (trajectoryPort.HasValue)
            {
                Trajectory.OpenClient(GetChannel(address, trajectoryPort.Value));
            }

            if (imdPort.HasValue)
            {
                Imd.OpenClient(GetChannel(address, imdPort.Value));
            }

            if (multiplayerPort.HasValue)
            {
                Multiplayer.OpenClient(GetChannel(address, multiplayerPort.Value));
            }
        }

        /// <summary>
        /// Connect to services as advertised by an ESSD service hub.
        /// </summary>
        public async Task Connect(ServiceHub hub)
        {
            Debug.Log($"Connecting to {hub.Name} ({hub.Id})");

            var services = hub.Properties["services"] as JObject;
            await Connect(hub.Address,
                          GetServicePort(TrajectoryServiceName),
                          GetServicePort(ImdServiceName),
                          GetServicePort(MultiplayerServiceName));

            int? GetServicePort(string name)
            {
                return services.ContainsKey(name) ? services[name].ToObject<int>() : (int?) null;
            }
        }

        /// <summary>
        /// Run an ESSD search and connect to the first service found, or none
        /// if the timeout elapses without finding a service.
        /// </summary>
        public async Task AutoConnect(int millisecondsTimeout = 1000)
        {
            var client = new Client();
            var services = await Task.Run(() => client.SearchForServices(millisecondsTimeout));
            if (services.Count > 0)
                await Connect(services.First());
        }

        /// <summary>
        /// Close all sessions.
        /// </summary>
        public async Task CloseAsync()
        {
            Trajectory.CloseClient();
            Imd.CloseClient();
            Multiplayer.CloseClient();

            foreach (var channel in channels.Values)
            {
                await channel.CloseAsync();
            }
            channels.Clear();
        }

        private GrpcConnection GetChannel(string address, int port)
        {
            string key = $"{address}:{port}";

            if (!channels.TryGetValue(key, out var channel))
            {
                channel = new GrpcConnection(address, port);
                channels[key] = channel;
            }

            return channel;
        }
    }
}
