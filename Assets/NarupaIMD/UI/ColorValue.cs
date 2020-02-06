using System;
using UnityEngine;
using UnityEngine.Events;

namespace NarupaIMD.UI
{
    [Serializable]
    public class UnityEventColor : UnityEvent<Color>
    {
        
    }
    
    public class ColorValue : MonoBehaviour
    {
        [SerializeField]
        private Color color;
        
        [SerializeField]
        public UnityEventColor colorChanged;

        private void OnValidate()
        {
            colorChanged?.Invoke(color);
        }

        private void OnDidApplyAnimationProperties()
        {
            colorChanged?.Invoke(color);
        }
    }
}