using Narupa.Core.Async;
using Narupa.Session;
using UnityEngine;

namespace NarupaIMD
{
    public class NarupaMultiplayer : MonoBehaviour
    {
        [SerializeField]
        private NarupaAvatarManager avatars;

        private void OnEnable()
        {
            avatars.enabled = true;
        }

        private void OnDisable()
        {
            avatars.enabled = false;
        }
    }
}