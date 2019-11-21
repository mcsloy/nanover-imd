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

        public ParticleSelection()
        {
        }

        public ParticleSelection(Dictionary<string, object> obj)
        {
            ID = obj["id"] as string;
            UpdateFromObject(obj);
        }

        public void UpdateFromObject(Dictionary<string, object> obj)
        {
            Name = obj["name"] as string;
            properties = obj["properties"] as Dictionary<string, object>;
            var selectedDict = obj["selected"] as Dictionary<string, object>;
            var ids = selectedDict["particle_ids"] as List<object>;
            selection.Clear();
            foreach (var id in ids)
                selection.Add((int) (double) id);
            selection.Sort();

            CollectionChanged?.Invoke(
                this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public static ParticleSelection CreateRootSelection()
        {
            return new ParticleSelection
            {
                selection = null,
                Name = "Base"
            };
        }
    }
}