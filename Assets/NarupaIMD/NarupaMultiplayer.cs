using System;
using Narupa.Core.Async;
using NarupaXR;
using UnityEngine;

namespace NarupaIMD
{
    public class NarupaMultiplayer : MonoBehaviour
    {
        [SerializeField]
        private NarupaImdSimulation simulation;

        [SerializeField]
        private NarupaXRAvatarManager avatars;

        private void OnEnable()
        {
            avatars.enabled = false;
            simulation.Multiplayer.MultiplayerJoined += OnMultiplayerJoined;
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