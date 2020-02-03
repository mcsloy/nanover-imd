// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System.Collections;
using Narupa.Frame;
using Narupa.Frontend.Manipulation;
using Narupa.Frontend.XR;
using Narupa.Visualisation;
using NarupaXR.Interaction;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Text = TMPro.TextMeshProUGUI;

using Essd;

namespace NarupaXR
{
    /// <summary>
    /// The entry point to the application, and central location for accessing
    /// shared resources.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NarupaXRPrototype : MonoBehaviour
    {
#pragma warning disable 0649
        /// <summary>
        /// The transform that represents the box that contains the simulation.
        /// </summary>
        [SerializeField]
        private Transform simulationSpaceTransform;

        /// <summary>
        /// The transform that represents the actual simulation.
        /// </summary>
        [SerializeField]
        private Transform rightHandedSimulationSpace;

        [SerializeField]
        private InteractableScene interactableScene;

        [FormerlySerializedAs("xrInteraction")]
        [SerializeField]
        private XRBoxInteractionManager xrBoxInteraction;

        [SerializeField]
        private NarupaXRAvatarManager avatars;
#pragma warning restore 0649

        public PhysicallyCalibratedSpace CalibratedSpace { get; } = new PhysicallyCalibratedSpace();

        /// <summary>
        /// The route through which simulation space can be manipulated with
        /// gestures to perform translation, rotation, and scaling.
        /// </summary>
        public ManipulableScenePose ManipulableSimulationSpace { get; private set; }

        /// <summary>
        /// The route through which simulated particles can be manipulated with
        /// grabs.
        /// </summary>
        public ManipulableParticles ManipulableParticles { get; private set; }

        public SynchronisedFrameSource FrameSynchronizer { get; private set; }

        public NarupaXRSessionManager Sessions { get; } = new NarupaXRSessionManager();

        /// <summary>
        /// Connect to remote Narupa services.
        /// </summary>
        public void Connect(string address, int? trajectoryPort = null, int? imdPort = null, int? multiplayerPort = null)
            => Sessions.Connect(address, trajectoryPort, imdPort, multiplayerPort);

        /// <summary>
        /// Connect to the Narupa services described in a given ServiceHub.
        /// </summary>
        public void Connect(ServiceHub hub) => Sessions.Connect(hub);

        /// <summary>
        /// Connect to the first set of Narupa services found via ESSD.
        /// </summary>
        public void AutoConnect() => Sessions.AutoConnect();

        /// <summary>
        /// Disconnect from all Narupa services.
        /// </summary>
        public void Disconnect() => Sessions.CloseAsync();

        /// <summary>
        /// Called from UI to quit the application.
        /// </summary>
        public void Quit() => Application.Quit();

        private void Awake()
        {
            ManipulableSimulationSpace = new ManipulableScenePose(simulationSpaceTransform,
                                                                  Sessions.Multiplayer,
                                                                  this);

            ManipulableParticles = new ManipulableParticles(simulationSpaceTransform,
                                                            Sessions.Imd,
                                                            interactableScene);
            
            ManipulableParticles = new ManipulableParticles(rightHandedSimulationSpace,
                                                            Sessions.Imd,
                                                            interactableScene);
            
            SetupVisualisation();
        }

        private void Update()
        {
            CalibratedSpace.CalibrateFromLighthouses();
        }

        private async void OnDestroy()
        {
            await Sessions.CloseAsync();
        }

        private void SetupVisualisation()
        {
            FrameSynchronizer = gameObject.GetComponent<SynchronisedFrameSource>(); 
            if (FrameSynchronizer == null)
                FrameSynchronizer = gameObject.AddComponent<SynchronisedFrameSource>();
            FrameSynchronizer.FrameSource = Sessions.Trajectory;
        }
    }
}