// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using Narupa.Session;
using System.Threading.Tasks;
using Narupa.Core.Async;
using Narupa.Grpc;
using Narupa.Grpc.Trajectory;
using UnityEngine;

namespace NarupaXR
{
    /// <summary>
    /// Manages trajectory and IMD sessions for the NarupaXR application.
    /// </summary>
    public sealed class NarupaXRSessionManager
    {
        private GrpcConnection remoteTrajectoryConnection;
        private GrpcConnection remoteImdConnection;
        private GrpcConnection remoteMultiplayerConnection;

        public TrajectorySession Trajectory { get; } = new TrajectorySession();
        public ImdSession Imd { get; } = new ImdSession();
        public MultiplayerSession Multiplayer { get; } = new MultiplayerSession();

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
                remoteTrajectoryConnection = new GrpcConnection(address, trajectoryPort.Value);
                Trajectory.OpenClient(remoteTrajectoryConnection);
            }

            if (imdPort.HasValue)
            {
                remoteImdConnection = new GrpcConnection(address, imdPort.Value);
                Imd.OpenClient(remoteImdConnection);
            }

            if (multiplayerPort.HasValue)
            {
                Debug.Log($"Multiplayer port: {multiplayerPort}");

                remoteMultiplayerConnection = new GrpcConnection(address, multiplayerPort.Value);
                Multiplayer.OpenClient(remoteMultiplayerConnection);

                Multiplayer.JoinMultiplayer("test").AwaitInBackgroundIgnoreCancellation();
            }
        }

        /// <summary>
        /// Close all sessions.
        /// </summary>
        public async Task CloseAsync()
        {
            Trajectory.CloseClient();
            Imd.CloseClient();
            Multiplayer.CloseClient();

            if (remoteTrajectoryConnection != null)
                await remoteTrajectoryConnection.CloseAsync();

            if (remoteImdConnection != null)
                await remoteImdConnection.CloseAsync();

            if (remoteMultiplayerConnection != null)
                await remoteMultiplayerConnection.CloseAsync();
        }
    }
}
