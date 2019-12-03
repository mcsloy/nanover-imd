using Narupa.Frontend.Controllers;
using Narupa.Frontend.UI;
using NarupaIMD.Selection;
using UnityEngine;
using UnityEngine.Assertions;
using Valve.VR;

namespace NarupaXR.UI
{
    public class VisualiserRadialMenu : MonoBehaviour
    {
        [SerializeField]
        private GameObject radialMenuPrefab;

        [SerializeField]
        private SteamVR_Action_Boolean visualiserMenuAction;

        [SerializeField]
        private CursorProvider cursorProvider;

        [SerializeField]
        private NarupaXRPrototype prototype;

        [SerializeField]
        private SteamVR_Input_Sources inputSource;

        private void Start()
        {
            Assert.IsNotNull(radialMenuPrefab, "Missing radial menu prefab");
            Assert.IsNotNull(visualiserMenuAction,
                             "Missing action to trigger visualiser radial menu");
            Assert.IsNotNull(cursorProvider, "Missing VR controller");
            visualiserMenuAction.onStateDown += VisualiserMenuActionOnStateDown;
            visualiserMenuAction.onStateUp += VisualiserMenuActionOnStateUp;
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
            menu = Instantiate(radialMenuPrefab);
            menu.GetComponent<HoverCanvas>().SetCamera(Camera.main);
            menu.GetComponent<HoverCanvas>().SetCursor(cursorProvider);
            menu.SetActive(true);
            
            var dynamic = menu.GetComponentInChildren<DynamicMenu>();
            
            foreach (var prefab in VisualiserFactory.GetRenderSubgraphs())
                dynamic.AddItem(prefab.name, null,
                                () => { });
                                
            menu.transform.position = cursorProvider.Pose.Value.Position;
            menu.transform.rotation =
                Quaternion.LookRotation(menu.transform.position - Camera.main.transform.position,
                                        Vector3.up);

            //prototype.GotoUiMode(inputSource);
        }

        private void CloseMenu()
        {
            WorldSpaceCursorInput.TriggerClick();
            Destroy(menu);
            //prototype.GotoInteractionMode();
        }
    }
}