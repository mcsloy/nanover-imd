// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using Narupa.Frontend.Input;
using UnityEngine;

namespace Narupa.Frontend.Controllers
{
    /// <summary>
    /// Component that indicates the root game object responsible for representing a
    /// controller that is in the scene.
    /// </summary>
    /// <remarks>
    /// This class exposes various poses (such as cursor and grip points) in a way that
    /// the posed object always exists, and is automatically linked to the active
    /// controller if there is one.
    /// </remarks>
    public class VrController : MonoBehaviour
    {
        /// <summary>
        /// Indicate the controller has been reset (connected or disconnected).
        /// </summary>
        /// <param name="controller"></param>
        public void ResetController(VrControllerPrefab controller)
        {
            IsControllerActive = controller != null;
            
            SetupPose(ref cursor, controller?.Cursor, cursorPose, OnCursorPoseChanged);
            SetupPose(ref grip, controller?.Grip, gripPose, OnGripPoseChanged);
            SetupPose(ref head, controller?.Head, headPose, OnHeadPoseChanged);

            ControllerReset?.Invoke();
        }

        private void SetupPose(ref ControllerPivot pivot,
                               ControllerPivot newPivot,
                               DirectPosedObject posedObject,
                               Action onPoseChanged)
        {
            if (IsControllerActive)
            {
                if (pivot != null)
                    pivot.PoseChanged -= onPoseChanged;
                pivot = newPivot;
                if (pivot != null)
                    pivot.PoseChanged += onPoseChanged;
            }
            else
            {
                if (pivot != null)
                    pivot.PoseChanged -= onPoseChanged;
                pivot = null;
                posedObject.SetPose(null);
            }
        }

        private void OnCursorPoseChanged()
        {
            cursorPose.SetPose(cursor.Pose);
        }

        private void OnGripPoseChanged()
        {
            gripPose.SetPose(grip.Pose);
        }
        
        private void OnHeadPoseChanged()
        {
            headPose.SetPose(cursor.Pose);
        }

        private ControllerPivot cursor;
        private ControllerPivot grip;
        private ControllerPivot head;

        private DirectPosedObject cursorPose = new DirectPosedObject();
        private DirectPosedObject gripPose = new DirectPosedObject();
        private DirectPosedObject headPose = new DirectPosedObject();

        /// <summary>
        /// The pose marking the location of a gripped hand.
        /// </summary>
        public IPosedObject GripPose => gripPose;
        
        /// <summary>
        /// The pose marking the location where the bulk of the controller is
        /// </summary>
        public IPosedObject HeadPose => headPose;

        /// <summary>
        /// The cursor point where tools should be centered.
        /// </summary>
        public IPosedObject CursorPose => cursorPose;

        /// <summary>
        /// Is the controller currently active?
        /// </summary>
        public bool IsControllerActive { get; private set; } = false;

        public event Action ControllerReset;

        private GameObject cursorGizmo = null;

        public void InstantiateCursorGizmo(GameObject interactionGizmo)
        {
            if(cursorGizmo != null)
                Destroy(cursorGizmo);
            if (cursor != null)
            {
                cursorGizmo = Instantiate(interactionGizmo, cursor.transform);
            }
        }
    }
}