using System.Collections.Generic;
using System.Collections.Specialized;
using Narupa.Core;

namespace NarupaIMD.Selection
{
    /// <summary>
    /// A selection containing a group of particles.
    /// </summary>
    public class ParticleSelection : INotifyCollectionChanged
    {
        /// <summary>
        /// The unique identifier for this selection.
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// The set of indices that are contained in this selection. When null, this
        /// represents a selection containing everything.
        /// </summary>
        public IReadOnlyList<int> Selection => selection;

        /// <summary>
        /// The user-facing name of this selection.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// A set of arbitrary properties associated with this selection.
        /// </summary>
        public IDictionary<string, object> Properties => properties;

        private List<int> selection = new List<int>();

        private Dictionary<string, object> properties = new Dictionary<string, object>();

        /// <summary>
        /// Create a selection with the given ID, that contains all atoms.
        /// </summary>
        public ParticleSelection(string id)
        {
            ID = id;
        }

        /// <summary>
        /// Create a selection from a dictionary representation of the selection.
        /// </summary>
        public ParticleSelection(Dictionary<string, object> obj) : this(obj["id"] as string)
        {
            UpdateFromObject(obj);
        }

        /// <summary>
        /// Update this selection based upon a dictionary representation.
        /// </summary>
        public void UpdateFromObject(Dictionary<string, object> obj)
        {
            Name = obj["name"] as string;
            properties = obj.TryGetValue("properties", out var propertiesValue)
                             ? propertiesValue as Dictionary<string, object>
                             : new Dictionary<string, object>();
            var selectedDict = obj.TryGetValue("selected", out var selectedValue)
                                   ? selectedValue as Dictionary<string, object>
                                   : null;
            if (selectedDict != null)
            {
                var ids = selectedDict["particle_ids"] as List<object>;
                if (ids == null || selection == null)
                {
                    selection = null;
                }
                else
                {
                    selection.Clear();
                    foreach (var id in ids)
                        selection.Add((int) (double) id);
                    selection.Sort();
                }
            }
            else
            {
                selection = null;
            }

            CollectionChanged?.Invoke(
                this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <inheritdoc cref="INotifyCollectionChanged.CollectionChanged" />
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Create a selection representing the shared root selection.
        /// </summary>
        public static ParticleSelection CreateRootSelection()
        {
            return new ParticleSelection("selection.root")
            {
                selection = null,
                Name = "Base"
            };
        }

        private const string KeyHideProperty = "narupa.rendering.hide";
        private const string KeyRendererProperty = "narupa.rendering.renderer";
        private const string KeyInteractionMethod = "narupa.interaction.method";
        private const string KeyResetVelocities = "narupa.interaction.velocity_reset";

        public const string InteractionMethodSingle = "single";
        public const string InteractionMethodGroup = "group";

        public bool HideRenderer => Properties.GetValueOrDefault(KeyHideProperty, false);

        public object Renderer => Properties.GetValueOrDefault<object>(KeyRendererProperty, null);

        public string InteractionMethod
            => Properties.GetValueOrDefault(KeyInteractionMethod, InteractionMethodSingle);

        public bool ResetVelocities => Properties.GetValueOrDefault(KeyResetVelocities, false);
    }
}