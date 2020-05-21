using Narupa.Core.Async;
using Narupa.Session;
using UnityEngine;

namespace NarupaIMD
{
    public class NarupaMultiplayer : MonoBehaviour
    {
        [SerializeField]
        private NarupaImdSimulation simulation;

        [SerializeField]
        private NarupaAvatarManager avatars;

        public MultiplayerSession Session => simulation.Multiplayer;

        private void OnEnable()
        {
            avatars.enabled = true;
            Session.JoinMultiplayer("Narupa iMD")
                   .AwaitInBackground();
        }

        private void OnDisable()
        {
            avatars.enabled = false;
        }
    }
}