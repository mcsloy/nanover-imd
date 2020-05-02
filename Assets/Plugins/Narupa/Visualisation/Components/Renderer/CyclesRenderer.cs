using Narupa.Visualisation.Node.Renderer;
using UnityEngine;

namespace Narupa.Visualisation.Components.Renderer
{
    public class CyclesRenderer : VisualisationComponentRenderer<CyclesRendererNode>
    {
        protected override void Awake()
        {
            base.Awake();
            node.Transform = transform;
        }

        protected override void Render(Camera camera)
        {
            if (camera.name == "Preview Scene Camera")
                return;
            node.Render(camera);
        }
    }
}