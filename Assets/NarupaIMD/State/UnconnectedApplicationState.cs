using System;
using NarupaIMD.State;
using NarupaXR;
using UnityEngine;
using Valve.VR;

namespace NarupaIMD.UI
{
    /// <summary>
    /// Represents the state of not being connected to a server.
    /// </summary>
    public class UnconnectedApplicationState : ApplicationState
    {
        private void OnEnable()
        {
            Application.GotoUserInterfaceState(SteamVR_Input_Sources.RightHand)
                       .GotoMainMenu();
        }
    }
}