// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using Narupa.Frontend.Controllers;
using Narupa.Frontend.Manipulation;
using Narupa.Frontend.XR;
using Narupa.Visualisation;
using NarupaXR.Interaction;
using Plugins.Narupa.Frontend;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Valve.VR;
using Text = TMPro.TextMeshProUGUI;

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
        [SerializeField]
        private NarupaApplication application;

        [SerializeField]
        private Transform simulationSpaceTransform;

        [SerializeField]
        private InteractableScene interactableScene;

        [FormerlySerializedAs("xrInteraction")]
        [SerializeField]
        private XRBoxInteractionManager xrBoxInteraction;

        [SerializeField]
        private NarupaXRAvatarManager avatars;

        [SerializeField]
        private InteractionMode interactionMode;

        [SerializeField]
        private UIMode uiMode;
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
        /// Connect to remote trajectory and IMD services.
        /// </summary>
        public void Connect(string address, int? trajectoryPort, int? imdPort, int? multiplayerPort)
        {
            Sessions.Connect(address, trajectoryPort, imdPort, multiplayerPort);
            GotoInteractionMode();
        }

        /// <summary>
        /// Called from UI to quit the application.
        /// </summary>
        public void Quit() => Application.Quit();

        private void Awake()
        {
            Assert.IsNotNull(application, "Narupa iMD script is missing reference to application");
            NarupaApplication.SetApplication(application);

            ManipulableSimulationSpace = new ManipulableScenePose(simulationSpaceTransform,
                                                                  Sessions.Multiplayer,
                                                                  this);
            ManipulableParticles = new ManipulableParticles(simulationSpaceTransform,
                                                            Sessions.Imd,
                                                            interactableScene);

            SetupVisualisation();
        }

        private void Update()
        {
            CalibratedSpace.CalibrateFromLighthouses();

            ManipulableParticles.ForceScale = 1000;
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

        public void GotoInteractionMode()
        {
            uiMode.gameObject.SetActive(false);
            interactionMode.gameObject.SetActive(true);
        }

        public void GotoUiMode(SteamVR_Input_Sources defaultInput)
        {
            interactionMode.gameObject.SetActive(false);
            uiMode.gameObject.SetActive(true);
            uiMode.SetInputSource(defaultInput);
        }
    }
}