// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using Essd;
using Narupa.Frontend.XR;
using UnityEngine;
using UnityEngine.Events;
using NarupaImd.Interaction;
using System.Threading.Tasks;
using Narupa.Core.Math;
using Valve.VR;

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

        public PlayAreaCollection PlayAreas { get; private set; }
        public PlayOriginCollection PlayOrigins { get; private set; }

        [SerializeField]
        private UnityEvent connectionEstablished;

        private void Awake()
        {
            simulation.ConnectionEstablished += connectionEstablished.Invoke;
            PlayAreas = new PlayAreaCollection(Simulation.Multiplayer);
            PlayOrigins = new PlayOriginCollection(Simulation.Multiplayer);
        }

        /// <summary>
        /// Connect to remote Narupa services.
        /// </summary>
        public Task Connect(string address,
                            int? trajectoryPort = null,
                            int? imdPort = null,
                            int? multiplayerPort = null) =>
            simulation.Connect(address, trajectoryPort, imdPort, multiplayerPort);

        // These methods expose the underlying async methods to Unity for use
        // in the UI so we disable warnings about not awaiting them, and use
        // void return type instead of Task.
        #pragma warning disable 4014
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
        #pragma warning restore 4014

        private void Update()
        {

            if (ColocateLighthouses)
                CalibratedSpace.CalibrateFromLighthouses();
            else
                CheckSpaceReposition();

            UpdatePlayArea();
        }

        private Vector3 playareaSize = Vector3.zero;

        /// <summary>
        /// Determine VR playarea size;
        /// </summary>
        private void UpdatePlayArea()
        {
            var chaperone = OpenVR.Chaperone;
            if (chaperone == null)
                return;

            if (chaperone.GetCalibrationState() != ChaperoneCalibrationState.OK)
                return;

            var rect = new HmdQuad_t();
            if (!chaperone.GetPlayAreaRect(ref rect))
                return;

            chaperone.GetPlayAreaSize(ref playareaSize.x, ref playareaSize.z);

            if (simulation.Multiplayer.AccessToken == null)
                return;

            var area = new PlayArea
            {
                A = TransformCornerPosition(rect.vCorners0),
                B = TransformCornerPosition(rect.vCorners1),
                C = TransformCornerPosition(rect.vCorners2),
                D = TransformCornerPosition(rect.vCorners3),
            };

            PlayAreas.UpdateValue(simulation.Multiplayer.AccessToken, area);

            Vector3 TransformCornerPosition(HmdVector3_t corner)
            {
                var position = new Vector3(corner.v0, corner.v1, corner.v2);
                var transform = new Transformation(position, Quaternion.identity, Vector3.one);
                return CalibratedSpace.TransformPoseWorldToCalibrated(transform).Position;
            }
        }

        /// <summary>
        /// If a suggested origin exists for the local user, apply it to the
        /// calibrated space.
        /// </summary>
        private void CheckSpaceReposition()
        {
            var key = "user-origin." + simulation.Multiplayer.AccessToken;

            if (PlayOrigins.ContainsKey(key))
            {
                var origin = PlayOrigins.GetValue(key);

                var radiusFactor = 0.5f;
                var RotationCorrection = 0f;

                var longest = Mathf.Max(playareaSize.x, playareaSize.z);
                var offset = longest * radiusFactor;
                var playspaceToShared = origin.Transformation.matrix.inverse;
                var deviceToPlayspace = Matrix4x4.TRS(
                    Vector3.zero,
                    Quaternion.AngleAxis(RotationCorrection, Vector3.up),
                    Vector3.one
                ) * Matrix4x4.TRS(
                    Vector3.left * offset,
                    Quaternion.identity,
                    Vector3.one
                );


                CalibratedSpace.CalibrateFromMatrix(deviceToPlayspace * playspaceToShared);
            }
        }
    }
}