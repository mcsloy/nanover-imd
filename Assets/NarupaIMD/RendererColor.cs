using System;
using UnityEngine;

namespace NarupaIMD
{
    [SerializeField]
    [RequireComponent(typeof(Renderer))]
    public class RendererColor : MonoBehaviour
    {
        private Renderer renderer;

        private void Awake()
        {
            renderer = GetComponent<Renderer>();
        }

        public Color Color
        {
            get => renderer.material.color;
            set => renderer.material.color = value;
        }
    }
}