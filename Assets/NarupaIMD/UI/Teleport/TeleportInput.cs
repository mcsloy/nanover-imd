using System;
using Narupa.Frontend.Controllers;
using UnityEngine;
using Valve.VR;

namespace NarupaIMD.UI.Teleport
{
    public class TeleportInput : MonoBehaviour
    {
        [SerializeField]
        private SteamVR_Action_Boolean startTeleportAction;

        [SerializeField]
        private ControllerManager controllers;
        
        [SerializeField]
        private TeleportMode mode;

        private void Awake()
        {
            startTeleportAction.onStateDown += OnStateDown;
            controllers.ModeChanged += ModeChanged;
        }

        private void ModeChanged(ControllerInputMode previous, ControllerInputMode current)
        {
            // If a new mode has replaced the teleporter, turn it off.
            if(previous == mode)
                mode.gameObject.SetActive(false);
        }

        private void OnStateDown(SteamVR_Action_Boolean fromaction, SteamVR_Input_Sources fromsource)
        {
            if (controllers.CurrentInputMode != mode && mode.WouldBeActiveIfEnabled())
            {
                mode.gameObject.SetActive(true);
            }
            else if (controllers.CurrentInputMode == mode)
            {
                mode.gameObject.SetActive(false);
            }
        }
    }
}