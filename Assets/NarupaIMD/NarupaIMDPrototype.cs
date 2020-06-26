// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using Essd;
using Narupa.Frontend.XR;
using NarupaIMD.Interaction;
using UnityEngine;
using UnityEngine.Events;
using Text = TMPro.TextMeshProUGUI;

namespace NarupaIMD
{
    /// <summary>
    /// The entry point to the application, and central location for accessing
    /// shared resources.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NarupaIMDPrototype : MonoBehaviour
    {
#pragma warning disable 0649
        
        [SerializeField]
        private NarupaImdSimulation simulation;
        
        private InteractableScene interactableScene;
#pragma warning restore 0649

        public NarupaImdSimulation Simulation => simulation;

        public PhysicallyCalibratedSpace CalibratedSpace { get; } = new PhysicallyCalibratedSpace();

        [SerializeField]
        private UnityEvent connectionEstablished;

        private void Awake()
        {
            simulation.ConnectionEstablished += connectionEstablished.Invoke;
            Colocation.ColocationSettingChanged +=
                () => isColocationActive = Colocation.IsEnabled();
            isColocationActive = Colocation.IsEnabled();
        }

        /// <summary>
        /// Connect to remote Narupa services.
        /// </summary>
        public void Connect(string address,
                            int? trajectoryPort = null,
                            int? imdPort = null,
                            int? multiplayerPort = null) =>
            simulation.Connect(address, trajectoryPort, imdPort, multiplayerPort);

        /// <summary>
        /// Connect to the Narupa services described in a given ServiceHub.
        /// </summary>
        public void Connect(ServiceHub hub) => simulation.Connect(hub);

        /// <summary>
        /// Connect to the first set of Narupa services found via ESSD.
        /// </summary>
        public void AutoConnect() => simulation.AutoConnect();

        /// <summary>
        /// Disconnect from all Narupa services.
        /// </summary>
        public void Disconnect() => simulation.CloseAsync();

        /// <summary>
        /// Called from UI to quit the application.
        /// </summary>
        public void Quit() => Application.Quit();

        private bool isColocationActive = false;
        
        private void Update()
        {
            if(isColocationActive)
                CalibratedSpace.CalibrateFromLighthouses();
            else
                CalibratedSpace.ResetCalibration();
        }
    }
}