using System;
using Narupa.Core.Async;
using Narupa.Session;
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

        public MultiplayerSession Session => simulation.Multiplayer;

        private void OnEnable()
        {
            avatars.enabled = true;
            Session.JoinMultiplayer("NarupaXR")
                   .AwaitInBackground();
        }

        private void OnDisable()
        {
            avatars.enabled = false;
        }
    }
}