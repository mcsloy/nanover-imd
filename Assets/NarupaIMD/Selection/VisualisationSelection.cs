using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Narupa.Core;
using Narupa.Core.Math;
using Narupa.Frame;
using Narupa.Grpc.Selection;
using Narupa.Utility;
using Narupa.Visualisation.Components;
using Narupa.Visualisation.Components.Adaptor;
using Narupa.Visualisation.Components.Input;
using Narupa.Visualisation.Components.Renderer;
using Narupa.Visualisation.Property;
using UnityEngine;
using Color = System.Drawing.Color;

namespace NarupaIMD.Selection
{
    public class VisualisationSelection : MonoBehaviour
    {
        [SerializeField]
        private VisualisationLayer layer;

        private void Awake()
        {
            layer = GetComponentInParent<VisualisationLayer>();
        }

        public ParticleSelection Selection
        {
            get => selection;
            set
            {
                selection = value;
                selection.CollectionChanged += SelectionOnCollectionChanged;
            }
        }

        public event Action UnderlyingSelectionChanged;

        private void SelectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UnderlyingSelectionChanged?.Invoke();
            UpdateRenderer();
        }

        private ParticleSelection selection;

        /// <summary>
        /// The indices of particles that should be rendered in this selection.
        /// </summary>
        public IntArrayProperty FilteredIndices { get; } = new IntArrayProperty();

        /// <summary>
        /// The indices of particles not rendered by this or any higher selections.
        /// </summary>
        public IntArrayProperty UnfilteredIndices { get; } = new IntArrayProperty();

        private int[] filteredIndices = new int[0];

        private int[] unfilteredIndices = new int[0];

        public void CalculateFilteredIndices(VisualisationSelection upperSelection, int maxCount)
        {
            if (Selection.Selection != null)
            {
                var possibleIndices = upperSelection?.unfilteredIndices?.Length ?? maxCount;
                var maxSize = Mathf.Min(Selection.Selection.Count, possibleIndices);
                Array.Resize(ref filteredIndices, maxSize);
                Array.Resize(ref unfilteredIndices, possibleIndices);

                var filteredIndex = 0;
                var unfilteredIndex = 0;

                var selectionIndices = Selection.Selection;

                foreach (var unhandledIndex in upperSelection?.unfilteredIndices ??
                                               Enumerable.Range(0, maxCount))
                {
                    if (SearchAlgorithms.BinarySearch(unhandledIndex, selectionIndices))
                        filteredIndices[filteredIndex++] = unhandledIndex;
                    else
                        unfilteredIndices[unfilteredIndex++] = unhandledIndex;
                }

                Array.Resize(ref filteredIndices, filteredIndex);
                Array.Resize(ref unfilteredIndices, unfilteredIndex);

                FilteredIndices.Value = filteredIndices;
                UnfilteredIndices.Value = unfilteredIndices;
            }
            else
            {
                var upperIndices = upperSelection?.unfilteredIndices;
                if (upperIndices == null)
                {
                    // This selection selects everything
                    filteredIndices = null;
                    unfilteredIndices = new int[0];
                    FilteredIndices.UndefineValue();
                    UnfilteredIndices.Value = unfilteredIndices;
                }
                else
                {
                    // This selection selects everything, and the upper selection exists
                    FilteredIndices.LinkedProperty = upperSelection.UnfilteredIndices;
                    unfilteredIndices = new int[0];
                    UnfilteredIndices.Value = unfilteredIndices;
                }
            }
        }

        private GameObject visualiser;


        public void UpdateRenderer()
        {
            var rendererInfo = selection.Properties["narupa.rendering.renderer"];
            GameObject visualiser = null;
            bool prefab = true;
            if (rendererInfo is string visName)
            {
                visualiser =
                    layer.VisualisationManager.GetVisualiser(visName);
            }
            else if (rendererInfo is Dictionary<string, object> dict)
            {
                visualiser = new GameObject();
                var adaptor = visualiser.AddComponent<FrameAdaptor>();
                var dynamic = visualiser.AddComponent<DynamicComponent>();
                dynamic.FrameAdaptor = adaptor;

                var filterInput = visualiser.AddComponent<IntArrayInput>();
                filterInput.Node.Name = "particle.filter";

                if (dict.ContainsKey("color") && dict["color"] is string colorName)
                {
                    var htmlColor = Color.FromName(colorName);
                    var color = new UnityEngine.Color(htmlColor.R / 255f,
                                                      htmlColor.G / 255f,
                                                      htmlColor.B / 255f,
                                                      htmlColor.A / 255f);
                    var input = visualiser.AddComponent<ColorInput>();
                    input.Node.Name = "color";
                    input.Node.Input.Value = color;
                }

                var renderName = dict.GetValueOrDefault<string>("render");
                var render =
                    layer.VisualisationManager.GetRenderSubgraph(renderName ?? "ball and stick");

                dynamic.SetSubgraphs(render);

                prefab = false;
            }


            if (visualiser != null)
            {
                var index = layer.Selections.IndexOf(this);
                if(index == 0)
                    foreach (var renderer in visualiser
                        .GetComponentsInChildren<ParticleBondRenderer>())
                        renderer.Node.BondToNonFiltered = true;
                SetVisualiser(visualiser, prefab);
            }
        }


        public void SetVisualiser(GameObject visualiserPrefab, bool isPrefab = true)
        {
            if (visualiser != null)
                Destroy(visualiser);
            
            if (isPrefab)
                visualiser = Instantiate(visualiserPrefab, transform);
            else
            {
                visualiser = visualiserPrefab;
                visualiser.transform.parent = transform;
                visualiser.transform.localPosition = Vector3.zero;
                visualiser.transform.localRotation = Quaternion.identity;
            }

            visualiser.GetComponent<IFrameConsumer>().FrameSource =
                layer.VisualisationManager.FrameSource;
            var filter = visualiser.GetComponents<IntArrayInput>()
                                   .FirstOrDefault(p => p.Node.Name == "filter" || p.Node.Name == "particle.filter");
            if (filter != null)
                filter.Node.Input.LinkedProperty = FilteredIndices;
        }
    }
}