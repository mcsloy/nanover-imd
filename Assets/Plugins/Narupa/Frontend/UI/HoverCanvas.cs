// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using Narupa.Frontend.Controllers;
using Narupa.Frontend.Input;
using Narupa.Frontend.XR;
using UnityEngine;
using UnityEngine.Assertions;
using Valve.VR;

namespace Narupa.Frontend.UI
{
    /// <summary>
    /// Component required to register a Unity canvas such that the given controller
    /// can hover over it.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class HoverCanvas : MonoBehaviour
    {
        /// <summary>
        /// The controller that can interact with this canvas.
        /// </summary>
        /// <remarks>
        /// Currently, only one controller can interact with a given canvas.
        /// </remarks>
        [SerializeField]
        private VrController controller;

        private Canvas canvas;

        private void Awake()
        {
            Assert.IsNotNull(controller, $"{nameof(NarupaCanvas)} must have a pointer to the {nameof(VrController)} that will control it.");
            canvas = GetComponent<Canvas>();
        }

        private void OnEnable()
        {
            RegisterCanvas();
        }

        /// <summary>
        /// Register the canvas with the cursor input system.
        /// </summary>
        protected virtual void RegisterCanvas()
        {
            WorldSpaceCursorInput.SetCanvasAndCursor(canvas,
                                                     controller.HeadPose,
                                                     null);
        }

        private void OnDisable()
        {
            WorldSpaceCursorInput.ReleaseCanvas(canvas);
        }

        public void SetCamera(Camera camera)
        {
            if(canvas == null)
                canvas = GetComponent<Canvas>();
            canvas.worldCamera = camera;
        }

        public void SetController(VrController vrController)
        {
            controller = vrController;
        }
    }
}