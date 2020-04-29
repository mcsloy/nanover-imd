using Narupa.Core.Async;
using UnityEngine;

namespace NarupaIMD
{
    public class NarupaMultiplayer : MonoBehaviour
    {
        [SerializeField]
        private NarupaImdSimulation simulation;

        [SerializeField]
        private NarupaAvatarManager avatars;

        private void OnEnable()
        {
            avatars.enabled = false;
            simulation.Multiplayer.MultiplayerJoined += OnMultiplayerJoined;
            simulation.Multiplayer.JoinMultiplayer("Narupa iMD")
                      .AwaitInBackground();
        }

        private void OnDisable()
        {
            avatars.enabled = false;
            simulation.Multiplayer.MultiplayerJoined -= OnMultiplayerJoined;
        }

        private void OnMultiplayerJoined()
        {
            avatars.enabled = true;
        }
    }
}