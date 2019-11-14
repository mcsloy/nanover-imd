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
    /// can interact with it.
    /// </summary>
    /// <remarks>
    /// All canvases that would like to be interacted with by physical controllers
    /// should have a script that derives from <see cref="NarupaCanvas" />. This should
    /// provide the controller and the action which is counted as a 'click'. The
    /// <see cref="RegisterCanvas" /> method can be overriden to provide a custom
    /// <see cref="IPosedObject" /> and <see cref="IButton" /> to provide the cursor
    /// location and click button.
    /// </remarks>
    [RequireComponent(typeof(Canvas))]
    public class NarupaCanvas : MonoBehaviour
    {
        /// <summary>
        /// The current cursor.
        /// </summary>
        [SerializeField]
        private CursorProvider cursor;

        private Canvas canvas;

        private void Awake()
        {
            Assert.IsNotNull(cursor, $"{nameof(NarupaCanvas)} must have a pointer to the {nameof(CursorProvider)} that will control it.");
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
                                                     cursor);
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

        public void SetCursor(CursorProvider cursor)
        {
            this.cursor = cursor;
        }
    }
}