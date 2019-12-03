using System.Collections;
using System.Collections.Generic;
using Narupa.Visualisation.Components.Renderer;
using Narupa.Visualisation.Node.Renderer;
using UnityEditor;
using UnityEngine;

namespace Plugins.Narupa.Visualisation.Components.Renderer
{
    /// <inheritdoc cref="Narupa.Visualisation.Node.Renderer" />
    public class GoodsellRenderer : VisualisationComponentRenderer<GoodsellRendererNode>
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            node.Transform = transform;
            node.Setup();
            node.Cleanup();
            StartCoroutine(ResetAfterOneFrame());
        }

        private IEnumerator ResetAfterOneFrame()
        {
            yield return new WaitForEndOfFrame();
            node.Cleanup();
        }

        private void OnDestroy()
        {
            node.Cleanup();
        }

        protected override void Render(Camera camera)
        {
            node.Render(camera);
        }

        protected override void UpdateInEditor()
        {
            base.UpdateInEditor();
            node.Cleanup();
        }
    }
}