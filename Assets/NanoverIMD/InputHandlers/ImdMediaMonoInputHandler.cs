using Nanover.Frontend.InputControlSystem.InputControllers;
using Nanover.Frontend.InputControlSystem.InputHandlers;
using Nanover.Frontend.UI.ContextMenus;
using Nanover.Grpc.Trajectory;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Nanover.Frontend.InputControlSystem.Utilities.InputControlUtilities;

namespace NanoverImd.InputHandlers
{

    /// <summary>
    /// Adds media playback functionality to the interactive molecular dynamics code. 
    /// </summary>
    /// <remarks>
    /// This input handler adds a simple radial menu offering pause, play, and restart capabilities.
    /// </remarks>
    public class ImdMediaMonoInputHandler : MonoInputHandler, IUserSelectableInputHandler, ITrajectorySessionDependentInputHandler
    {
        /* Developer's Notes:
         * Currently the "Show Main Menu" option in the in the radial menu is not hooked up to anything
         * as the GUI has not yet been reimplemented. This will need to be done once the GUI has been
         * hooked back up again.
         */


        /// <summary>
        /// User friendly name that uniquely identifies the impulse handler.
        /// </summary>
        public string Name => "IMD Media";

        /// <summary>
        /// An icon that may be shown to the user in a selection menu to represent the impulse handler.
        /// </summary>
        public Sprite Icon => Resources.Load<Sprite>("UI/Icons/InputHandlers/Media");

        /// <summary>
        /// Priority of the input handler relative to others.
        /// </summary>
        public ushort Priority => 999;

        /// <summary>
        /// Current state of the input handler. This dictates the handler's level of activity and
        /// responsiveness to input.
        /// </summary>
        public override State State
        {
            get => state;

            set
            {
                if (value == State.Paused || value == State.Passive)
                    value = State.Disabled;

                switch (state, value)
                {
                    case (State.Disabled, State.Active):
                        state = value;
                        gameObject.SetActive(true);
                        break;
                    case (State.Active, State.Disabled):
                        state = value;
                        gameObject.SetActive(false);
                        break;

                }
            }
        }
        
        /// <summary>
        /// Radial selection menu for showing the various media options.
        /// </summary>
        private RadialMenu radialMenu;

        /// <summary>
        /// The trajectory session instance through with the simulation may be paused, resumed, or
        /// restarted.
        /// </summary>
        private TrajectorySession trajectory;


        /// <summary>
        /// Set the required trajectory session.
        /// </summary>
        /// <param name="trajectory">The required trajectory session</param>
        public void SetTrajectorySession(TrajectorySession trajectory)
        {

            // Store the trajectory session entity as it will be needed later on in the callback
            // function.
            this.trajectory = trajectory;

            // Once the trajectory session instance has been provided a radial menu can be constructed
            // which allows users to pause, play, and restart the simulation.
            radialMenu = gameObject.AddComponent<RadialMenu>();
            radialMenu.Initialise(Controller, Controller.InputActionMap.FindAction("Primary Button"), menuName: "Media Options");
            string path = "UI/Icons/InputHandlers";
            radialMenu.ConfigureMenuElements(
                new[] {$"{path}/Play", $"{path}/Pause", $"{path}/Restart", $"{path}/Menu"},
                new[] {"Play", "Pause", "Restart", "Menu"});

            // The selecting and option in the radial menu will trigger a call to `RadialMenuCallback`
            radialMenu.OptionSelected += RadialMenuCallback;

            // Finalise the radial menu so that it can be used.
            radialMenu.Finalise();

        }

        /// <summary>
        /// Callback method for radial menu element selection events
        /// </summary>
        /// <param name="index">Index the of the option selected in the radial menu.</param>
        private void RadialMenuCallback(int index)
        {

            // Perform the appropriate action based on which element was selected in the menu.
            // The indices of the menu options is determined by the order in which they were
            // supplied to the radial menu during its initialisation.
            switch (index)
            {
                case 0:
                    // Play/resume trajectory playback.
                    trajectory.Play();
                    break;
                case 1:
                    // Pause trajectory playback.
                    trajectory.Pause();
                    break;
                case 2:
                    // Reset trajectory playback back to the first frame.
                    trajectory.Reset();
                    break;
                case 3:
                    // Show the main menu. This will be implemented once the GUI has been hooked up.
                    break;
            }
        }

        void Awake() => gameObject.SetActive(false);
        
        public override void UnbindController(InputController controller)
        {
            throw new System.NotImplementedException();
        }

        public override void Background() => State = State.Disabled;

        /// <summary>
        /// A list of <c>InputAction</c> entities specifying which input actions that the handler
        /// expects to be given sole binding privilege to.
        /// </summary>
        /// <returns>Hash set containing the input actions that this handler is expected to use.</returns>
        public override HashSet<InputAction> RequiredBindings()
        {
            return new HashSet<InputAction> { Controller.InputActionMap.FindAction("Primary Button")};
        }


        public override bool IsCompatibleWithInputController(InputController controller) => controller is BasicInputController;




        void OnDisable()
        {
            if (state != State.Disabled) State = State.Disabled;
        }

        void OnEnable()
        {
            if (state == State.Disabled) State = State.Active;
        }

    }
}
