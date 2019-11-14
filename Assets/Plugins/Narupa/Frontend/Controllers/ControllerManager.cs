// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using UnityEngine;
using Valve.VR;

namespace Narupa.Frontend.Controllers
{
    /// <summary>
    /// Manager class for accessing the left and right controller.
    /// </summary>
    public class ControllerManager : MonoBehaviour
    {
        [SerializeField]
        private VrController leftController;

        [SerializeField]
        private VrController rightController;

        [SerializeField]
        private SteamVR_ActionSet controllerActionSet;

        /// <summary>
        /// The left <see cref="VrController" />.
        /// </summary>
        public VrController LeftController => leftController;

        /// <summary>
        /// The right <see cref="VrController" />.
        /// </summary>
        public VrController RightController => rightController;

        private void Awake()
        {
            controllerActionSet.Initialize();
            controllerActionSet.Activate();
        }
    }
}