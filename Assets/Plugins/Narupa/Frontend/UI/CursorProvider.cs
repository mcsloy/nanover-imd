using System;
using Narupa.Core.Math;
using Narupa.Frontend.Controllers;
using Narupa.Frontend.Input;
using Narupa.Frontend.XR;
using UnityEngine;
using Valve.VR;

namespace Narupa.Frontend.UI
{
    public class CursorProvider : MonoBehaviour, ICursorProvider
    {
        [SerializeField]
        private VrController controller;

        [SerializeField]
        private SteamVR_Action_Boolean clickAction;

        [SerializeField]
        private SteamVR_Input_Sources inputSource;

        private IButton button;

        public void Awake()
        {
            SetupButton(inputSource);
            if (controller != null)
                SetController(controller);
        }

        public void SetController(VrController controller)
        {
            if (controller != null)
            {
                controller.CursorPose.PoseChanged -= CursorPoseOnPoseChanged;
            }

            this.controller = controller;
            controller.CursorPose.PoseChanged += CursorPoseOnPoseChanged;
        }

        public void SetInputSource(SteamVR_Input_Sources inputSource)
        {
            if (this.inputSource == inputSource)
                return;
            this.inputSource = inputSource;
            SetupButton(this.inputSource);
        }

        private void SetupButton(SteamVR_Input_Sources source)
        {
            if (button != null)
            {
                button.Pressed -= ButtonOnPressed;
                button.Released -= ButtonOnReleased;
            }

            button = clickAction.WrapAsButton(inputSource);
            button.Pressed += ButtonOnPressed;
            button.Released += ButtonOnReleased;
        }

        private void ButtonOnReleased()
        {
            Released?.Invoke();
        }

        private void ButtonOnPressed()
        {
            Pressed?.Invoke();
        }

        private void CursorPoseOnPoseChanged()
        {
            PoseChanged?.Invoke();
        }

        public Transformation? Pose => controller.CursorPose.Pose;

        public event Action PoseChanged;

        public bool IsPressed => button.IsPressed;

        public event Action Pressed;

        public event Action Released;

        public bool IsCursorActive => controller != null;
    }
}