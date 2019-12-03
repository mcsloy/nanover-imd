using Narupa.Core.Math;
using Narupa.Frontend.Controllers;
using Narupa.Frontend.Input;
using Narupa.Frontend.Manipulation;
using Narupa.Frontend.XR;
using UnityEngine;
using UnityEngine.Assertions;
using Valve.VR;

namespace NarupaIMD.State
{
    public abstract class ManipulableWidget : MonoBehaviour
    {
        [SerializeField]
        private ControllerManager controllerManager;

        protected abstract SteamVR_Action_Boolean manipulateAction { get; }

        protected abstract IActiveManipulation AttemptManipulation(Transformation transformation);


        private Manipulator leftManipulator;

        private Manipulator rightManipulator;

        private SteamVrButton leftButton;

        private SteamVrButton rightButton;


        protected virtual void OnEnable()
        {
            Assert.IsNotNull(controllerManager);
            Assert.IsNotNull(manipulateAction);

            controllerManager.LeftController.ControllerReset += OnLeftControllerReset;
            controllerManager.RightController.ControllerReset += OnRightControllerReset;

            OnLeftControllerReset();
            OnRightControllerReset();
        }


        private void OnDisable()
        {
            if (leftManipulator != null)
            {
                leftManipulator.EndActiveManipulation();
                leftManipulator = null;
            }

            if (rightManipulator != null)
            {
                rightManipulator.EndActiveManipulation();
                rightManipulator = null;
            }

            leftButton?.ReleaseBinding();
            rightButton?.ReleaseBinding();

            controllerManager.LeftController.ControllerReset -= OnLeftControllerReset;
            controllerManager.RightController.ControllerReset -= OnRightControllerReset;
        }

        private void OnLeftControllerReset()
        {
            CreateManipulator(ref leftManipulator,
                              ref leftButton,
                              controllerManager.LeftController,
                              SteamVR_Input_Sources.LeftHand);
        }

        private void OnRightControllerReset()
        {
            CreateManipulator(ref rightManipulator,
                              ref rightButton,
                              controllerManager.RightController,
                              SteamVR_Input_Sources.RightHand);
        }


        private void CreateManipulator(ref Manipulator manipulator,
                                       ref SteamVrButton button,
                                       VrController controller,
                                       SteamVR_Input_Sources source)
        {
            // End manipulations if controller has been removed/replaced
            if (manipulator != null)
            {
                manipulator.EndActiveManipulation();
                manipulator = null;
            }

            button?.ReleaseBinding();

            if (!controller.IsControllerActive)
                return;

            var toolPoser = GetPose(controller);
            manipulator = new Manipulator(toolPoser);

            button = manipulateAction.WrapAsButton(source);

            manipulator.BindButtonToManipulation(button, AttemptManipulation);
        }

        protected abstract IPosedObject GetPose(VrController controller);
    }
}