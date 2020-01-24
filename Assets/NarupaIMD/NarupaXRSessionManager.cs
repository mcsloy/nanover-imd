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

namespace NarupaXR
{
    /// <summary>
    /// Manages trajectory and IMD sessions for the NarupaXR application.
    /// </summary>
    public sealed class NarupaXRSessionManager
    {
        public TrajectorySession Trajectory { get; } = new TrajectorySession();
        public ImdSession Imd { get; } = new ImdSession();
        public MultiplayerSession Multiplayer { get; } = new MultiplayerSession();

        private Dictionary<string, GrpcConnection> channels
            = new Dictionary<string, GrpcConnection>();

        private Client essdClient = new Client();

        /// <summary>
        /// Connect to the host address and attempt to open clients for the
        /// trajectory and IMD services.
        /// </summary>
        public void Connect(string address, 
                            int? trajectoryPort, 
                            int? imdPort = null,
                            int? multiplayerPort = null)
        {
            CloseAsync();

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
                Multiplayer.JoinMultiplayer("test").AwaitInBackgroundIgnoreCancellation();
            }
        }

        public void Connect(ServiceHub hub)
        {
            Debug.Log($"Connecting to {hub.Name} ({hub.Id})");

            var services = hub.Properties["services"] as JObject;
            Connect(hub.Address,
                    GetServicePort("trajectory"),
                    GetServicePort("imd"),
                    GetServicePort("multiplayer"));

            int? GetServicePort(string name)
            {
                return services.ContainsKey(name) ? services[name].ToObject<int>() : (int?) null;
            }
        }

        public void AutoConnect()
        {
            StopEssd();
            essdClient = new Client();
            essdClient.ServiceFound += async (sender, hub) =>
            {
                await StopEssd();
                Connect(hub);
            };
            essdClient.StartSearch();
        }

        /// <summary>
        /// Close all sessions.
        /// </summary>
        public async Task CloseAsync()
        {
            Trajectory.CloseClient();
            Imd.CloseClient();
            Multiplayer.CloseClient();

            await StopEssd();

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

        private async Task StopEssd()
        {
            await essdClient?.StopSearch();
            essdClient = null;
        }
    }
}
