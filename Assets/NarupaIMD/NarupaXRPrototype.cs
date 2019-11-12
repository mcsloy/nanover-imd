// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using UnityEngine;
using Narupa.Frontend.Manipulation;
using Narupa.Visualisation;
using UnityEngine.UI;
using NarupaXR.Interaction;
using System.Collections;
using Narupa.Frame;
using Narupa.Frontend.XR;
using UnityEditor;
using UnityEngine.Serialization;
using Text = TMPro.TextMeshProUGUI;

namespace NarupaXR
{
    /// <summary>
    /// The entry point to the application, and central location for accessing
    /// shared resources.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NarupaXRPrototype : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private Transform simulationSpaceTransform;

        [SerializeField]
        private GameObject visualiser;

        [SerializeField]
        private Slider forceScaleSlider;

        [SerializeField]
        private Text forceScaleValueLabel;

        [SerializeField]
        private Text debugText;

        [FormerlySerializedAs("xrInteraction")]
        [SerializeField]
        private XRBoxInteractionManager xrBoxInteraction;

        [SerializeField]
        private NarupaXRAvatarManager avatars;
#pragma warning restore 0649

        public PhysicallyCalibratedSpace CalibratedSpace { get; } = new PhysicallyCalibratedSpace();

        /// <summary>
        /// The route through which simulation space can be manipulated with
        /// gestures to perform translation, rotation, and scaling.
        /// </summary>
        public ManipulableScenePose ManipulableSimulationSpace { get; private set; }
        
        /// <summary>
        /// The route through which simulated particles can be manipulated with
        /// grabs.
        /// </summary>
        public ManipulableParticles ManipulableParticles { get; private set; }

        public SynchronisedFrameSource FrameSynchronizer { get; private set; }

        public NarupaXRSessionManager Sessions { get; } = new NarupaXRSessionManager();

        /// <summary>
        /// Connect to remote trajectory and IMD services.
        /// </summary>
        public void Connect(string address, int? trajectoryPort, int? imdPort, int? multiplayerPort)
            => Sessions.Connect(address, trajectoryPort, imdPort, multiplayerPort);

        /// <summary>
        /// Called from UI to quit the application.
        /// </summary>
        public void Quit() => Application.Quit();

        private bool HaveBoxLock => !Sessions.Multiplayer.IsOpen 
                                 && Sessions.Multiplayer.HasSimulationPoseLock;

        private void Awake()
        {
            ManipulableSimulationSpace = new ManipulableScenePose(simulationSpaceTransform,
                                                                  Sessions.Multiplayer,
                                                                  this);
            ManipulableParticles = new ManipulableParticles(simulationSpaceTransform,
                                                            Sessions.Trajectory,
                                                            Sessions.Imd);
            
            SetupVisualisation();

            StartCoroutine(AttemptLockTest());
        }

        private void Update()
        {
            CalibratedSpace.CalibrateFromLighthouses();

            ManipulableParticles.ForceScale = forceScaleSlider.value;
            forceScaleValueLabel.text = $"{forceScaleSlider.value}x";
            debugText.text = $"Frame Index: {Sessions.Trajectory.CurrentFrameIndex}";

            if (Input.GetKeyDown(KeyCode.Return))
                AttemptLock();
        }

        private void AttemptLock()
        {
            if (Sessions.Multiplayer.HasPlayer && !Sessions.Multiplayer.HasSimulationPoseLock)
                Sessions.Multiplayer.LockSimulationPose()
                    .ContinueWith(task => Debug.Log($"Got Lock? {task.Result}"));
        }

        private IEnumerator AttemptLockTest()
        {
            while (!Sessions.Multiplayer.HasPlayer)
                yield return new WaitForSeconds(1f / 10f);

            AttemptLock();
        }

        private async void OnDestroy()
        {
            await Sessions.CloseAsync();
        }

        private void SetupVisualisation()
        {
            FrameSynchronizer = gameObject.AddComponent<SynchronisedFrameSource>();
            FrameSynchronizer.FrameSource = Sessions.Trajectory;
            visualiser.GetComponent<IFrameConsumer>().FrameSource = FrameSynchronizer;
        }

        /// <summary>
        /// Play the current server.
        /// </summary>
        [MenuItem("Narupa/Play")]
        public static void PlayServer()
        {
            FindObjectOfType<NarupaXRPrototype>().Sessions.Trajectory?.Play();
        }
        
        /// <summary>
        /// Pause the current server.
        /// </summary>
        [MenuItem("Narupa/Pause")]
        public static void PauseServer()
        {
            FindObjectOfType<NarupaXRPrototype>().Sessions.Trajectory?.Pause();
        }
        
        /// <summary>
        /// Reset the current server.
        /// </summary>
        [MenuItem("Narupa/Reset")]
        public static void ResetServer()
        {
            FindObjectOfType<NarupaXRPrototype>().Sessions.Trajectory?.Reset();
        }
        
        /// <summary>
        /// Step the current server.
        /// </summary>
        [MenuItem("Narupa/Step")]
        public static void StepServer()
        {
            FindObjectOfType<NarupaXRPrototype>().Sessions.Trajectory?.Step();
        }
    }
}