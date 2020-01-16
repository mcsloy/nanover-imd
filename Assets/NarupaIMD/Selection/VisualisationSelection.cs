using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Narupa.Core.Math;
using Narupa.Frame;
using Narupa.Utility;
using Narupa.Visualisation.Components;
using Narupa.Visualisation.Node.Input;
using Narupa.Visualisation.Node.Renderer;
using Narupa.Visualisation.Property;
using Plugins.Narupa.Core;
using UnityEngine;

namespace NarupaIMD.Selection
{
    /// <summary>
    /// Scene representation of a selection, which will render the selection using a
    /// given visualiser.
    /// </summary>
    public class VisualisationSelection : MonoBehaviour
    {
        /// <summary>
        /// Callback for when the underlying selection has changed.
        /// </summary>
        public event Action SelectionUpdated;

        /// <summary>
        /// The indices of particles that should be rendered in this selection.
        /// </summary>
        public IntArrayProperty FilteredIndices { get; } = new IntArrayProperty();

        private int[] filteredIndices = new int[0];

        /// <summary>
        /// The indices of particles not rendered by this or any higher selections.
        /// </summary>
        /// /// <remark> 
        /// Unfiltered indices in this selection form the set of indices to be filtered
        /// by the selections beneath it in the stack. By default, any indices left at the bottom of the stack
        /// will be rendered by the base selection.
        /// </remark>
        public IntArrayProperty UnfilteredIndices { get; } = new IntArrayProperty();

        private int[] unfilteredIndices = new int[0];

        private ParticleSelection selection;

        [SerializeField]
        private VisualisationLayer layer;

        private GameObject currentVisualiser;

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

        private void OnSelectionUpdated()
        {
            SelectionUpdated?.Invoke();
            UpdateVisualiser();
        }

        /// <summary>
        /// Given a selection that is at a higher level in the layer, which will have drawn
        /// some particles, work out which particles that have not been drawn should be
        /// drawn by this selection and which should be left for another selection further
        /// down the stack.
        /// </summary>
        public void CalculateFilteredIndices(VisualisationSelection upperSelection, int maxCount)
        {
            var indices = upperSelection?.unfilteredIndices;

            FilterIndices(indices,
                          Selection.Selection,
                          maxCount,
                          ref filteredIndices,
                          ref unfilteredIndices);

            if (filteredIndices == null)
                FilteredIndices.UndefineValue();
            else
                FilteredIndices.Value = filteredIndices;
            UnfilteredIndices.Value = unfilteredIndices;
        }

        public static void FilterIndices([CanBeNull] IReadOnlyCollection<int> indices,
                                         [CanBeNull] IReadOnlyList<int> filter,
                                         int maxCount,
                                         ref int[] filteredIndices,
                                         ref int[] unfilteredIndices)
        {
            if (filter != null)
            {
                if (filter.Count == 0) // Selection is empty
                {
                    filteredIndices = new int[0];
                    
                    if (indices == null) // Empty selection, indices was all
                        unfilteredIndices = null;
                    else // Empty selection, indices is a subset
                        unfilteredIndices = indices.ToArray();
                }
                else
                {
                    // Calculate the subset of indices which belong in this selection
                    FilterIndices(indices ?? Enumerable.Range(0, maxCount).ToArray(),
                                  filter,
                                  ref filteredIndices,
                                  ref unfilteredIndices);
                }
            }
            else // This selection selects everything
            {
                if (indices == null)
                {
                    // The upper selection selected everything
                    filteredIndices = null;
                }
                else
                {
                    // The upper selection has left some indices
                    filteredIndices = indices.ToArray();
                }

                unfilteredIndices = new int[0];
            }
        }

        /// <summary>
        /// Given a set of indices and a filter, split the indices into the set which are
        /// in filter and those which are not.
        /// </summary>
        /// <param name="indices">A set of indices to filter</param>
        /// <param name="filter">A set of indices to search for in indices</param>
        /// <param name="filteredIndices">
        /// An array to fill with indices present in both inputs.
        /// </param>
        /// <param name="unfilteredIndices">
        /// An array to fill with indices present in indices, but not filter.
        /// </param>
        private static void FilterIndices([NotNull] IReadOnlyCollection<int> indices,
                                          [NotNull] IReadOnlyList<int> filter,
                                          ref int[] filteredIndices,
                                          ref int[] unfilteredIndices)
        {
            var totalIndicesCount = indices.Count;
            var maxSize = Mathf.Min(filter.Count, totalIndicesCount);
            Array.Resize(ref filteredIndices, maxSize);
            Array.Resize(ref unfilteredIndices, totalIndicesCount);

            var filteredIndex = 0;
            var unfilteredIndex = 0;

            // For each index, check if it is in the filter and add it to the appropriate array
            foreach (var unhandledIndex in indices)
                if (SearchAlgorithms.BinarySearch(unhandledIndex, filter))
                    filteredIndices[filteredIndex++] = unhandledIndex;
                else
                    unfilteredIndices[unfilteredIndex++] = unhandledIndex;

            Array.Resize(ref filteredIndices, filteredIndex);
            Array.Resize(ref unfilteredIndices, unfilteredIndex);
        }

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
        /// Keys in the visualisation stack that represent a particle filter
        /// </summary>
        public static string[] KeyParticleFilters =
        {
            "filter", "particle.filter"
        };

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
                currentVisualiser.transform.SetToLocalIdentity();
            }

            // Set visualiser's frame input
            currentVisualiser.GetComponent<IFrameConsumer>().FrameSource = layer.Scene.FrameSource;

            // Setup any filters so the visualiser only draws this selection.
            var filter = currentVisualiser.GetVisualisationNodes<IntArrayInputNode>()
                                          .FirstOrDefault(
                                              p => KeyParticleFilters.Contains(p.Name));
            if (filter != null)
                filter.Input.LinkedProperty = FilteredIndices;
        }
    }
}