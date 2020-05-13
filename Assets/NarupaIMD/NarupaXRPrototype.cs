// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System.Linq;
using Essd;
using Narupa.Core.Math;
using Narupa.Frontend.XR;
using NarupaIMD;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
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
        private NarupaImdSimulation simulation;
#pragma warning restore 0649

        public NarupaImdSimulation Simulation => simulation;

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
                CalibrateFromLighthouses();
        }
        
        public void CalibrateFromLighthouses()
        {
            var trackers = XRNode.TrackingReference.GetNodeStates()
                                 .OrderBy(state => state.uniqueID)
                                 .Take(2)
                                 .ToList();

            if (trackers.Count == 2
             && trackers[0].GetPose() is UnitScaleTransformation pose0
             && trackers[1].GetPose() is UnitScaleTransformation pose1)
            {
                CalibrateFromTwoControlPoints(pose0.position, pose1.position);
            }
        }

        /// <summary>
        /// Calibrate the space from our personal world position of two 
        /// physical points.
        /// </summary>
        public void CalibrateFromTwoControlPoints(Vector3 worldPoint0, 
                                                  Vector3 worldPoint1)
        {
            var position = 0.5f * (worldPoint0 + worldPoint1);
            position.y = 0;
            var forward = worldPoint1 - worldPoint0;
            forward.y = 0;
            var rotation = Quaternion.LookRotation(forward, Vector3.up);

            simulation.transform.position = position;
            simulation.transform.rotation = rotation;
        }
    }
}