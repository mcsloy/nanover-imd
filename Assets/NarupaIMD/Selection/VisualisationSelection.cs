using System;
using System.Collections.Specialized;
using System.Linq;
using Narupa.Core;
using Narupa.Core.Math;
using Narupa.Frame;
using Narupa.Utility;
using Narupa.Visualisation.Components;
using Narupa.Visualisation.Node.Input;
using Narupa.Visualisation.Node.Renderer;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace NarupaIMD.Selection
{
    /// <summary>
    /// Scene representation of a selection, which will render the selection using a
    /// given visualiser.
    /// </summary>
    public class VisualisationSelection : MonoBehaviour
    {
        [SerializeField]
        private VisualisationLayer layer;

        private void Awake()
        {
            layer = GetComponentInParent<VisualisationLayer>();
        }

        /// <summary>
        /// The underlying selection that is reflected by this visualisation.
        /// </summary>
        public ParticleSelection Selection
        {
            get => selection;
            set
            {
                if (selection != null)
                    selection.SelectionUpdated -= OnSelectionUpdated;
                selection = value;
                if (selection != null)
                    selection.SelectionUpdated += OnSelectionUpdated;
            }
        }

        /// <summary>
        /// Callback for when the underlying selection has changed.
        /// </summary>
        public event Action SelectionUpdated;

        private void OnSelectionUpdated()
        {
            SelectionUpdated?.Invoke();
            UpdateVisualiser();
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

        /// <summary>
        /// Given potentially a higher selection, which will have drawn some particles,
        /// work out which particles are left and should be drawn by this visualiser.
        /// </summary>
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
                    if (SearchAlgorithms.BinarySearch(unhandledIndex, selectionIndices))
                        filteredIndices[filteredIndex++] = unhandledIndex;
                    else
                        unfilteredIndices[unfilteredIndex++] = unhandledIndex;

                Array.Resize(ref filteredIndices, filteredIndex);
                Array.Resize(ref unfilteredIndices, unfilteredIndex);

                FilteredIndices.Value = filteredIndices;
                UnfilteredIndices.Value = unfilteredIndices;
            }
            else // This selection selects everything
            {
                var upperIndices = upperSelection?.unfilteredIndices;
                // The upper selection selected everything
                if (upperIndices == null)
                {
                    filteredIndices = null;
                    FilteredIndices.UndefineValue();
                }
                else
                {
                    // The upper selection has left some indices
                    FilteredIndices.LinkedProperty = upperSelection.UnfilteredIndices;
                }

                unfilteredIndices = new int[0];
                UnfilteredIndices.Value = unfilteredIndices;
            }
        }

        private GameObject currentVisualiser;

        /// <summary>
        /// Update the visualiser based upon the data stored in the selection.
        /// </summary>
        public void UpdateVisualiser()
        {
            // The hide property turns off any visualiser
            if (Selection.HideRenderer)
            {
                SetVisualiser(null, false);
                return;
            }

            GameObject visualiser = null;
            var isPrefab = true;

            // Construct a visualiser from any provided renderer info
            if (Selection.Renderer is object data)
                (visualiser, isPrefab) = VisualiserFactory.ConstructVisualiser(data);

            // Use the predefined ball and stick renderer as a default
            if (visualiser == null)
            {
                (visualiser, isPrefab) = VisualiserFactory.ConstructVisualiser("ball and stick");
            }

            if (visualiser != null)
            {
                // Todo: Formalise this
                // Set the bottom-most selection so it draws bonds between itself and other atoms.
                var index = layer.Selections.IndexOf(this);
                if (index == 0)
                    foreach (var renderer in visualiser
                        .GetVisualisationNodesInChildren<ParticleBondRendererNode>())
                        renderer.BondToNonFiltered = true;

                SetVisualiser(visualiser, isPrefab);
            }
            else
            {
                SetVisualiser(null, false);
            }
        }

        /// <summary>
        /// Set the visualiser of this selection
        /// </summary>
        /// <param name="isPrefab">Is the argument a prefab, and hence needs instantiating?</param>
        public void SetVisualiser(GameObject newVisualiser, bool isPrefab = true)
        {
            if (currentVisualiser != null)
                Destroy(currentVisualiser);

            if (newVisualiser == null)
                return;

            if (isPrefab)
            {
                currentVisualiser = Instantiate(newVisualiser, transform);
            }
            else
            {
                currentVisualiser = newVisualiser;
                currentVisualiser.transform.parent = transform;
                currentVisualiser.transform.localPosition = Vector3.zero;
                currentVisualiser.transform.localRotation = Quaternion.identity;
                currentVisualiser.transform.localScale = Vector3.one;
            }

            // Set visualiser's frame input
            currentVisualiser.GetComponent<IFrameConsumer>().FrameSource = layer.Scene.FrameSource;

            // Setup any filters so the visualiser only draws this selection.
            var filter = currentVisualiser.GetVisualisationNodes<IntArrayInputNode>()
                .FirstOrDefault(p => p.Name == "filter" ||
                                     p.Name == "particle.filter");
            if (filter != null)
                filter.Input.LinkedProperty = FilteredIndices;
            
            // Hookup the particle highlights
            var highlight = currentVisualiser.GetVisualisationNodes<IInputNode>()
                                      .FirstOrDefault(i => i.Name == "particle.highlighted");
            highlight?.Input.TrySetLinkedProperty(layer.Scene.InteractedParticles);



        }
    }
}