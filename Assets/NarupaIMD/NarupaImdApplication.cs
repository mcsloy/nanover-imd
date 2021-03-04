// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using Essd;
using Narupa.Frontend.XR;
using NarupaImd;
using UnityEngine;
using UnityEngine.Events;
using NarupaImd.Interaction;
using System.Threading.Tasks;

namespace NarupaImd
{
    /// <summary>
    /// The entry point to the application, and central location for accessing
    /// shared resources.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NarupaImdApplication : MonoBehaviour
    {
#pragma warning disable 0649
        
        [SerializeField]
        private NarupaImdSimulation simulation;
        
        private InteractableScene interactableScene;
#pragma warning restore 0649

        public NarupaImdSimulation Simulation => simulation;

        public bool ColocateLighthouses { get; set; } = false;

        /// <summary>
        /// The route through which simulation space can be manipulated with
        /// gestures to perform translation, rotation, and scaling.
        /// </summary>
        public ManipulableScenePose ManipulableSimulationSpace { get; private set; }

        public PhysicallyCalibratedSpace CalibratedSpace { get; } = new PhysicallyCalibratedSpace();

        [SerializeField]
        private UnityEvent connectionEstablished;

        private void Awake()
        {
            simulation.ConnectionEstablished += connectionEstablished.Invoke;
        }

        /// <summary>
        /// Connect to remote Narupa services.
        /// </summary>
        public Task Connect(string address,
                            int? trajectoryPort = null,
                            int? imdPort = null,
                            int? multiplayerPort = null) =>
            simulation.Connect(address, trajectoryPort, imdPort, multiplayerPort);

        /// <summary>
        /// Connect to the Narupa services described in a given ServiceHub.
        /// </summary>
        public Task Connect(ServiceHub hub) => simulation.Connect(hub);

        /// <summary>
        /// Connect to the first set of Narupa services found via ESSD.
        /// </summary>
        public Task AutoConnect() => simulation.AutoConnect();

        /// <summary>
        /// Disconnect from all Narupa services.
        /// </summary>
        public Task Disconnect() => simulation.CloseAsync();

        /// <summary>
        /// Called from UI to quit the application.
        /// </summary>
        public void Quit() => Application.Quit();
        
        private void Update()
        {
            if (ColocateLighthouses) CalibratedSpace.CalibrateFromLighthouses();
        }
    }
}