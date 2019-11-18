using System;
using System.Collections.Generic;
using System.Linq;
using Essd;
using Narupa.Core.Async;
using NarupaXR;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NarupaIMD
{
    /// <summary>
    /// Autoconnect to the first server we find.
    /// </summary>
    public class ServiceDiscovery : MonoBehaviour
    {
        [SerializeField]
        private NarupaXRPrototype prototype;

        private void Start()
        {
            var client = new Client();
            client.StartSearch().AwaitInBackground();
            client.ServiceFound += ClientOnServiceFound;
        }
        private readonly HashSet<ServiceHub> services = new HashSet<ServiceHub>();

        public event  Action<ServiceHub> ServiceDiscovered;
        
        private void ClientOnServiceFound(object sender, ServiceHub e)
        {
            if (!services.Add(e))
                return;

            ServiceDiscovered?.Invoke(e);
        }

        /// <summary>
        /// Connect to the given service hub.
        /// </summary>
        public void Connect(ServiceHub e)
        {
            var services = e.Properties["services"] as JObject;
            prototype.Connect(e.Address, services["trajectory"].ToObject<int>(),
                              services["imd"].ToObject<int>(),
                              services["multiplayer"].ToObject<int>());
        }
    }
}