using System;
using System.Collections.Generic;
using Narupa.Grpc.Multiplayer;
using UnityEditor.UI;
using UnityEngine;

namespace NarupaIMD.Selection
{
    /// <summary>
    /// Represents an instance of a visualisation, wrapping a <see cref="VisualisationData"/> and
    /// watching the corresponding <see cref="ParticleSelectionData"/> for changes in what is
    /// selected.
    /// </summary>
    public class ParticleVisualisation
    {
        /// <summary>
        /// Invoked when the selection used by this visualisation is altered. This should trigger
        /// a recalculation of which particle indices belong in each visualisation, based upon
        /// their priorities and layers.
        /// </summary>
        public event Action SelectionUpdated;

        /// <summary>
        /// Invoked when this visualisation should be removed.
        /// </summary>
        public event Action Removed;

        private VisualisationData visualisation;

        private ParticleSelectionData selection;

        /// <inheritdoc cref="VisualisationData.DisplayName"/>
        public string DisplayName => visualisation.DisplayName ?? "Unnamed";

        /// <inheritdoc cref="VisualisationData.Priority"/>
        public float Priority => visualisation.Priority ?? 0;

        /// <inheritdoc cref="VisualisationData.Layer"/>
        public int Layer => visualisation.Layer ?? 0;

        /// <inheritdoc cref="VisualisationData.Hide"/>
        public bool Hide => visualisation.Hide ?? false;

        /// <inheritdoc cref="VisualisationData.Visualiser"/>
        public object Visualiser => visualisation.Visualiser;

        /// <summary>
        /// The particle indices that can be drawn by this visualisation. Depending on other
        /// visualisations present, not all of these particles may be drawn.
        /// </summary>
        public IReadOnlyList<int> ParticleIndices => selection?.ParticleIds;

        /// <summary>
        /// Create a <see cref="ParticleVisualisation"/> that wraps the given
        /// <see cref="VisualisationData"/> provided in the shared state.
        /// </summary>
        public ParticleVisualisation(VisualisationData visualisation)
        {
            this.visualisation = visualisation;
        }

        private MultiplayerResource<ParticleSelectionData> linkedSelection;

        internal MultiplayerResource<ParticleSelectionData> LinkedSelection
        {
            set
            {
                if (linkedSelection != null)
                {
                    linkedSelection.ValueChanged -= LinkedSelectionChanged;
                }

                linkedSelection = value;
                if (linkedSelection != null)
                {
                    linkedSelection.ValueChanged += LinkedSelectionChanged;
                    selection = linkedSelection.Value;
                }
                else
                {
                    selection = default;
                }
            }
        }

        private void LinkedSelectionChanged()
        {
            selection = linkedSelection.Value;
            SelectionUpdated?.Invoke();
        }

        public void Delete()
        {
            LinkedSelection = null;
            Removed?.Invoke();
        }
    }
}