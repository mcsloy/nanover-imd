using System;
using System.Collections;
using System.Collections.Generic;
using Narupa.Visualisation.Node.Adaptor;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Components
{
    [ExecuteAlways]
    public class VisualisationSceneGraph : MonoBehaviour, IEnumerable<IVisualisationNode>
    {
        [SerializeField]
        [SerializeReference]
        private List<IVisualisationNode> nodes = new List<IVisualisationNode>();

        private List<VisualisationSubgraph> subgraphs = new List<VisualisationSubgraph>();

        public IReadOnlyList<VisualisationSubgraph> Subgraphs => subgraphs;
        
        public IReadOnlyCollection<IVisualisationNode> Nodes => nodes;

        private ParentedAdaptorNode parentAdaptor;

        private ParticleFilteredAdaptorNode filterAdaptor;

        public void AddNode(IVisualisationNode node)
        {
            nodes.Add(node);
            if (node is IRenderNode renderNode)
                renderNode.Transform = transform;
        }

        public void SetGraphs(List<VisualisationSubgraph> subgraphs)
        {
            this.subgraphs = subgraphs;
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

        public void SetParent(BaseAdaptorNode source)
        {
            if (source != null)
            {
                parentAdaptor.ParentAdaptor.Value = source;
            }
            else
            {
                parentAdaptor.ParentAdaptor.UndefineValue();
            }
        }

        internal void SetAdaptors(ParentedAdaptorNode root, ParticleFilteredAdaptorNode filter)
        {
            parentAdaptor = root;
            filterAdaptor = filter;
        }
        
        public void SetFilter(IReadOnlyProperty<int[]> source)
        {
            if (source != null)
            {
                filterAdaptor.ParticleFilter.LinkedProperty = source;
            }
            else
            {
                filterAdaptor.ParticleFilter.UndefineValue();
            }
        }
    }
}