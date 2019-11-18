using System;
using System.Collections.Generic;
using Narupa.Grpc.Selection;
using UnityEngine;

namespace NarupaIMD.Selection
{
    public class RenderableLayer : MonoBehaviour
    {
        [SerializeField]
        private VisualisationManager visualisationManager;

        public VisualisationManager VisualisationManager => visualisationManager;

        private void Awake()
        {
            visualisationManager = GetComponentInParent<VisualisationManager>();
        }

        private List<RenderableSelection> selections = new List<RenderableSelection>();

        [SerializeField]
        private RenderableSelection selectionPrefab;

        public RenderableSelection AddSelection(ParticleSelection selection)
        {
            var renderableSelection = Instantiate(selectionPrefab, transform);
            renderableSelection.Selection = selection;
            renderableSelection.gameObject.name = selection.Name;
            renderableSelection.UnderlyingSelectionChanged +=
                () => UpdateSelection(renderableSelection);
            return renderableSelection;
        }

        private void UpdateSelection(RenderableSelection selection)
        {
            var index = selections.IndexOf(selection);
            for (var i = index; i >= 0; i--)
            {
                selections[i].Strip(i == selections.Count ? null : selections[i + 1],
                                    visualisationManager.ParticleCount);
            }
        }
    }
}