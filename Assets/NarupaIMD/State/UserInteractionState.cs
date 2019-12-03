// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using NarupaIMD;
using NarupaIMD.UI;
using NarupaXR;
using UnityEngine;
using Valve.VR;

namespace Narupa.Frontend.Controllers
{
    /// <summary>
    /// Define the mode of the controller as 'interaction', setting up the correct
    /// gizmo.
    /// </summary>
    /// <remarks>
    /// This is a temporary object until a more sophisticated multi-mode tool setup is
    /// completed.
    /// </remarks>
    public class UserInteractionState : UserState
    {
        [SerializeField]
        private GameObject gizmoPrefab;

        [SerializeField]
        private ConnectedApplicationState applicationState;

        protected override void SetupController(VrController controller)
        {
            base.SetupController(controller);
            if (controller.IsControllerActive)
                controller.InstantiateCursorGizmo(gizmoPrefab);
        }

        [SerializeField]
        private SteamVR_Action_Boolean playAction;

        [SerializeField]
        private SteamVR_Action_Boolean pauseAction;

        protected override void OnEnable()
        {
            base.OnEnable();
            playAction.onStateDown += PlayActionOnStateDown;
            pauseAction.onStateDown += PauseActionOnStateDown;
        }

        private void PauseActionOnStateDown(SteamVR_Action_Boolean fromaction, SteamVR_Input_Sources fromsource)
        {
            applicationState.Sessions.Trajectory.Pause();
        }

        private void PlayActionOnStateDown(SteamVR_Action_Boolean fromaction, SteamVR_Input_Sources fromsource)
        {
            applicationState.Sessions.Trajectory.Play();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            playAction.onStateDown -= PlayActionOnStateDown;
            pauseAction.onStateDown -= PauseActionOnStateDown;
        }
    }
}