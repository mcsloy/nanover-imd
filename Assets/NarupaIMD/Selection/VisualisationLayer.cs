using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Core.Math;
using UnityEngine;

namespace NarupaIMD.Selection
{
    /// <summary>
    /// A group of <see cref="VisualisationSelection" />s which are mutually exclusive.
    /// </summary>
    public class VisualisationLayer : MonoBehaviour
    {
        [SerializeField]
        private VisualisationScene scene;

        /// <summary>
        /// The <see cref="VisualisationScene" /> to which this layer belongs.
        /// </summary>
        public VisualisationScene Scene
        {
            get => scene;
            set => scene = value;
        }

        /// <summary>
        /// The set of selections that form this layer.
        /// </summary>
        public IReadOnlyList<VisualisationSelection> Selections => selections;

        private void Awake()
        {
            Scene = GetComponentInParent<VisualisationScene>();
        }

        private readonly List<VisualisationSelection> selections =
            new List<VisualisationSelection>();

        [SerializeField]
        private VisualisationSelection selectionPrefab;

        /// <summary>
        /// Add a selection to this visualisation, based upon a
        /// <see cref="ParticleSelection" />.
        /// </summary>
        public VisualisationSelection AddSelection(ParticleSelection selection)
        {
            var visualisationSelection = Instantiate(selectionPrefab, transform);
            visualisationSelection.Selection = selection;
            visualisationSelection.gameObject.name = selection.Name;
            visualisationSelection.UnderlyingSelectionChanged +=
                () => OnSelectionUpdated(visualisationSelection);
            selections.Add(visualisationSelection);
            return visualisationSelection;
        }

        /// <summary>
        /// Refresh all selections that are lower down the layer than the given selection.
        /// </summary>
        private void OnSelectionUpdated(VisualisationSelection selection)
        {
            var index = selections.IndexOf(selection);
            if (index < 0)
                throw new ArgumentException(
                    "Tried to update selection not in layer.");
            for (var i = index; i >= 0; i--)
                selections[i].CalculateFilteredIndices(
                    i == selections.Count - 1 ? null : selections[i + 1],
                    Scene.ParticleCount);
        }

        /// <summary>
        /// Either update an existing selection with the given key or add a new selection
        /// with the given key.
        /// </summary>
        public void UpdateOrCreateSelection(string key, object value)
        {
            if (!(value is Dictionary<string, object> dict))
                return;
            foreach (var selection in selections)
                if (selection.Selection.ID == key)
                {
                    selection.Selection.UpdateFromObject(dict);
                    selection.gameObject.name = selection.Selection.Name;
                    return;
                }

            var newSelection = new ParticleSelection(dict);
            var selec = AddSelection(newSelection);
            selec.UpdateVisualiser();
        }

        /// <summary>
        /// Remove the selection with the given key.
        /// </summary>
        public void RemoveSelection(string key)
        {
            var selection = selections.FirstOrDefault(s => s.Selection.ID == key);
            if (selection == null)
                return;

            Destroy(selection.gameObject);
            selections.Remove(selection);

            OnSelectionUpdated(selections.Last());
        }

        public ParticleSelection GetSelectionForParticle(int particleIndex)
        {
            for (var i = selections.Count - 1; i >= 0; i--)
            {
                var selection = selections[i].Selection;
                if (selection.Selection == null)
                    return selection;
                if (SearchAlgorithms.BinarySearch(particleIndex, selection.Selection))
                    return selection;
            }

            return null;
        }
    }
}