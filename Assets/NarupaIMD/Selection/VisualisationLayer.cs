using System;
using System.Collections.Generic;
using Narupa.Grpc.Selection;
using UnityEngine;

namespace NarupaIMD.Selection
{
    /// <summary>
    /// A group of <see cref="VisualisationSelection"/>s which are mutually exclusive.
    /// </summary>
    public class VisualisationLayer : MonoBehaviour
    {
        [SerializeField]
        private VisualisationScene visualisationManager;

        public VisualisationScene VisualisationManager => visualisationManager;

        private void Awake()
        {
            visualisationManager = GetComponentInParent<VisualisationScene>();
        }

        private List<VisualisationSelection> selections = new List<VisualisationSelection>();

        [SerializeField]
        private VisualisationSelection selectionPrefab;

        public VisualisationSelection AddSelection(ParticleSelection selection)
        {
            var renderableSelection = Instantiate(selectionPrefab, transform);
            renderableSelection.Selection = selection;
            renderableSelection.gameObject.name = selection.Name;
            renderableSelection.UnderlyingSelectionChanged +=
                () => UpdateSelection(renderableSelection);
            selections.Add(renderableSelection);
            return renderableSelection;
        }

        private void UpdateSelection(VisualisationSelection selection)
        {
            var index = selections.IndexOf(selection);
            if (index < 0)
                throw new InvalidOperationException(
                    "Tried to update selection no longer in layer.");
            for (var i = index; i >= 0; i--)
            {
                selections[i].CalculateFilteredIndices(i == selections.Count-1 ? null : selections[i + 1],
                                    visualisationManager.ParticleCount);
            }
        }

        public void UpdateOrCreateSelection(string key, object value)
        {
            if (!(value is Dictionary<string, object> dict))
                return;
            foreach (var selection in selections)
            {
                if (selection.Selection.ID == key)
                {
                    selection.Selection.UpdateFromObject(dict);
                    selection.gameObject.name = selection.Selection.Name;
                    return;
                }
            }

            var newSelection = new ParticleSelection(dict);
            var selec = AddSelection(newSelection);
            if(visualisationManager.GetVisualiser("liquorice") is GameObject visualiserPrefab)
                selec.SetVisualiser(visualiserPrefab);
        }
    }
}