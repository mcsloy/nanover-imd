using Essd;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NarupaIMD;
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
        private NarupaXRPrototype application;
        
        [SerializeField]
        private NarupaImdSimulation simulation;

        [SerializeField]
        private GameObject xrSimulatorContainer;

        private bool directConnect;
        private string directConnectAddress = "localhost";
        private string trajectoryPort = "38801";
        private string interactionPort = "38801";
        private string multiplayerPort = "38801";

        private bool discovery;
        private bool debugger;
        private ICollection<ServiceHub> knownServiceHubs = new List<ServiceHub>();

        public float interactionForceMultiplier = 1000;

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(16, 16, 192, 512));
            GUILayout.Box("Narupa iMD");

            GUILayout.Box("Server");
            if (GUILayout.Button("Autoconnect"))
                simulation.AutoConnect();

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
                simulation.Disconnect();
            }

            if (simulation.gameObject.activeSelf)
            {
                GUILayout.Box("User");
                GUILayout.Label(
                    $"Interaction Force: {simulation.ManipulableParticles.ForceScale:0.}x");
                simulation.ManipulableParticles.ForceScale =
                    GUILayout.HorizontalSlider(simulation.ManipulableParticles.ForceScale, 0, 5000);

                GUILayout.Box("Simulation");
                if (GUILayout.Button("Play"))
                    simulation.Trajectory.Play();

                if (GUILayout.Button("Pause"))
                    simulation.Trajectory.Pause();

                if (GUILayout.Button("Step"))
                    simulation.Trajectory.Step();
                
                if (GUILayout.Button("Reset"))
                    simulation.Trajectory.Reset();
                
                if (GUILayout.Button("Reset Box"))
                    simulation.ResetBox();
            }
            
            GUILayout.Box("Debug");
            xrSimulatorContainer.SetActive(GUILayout.Toggle(xrSimulatorContainer.activeSelf, "Simulate Controllers"));

            if (GUILayout.Button("Debugger"))
            {
                debugger = !debugger;
            }
            
            GUILayout.Box("Misc");
            if (GUILayout.Button("Quit"))
                application.Quit();

            GUILayout.EndArea();

            if (directConnect)
                ShowDirectConnectWindow();
            else if (discovery)
                ShowServiceDiscoveryWindow();
            else if (debugger)
                ShowDebugTimings();
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
                application.Connect(
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
                var client = new Client();
                knownServiceHubs = client
                    .SearchForServices(500)
                    .GroupBy(hub => hub.Id)
                    .Select(group => group.First())
                    .ToList();
            }

            if (GUILayout.Button("Cancel"))
                discovery = false;

            if (knownServiceHubs.Count > 0)
            {
                GUILayout.Box("Found Services");

                foreach (var hub in knownServiceHubs)
                {
                    if (GUILayout.Button($"{hub.Name} ({hub.Address})"))
                    {
                        discovery = false;
                        knownServiceHubs = new List<ServiceHub>();
                        application.Connect(hub);
                    }
                }
            }

            GUILayout.EndArea();
        }
        
        private void ShowDebugTimings()
        {
            GUILayout.BeginArea(new Rect(192 + 16 * 2, 10, 192 * 2, 512));
            GUILayout.Box("Debugging");

            GUILayout.Label($"Current Frame Index: {simulation.Trajectory.CurrentFrameIndex}");
            
            GUILayout.Label($"Received Frame Rate: {application.Debugger.FrameReceiving.AverageNumberPerSecond():#.0} /s");
            
            GUILayout.Label($"Multiplayer Delay: {1000f * application.Debugger.MultiplayerPingPong.AverageTimeDifference():#.0} ms");

            GUILayout.Label($"Multiplayer Recieve Rate: {application.Debugger.MultiplayerReceive.AverageNumberPerSecond():#.0} /s");

            GUILayout.Label($"Multiplayer Send Rate: {application.Debugger.MultiplayerSend.AverageNumberPerSecond():#.0} /s");

            var currentLogging = application.Debugger.IsLogging;
            var newLogging = GUILayout.Toggle(currentLogging, "Log to File");
            if (currentLogging != newLogging)
            {
                if(newLogging)
                    application.Debugger.StartLogging();
                else
                    application.Debugger.StopLogging();
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
