using Nanover.Frontend.InputControlSystem.InputControllers;
using Nanover.Frontend.InputControlSystem.InputHandlers;
using NanoverImd.Interaction;
using System;
using Nanover.Frontend.UI.ContextMenus;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector3 = UnityEngine.Vector3;
using System.Collections.Generic;
using Nanover.Grpc.Multiplayer;

namespace NanoverImd.InputHandlers
{
    /// <summary>
    /// This input handler permits user to apply a force to an atom, or group thereof. Thus facilitating,
    /// interactive molecular dynamics.
    /// </summary>
    /// <remarks>
    /// When the user presses the <c>triggerButtonName</c> button on their controller, the closest
    /// atom is identified and a force is applied to it which drives it towards the controller.
    /// Using the radial selection menu, users may choose between interacting with a single atom at
    /// a time or an entire residue. The amount of force being applied to the target entity may be
    /// increased or decreased by manipulating the <c>scaleAnalogueName</c> button.
    /// </remarks>
    public class ImpulseMonoInputInputHandler : SelectorMonoInputInputHandler, IUserSelectableInputHandler, IMultiplayerSessionDependentInputHandler
    {

        /* Developer's notes:
         * i  ) A limit should be placed on the maximum distance (in world space) that an atom can be
         *      from the controller and still be actioned. That is to say, if the controller is very
         *      far away from all of the atoms then depressing the trigger should have no effect.
         * ii ) The atom, or atoms, that will be acted upon a trigger press event should be highlighted.
         *      This way users know exactly which atom/residue is going to be effected when they press
         *      the trigger.
         * iii) An outline should be used to highlight the atoms and residues as this will
         *      provide a more obvious highlighting effect.
         */

        /// <summary>
        /// User friendly name that uniquely identifies the impulse handler.
        /// </summary>
        public string Name => "Impulse";

        /// <summary>
        /// An icon that may be shown to the user in a selection menu to represent the impulse handler.
        /// </summary>
        public Sprite Icon => Resources.Load<Sprite>("UI/Icons/InputHandlers/Force");

        /// <summary>
        /// Priority of the input handler relative to others.
        /// </summary>
        public ushort Priority => 1000;

        /// <summary>
        /// Force multiplier for the impulse interaction.
        ///
        /// The amount of force applied to the selected target will increase with increasing
        /// <code>forceScaleFactor</code>, this defaults to a value of 50.
        /// </summary>
        private float forceScaleFactor = 50f;

        /// <summary>
        /// Force multiplier for the impulse interaction.
        ///
        /// The amount of force applied to the selected target will increase with increasing
        /// <code>forceScaleFactor</code>. Provided values will be automatically clammed to within
        /// the domain of [<c>forceScaleFactorLowerBounds</c>, <c>forceScaleFactorUpperBounds</c>].
        /// </summary>
        public float ForceScaleFactor
        {
            set => forceScaleFactor = Mathf.Clamp(value, forceScaleFactorLowerBounds, forceScaleFactorUpperBounds);
            get => forceScaleFactor;
        }

        /// <summary>
        /// The maximum value to which the force scale factor can be set.
        /// </summary>
        /// <remarks>
        /// Establishing a sensible upper limit on the force scale factor prevents the handler from
        /// selecting a value that could lead to instant thermalisation upon application. Such a
        /// precaution helps in safeguarding the user experience.
        /// </remarks>
        public float forceScaleFactorUpperBounds = 200f;

        /// <summary>
        /// The minimum value to which the force scale factor can be set.
        /// </summary>
        /// <remarks>
        /// Generally this should be set to zero. However, selecting a negative value permits push
        /// type interactions, rather than the default pull type.
        /// </remarks>
        public float forceScaleFactorLowerBounds = 0f;

        /// <summary>
        /// The maximum rate at which the force scale factor is allowed to change per second.
        /// </summary>
        /// <remarks>
        /// Rate limiting is applied to avoid immediate escalation to maximum or minimum values of
        /// the scale factor in response to rapid input updates. It ensures that the force adjustment
        /// remains controlled and does not react excessively to each hardware update signal received
        /// through OpenXR.
        /// </remarks>
        public float forceScaleFactorMaxRate = 50f;
        
        /// <summary>
        /// The minimum increment by which the force scale factor can be adjusted.
        /// </summary>
        /// <remarks>
        /// This setting controls the granularity of adjustments to the force scale factor, ensuring
        /// that changes occur in discrete steps. A value of 1, for example, restricts adjustments
        /// to whole numbers, facilitating more predictable and manageable scaling.
        /// </remarks>
        public float forceScaleFactorStep = 1f;

        /// <summary>
        /// Name of the input action which will provide the analogue input used to modify the force
        /// scaling factor.
        /// </summary>
        /// <remarks>
        /// Currently this is hard-coded to the thumbstick, as such the rest of the code expects a
        /// <code>Vector2</code> input source. Thus changes will need to be made if one wishes to
        /// use a float input source.
        /// </remarks>
        private string scaleAnalogueName = "Thumbstick";

        /// <summary>
        /// A unique ID for interactions performed by this input handler.
        /// </summary>
        private string ID = Guid.NewGuid().ToString();

        /// <summary>
        /// Structure for providing information about active particle interactions with the server.
        /// </summary>
        /// <remarks>
        /// This attribute has temporarily been moved up to the class level so that all instances
        /// share the same <c>ParticleInteractionCollection</c> entity. This is to make interfacing
        /// with the old monolithic code a little easier until it can be rewritten.
        /// </remarks>
        public static ParticleInteractionCollection interactionCollection;
        
        /// <summary>
        /// Text panel which displays the force scale factor to the user during modification.
        /// </summary>
        private TMP_Text scaleText;
        
        /// <summary>
        /// Game object to within which the text panel will be placed.
        /// </summary>
        /// <remarks>
        /// This is only made visible as and when the user modifies the scale factor, i.e. they move
        /// the thumbstick up and down. This object will be made a child of the controller when the
        /// input handler becomes active, so that the text tracks the controller. When the handler is
        /// inactive the object will be moved back under the input handler object to avoid clutter.
        /// </remarks>
        private GameObject scaleTextPanel;

        /// <summary>
        /// The rectangular transform of the scale text game object.
        /// </summary>
        /// <remarks>
        /// This just saves having to make repeated, expensive calls to <c>GetComponent</c>.
        /// </remarks>
        private RectTransform scaleTextRectTransform;

        /// <summary>
        /// Last time the force scale factor was modified.
        /// </summary>
        /// <remarks>
        /// When the thumbstick is actively being manipulated this field will be used to store the
        /// time at which the last input event was revived from the controller. This is required to
        /// ensure that the rate at which the force scale factor changes is invariant to poll rate
        /// and frame rate.
        /// </remarks>
        private float lastForceScaleFactorUpdateTime = 0.0f;

        /// <summary>
        /// Used to draw a linking line between the controller and the entity that is being actioned.
        /// </summary>
        private LineRenderer lineRenderer;

        /// <summary>
        /// Maximum rate at which impulse request messages can be sent.
        /// </summary>
        /// <remarks>
        /// This places an upper bounds on how quickly successive impulse request message can be
        /// sent. Without this a message would be constructed and sent at each and every call to
        /// `Update` so long as the trigger is held down. The server is only able to process one
        /// impulse request on a target per frame, so sending thousands of requests does nothing
        /// other than place an unnecessary load on the network link & its associated processors.
        /// Hence an upper rate limit has been implemented.
        /// </remarks>
        private readonly float maxImpulseMessageFrequency = 1f / 60f;

        /// <summary>
        /// Time at which the last impulse message was sent.
        /// </summary>
        /// <remarks>
        /// This is used in conjunction with the <code>maxImpulseMessageFrequency</code> field to
        /// set an upper bounds limit on the frequency at which impulse request messages are sent
        /// to the server.
        /// </remarks>
        private DateTime timeOfLastImpulseMessage = DateTime.Now;

        /// <summary>
        /// A Boolean indicating if residue selection mode is active.
        /// </summary>
        /// <remarks>
        /// By default, only the closest atom to the controller will be selected when the trigger
        /// is depressed, and thus force will only be applied to that one atom. However, all atoms
        /// in the closest residue will be selected instead if residue selection mode is enabled.
        /// </remarks>
        public bool ResidueSelectionModeIsActive { get; set; }

        /// <summary>
        /// The radial menu used to present selection modes to the user.
        /// </summary>
        /// <remarks>
        /// This permits users to select between single atom and residue based selection methods.
        /// </remarks>
        private RadialMenu radialMenu;

        /// <summary>
        /// Game object upon which the radial menu is placed.
        /// </summary>
        /// <remarks>
        /// The radial menu is placed onto a child object to neaten up the object and to help prevent
        /// possible conflicts if other menu systems are added to this handler in the future. 
        /// </remarks>
        private GameObject radialGameObject;


        // Temporary stuff to do with the current way in which the line tenderer is drawn. These
        // components will be removed prior to pushing this branch into production.
        private Mixer mixer;
        private Vector3 oldPosition;
        private float oldTime;
        private Vector3 oldVelocity = Vector3.zero;

        /// <summary>
        /// Set the required multiplayer session.
        /// </summary>
        /// <param name="multiplayer">The required multiplayer session</param>
        public void SetMultiplayerSession(MultiplayerSession multiplayer)
        {
            // Access to the multiplayer session is needed to construct a new particle interaction
            // collection entity which can be used to instruct the server to apply an impulse to
            // the selected atoms.

            // Instantiate the class level field `interactionCollection` if it has not yet been
            // done so already.
            if (interactionCollection == null)
                interactionCollection = new ParticleInteractionCollection(multiplayer);
        }
        
        /// <summary>
        /// Perform state change event.
        ///
        /// This method handles transitions between the various different states. This is intended
        /// as a minimum working example, and should therefore be overridden locally with a more
        /// appropriate method.
        /// </summary>
        /// <param name="currentState">Current state in which the handler exists.</param>
        /// <param name="newState">New state to which the handler should transition.</param>
        protected override void PerformStateChange(State currentState, State newState)
        {

            switch ((currentState, newState))
            {

                case (State.Disabled, State.Active):
                    // Subscribe to controller events if transitioning from a `Disabled` state to
                    // an `Active` state.
                    gameObject.SetActive(true);
                    Subscribe();
                    break;
                case (State.Active, State.Disabled):
                    // Abort any active engagements and unsubscribe from controller events if
                    // transitioning from an `Active`  state to a `Disabled` state.
                    Unsubscribe();
                    if (isEngaged)
                        Disengage();
                    gameObject.SetActive(false);
                    break;
                case (State.Paused, State.Active):
                    // Treat resuming from a pause the same as `Disabled` -> `Active`
                    PerformStateChange(State.Disabled, newState);
                    break;
                case (State.Active, State.Paused):
                    // Treat pausing the same as `Active` -> `Disabled`
                    PerformStateChange(currentState, State.Disabled);
                    break;
                default:
                    break;
            }

            // Now that all necessary state change event actions have been performed the state can
            // be safely updated.
            state = newState;
        }

        /// <summary>
        /// Callback method for the radial selection menu.
        /// </summary>
        /// <param name="index">Index of the selected menu element</param>
        /// <exception cref="ArgumentOutOfRangeException">Index outside of expected bounds</exception>
        /// <remarks>
        /// When the user selects either "Atom" or "Residue" select mode from the radial menu,
        /// this method will be called to update the local selection mode setting.
        /// </remarks>
        private void SetSelectionMode(int index)
        {
            // The first element in the radial menu is "Atom" selection mode and the second is
            // "residue" selection mode.
            if (index == 0)
                ResidueSelectionModeIsActive = false;
            else if (index == 1)
                ResidueSelectionModeIsActive = true;
            else
                throw new ArgumentOutOfRangeException(nameof(index), "Index must be 0 or 1");
            
        }

        void Update()
        {
            // Todo: Move this to the Bind Controller method
            // Setup the radial selection menu to which permits user to change the current selection mode
            if (radialMenu == null && Controllers != null)
            {
                // Construct the object upon which the radial menu selection menu is to be placed.
                radialGameObject = new GameObject("Target Selection Mode Radial Menu");
                radialGameObject.transform.SetParent(transform);
                radialGameObject.transform.localPosition = Vector3.zero;
                radialGameObject.transform.localRotation = Quaternion.identity;


                radialMenu = radialGameObject.AddComponent<RadialMenu>();
                radialMenu.Initialise(Controller, Controller.InputActionMap.FindAction("Primary Button"), menuName: "Selection Mode");
                radialMenu.ConfigureMenuElements(
                    new string[]
                    {
                        "UI/Icons/InputHandlers/AtomSelect",
                        "UI/Icons/InputHandlers/ResidueSelect"
                    },
                    new string[]
                    {
                        "Atom Select",
                        "Residue Select"
                    });

                radialMenu.OptionSelected += SetSelectionMode;
                radialMenu.Finalise();

            }

            // If the handler is currently engaged and a closest atom has been identified
            if (isEngaged && closestAtomIndex != -1)
            {

                // Ensue that impulse message are constructed and sent to the server at a reasonable
                // rate to avoid spamming the server with messages.
                if ((DateTime.Now - timeOfLastImpulseMessage).TotalSeconds >= maxImpulseMessageFrequency)
                {

                    // Record the current time so that the above check can be performed in the next
                    // call to `Update`.
                    timeOfLastImpulseMessage = DateTime.Now;

                    // Get the position of the controller in the local coordinate system of the simulation space
                    Vector3 cursorPosition = simulationGameObject.transform.InverseTransformPoint(Controller.transform.position);

                    // List of atoms to which an impulse force should be applied.
                    List<int> targetAtoms = new List<int>();

                    var frame = frameSynchroniser.CurrentFrame;

                    // If either of the following conditions holds true:
                    //  - i)   currently in atom select mode
                    //  - ii)  the residue array is uninitialised
                    //  - iii) there are no residues present in the system
                    //  - iv)  the closest atom does not belong to any residue
                    // Then only select the closest atom to apply an impulse to
                    if (!ResidueSelectionModeIsActive || frame.ParticleResidues == null || frame.ParticleResidues.Length == 0 || frame.ParticleResidues[closestAtomIndex] == -1)
                        targetAtoms.Add(closestAtomIndex);

                    // Identify all atoms that belong to the same residue as the closest atom and
                    // append them to the `targetAtoms` list.
                    else
                    {
                        int targetResidue = frame.ParticleResidues[closestAtomIndex];
                        for (int i = 0; i < frame.ParticleCount; i++)
                            if (frame.ParticleResidues[i] == targetResidue)
                                targetAtoms.Add(i);
                    }


                    // Provide information to the server that an impulse action is being performed.
                    interactionCollection.UpdateValue(ID, new ParticleInteraction()
                    {
                        Particles = targetAtoms,
                        Position = cursorPosition,
                        Scale = forceScaleFactor,
                        InteractionType = "spring",
                        ResetVelocities = false
                    });

                    // This method is a little overly complicated and should be replaced with the old
                    // line render until a more ascetically pleasing approach can be created.

                    // Now that an impulse command has been sent to the server a visual cue must be
                    // provided to the user so that they know which atoms they are interacting with.
                    // Specifically in the form of a line render. Note that atom highlighting takes
                    // place elsewhere in the code-base. Currently the line render takes the form of
                    // a cubic Bezier curve, the command points of which are used to indicate the
                    // amount of force being applied.
                    // Vectors:
                    //  - vec0: Position of the controller.
                    //  - vec1: Position of the first quartile along the vector between the
                    //          controller and the atom. In the future this will be modified so
                    //          that it moves further along the vector as the force scale factor
                    //          is increased.
                    //  - vec2: Position of the closest atom + velocity
                    //  - vec3: Position of the closest atom
                    Vector3[] positions = GetPositions();

                    Vector3 vec3 = simulationGameObject.transform.TransformPoint(positions[closestAtomIndex]);
                    Vector3 vec2 = simulationGameObject.transform.TransformPoint(
                        positions[closestAtomIndex] +
                        getVelocity());
                    Vector3 vec0 = Controller.transform.position;
                    Vector3 vec1 = Vector3.Lerp(vec0, vec3, 0.9f);

                    // Construct the cubic Bezier curve
                    Vector3[] points = CubicBezier(vec0, vec1, vec2, vec3, Linspace(0f, 1f, 50));

                    // Smooth out the Bezier curve
                    Vector3[] mixedPoints = mixer.Mix(points);

                    // The first and last positions should not undergo smoothing.
                    (mixedPoints[0], mixedPoints[^1]) = (points[0], points[^1]);

                    lineRenderer.positionCount = points.Length;
                    lineRenderer.SetPositions(mixedPoints);
                }

            }
        }

        /// <summary>
        /// A callback for initiating engagement. The closest atom to the controller's cursor position
        /// is identified, and the handler is marked as engaged.
        /// </summary>
        protected override void Engage()
        {
            // Find the closest atom to the controller's cursor position and assign the index
            // of that atom to the `closestAtomIndex` field.
            closestAtomIndex = ClosestAtomicIndex(Controller.transform.position);

            // Show the line tenderer
            lineRenderer.enabled = true;

            // Signal that the handler is now engaged. This is used to indicate to other parts
            // of the handler that they should now act.
            isEngaged = true;
            
        }

        /// <summary>
        /// A callback for terminating engagement. This method is invoked typically when a user input
        /// signifies the end of an interaction, leading to the disengagement of the handler.
        /// </summary>
        protected override void Disengage()
        {
            // Handler is no longer engaged.
            isEngaged = false;

            // And thus may not have a closest atom.
            closestAtomIndex = -1;

            // Hide the line tenderer
            lineRenderer.enabled = false;
            mixer.Reset();

            // Flush the interaction collection
            interactionCollection.RemoveValue(ID);
        }

        /// <summary>
        /// Callback method for the scale modification input action.
        /// </summary>
        /// <param name="context">Input action callback context</param>
        /// <remarks>
        /// This method is invoked whenever the user modifies the force scale factor, commonly by
        /// moving the thumbstick. This will not only change the scale factor but it will also show
        /// a small window to provide visual feedback to the user.
        /// </remarks>
        private void ModifyScale(InputAction.CallbackContext context)
        {
            // Get the position of the thumbstick along the x & y axes over the domain [-1, 1]
            Vector2 xy = context.ReadValue<Vector2>();
            float y = xy.y;
            float x = xy.x;

            // If this callback represents the start or continuation of a thumbstick input event
            // then update the force scale factor accordingly and display the feedback panel.
            // Thumbstick movement is only considered to be intentional if the deflection along
            // the y-axis is greater than that along the x-axis.
            if ((context.phase == InputActionPhase.Performed) && (MathF.Abs(y) >= MathF.Abs(x)))
            {
                if (lastForceScaleFactorUpdateTime == 0f)
                {
                    lastForceScaleFactorUpdateTime = Time.time;
                    return;
                }

                // Set the position relative to the controller. This approach to updating the window's
                // position will be a little laggy, but should be tolerable.
                scaleTextRectTransform.rotation = Controller.transform.rotation;
                scaleTextRectTransform.position = Controller.transform.position;

                // Transform from linear to non-linear scaling. This allows for both fine grained
                // control as well as larger movements. That is to say the further the analogue
                // stick is moved, the faster the scale factor changes.
                y = Mathf.Pow(Mathf.Abs(y), Mathf.Exp(1f)) * Mathf.Sign(y);

                // Work out the time elapsed since the last update (in seconds)
                float t = Time.time - lastForceScaleFactorUpdateTime;

                // Work out how much by which the scale factor is to be increased or decreased
                float delta = t * forceScaleFactorMaxRate * y;

                // Round the change to the nearest integer multiple of `forceScaleFactorStep`
                float deltaRounded = Mathf.Round(delta / forceScaleFactorStep) * forceScaleFactorStep;

                // If the amount of change gets rounded to zero then abort the scale attempt & allow
                // more time to pass so that a greater degree of change may accumulate.
                if (Mathf.Abs(deltaRounded) < forceScaleFactorStep) return;

                // Scale the force multiplier appropriately, while ensuring that the value remains
                // within the domain of [forceScaleFactorLowerBounds, forceScaleFactorUpperBounds]
                forceScaleFactor = Mathf.Clamp(
                    forceScaleFactor + deltaRounded,
                    forceScaleFactorLowerBounds, forceScaleFactorUpperBounds);

                // Update the text displayed in the feedback panel to show the updated force
                scaleText.text = $"{forceScaleFactor:000.0}";

                // Record when this function was last called to allow for the time elapsed to be
                // calculated next call.
                lastForceScaleFactorUpdateTime = Time.time;

                // Ensure that the feedback panel is active
                scaleTextPanel.SetActive(true);
            }

            // Otherwise if this the input event session coming to an end, i.e. the thumbstick is
            // released or is no longer clearly pushed up/down, then hide the feedback panel.
            else
            {
                // Disable the feedback panel
                scaleTextPanel.SetActive(false);

                // Zero out the last update time tracker.
                lastForceScaleFactorUpdateTime = 0f;
            }
        }

        
        /// <summary>
        /// Subscribe to user input action events.
        /// </summary>
        protected override void SubscribeAncillary()
        {
            Controller.InputActionMap.FindAction(scaleAnalogueName).performed += ModifyScale;
            Controller.InputActionMap.FindAction(scaleAnalogueName).canceled += ModifyScale;
        }


        /// <summary>
        /// Unsubscribe from user input action events.
        /// </summary>
        protected override void UnsubscribeAncillary()
        {
            Controller.InputActionMap.FindAction(scaleAnalogueName).performed -= ModifyScale;
            Controller.InputActionMap.FindAction(scaleAnalogueName).canceled -= ModifyScale;
        }

        void Awake()
        {

            // Set up the line renderer 
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.enabled = false;
            lineRenderer.material = Resources.Load("PulseMaterial") as Material;
            lineRenderer.startWidth = 0.005f;
            lineRenderer.endWidth = 0.005f;
            mixer = new Mixer(6, 50);


            // # Text Panel Setup
            // Initialise the game object responsible for visually reporting the force scale factor.
            scaleTextPanel = new GameObject("ImpulseMonoInputInputHandlerScaleText");

            // Make the panel a child of the input handler, this is done to avoid having a free
            // floating object. The position of the panel will be updated to track the controller
            // when it is made viable (i.e. when users change the scale value).
            scaleTextPanel.transform.SetParent(transform);

            // The panel should use a `RectTransform` rather than a standard transform as it is a
            // 2D window rather than 3D object.
            scaleTextRectTransform = scaleTextPanel.AddComponent<RectTransform>();

            // Add and configure the text mesh pro object
            scaleText = scaleTextPanel.AddComponent<TextMeshPro>();
            scaleText.alignment = TextAlignmentOptions.Center;
            scaleText.fontSize = 0.15f;

            // Ensure that it does not show yet. It should only be shown as and when the force
            // scale factor value changes.
            scaleTextPanel.SetActive(false);

            // Input handlers should always start in the disabled state
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Background the impulse handler.
        /// </summary>
        /// <remarks>
        /// The impulse handler will be disabled when backgrounded as it does not need to run
        /// passively in the background.
        /// </remarks>
        public override void Background() => State = State.Disabled;


        /// <summary>
        /// Determines whether a given input controller is compatible with the impulse handler.
        /// The impulse handler is compatible with all <see cref="BasicInputController"/> based
        /// input controllers.
        /// </summary>
        /// <param name="controller">The input controller to check for compatibility.</param>
        /// <returns>True if the controller is compatible; otherwise, false.</returns>
        public override bool IsCompatibleWithInputController(InputController controller) => controller is BasicInputController;


        /// <summary>
        /// A list of <c>InputAction</c> entities specifying which input actions that the handler
        /// expects to be given sole binding privilege to.
        /// </summary>
        /// <returns>Hash set containing the input actions that this handler is expected to use.</returns>
        public override HashSet<InputAction> RequiredBindings()
        {
            return new HashSet<InputAction>
            {
                Controller.InputActionMap.FindAction(triggerButtonName),
                Controller.InputActionMap.FindAction("Primary Button")
            };
        }

        public override void UnbindController(InputController controller) { }


        // All functionality in the file falling below this point is temporary and wil be removed.

        private Vector3 getVelocity()
        {
            Vector3 newPosition = GetPositions()[closestAtomIndex];

            if (newPosition == oldPosition)
            {
                return oldVelocity;
            }

            Vector3 distance = newPosition - oldPosition;

            float newTime = (float)DateTime.Now.TimeOfDay.TotalSeconds;
            float time = newTime - oldTime;

            Vector3 velocity = distance / time;

            Vector3 smoothed = (velocity + oldVelocity) / 2f;

            oldPosition = newPosition;
            oldTime = newTime;
            oldVelocity = velocity;

            return smoothed;
        }

        private float[] Linspace(float start, float end, int num)
        {
            float[] result = new float[num];
            float step = (end - start) / (num - 1);

            for (int i = 0; i < num; i++)
            {
                result[i] = start + (i * step);
            }

            return result;
        }

        private Vector3[] CubicBezier(
            Vector3 p0, Vector3 p1,
            Vector3 p2, Vector3 p3,
            float[] tValues)
        {

            Vector3[] points = new Vector3[tValues.Length];

            for (int i = 0; i < tValues.Length; i++)
            {
                float t = tValues[i];
                float oneLessT = 1f - t;

                points[i] = oneLessT * oneLessT * oneLessT * p0 +
                            3f * oneLessT * oneLessT * t * p1 +
                            3f * oneLessT * t * t * p2 +
                            t * t * t * p3;
            }

            return points;
        }



    }

    internal class Mixer
    {

        private Vector3[,] history;
        private int count = 0;
        private int n;
        private int m;


        public Mixer(int n, int m)
        {
            history = new Vector3[n, m];
            this.n = n;
            this.m = m;
        }

        public Vector3[] Mix(Vector3[] newArray)
        {
            count += 1;
            RollArray(newArray);
            Vector3[] mixedArray = SumColumns(history);

            float x = MathF.Min(n, count);
            for (int i = 0; i < m; i++)
            {
                mixedArray[i] /= x;
            }

            
            return mixedArray;
        }


        public void RollArray(Vector3[] newRow)
        {
            int rows = history.GetLength(0);
            int cols = history.GetLength(1);

            // Shift rows down
            for (int row = rows - 1; row > 0; row--)
            {
                for (int col = 0; col < cols; col++)
                {
                    history[row, col] = history[row - 1, col];
                }
            }

            // Insert the new row at the top
            for (int col = 0; col < cols; col++)
            {
                history[0, col] = newRow[col];
            }
        }


        public Vector3[] SumColumns(Vector3[,] array)
        {
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);
            Vector3[] columnSums = new Vector3[cols];

            for (int col = 0; col < cols; col++)
            {
                Vector3 sum = new Vector3(0, 0, 0);
                for (int row = 0; row < rows; row++)
                {
                    sum += array[row, col];
                }
                columnSums[col] = sum;
            }

            return columnSums;
        }

        public void Reset()
        {
            history = new Vector3[n, m];
            count = 0;
        }

    }

}