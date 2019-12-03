using Narupa.Frontend.Controllers;
using NarupaIMD.UI;
using UnityEngine;
using Valve.VR;

namespace NarupaIMD.State
{
    public class ApplicationStateManager : MonoBehaviour
    {
        [SerializeField]
        private ConnectedApplicationState connectedApplicationState;

        [SerializeField]
        private UnconnectedApplicationState unconnectedApplicationState;

        [SerializeField]
        private UserInterfaceState userInterfaceState;

        [SerializeField]
        private UserInteractionState userInteractionState;

        public bool IsConnected => connectedApplicationState.gameObject.activeInHierarchy;

        public void RefreshConnectedState()
        {
            connectedApplicationState.gameObject.SetActive(false);
            connectedApplicationState.gameObject.SetActive(true);
        }
        
        public void GotoConnectedState()
        {
            unconnectedApplicationState.gameObject.SetActive(false);
            connectedApplicationState.gameObject.SetActive(true);
        }

        public void GotoDisconnectedState()
        {
            connectedApplicationState.gameObject.SetActive(false);
            unconnectedApplicationState.gameObject.SetActive(true);
        }

        public void GotoUserInteractionState()
        {
            userInterfaceState.gameObject.SetActive(false);
            userInteractionState.gameObject.SetActive(true);
        }

        public UserInterfaceState GotoUserInterfaceState(SteamVR_Input_Sources defaultInput)
        {
            userInteractionState.gameObject.SetActive(false);
            userInterfaceState.gameObject.SetActive(true);
            userInterfaceState.SetInputSource(defaultInput);
            return userInterfaceState;
        }
    }
}