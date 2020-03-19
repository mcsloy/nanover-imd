using System;
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

        private void OnEnable()
        {
            client = new Client();
            Refresh();
        }

        /// <summary>
        /// Returns either the <see cref="ServiceHub"/> with a localhost IP address or the first hub.
        /// </summary>
        private ServiceHub SelectBestService(IGrouping<string, ServiceHub> group)
        {
            foreach (var hub in group)
            {
                if (IsLocalhost(hub))
                    return hub;
            }

            return group.First();
        }

        private bool IsLocalhost(ServiceHub hub)
        {
            return hub.Address.Equals("127.0.0.1") || hub.Address.Equals("localhost");
        }

        public void Refresh()
        {
            hubs = client.SearchForServices(500)
                               .GroupBy(hub => hub.Id)
                               .Select(SelectBestService)
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
