using System;
using System.Collections.Generic;
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
        private readonly Dictionary<string, VisualisationLayer> layers = new Dictionary<string, VisualisationLayer>();

        [SerializeField]
        private NarupaImdSimulation simulation;

        [SerializeField]
        private SynchronisedFrameSource frameSource;

        [SerializeField]
        private InteractableScene interactableScene;

        /// <inheritdoc cref="InteractableScene.InteractedParticles"/>
        public IReadOnlyProperty<int[]> InteractedParticles
            => interactableScene.InteractedParticles;

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
        /// The root selection of the scene.
        /// </summary>
        private ParticleSelection rootSelection;

        private const string BaseLayerName = "base";

        private VisualisationLayer BaseLayer => layers[BaseLayerName];

        /// <summary>
        /// Create a visualisation layer with the given name.
        /// </summary>
        public VisualisationLayer AddLayer(string name)
        {
            var layer = Instantiate(layerPrefab, transform);
            layer.gameObject.name = name;
            layers[name] = layer;
            layer.Name = name;
            return layer;
        }

        public void DestroyLayer(string name) 
        {
            if (layers.ContainsKey(name)) 
            {
                Destroy(layers[name].gameObject);
                layers.Remove(name);
            }
        }

        private const string HighlightedParticlesKey = "highlighted.particles";

        private void OnEnable()
        {
            frameAdaptor = gameObject.AddComponent<FrameAdaptor>();
            frameAdaptor.FrameSource = frameSource;
            frameAdaptor.Node.AddOverrideProperty<int[]>(HighlightedParticlesKey).LinkedProperty = InteractedParticles;

            simulation.Multiplayer.SharedStateDictionaryKeyUpdated +=
                MultiplayerOnSharedStateDictionaryKeyChanged;
            simulation.Multiplayer.SharedStateDictionaryKeyRemoved +=
                MultiplayerOnSharedStateDictionaryKeyRemoved;
            var baseLayer = AddLayer(BaseLayerName);
            rootSelection = ParticleSelection.CreateRootSelection();
            var baseRenderableSelection = baseLayer.AddSelection(rootSelection);
            baseRenderableSelection.UpdateVisualiser();
        }

        private void OnDisable()
        {
            simulation.Multiplayer.SharedStateDictionaryKeyUpdated -=
                MultiplayerOnSharedStateDictionaryKeyChanged;
            simulation.Multiplayer.SharedStateDictionaryKeyRemoved -=
                MultiplayerOnSharedStateDictionaryKeyRemoved;

            Destroy(frameAdaptor);
        }

        /// <summary>
        /// Callback for when a key is removed from the multiplayer shared state.
        /// </summary>
        private void MultiplayerOnSharedStateDictionaryKeyRemoved(string key)
        {
            if (key.StartsWith(VisualisationLayer.LayerPrefix))
            {
                DestroyLayer(key);
            }
            else if (key.StartsWith(ParticleSelection.SelectionIdPrefix)) 
            {
                foreach (string layerName in layers.Keys) 
                    layers[layerName].RemoveSelection(key);
            }
        }

        /// <summary>
        /// Callback for when a key is modified in the multiplayer shared state.
        /// </summary>
        private void MultiplayerOnSharedStateDictionaryKeyChanged(string key, object value)
        {
            if (key.StartsWith(VisualisationLayer.LayerPrefix))
            {
                ///Add layer to layers dictionary.
                if (!(value is Dictionary<string, object> dict))
                    return;

                Dictionary<string, object> layerProperties = (Dictionary<string, object>)value;
                
                AddLayer(key);

                ///Set layer properties 
                layers[key].Name = (string)layerProperties["name"];
                layers[key].Order = Convert.ToInt32(layerProperties["order"]);
                layers[key].Alias = (string)layerProperties["alias"];

                ///Apply alias for reading alternative atom data
                if (layers[key].Alias != "")
                    layers[key].ApplyAlias(layers[key].Alias, frameSource);
                else
                    layers[key].ResetAlias();

                ///Set selections.
                List<object> selectionIDs = (List<object>)layerProperties["selections"];

                foreach (string selection in selectionIDs)
                {
                    if (simulation.Multiplayer.SharedStateDictionary.ContainsKey(selection))
                    {
                        var selectionProperties = simulation.Multiplayer.SharedStateDictionary[selection];
                        layers[key].UpdateOrCreateSelection(selection, selectionProperties);
                    }
                }
                
                ///Update layer visualisation.
                foreach (VisualisationSelection selection in layers[key].Selections)
                    selection.UpdateVisualiser();
            }
        }

        /// <summary>
        /// Get the selection in the base layer which contains the particle.
        /// </summary>
        public VisualisationSelection GetSelectionForParticle(int particleIndex)
        {
            return BaseLayer.GetSelectionForParticle(particleIndex);
        }
    }
}
