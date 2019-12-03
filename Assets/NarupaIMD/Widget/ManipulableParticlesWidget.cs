using System;
using Narupa.Core.Math;
using Narupa.Frontend.Controllers;
using Narupa.Frontend.Input;
using Narupa.Frontend.Manipulation;
using Narupa.Frontend.XR;
using NarupaIMD.UI;
using NarupaXR.Interaction;
using UnityEngine;
using UnityEngine.Assertions;
using Valve.VR;

namespace NarupaIMD.State
{
    public class ManipulableParticlesWidget : ManipulableWidget
    {
        [SerializeField]
        private ConnectedApplicationState application;

        [SerializeField]
        private Transform simulationSpaceTransform;

        [SerializeField]
        private InteractableScene interactableScene;

        /// <summary>
        /// The route through which simulated particles can be manipulated with
        /// grabs.
        /// </summary>
        public ManipulableParticles ManipulableParticles { get; private set; }

        protected override void OnEnable()
        {
            ManipulableParticles = new ManipulableParticles(simulationSpaceTransform,
                                                            application.Sessions.Imd,
                                                            interactableScene);

            base.OnEnable();
        }

        protected override IPosedObject GetPose(VrController controller)
        {
            return controller.CursorPose;
        }


        [Header("Controller Actions")]
        [SerializeField]
        private SteamVR_Action_Boolean grabObjectAction;
        
        protected override SteamVR_Action_Boolean manipulateAction => grabObjectAction;

        protected override IActiveManipulation AttemptManipulation(Transformation grabberPose)
        {
            return ManipulableParticles.StartParticleGrab(grabberPose);
        }
    }
}