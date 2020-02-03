using Essd;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NarupaXR
{
    /// <summary>
    /// Unity Immediate Mode GUI for connecting, configuring, etc from the
    /// desktop (without needing VR).
    /// </summary>
    public class DesktopDebugUI : MonoBehaviour
    {
        [SerializeField]
        private NarupaXRPrototype narupa;

        [SerializeField]
        private GameObject xrSimulatorContainer;

        private bool directConnect;
        private string directConnectAddress = "localhost";
        private string trajectoryPort = "38801";
        private string interactionPort = "38801";
        private string multiplayerPort = "38801";

        private bool discovery;
        private Dictionary<string, ServiceHub> knownServiceHubs = new Dictionary<string, ServiceHub>();

        public float interactionForceMultiplier = 1000;

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(16, 16, 192, 512));
            GUILayout.Box("Narupa iMD");

            GUILayout.Box("Server");
            if (GUILayout.Button("Autoconnect"))
                narupa.AutoConnect();

            if (GUILayout.Button("Direct Connect"))
            {
                directConnect = !directConnect;
            }

            if (GUILayout.Button("Discover Services"))
            {
                discovery = !discovery;
            }

            if (GUILayout.Button("Disconnect"))
            {
                narupa.Disconnect();
            }

            GUILayout.Box("User");
            GUILayout.Label($"Interaction Force: {narupa.ManipulableParticles.ForceScale:0.}x");
            narupa.ManipulableParticles.ForceScale = GUILayout.HorizontalSlider(narupa.ManipulableParticles.ForceScale, 0, 5000);

            GUILayout.Box("Simulation");
            if (GUILayout.Button("Play"))
                narupa.Sessions.Trajectory.Play();

            if (GUILayout.Button("Pause"))
                narupa.Sessions.Trajectory.Pause();

            if (GUILayout.Button("Step"))
                narupa.Sessions.Trajectory.Step();

            if (GUILayout.Button("Reset"))
                narupa.Sessions.Trajectory.Reset();

            GUILayout.Box("Debug");
            xrSimulatorContainer.SetActive(GUILayout.Toggle(xrSimulatorContainer.activeSelf, "Simulate Controllers"));

            GUILayout.Box("Misc");
            if (GUILayout.Button("Quit"))
                narupa.Quit();

            GUILayout.EndArea();

            if (directConnect)
                ShowDirectConnectWindow();

            if (discovery)
                ShowServiceDiscoveryWindow();
        }

        private void ShowDirectConnectWindow()
        {
            GUILayout.BeginArea(new Rect(192 + 16 * 2, 10, 192, 512));
            GUILayout.Box("Direct Connect");

            GUILayout.Label("Addresss");
            directConnectAddress = GUILayout.TextField(directConnectAddress);
            GUILayout.Label("Trajectory Port");
            trajectoryPort = GUILayout.TextField(trajectoryPort);
            GUILayout.Label("IMD Port");
            interactionPort = GUILayout.TextField(interactionPort);
            GUILayout.Label("Multiplayer Port");
            multiplayerPort = GUILayout.TextField(multiplayerPort);

            if (GUILayout.Button("Connect"))
            {
                directConnect = false;
                narupa.Connect(
                    directConnectAddress,
                    ParseInt(trajectoryPort),
                    ParseInt(interactionPort),
                    ParseInt(multiplayerPort));
            }

            if (GUILayout.Button("Cancel"))
                directConnect = false;

            GUILayout.EndArea();
        }

        private void ShowServiceDiscoveryWindow()
        {
            GUILayout.BeginArea(new Rect(192 * 2 + 16 * 3, 10, 192, 512));
            GUILayout.Box("Discover Servers");

            if (GUILayout.Button("Search"))
            {
                knownServiceHubs = (new Client()).SearchForServices(500).ToDictionary(hub => hub.Id, hub => hub);
            }

            if (GUILayout.Button("Cancel"))
                discovery = false;

            if (knownServiceHubs.Count > 0)
            {
                GUILayout.Box("Found Services");

                foreach (var hub in knownServiceHubs.Values)
                {
                    if (GUILayout.Button($"{hub.Name} ({hub.Address})"))
                    {
                        discovery = false;
                        knownServiceHubs = new Dictionary<string, ServiceHub>();
                        narupa.Connect(hub);
                    }
                }
            }

            GUILayout.EndArea();
        }

        private int? ParseInt(string text)
        {
            return int.TryParse(text, out int number)
                 ? number
                 : (int?) null;
        }
    }
}
