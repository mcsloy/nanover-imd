using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Narupa.Visualisation.Components
{
    [ExecuteAlways]
    public class VisualisationSceneGraph : MonoBehaviour, IEnumerable<IVisualisationNode>
    {
        [SerializeField]
        [SerializeReference]
        private List<IVisualisationNode> nodes = new List<IVisualisationNode>();

        public IReadOnlyCollection<IVisualisationNode> Nodes => nodes;

        public void AddNode(IVisualisationNode node)
        {
            nodes.Add(node);
            if (node is IRenderNode renderNode)
                renderNode.Transform = transform;
        }

        private void OnDestroy()
        {
            foreach (var node in nodes)
                if (node is IDisposable disposable)
                    disposable.Dispose();
        }

        protected void Render(Camera camera)
        {
            foreach (var node in nodes)
                if (node is IRenderNode render)
                    render.Render(camera);
        }

        protected void OnEnable()
        {
            Camera.onPreCull += Render;
        }

        protected void OnDisable()
        {
            Camera.onPreCull -= Render;
        }

        private void Update()
        {
            foreach (var node in nodes)
                node.Refresh();
        }

        public IEnumerator<IVisualisationNode> GetEnumerator()
        {
            return nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}