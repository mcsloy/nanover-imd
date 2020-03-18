// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using Narupa.Core.Math;
using Narupa.Frontend.Controllers;
using Narupa.Frontend.Manipulation;
using Narupa.Frontend.XR;
using NarupaIMD;
using UnityEngine;
using UnityEngine.Assertions;
using Valve.VR;

namespace NarupaXR.Interaction
{
    /// <summary>
    /// Translates XR input into interactions the box in NarupaXR.
    /// </summary>
    public class XRBoxInteractionManager : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private NarupaImdSimulation simulation;

        [Header("Controller Actions")]

        [SerializeField]
        private SteamVR_Action_Boolean grabSpaceAction;

        [SerializeField]
        private ControllerManager controllerManager;
#pragma warning restore 0649

        private Manipulator leftManipulator;
        
        private Manipulator rightManipulator;

        private void Awake()
        {
            Assert.IsNotNull(simulation);
            Assert.IsNotNull(controllerManager);
            Assert.IsNotNull(grabSpaceAction);

            controllerManager.LeftController.ControllerReset += () =>
            {
                CreateManipulator(ref leftManipulator, 
                                  controllerManager.LeftController,
                                  SteamVR_Input_Sources.LeftHand);
            };
            
            controllerManager.RightController.ControllerReset += () =>
            {
                CreateManipulator(ref rightManipulator, 
                                  controllerManager.RightController,
                                  SteamVR_Input_Sources.RightHand);
            };
        }
        private void CreateManipulator(ref Manipulator manipulator,
                                       VrController controller,
                                       SteamVR_Input_Sources source)
        {
            // End manipulations if controller has been removed/replaced
            if (manipulator != null)
            {
                manipulator.EndActiveManipulation();
                manipulator = null;
            }

            if (!controller.IsControllerActive)
                return;

            var controllerPoser = controller.GripPose;
            manipulator = new Manipulator(controllerPoser);

            var button = grabSpaceAction.WrapAsButton(source);

            manipulator.BindButtonToManipulation(button, AttemptGrabSpace);
        }

        private IActiveManipulation AttemptGrabSpace(UnitScaleTransformation grabberPose)
        {
            // there is presently only one grabbable space
            return simulation.ManipulableSimulationSpace.StartGrabManipulation(grabberPose);
        }
    }
}