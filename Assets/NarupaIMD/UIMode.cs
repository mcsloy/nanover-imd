// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using Narupa.Frontend.UI;
using NarupaIMD;
using UnityEngine;
using Valve.VR;

namespace Narupa.Frontend.Controllers
{
    public class UIMode : ApplicationMode
    {
        [SerializeField]
        private GameObject cursorPrefab;

        [SerializeField]
        private ControllerManager controllerManager;

        [SerializeField]
        private SteamVR_Input_Sources inputSource;

        [SerializeField]
        private CursorProvider cursorProvider;

        [Header("Cursor Switching")]
        [SerializeField]
        private bool cursorSwitchingEnabled = false;

        [SerializeField]
        private SteamVR_Action_Boolean switchCursor;

        private void Awake()
        {
            switchCursor.AddOnStateDownListener(
                (_, __) => SwitchCursor(SteamVR_Input_Sources.RightHand),
                SteamVR_Input_Sources.RightHand);
            switchCursor.AddOnStateDownListener(
                (_, __) => SwitchCursor(SteamVR_Input_Sources.LeftHand),
                SteamVR_Input_Sources.LeftHand);
        }

        private void SwitchCursor(SteamVR_Input_Sources src)
        {
            if (cursorSwitchingEnabled && src != inputSource)
                SetInputSource(src);
        }

        protected override void SetupController(VrController controller)
        {
            base.SetupController(controller);
            if (controller.IsControllerActive && controller.InputSource == inputSource)
                controller.InstantiateCursorGizmo(cursorPrefab);
            else
                controller.InstantiateCursorGizmo(null);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            RefreshController();
        }

        public void SetInputSource(SteamVR_Input_Sources inputSource)
        {
            this.inputSource = inputSource;
            RefreshController();
        }

        private void RefreshController()
        {
            var controller = inputSource == SteamVR_Input_Sources.LeftHand
                                 ? controllerManager.LeftController
                                 : controllerManager.RightController;
            cursorProvider.SetInputSource(inputSource);
            cursorProvider.SetController(controller);
            SetupController(controllerManager.LeftController);
            SetupController(controllerManager.RightController);
        }
    }
}