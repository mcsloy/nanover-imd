using Essd;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NanoverImd;
using UnityEngine;
using NanoverImd.InputHandlers;

namespace NanoverImd
{
    /// <summary>
    /// Unity Immediate Mode GUI for connecting, configuring, etc from the
    /// desktop (without needing VR).
    /// </summary>
    public class DesktopDebugUI : MonoBehaviour
    {
        [SerializeField]
        private NanoverImdApplication application;
        
        [SerializeField]
        private NanoverImdSimulation simulation;

        //[SerializeField]
        //private GameObject xrSimulatorContainer;

        private bool directConnect;
        private string directConnectAddress = "localhost";
        private string trajectoryPort = "38801";
        private string interactionPort = "38801";
        private string multiplayerPort = "38801";

        private bool discovery;
        private ICollection<ServiceHub> knownServiceHubs = new List<ServiceHub>();

        public float interactionForceMultiplier = 1000;

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(16, 16, 192, 1024));
            GUILayout.Box("Nanover iMD");

            GUILayout.Box("Server");
            if (GUILayout.Button("Autoconnect"))
                _ = simulation.AutoConnect();

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
                    $"Interaction Force: {ImpulseMonoInputInputHandler.ForceScaleFactor:0.}x");
                ImpulseMonoInputInputHandler.ForceScaleFactor =
                    GUILayout.HorizontalSlider(
                        ImpulseMonoInputInputHandler.ForceScaleFactor,
                        ImpulseMonoInputInputHandler.forceScaleFactorLowerBounds,
                        ImpulseMonoInputInputHandler.forceScaleFactorUpperBounds);

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

                GUILayout.Box("Colocation");
                application.ColocateLighthouses = GUILayout.Toggle(application.ColocateLighthouses, "Colocated Lighthouses");

                if (!application.ColocateLighthouses)
                {
                    if (GUILayout.Button("Reset Radial Orientation"))
                        simulation.RunRadialOrientation();

                    GUILayout.Label("Radial Displacement");
                    application.PlayAreaRadialDisplacementFactor = GUILayout.HorizontalSlider(application.PlayAreaRadialDisplacementFactor, 0f, 1f);
                    GUILayout.Label("Rotation Correction");
                    application.PlayAreaRotationCorrection = GUILayout.HorizontalSlider(application.PlayAreaRotationCorrection, -180f, 180f);
                }
            }
            
            //GUILayout.Box("Debug");
            //GUILayout.Label("DISABLED");
            //xrSimulatorContainer.SetActive(GUILayout.Toggle(xrSimulatorContainer.activeSelf, "Simulate Controllers"));

            GUILayout.Box("Misc");
            if (GUILayout.Button("Quit"))
                application.Quit();

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

            GUILayout.Label("Address");
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
        
        private int? ParseInt(string text)
        {
            return int.TryParse(text, out int number)
                 ? number
                 : (int?) null;
        }
    }
}
