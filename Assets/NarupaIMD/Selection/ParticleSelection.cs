using System.Collections.Generic;
using System.Collections.Specialized;

namespace Narupa.Grpc.Selection
{
    public class ParticleSelection : INotifyCollectionChanged
    {
        public string ID { get; }

        public IReadOnlyList<int> Selection => selection;

        public string Name { get; private set; }

        public IDictionary<string, object> Properties => properties;

        private List<int> selection = new List<int>();

        private Dictionary<string, object> properties = new Dictionary<string, object>();

        public ParticleSelection(string id)
        {
            ID = id;
        }

        public ParticleSelection(Dictionary<string, object> obj) : this(obj["id"] as string)
        {
            UpdateFromObject(obj);
        }

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

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public static ParticleSelection CreateRootSelection()
        {
            return new ParticleSelection("selection.root")
            {
                selection = null,
                Name = "Base"
            };
        }
    }
}