using System;
using UnityEngine;
using UnityEngine.Events;

namespace NarupaIMD.UI
{
    [Serializable]
    public class UnityEventColor : UnityEvent<Color>
    {
    }

    /// <summary>
    /// A settable color value that triggers a UI event. Used to allow animations to
    /// set the color without knowing what actual components are involved.
    /// </summary>
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