using System;
using System.Collections.Generic;
using Essd;
using Narupa.Core.Async;
using NarupaXR;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NarupaIMD
{
    /// <summary>
    /// Tracks what services are available.
    /// </summary>
    public class ServiceDiscovery : MonoBehaviour
    {
        [SerializeField]
        private NarupaXRPrototype prototype;

        public IEnumerable<ServiceHub> Services => services;

        private void Start()
        {
            var client = new Client();
            client.StartSearch().AwaitInBackground();
            client.ServiceFound += ClientOnServiceFound;
        }

        private readonly HashSet<ServiceHub> services = new HashSet<ServiceHub>();

        public event Action<ServiceHub> ServiceDiscovered;

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
            if (services == null)
                return;

            prototype.Connect(e.Address,
                              services.TryGetValue("trajectory", out var traj)
                                  ? (int?) traj.ToObject<int>()
                                  : null,
                              services.TryGetValue("imd", out var imd)
                                  ? (int?) imd.ToObject<int>()
                                  : null,
                              services.TryGetValue("multiplayer", out var multi)
                                  ? (int?) multi.ToObject<int>()
                                  : null);
        }
    }
}