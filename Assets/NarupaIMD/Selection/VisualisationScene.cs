using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Frame;
using Narupa.Grpc.Selection;
using Narupa.Visualisation;
using NarupaXR;
using UnityEngine;

namespace NarupaIMD.Selection
{
    /// <summary>
    /// Core visualisation class, that manages layers and selections.
    /// </summary>
    public class VisualisationScene : MonoBehaviour
    {
        private List<VisualisationLayer> layers = new List<VisualisationLayer>();

        [SerializeField]
        private NarupaXRPrototype narupaIMD;

        [SerializeField]
        private SynchronisedFrameSource frameSource;

        [SerializeField]
        private VisualisationLayer layerPrefab = null;

        [SerializeField]
        private GameObject defaultVisualiser;

        public int ParticleCount => frameSource.CurrentFrame?.ParticleCount ?? 0;

        public ITrajectorySnapshot FrameSource => frameSource;

        /// <summary>
        /// Create a layer with the given name.
        /// </summary>
        public VisualisationLayer AddLayer(string name)
        {
            var layer = Instantiate(layerPrefab, transform);
            layer.gameObject.name = name;
            layers.Add(layer);
            return layer;
        }

        private ParticleSelection rootSelection;

        private void Start()
        {
            narupaIMD.Sessions.Multiplayer.SharedStateDictionaryKeyChanged +=
                MultiplayerOnSharedStateDictionaryKeyChanged;
            narupaIMD.Sessions.Multiplayer.SharedStateDictionaryKeyRemoved +=
                MultiplayerOnSharedStateDictionaryKeyRemoved;
            var baseLayer = AddLayer("Base Layer");
            rootSelection = ParticleSelection.CreateRootSelection();
            rootSelection.Properties["narupa.rendering.renderer"] = "ball and stick";
            var baseRenderableSelection = baseLayer.AddSelection(rootSelection);
            baseRenderableSelection.UpdateRenderer();
        }

        private void MultiplayerOnSharedStateDictionaryKeyRemoved(string key)
        {
            if (key == "selection.root")
            {
                // Reset root selection
            }
            else if (key.StartsWith("selection."))
            {
                var layer = layers.First();
                layer.RemoveSelection(key);
            }
        }

        private void MultiplayerOnSharedStateDictionaryKeyChanged(string key, object value)
        {
            if (key.StartsWith("selection."))
            {
                var layer = layers.First();
                layer.UpdateOrCreateSelection(key, value);
            }
        }

        [SerializeField]
        private GameObject[] visualiserPrefabs;

        [SerializeField]
        private GameObject[] renderSubgraphs;

        public GameObject GetVisualiser(string name)
        {
            return visualiserPrefabs.FirstOrDefault(
                v => v.name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public GameObject GetRenderSubgraph(string name)
        {
            return renderSubgraphs.FirstOrDefault(
                v => v.name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}