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
    public class VisualisationManager : MonoBehaviour
    {
        private List<RenderableLayer> layers = new List<RenderableLayer>();

        [SerializeField]
        private NarupaXRPrototype narupaIMD;

        [SerializeField]
        private SynchronisedFrameSource frameSource;

        [SerializeField]
        private RenderableLayer layerPrefab = null;

        [SerializeField]
        private GameObject defaultVisualiser;

        public GameObject DefaultVisualiser => defaultVisualiser;

        public int ParticleCount => frameSource.CurrentFrame?.ParticleCount ?? 0;

        public ITrajectorySnapshot FrameSource => frameSource;

        public RenderableLayer AddLayer()
        {
            var layer = Instantiate(layerPrefab, transform);
            layer.gameObject.name = "Base Layer";
            layers.Add(layer);
            return layer;
        }

        private void Start()
        {
            narupaIMD.Sessions.Multiplayer.SharedStateDictionaryKeyChanged +=
                MultiplayerOnSharedStateDictionaryKeyChanged;
            var baseLayer = AddLayer();
            var baseSelection = ParticleSelection.CreateRootSelection();
            var baseRenderableSelection = baseLayer.AddSelection(baseSelection);
            baseRenderableSelection.SetVisualiser(defaultVisualiser);
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

        public GameObject GetVisualiser(string name)
        {
            return visualiserPrefabs.FirstOrDefault(
                v => v.name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}