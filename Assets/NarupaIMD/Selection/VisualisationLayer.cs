using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Narupa.Core.Math;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace NarupaIMD.Selection
{
    /// <summary>
    /// A group of <see cref="VisualisationInstance" />s which are mutually exclusive.
    /// </summary>
    /// <remarks>
    /// Selections within a visualisation layer display an atom precisely once,
    /// with the selection settings overwriting one another in the order that they are
    /// established.
    /// For example, a <see cref="VisualisationLayer" /> with selections [1,2,3] and
    /// [3,4,5] will display atoms [1,2] in the first (lower) selection and atoms
    /// [3,4,5] in the second (upper) selection.
    /// </remarks>
    public class VisualisationLayer : MonoBehaviour
    {
        [SerializeField]
        private VisualisationScene scene;

        [SerializeField]
        private int layer;
        
        private readonly List<VisualisationInstance> currentMembers =
            new List<VisualisationInstance>();

        public event Action Removed;

        /// <summary>
        /// The order of this layer.
        /// </summary>
        public int Layer
        {
            get => layer;
            set => layer = value;
        }

        /// <summary>
        /// The <see cref="VisualisationScene" /> to which this layer belongs.
        /// </summary>
        public VisualisationScene Scene
        {
            get => scene;
            set => scene = value;
        }

        /// <summary>
        /// The set of visualisations on this layer.
        /// </summary>
        public IReadOnlyList<VisualisationInstance> Members => currentMembers;

        private void Awake()
        {
            Scene = GetComponentInParent<VisualisationScene>();
        }

        [SerializeField]
        private VisualisationInstance instancePrefab;

        private class PriorityComparer : IComparer
        {
            public int Compare(object a, object b)
            {
                if (a is ParticleVisualisation vis1 && b is ParticleVisualisation vis2)
                {
                    if (vis1.Priority > vis2.Priority)
                        return 1;
                    if (vis1.Priority < vis2.Priority)
                        return -1;
                }

                return 0;
            }
        }
        
        private static PriorityComparer priorityComparer = new PriorityComparer();
        
        /// <summary>
        /// Add a visualisation to this layer.
        /// </summary>
        public VisualisationInstance AddVisualisation(ParticleVisualisation visualisation)
        {
            var instance = Instantiate(instancePrefab, transform);
            instance.Visualisation = visualisation;
            instance.gameObject.name = visualisation.DisplayName;
            
            if (currentMembers.Any())
            {
                var insertIndex = 0;
                foreach (var existingSelection in currentMembers)
                    if (priorityComparer.Compare(existingSelection.Visualisation, visualisation) > 0)
                        insertIndex++;
                currentMembers.Insert(insertIndex, instance);
            }
            else
            {
                currentMembers.Add(instance);
            }

            RecalculateIndices(instance);
            instance.UpdateVisualiser();
            return instance;
        }

        /// <summary>
        /// Find the selection on this layer which contains the particle of the given
        /// index.
        /// </summary>
        [CanBeNull]
        public VisualisationInstance GetSelectionForParticle(int particleIndex)
        {
            for (var i = currentMembers.Count - 1; i >= 0; i--)
            {
                var selection = currentMembers[i];
                if (!selection.FilteredIndices.HasNonNullValue())
                    return selection;
                if (SearchAlgorithms.BinarySearch(particleIndex, selection.FilteredIndices.Value))
                    return selection;
            }

            return null;
        }

        public void RecalculateIndices(VisualisationInstance visualisation)
        {
            var index = currentMembers.IndexOf(visualisation);
            if (index < 0)
                throw new ArgumentException(
                    "Tried to update visualisation not in layer.");
            for (var i = index; i >= 0; i--)
                currentMembers[i].CalculateFilteredIndices(
                    i == currentMembers.Count - 1 ? null : currentMembers[i + 1],
                    Scene.ParticleCount);
        }

        public void RemoveInstance(VisualisationInstance instance)
        {
            Destroy(instance.gameObject);
            currentMembers.Remove(instance);
            if (currentMembers.Count > 0)
                RecalculateIndices(currentMembers.Last());
            else
                Removed?.Invoke();
        }
    }
}