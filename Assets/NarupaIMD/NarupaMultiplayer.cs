using System;
using Narupa.Core.Async;
using NarupaImd;
using UnityEngine;

namespace NarupaImd
{
    public class NarupaMultiplayer : MonoBehaviour
    {
        [SerializeField]
        private NarupaImdSimulation simulation;

        [SerializeField]
        private NarupaImdAvatarManager avatars;

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