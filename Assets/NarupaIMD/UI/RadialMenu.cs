using Narupa.Frontend.Controllers;
using Narupa.Frontend.UI;
using NarupaIMD.Selection;
using NarupaIMD.State;
using UnityEngine;
using UnityEngine.Assertions;
using Valve.VR;

namespace NarupaXR.UI
{
    public class RadialMenu : MonoBehaviour
    {
        [SerializeField]
        private GameObject radialMenuPrefab;

        [SerializeField]
        private SteamVR_Action_Boolean radialMenuAction;

        [SerializeField]
        private CursorProvider cursorProvider;

        [SerializeField]
        private ApplicationStateManager application;

        private void Start()
        {
            Assert.IsNotNull(radialMenuPrefab, "Missing radial menu prefab");
            Assert.IsNotNull(radialMenuAction,
                             "Missing action to trigger radial menu");
            Assert.IsNotNull(cursorProvider, "Missing VR controller");
            radialMenuAction.onStateDown += VisualiserMenuActionOnStateDown;
            radialMenuAction.onStateUp += VisualiserMenuActionOnStateUp;
        }

        private void VisualiserMenuActionOnStateUp(SteamVR_Action_Boolean fromaction,
                                                   SteamVR_Input_Sources fromsource)
        {
            CloseMenu();
        }

        private void VisualiserMenuActionOnStateDown(SteamVR_Action_Boolean fromaction,
                                                     SteamVR_Input_Sources fromsource)
        {
            ShowMenu(SteamVR_Input_Sources.LeftHand);
        }

        private GameObject menu;

        private void ShowMenu(SteamVR_Input_Sources source)
        {
            menu = Instantiate(radialMenuPrefab);
            menu.GetComponent<HoverCanvas>().SetCamera(Camera.main);
            menu.GetComponent<HoverCanvas>().SetCursor(cursorProvider);
            menu.SetActive(true);
            
            application.GotoUserInterfaceState(source);
            
            menu.transform.position = cursorProvider.Pose.Value.Position;
            menu.transform.rotation =
                Quaternion.LookRotation(menu.transform.position - Camera.main.transform.position,
                                        Vector3.up);

        }

        private void CloseMenu()
        {
            WorldSpaceCursorInput.TriggerClick();
            Destroy(menu);
            // Todo: reroute cleaner
            if(application.IsConnected)
                application.GotoUserInteractionState();
        }
    }
}