// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using Narupa.Core.Math;
using Narupa.Frontend.Controllers;
using Narupa.Frontend.Manipulation;
using Narupa.Frontend.XR;
using UnityEngine;
using UnityEngine.Assertions;
using Valve.VR;

namespace NarupaXR.Interaction
{
    /// <summary>
    /// Allows the controllers to manipulate the box.
    /// </summary>
    public class BoxManipulation : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private NarupaXRPrototype narupaXR;

        [Header("Controller Actions")]
        [SerializeField]
        private SteamVR_Action_Boolean controllerGrabSpaceAction;

        [SerializeField]
        private SteamVR_Action_Pose controllerPose;
#pragma warning restore 0649

        private Manipulator leftManipulator;
        
        private Manipulator rightManipulator;
        
        [SerializeField]
        private ControllerManager controllerManager;

        private void Awake()
        {
            Assert.IsNotNull(controllerManager, $"{nameof(controllerManager)} was not set.");
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
        private void CreateManipulator(ref Manipulator controllerManipulator,
                                       VrController controller,
                                       SteamVR_Input_Sources source)
        {
            // End manipulations if controller has been removed/replaced
            if (controllerManipulator != null)
            {
                controllerManipulator.EndActiveManipulation();
                controllerManipulator = null;
            }

            if (!controller.IsControllerActive)
                return;
            
            var controllerPoser = controllerPose.WrapAsPosedObject(source);
            controllerManipulator = new Manipulator(controllerPoser);

            var grabSpaceButton = controllerGrabSpaceAction.WrapAsButton(source);

            controllerManipulator.BindButtonToManipulation(grabSpaceButton, AttemptGrabSpace);
        }

        private IActiveManipulation AttemptGrabSpace(Transformation grabberPose)
        {
            // there is presently only one grabbable space
            return narupaXR.ManipulableSimulationSpace.StartGrabManipulation(grabberPose);
        }
    }
}