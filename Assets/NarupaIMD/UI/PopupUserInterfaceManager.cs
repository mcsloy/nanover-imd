using Narupa.Frontend.Controllers;
using Narupa.Frontend.UI;
using NarupaIMD.UI;
using UnityEngine;
using UnityEngine.Assertions;
using Valve.VR;

namespace NarupaXR.UI
{
    /// <summary>
    /// A <see cref="UserInterfaceManager"/> that only shows the UI while a cursor is held down.
    /// </summary>
    public class PopupUserInterfaceManager : UserInterfaceManager
    {
        [SerializeField]
        private GameObject menuPrefab;

        [SerializeField]
        private SteamVR_Action_Boolean openMenuAction;

        [SerializeField]
        private bool clickOnMenuClosed = true;

        [SerializeField]
        private ControllerManager controllers;
        
        [SerializeField]
        private UiInputMode mode;

        private void Start()
        {
            Assert.IsNotNull(menuPrefab, "Missing menu prefab");
            Assert.IsNotNull(openMenuAction, "Missing action to trigger menu");
            openMenuAction.onStateDown += VisualiserMenuActionOnStateDown;
            openMenuAction.onStateUp += VisualiserMenuActionOnStateUp;
        }

        private void VisualiserMenuActionOnStateUp(SteamVR_Action_Boolean fromaction,
                                                   SteamVR_Input_Sources fromsource)
        {
            CloseMenu();
        }

        private void VisualiserMenuActionOnStateDown(SteamVR_Action_Boolean fromaction,
                                                     SteamVR_Input_Sources fromsource)
        {
            ShowMenu();
        }

        private GameObject menu;

        private void ShowMenu()
        {
            if (!controllers.WouldBecomeCurrentMode(mode))
                return;
            
            GotoScene(menuPrefab);
            
            SceneUI.transform.position = SceneUI.GetComponent<PhysicalCanvasInput>()
                                                .Controller
                                                .HeadPose
                                                .Pose
                                                .Value
                                                .Position;
            SceneUI.transform.rotation =
                Quaternion.LookRotation(SceneUI.transform.position - Camera.main.transform.position,
                                        Vector3.up);
        }

        private void CloseMenu()
        {
            if (clickOnMenuClosed)
                WorldSpaceCursorInput.TriggerClick();
            CloseScene();
        }
    }
}