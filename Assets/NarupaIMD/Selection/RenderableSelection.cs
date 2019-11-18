using System;
using System.Collections.Specialized;
using System.Linq;
using Narupa.Frame;
using Narupa.Grpc.Selection;
using Narupa.Visualisation.Components.Input;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace NarupaIMD.Selection
{
    public class RenderableSelection : MonoBehaviour
    {
        [SerializeField]
        private RenderableLayer layer;

        private void Awake()
        {
            layer = GetComponentInParent<RenderableLayer>();
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

        public void Strip(RenderableSelection upperSelection, int maxCount)
        {
            if (Selection.Selection != null)
            {
                var possibleIndices = upperSelection?.unfilteredIndices?.Length ?? maxCount;
                var maxSize = Mathf.Min(Selection.Selection.Count, possibleIndices);
                Array.Resize(ref filteredIndices, maxSize);
                Array.Resize(ref unfilteredIndices, possibleIndices);

                var filteredIndex = 0;
                var unfilteredIndex = 0;

                foreach (var unhandledIndex in upperSelection?.unfilteredIndices ??
                                               Enumerable.Range(0, maxCount))
                {
                    if (Selection.Selection.Contains(unhandledIndex))
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

        public void SetVisualiser(GameObject visualiserPrefab)
        {
            if (visualiser != null)
                Destroy(visualiser);
            visualiser = Instantiate(visualiserPrefab, transform);
            visualiser.GetComponent<IFrameConsumer>().FrameSource =
                layer.VisualisationManager.FrameSource;
            var filter = visualiser.GetComponents<IntArrayInput>()
                                   .FirstOrDefault(p => p.Node.Name == "filter");
            if (filter != null)
                filter.Node.Input.LinkedProperty = FilteredIndices;
        }
    }
}