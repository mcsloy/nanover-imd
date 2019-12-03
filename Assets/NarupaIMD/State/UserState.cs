using Narupa.Frontend.Controllers;
using UnityEngine;
using Valve.VR;

namespace NarupaIMD
{
    /// <summary>
    /// In-scene driver of the current application mode.
    /// </summary>
    public abstract class UserState : MonoBehaviour
    {
        [SerializeField]
        private ControllerManager controller;

        [SerializeField]
        private SteamVR_ActionSet actionSet;

        protected virtual void OnEnable()
        {
            controller.LeftController.ControllerReset += LeftControllerOnControllerReset;
            controller.RightController.ControllerReset += RightControllerOnControllerReset;
            actionSet.Initialize();
            actionSet.Activate();
            SetupController(controller.LeftController);
            SetupController(controller.RightController);
        }

        private void RightControllerOnControllerReset()
        {
            SetupController(controller.RightController);
        }

        private void LeftControllerOnControllerReset()
        {
            SetupController(controller.LeftController);
        }

        protected virtual void OnDisable()
        {
            controller.LeftController.ControllerReset -= LeftControllerOnControllerReset;
            controller.RightController.ControllerReset -= RightControllerOnControllerReset;
            actionSet.Deactivate();
        }
        
        protected virtual void SetupController(VrController controller)
        {
        }
    }
}