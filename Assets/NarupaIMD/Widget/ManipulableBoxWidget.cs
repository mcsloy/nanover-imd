using Narupa.Core.Math;
using Narupa.Frontend.Controllers;
using Narupa.Frontend.Input;
using Narupa.Frontend.Manipulation;
using NarupaIMD.UI;
using NarupaXR.Interaction;
using UnityEngine;
using Valve.VR;

namespace NarupaIMD.State
{
    public class ManipulableBoxWidget : ManipulableWidget
    {
        [SerializeField]
        private Transform simulationSpaceTransform;

        [SerializeField]
        private ConnectedApplicationState application;

        [SerializeField]
        private CalibratedSpaceWidget calibratedSpace;

        [Header("Controller Actions")]
        [SerializeField]
        private SteamVR_Action_Boolean grabSpaceAction;

        protected override void OnEnable()
        {
            ManipulableSimulationSpace = new ManipulableScenePose(simulationSpaceTransform,
                                                                  application.Sessions.Multiplayer,
                                                                  calibratedSpace.CalibratedSpace);

            base.OnEnable();
        }

        protected override IPosedObject GetPose(VrController controller)
        {
            return controller.GripPose;
        }

        protected override SteamVR_Action_Boolean manipulateAction => grabSpaceAction;

        protected override IActiveManipulation AttemptManipulation(Transformation grabberPose)
        {
            return ManipulableSimulationSpace.StartGrabManipulation(grabberPose);
        }

        /// <summary>
        /// The route through which simulation space can be manipulated with
        /// gestures to perform translation, rotation, and scaling.
        /// </summary>
        public ManipulableScenePose ManipulableSimulationSpace { get; private set; }
    }
}