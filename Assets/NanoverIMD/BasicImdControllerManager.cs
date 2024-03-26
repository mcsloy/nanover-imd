using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using Nanover.Frontend.InputControlSystem.ControllerManagers;
using Nanover.Frontend.InputControlSystem.InputHandlers;


namespace NanoverImd
{
    public class BasicImdControllerManager : BasicControllerManager
    {
        /// <summary>
        /// Button used to show the application specific pause menu.
        /// </summary>
        /// <remarks>
        /// Note that this is not necessary the main menu.
        /// </remarks>
        [Tooltip("Button to display the pause menu.")] [SerializeField]
        private InputActionProperty showPauseMenuButton;

        /// <summary>
        /// Specifies what actions should be performed to display the pause menu.
        /// </summary>
        [Tooltip("Events that should be performed to display the pause menu.")] [SerializeField]
        private UnityEvent showPauseMenuEvents = new UnityEvent();

        /// <summary> Displays the pause menu when the <c>showPauseMenuButton</c> is actioned.</summary>
        /// <param name="context">Callback context for the button press event.</param>
        private void ReturnToMainMenu(InputAction.CallbackContext context)
        {
            // Block attempts to access the pause menu when the simulation is not active. This
            // prevents users from being able to access the pause menu from places like the main
            // menu.
            if (SystemObject.GetComponent<NanoverImdSimulation>().Trajectory.Client != null)
                showPauseMenuEvents.Invoke();
        }

        protected new void Awake()
        {
            SystemObject.GetComponent<NanoverImdSimulation>().ConnectionEstablished += EnableArbiter;
            sleepArbiterOnStart = true;
            base.Awake();
            showPauseMenuButton.action.performed += ReturnToMainMenu;
        }

        protected new void OnDestroy()
        {
            base.OnDestroy();
            showPauseMenuButton.action.performed -= ReturnToMainMenu;
        }




    }

}

