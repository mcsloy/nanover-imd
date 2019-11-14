// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using NarupaIMD;
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
    public class InteractionMode : ApplicationMode
    {
        [SerializeField]
        private GameObject gizmoPrefab;

        protected override void SetupController(VrController controller)
        {
            base.SetupController(controller);
            if (controller.IsControllerActive)
                controller.InstantiateCursorGizmo(gizmoPrefab);
        }
    }
}