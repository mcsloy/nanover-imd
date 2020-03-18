using System.Collections.Generic;
using System.Linq;
using Essd;
using Narupa.Frontend.UI;
using NarupaXR;
using UnityEngine;

namespace NarupaIMD.UI.Scene
{
    public class DiscoverServers : MonoBehaviour
    {
        private Client client;
        private List<ServiceHub> hubs = new List<ServiceHub>();

        [SerializeField]
        private Sprite localServerIcon;

        [SerializeField]
        private Sprite remoteServerIcon;

        [SerializeField]
        private NarupaXRPrototype application;
        
        [SerializeField]
        private DynamicMenu menu;
        
        public void Refresh()
        {
            hubs = client.SearchForServices(500)
                               .GroupBy(hub => hub.Id)
                               .Select(group => group.First())
                               .ToList();

            menu.ClearChildren();
            foreach (var hub in hubs)
            {
                var local = hub.Address.Equals("127.0.0.1") || hub.Address.Equals("localhost");
                menu.AddItem(hub.Name, local ? localServerIcon : remoteServerIcon, () => application.Connect(hub), hub.Address);
            }
        }
    }
}
