using System.Collections.Generic;
using Narupa.Grpc.Multiplayer;
using Narupa.Visualisation;
using Narupa.Visualisation.Components.Adaptor;
using Narupa.Visualisation.Property;
using NarupaXR;
using NarupaXR.Interaction;
using UnityEngine;

namespace NarupaIMD.Selection
{
    /// <summary>
    /// A set of layers and selections that are used to render a frame using multiple
    /// visualisers.
    /// </summary>
    /// <remarks>
    /// It contains a <see cref="FrameAdaptor" /> to which each visualiser will be
    /// linked.
    /// </remarks>
    public class VisualisationScene : MonoBehaviour
    {
        /// <summary>
        /// The <see cref="VisualisationLayer" />s that make up this scene.
        /// </summary>
        private readonly Dictionary<int, VisualisationLayer> layers =
            new Dictionary<int, VisualisationLayer>();

        [SerializeField]
        private NarupaXRPrototype narupaIMD;

        [SerializeField]
        private SynchronisedFrameSource frameSource;

        [SerializeField]
        private InteractableScene interactableScene;

        /// <inheritdoc cref="InteractableScene.InteractedParticles"/>
        public IReadOnlyProperty<int[]> InteractedParticles =>
            interactableScene.InteractedParticles;

        /// <inheritdoc cref="FrameAdaptor" />
        /// <remarks>
        /// This is automatically generated on <see cref="Start()" />.
        /// </remarks>
        private FrameAdaptor frameAdaptor;

        /// <summary>
        /// The <see cref="FrameAdaptor" /> that exposes all the data present in the frame
        /// in a way that is compatible with the visualisation system.
        /// </summary>
        public FrameAdaptor FrameAdaptor => frameAdaptor;

        [SerializeField]
        private VisualisationLayer layerPrefab;

        /// <summary>
        /// The number of particles in the current frame, or 0 if no frame is present.
        /// </summary>
        public int ParticleCount => frameSource.CurrentFrame?.ParticleCount ?? 0;

        /// <summary>
        /// Get or create the visualisation layer with the given index
        /// </summary>
        public VisualisationLayer GetOrCreateLayer(int ordinal)
        {
            if (layers.ContainsKey(ordinal))
                return layers[ordinal];
            var layer = Instantiate(layerPrefab, transform);
            layer.gameObject.name = $"Layer {ordinal}";
            layer.Layer = ordinal;
            layer.Removed += () => DestroyLayer(layer);
            layers[ordinal] = layer;
            return layer;
        }

        /// <summary>
        /// Destroy the given visualisation layer
        /// </summary>
        public void DestroyLayer(VisualisationLayer layer)
        {
            layers.Remove(layer.Layer);
            Destroy(layer.gameObject);
        }

        private const string HighlightedParticlesKey = "highlighted.particles";

        private MultiplayerCollection<VisualisationData> visualisations;
        private MultiplayerCollection<ParticleSelectionData> selections;

        private Dictionary<string, ParticleVisualisation> currentVisualisations =
            new Dictionary<string, ParticleVisualisation>();

        private void Start()
        {
            selections =
                narupaIMD.Sessions.Multiplayer.GetSharedCollection<ParticleSelectionData>(
                    "selections.");
            visualisations =
                narupaIMD.Sessions.Multiplayer
                         .GetSharedCollection<VisualisationData>("visualiser.");

            frameAdaptor = gameObject.AddComponent<FrameAdaptor>();
            frameAdaptor.FrameSource = frameSource;
            frameAdaptor.Node.AddOverrideProperty<int[]>(HighlightedParticlesKey).LinkedProperty =
                InteractedParticles;

            visualisations.ItemCreated += OnMultiplayerVisualisationCreated;
            visualisations.ItemUpdated += OnMultiplayerVisualisationUpdated;
            visualisations.ItemRemoved += OnMultiplayerVisualisationRemoved;

            foreach (var visualisation in visualisations.Keys)
                OnMultiplayerVisualisationCreated(visualisation);

            CreateDefaultVisualiser();
        }

        private void CreateDefaultVisualiser()
        {
            var defaultVisualisation = new DefaultVisualisation();
            var layer = GetOrCreateLayer(defaultVisualisation.Layer);
            layer.AddVisualisation(defaultVisualisation);
        }


        private void OnMultiplayerVisualisationCreated(string key)
        {
            var value = visualisations[key];
            var vis = new ParticleVisualisation(value);
            if (value.SelectionKey != null)
                vis.LinkedSelection = selections.GetReference(value.SelectionKey);
            currentVisualisations[key] = vis;
            var layer = GetOrCreateLayer(vis.Layer);
            layer.AddVisualisation(vis);
        }

        private void OnMultiplayerVisualisationUpdated(string key)
        {
            OnMultiplayerVisualisationRemoved(key);
            OnMultiplayerVisualisationCreated(key);
        }

        private void OnMultiplayerVisualisationRemoved(string key)
        {
            currentVisualisations[key].Delete();
            currentVisualisations.Remove(key);

            if (visualisations.Count == 0)
                CreateDefaultVisualiser();
        }
    }
}