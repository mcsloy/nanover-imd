using System;
using System.Collections.Generic;
using Narupa.Grpc.Multiplayer;
using UnityEditor.UI;
using UnityEngine;

namespace NarupaIMD.Selection
{
    public class ParticleVisualisation
    {
        private Visualisation visualisation;

        public event Action SelectionUpdated;

        public event Action Removed;

        private ParticleSelection selection;

        public ParticleSelection Selection
        {
            get { return selection; }
            set
            {
                selection = value;
                SelectionUpdated?.Invoke();
            }
        }

        public string DisplayName => visualisation.DisplayName ?? "Unnamed";

        public float Priority => visualisation.Priority ?? 0;

        public int Layer => visualisation.Layer ?? 0;

        public bool Hide => visualisation.Hide ?? false;

        public object Visualiser => visualisation.Visualiser;

        public IReadOnlyList<int> ParticleIndices => Selection?.ParticleIds;

        public ParticleVisualisation(Visualisation visualisation)
        {
            this.visualisation = visualisation;
        }

        private VariableReference<ParticleSelection> linkedSelection;

        internal VariableReference<ParticleSelection> LinkedSelection
        {
            set
            {
                if (linkedSelection != null)
                {
                    linkedSelection.Changed -= LinkedSelectionChanged;
                }

                linkedSelection = value;
                if (linkedSelection != null)
                {
                    linkedSelection.Changed += LinkedSelectionChanged;
                    Selection = linkedSelection.Value;
                }
            }
        }

        private void LinkedSelectionChanged(ParticleSelection value)
        {
            Selection = value;
        }

        public void Delete()
        {
            LinkedSelection = null;
            Removed?.Invoke();
        }
    }
}