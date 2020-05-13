using System;
using UnityEngine;
using UnityEngine.Events;

namespace NarupaIMD
{
    public class AvatarModel : MonoBehaviour
    {
        [Serializable]
        private class UnityEventColor : UnityEvent<Color>
        {
            
        }
        
        [Serializable]
        private class UnityEventString : UnityEvent<string>
        {
            
        }

        [SerializeField]
        private UnityEventColor colorUpdated;

        [SerializeField]
        private UnityEventString nameUpdated;

        public void SetPlayerColor(Color color)
        {
            colorUpdated?.Invoke(color);
        }
        
        public void SetPlayerName(string name)
        {
            nameUpdated?.Invoke(name);
        }
    }
}