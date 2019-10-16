// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using Narupa.Core.Math;
using Narupa.Frontend.Manipulation;
using Narupa.Frontend.XR;
using UnityEngine;
using Valve.VR;

namespace NarupaXR.Interaction
{
    /// <summary>
    /// Translates XR input into interactions with NarupaXR.
    /// </summary>
    public class XRInteractionManager : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private NarupaXRPrototype narupaXR;

        [Header("Controller Actions")]
        [SerializeField]
        private SteamVR_Action_Boolean controllerGrabObjectAction;

        [SerializeField]
        private SteamVR_Action_Boolean controllerGrabSpaceAction;

        [SerializeField]
        private SteamVR_Action_Pose controllerPose;
#pragma warning restore 0649

        private Manipulator leftManipulator, rightManipulator;

        private void Start()
        {
            leftManipulator = CreateManipulator(SteamVR_Input_Sources.LeftHand);
            rightManipulator = CreateManipulator(SteamVR_Input_Sources.RightHand);
        }

        private Manipulator CreateManipulator(SteamVR_Input_Sources source)
        {
            var poser = controllerPose.WrapAsPosedObject(source);
            var manipulator = new Manipulator(poser);

            var grabSpaceButton = controllerGrabSpaceAction.WrapAsButton(source);
            var grabObjectButton = controllerGrabObjectAction.WrapAsButton(source);

            manipulator.BindButtonToManipulation(grabSpaceButton, AttemptGrabSpace);
            manipulator.BindButtonToManipulation(grabObjectButton, AttemptGrabObject);

            return manipulator;
        }

        private IActiveManipulation AttemptGrabObject(Transformation grabberPose)
        {
            // there is presently only one grabbable set of objects
            return narupaXR.ManipulableParticles.StartParticleGrab(grabberPose);
        }

        private IActiveManipulation AttemptGrabSpace(Transformation grabberPose)
        {
            // there is presently only one grabbable space
            return narupaXR.ManipulableSimulationSpace.StartGrabManipulation(grabberPose);
        }
    }
}