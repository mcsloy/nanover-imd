using UnityEngine;

namespace Narupa.Visualisation.Components
{
    public interface IRenderNode
    {
        void Render(Camera camera);

        Transform Transform { get; set; }
    }
}