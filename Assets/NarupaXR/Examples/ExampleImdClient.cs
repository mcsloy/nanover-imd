// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using Narupa.Frame;
using Narupa.Grpc;
using Narupa.Grpc.Trajectory;
using Narupa.Session;
using UnityEngine;

namespace NarupaXR.Examples
{
    /// <summary>
    /// Example Interactive Molecular Dynamics client. Connects to a remote
    /// trajectory and imd service and visualises the particles.
    /// </summary>
    public sealed class ExampleImdClient : MonoBehaviour
    {
        /// <summary>
        /// Current <see cref="TrajectorySession" />.
        /// </summary>
        public TrajectorySession TrajectorySession { get; } = new TrajectorySession();

        /// <summary>
        /// Current <see cref="TrajectorySession" />.
        /// </summary>
        public ImdSession InteractiveSession { get; } = new ImdSession();

#pragma warning disable 0649
        [SerializeField]
        private string host;

        [SerializeField]
        private int portTrajectory;
        
        [SerializeField]
        private int portInteractions;

        [SerializeField]
        private GameObject visualiser;
#pragma warning restore 0649

        private GrpcConnection connectionTrajectory;
        private GrpcConnection connectionInteractions;

        private void Start()
        {
            connectionTrajectory = new GrpcConnection(host, portTrajectory);
            connectionInteractions = new GrpcConnection(host, portInteractions);
            TrajectorySession.OpenClient(connectionTrajectory);
            InteractiveSession.OpenClient(connectionInteractions);
            visualiser.GetComponent<IFrameConsumer>().FrameSource = TrajectorySession;
        }

        private async void OnDestroy()
        {
            await connectionTrajectory.CloseAsync();
            await connectionInteractions.CloseAsync();
            TrajectorySession.Dispose();
            InteractiveSession.Dispose();
        }
    }
}