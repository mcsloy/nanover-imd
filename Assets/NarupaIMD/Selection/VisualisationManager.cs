using System.Collections.Generic;
using Narupa.Frame;
using Narupa.Grpc.Selection;
using Narupa.Visualisation;
using UnityEngine;

namespace NarupaIMD.Selection
{
    public class VisualisationManager : MonoBehaviour
    {
        private List<RenderableLayer> layers = new List<RenderableLayer>();

        [SerializeField]
        private SynchronisedFrameSource frameSource;

        [SerializeField]
        private RenderableLayer layerPrefab = null;

        [SerializeField]
        private GameObject defaultVisualiser;

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
            var baseLayer = AddLayer();
            var baseSelection = ParticleSelection.CreateRootSelection();
            var baseRenderableSelection = baseLayer.AddSelection(baseSelection);
            baseRenderableSelection.SetVisualiser(defaultVisualiser);
        }
    }
}