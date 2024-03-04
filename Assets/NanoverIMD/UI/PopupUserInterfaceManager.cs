using Nanover.Frontend.Controllers;
using Nanover.Frontend.UI;
using Nanover.Frontend.XR;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR;

namespace NanoverImd.UI
{
    /// <summary>
    /// A <see cref="UserInterfaceManager"/> that only shows the UI while a cursor is held down.
    /// </summary>
    public class PopupUserInterfaceManager : UserInterfaceManager
    {
        [SerializeField]
        private GameObject menuPrefab;

        [SerializeField]
        private bool clickOnMenuClosed = true;

        [SerializeField]
        private ControllerManager controllers;
        
        [SerializeField]
        private UiInputMode mode;

        [SerializeField]
        private InputDeviceCharacteristics characteristics;

        private void Start()
        {
            Assert.IsNotNull(menuPrefab, "Missing menu prefab");

            var openMenu = characteristics.WrapUsageAsButton(CommonUsages.primaryButton);

            openMenu.Pressed += ShowMenu;
            openMenu.Released += CloseMenu;
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