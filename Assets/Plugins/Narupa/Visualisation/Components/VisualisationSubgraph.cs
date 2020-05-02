using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Components
{
    [Serializable]
    [CreateAssetMenu(menuName = "Visualisation/Subgraph", fileName = "Subgraph")]
    public class VisualisationSubgraph : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        [SerializeReference]
        private List<IVisualisationNode> nodes = new List<IVisualisationNode>();

        [SerializeField]
        private List<Link> links = new List<Link>();

        [Serializable]
        public class Link
        {
            [SerializeField]
            [SerializeReference]
            internal IVisualisationNode sourceObject;

            [SerializeField]
            internal string sourceName;

            [SerializeField]
            [SerializeReference]
            internal IVisualisationNode destinationObject;

            [SerializeField]
            internal string destinationName;
        }

        public IReadOnlyCollection<IVisualisationNode> Nodes => nodes;

        public bool IsLinkedTo(IVisualisationNode node, string key)
        {
            return links.Any(n => n.destinationObject == node && n.destinationName == key);
        }

        public Link GetLink(IVisualisationNode node, string key)
        {
            return links.FirstOrDefault(
                n => n.destinationObject == node && n.destinationName == key);
        }

        public Link GetLink(int nodeIndex, string key)
        {
            var node = GetNode(nodeIndex);
            return node != null ? GetLink(node, key) : null;
        }

        public IVisualisationNode GetNode(int index)
        {
            return nodes[index];
        }

        public void RemoveLink(IVisualisationNode node, string key)
        {
            links.RemoveAll(n => n.destinationObject == node && n.destinationName == key);
        }

        public void AddEmptyLink(IVisualisationNode node, string key)
        {
            var existing = GetLink(node, key);
            if (existing == null)
            {
                existing = new Link();
                links.Add(existing);
            }

            existing.destinationName = key;
            existing.destinationObject = node;
            existing.sourceName = null;
            existing.sourceObject = null;
        }

        public bool IsLinkedTo(int index, string key)
        {
            var node = GetNode(index);
            return node != null && IsLinkedTo(node, key);
        }

        public void RemoveLink(int index, string key)
        {
            var node = GetNode(index);
            if (node != null)
                RemoveLink(node, key);
        }

        public void AddEmptyLink(int index, string key)
        {
            var node = GetNode(index);
            if (node != null)
                AddEmptyLink(node, key);
        }

        public IEnumerable<(IVisualisationNode node, string key)> GetPossibleSourcesForLink(
            int index,
            string key)
        {
            var destination = GetNode(index);
            var destinationProperty = VisualisationUtility.GetPropertyField(destination, key);
            var destinationType = destinationProperty.PropertyType;
            for (var i = index - 1; i >= 0; i--)
            {
                var node = nodes[i];
                foreach (var potentialProperty in node
                                                  .AsProvider().GetPotentialProperties()
                                                  .Where(p => destinationType.IsAssignableFrom(
                                                             p.type)))
                    yield return (node, potentialProperty.name);
            }
        }

        public void OnBeforeSerialize()
        {
            ClearUpInvalidLinks();
        }

        /// <summary>
        /// Delete any links which don't have a valid destination. This is important so
        /// refactored field names result in invalid links being removed.
        /// </summary>
        private void ClearUpInvalidLinks()
        {
            links.RemoveAll(
                link => string.IsNullOrEmpty(link.destinationName)
                     || link.destinationObject == null);
        }


        public void OnAfterDeserialize()
        {
        }

        private void OnEnable()
        {
            SetupLinks();
        }

        /// <summary>
        /// Setup all the links that are serialized
        /// </summary>
        private void SetupLinks()
        {
            foreach (var link in links)
            {
                FindAndSetupLink(link);
            }
        }

        /// <summary>
        /// Setup a link between two properties without knowing the type
        /// </summary>
        private void FindAndSetupLink(Link link)
        {
            if (!(link.destinationObject.AsProvider().GetProperty(link.destinationName) is IProperty
                      destination))
            {
                Debug.LogWarning(
                    $"Link has invalid destination {link.destinationObject} {link.destinationName}");
                return;
            }

            var type = destination.PropertyType;

            if (!(link.sourceObject.AsProvider().GetOrCreateProperty(link.sourceName, type) is
                      IReadOnlyProperty source))
            {
                Debug.LogWarning($"Link has invalid source {link.sourceObject} {link.sourceName}");
                return;
            }

            destination.TrySetLinkedProperty(source);
        }

        public IEnumerable<TType> GetNodes<TType>()
        {
            foreach (var node in nodes)
                if (node is TType type)
                    yield return type;
        }
    }
}