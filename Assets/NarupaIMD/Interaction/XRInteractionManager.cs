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

        private Manipulator leftToolManipulator;
        private Manipulator leftControllerManipulator;
        
        private Manipulator rightToolManipulator;
        private Manipulator rightControllerManipulator;
        
        [SerializeField]
        private ControllerManager controllerManager;

        private void Awake()
        {
            Assert.IsNotNull(controllerManager, $"{nameof(controllerManager)} was not set.");
            controllerManager.LeftController.ControllerReset += () =>
            {
                CreateManipulator(ref leftToolManipulator, 
                                  ref leftControllerManipulator,
                                  controllerManager.LeftController,
                                  SteamVR_Input_Sources.LeftHand);
            };
            
            controllerManager.RightController.ControllerReset += () =>
            {
                CreateManipulator(ref rightToolManipulator, 
                                  ref rightControllerManipulator,
                                  controllerManager.RightController,
                                  SteamVR_Input_Sources.RightHand);
            };
        }
        private void CreateManipulator(ref Manipulator toolManipulator,
                                       ref Manipulator controllerManipulator,
                                       VrController controller,
                                       SteamVR_Input_Sources source)
        {
            // End manipulations if controller has been removed/replaced
            if (toolManipulator != null)
            {
                toolManipulator.EndActiveManipulation();
                toolManipulator = null;
            }
            if (controllerManipulator != null)
            {
                controllerManipulator.EndActiveManipulation();
                controllerManipulator = null;
            }

            if (!controller.IsControllerActive)
                return;
            
            var controllerPoser = controllerPose.WrapAsPosedObject(source);
            var toolPoser = controller.Cursor;
            controllerManipulator = new Manipulator(controllerPoser);
            toolManipulator = new Manipulator(toolPoser);

            var grabSpaceButton = controllerGrabSpaceAction.WrapAsButton(source);
            var grabObjectButton = controllerGrabObjectAction.WrapAsButton(source);

            controllerManipulator.BindButtonToManipulation(grabSpaceButton, AttemptGrabSpace);
            toolManipulator.BindButtonToManipulation(grabObjectButton, AttemptGrabObject);
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