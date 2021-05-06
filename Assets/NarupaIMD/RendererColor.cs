using UnityEngine;

namespace NarupaImd
{
    [SerializeField]
    [RequireComponent(typeof(Renderer))]
    public class RendererColor : MonoBehaviour
    {
        private Material material;

        private void Awake()
        {
            material = GetComponent<Renderer>().material;
        }

        public Color Color
        {
            get => material.color;
            set => material.color = value;
        }
    }
}