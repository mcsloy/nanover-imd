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
    /// Translates XR input into interactions with particles in NarupaXR.
    /// </summary>
    public class XRParticleInteractionManager : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private NarupaXRPrototype narupaXR;

        [Header("Controller Actions")]
        [SerializeField]
        private SteamVR_Action_Boolean grabObjectAction;
#pragma warning restore 0649

        private Manipulator leftManipulator;
        
        private Manipulator rightManipulator;
        
        [SerializeField]
        private ControllerManager controllerManager;

        private void Awake()
        {
            Assert.IsNotNull(narupaXR);
            Assert.IsNotNull(controllerManager);
            Assert.IsNotNull(grabObjectAction);
            
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
            
            var toolPoser = controller.CursorPose;
            manipulator = new Manipulator(toolPoser);

            var button = grabObjectAction.WrapAsButton(source);

            manipulator.BindButtonToManipulation(button, AttemptGrabObject);
        }

        private IActiveManipulation AttemptGrabObject(Transformation grabberPose)
        {
            // there is presently only one grabbable set of objects
            return narupaXR.ManipulableParticles.StartParticleGrab(grabberPose);
        }
    }
}