using Narupa.Frontend.UI;
using UnityEngine;
using UnityEngine.Assertions;
using Valve.VR;

namespace NarupaXR.UI
{
    public class PopupRadialMenu : MonoBehaviour
    {
        [SerializeField]
        private GameObject radialMenuPrefab;

        [SerializeField]
        private SteamVR_Action_Boolean radialMenuAction;

        [SerializeField]
        private HoverCanvas hover;

        private void Start()
        {
            Assert.IsNotNull(radialMenuPrefab, "Missing radial menu prefab");
            Assert.IsNotNull(radialMenuAction, "Missing action to trigger radial menu");
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

            transform.position = hover.Controller.HeadPose.Pose.Value.Position;
            transform.rotation =
                Quaternion.LookRotation(menu.transform.position - Camera.main.transform.position,
                                        Vector3.up);

        }

        private void CloseMenu()
        {
            WorldSpaceCursorInput.TriggerClick();
            Destroy(menu);
        }
    }
}