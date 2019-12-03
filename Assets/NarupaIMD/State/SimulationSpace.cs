using NarupaIMD.UI;
using UnityEngine;

namespace NarupaIMD.State
{
    /// <summary>
    /// Represents the root space used for simulations.
    /// </summary>
    public class SimulationSpace : MonoBehaviour
    {
        [SerializeField]
        private ConnectedApplicationState application;

        [SerializeField]
        private Transform simulationSpace;

        private void Awake()
        {
            application.ConnectToServer += ApplicationOnConnectToServer;
            application.DisconnectFromServer += ApplicationOnDisconnectFromServer;
        }

        private void ApplicationOnDisconnectFromServer()
        {
            simulationSpace.gameObject.SetActive(false);
        }

        private void ApplicationOnConnectToServer()
        {
            simulationSpace.gameObject.SetActive(true);
        }
    }
}