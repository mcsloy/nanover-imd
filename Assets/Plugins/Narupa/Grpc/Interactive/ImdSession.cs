// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Narupa.Core;
using Narupa.Core.Async;
using Narupa.Grpc;
using Narupa.Grpc.Interactive;
using Narupa.Grpc.Serialization;
using Narupa.Grpc.Stream;
using Narupa.Protocol.Imd;
using UnityEngine;

namespace Narupa.Session
{
    /// <summary>
    /// Provides functionality to manage the sending of particle interactions
    /// over an <see cref="ImdClient"/>.
    /// </summary>
    public class ImdSession : IDisposable
    {
        private Dictionary<string, Interaction> interactions;

        /// <summary>
        /// Dictionary of all currently known interactions.
        /// </summary>
        public IReadOnlyCollection<Interaction> Interactions => interactions.Values;

        public OutgoingStreamCollection<ParticleInteraction, InteractionEndReply> 
            InteractionStreams { get; private set; }

        private ImdClient client;

        public Action<Interaction> InteractionStarted;

        public Action<Interaction> InteractionUpdated;

        public Action<string> InteractionEnded;

        /// <summary>
        /// Connect to an IMD service over the given connection. Closes any 
        /// existing client.
        /// </summary>
        public void OpenClient(GrpcConnection connection)
        {
            CloseClient();

            client = new ImdClient(connection);
            client.SharedState.KeyUpdated += SharedStateOnKeyUpdated;
            client.SharedState.KeyRemoved += SharedStateOnKeyRemoved;
        }

        private void SharedStateOnKeyRemoved(string key)
        {
            if (key.StartsWith("interaction."))
            {
                var id = key.Substring(12);
                if (interactions.ContainsKey(id))
                {
                    interactions.Remove(id);
                    InteractionEnded?.Invoke(id);
                }
            }
        }

        private void SharedStateOnKeyUpdated(string key, object value)
        {
            if (key.StartsWith("interaction."))
            {
                var id = key.Substring(12);
                if (interactions.ContainsKey(id))
                {
                    Serialization.UpdateFromDataStructure(value, interactions[id]);
                    InteractionUpdated?.Invoke(interactions[id]);
                }
                else
                {
                    var interaction = Serialization.FromDataStructure<Interaction>(value);
                    interaction.InteractionId = id;
                    interactions[id] = interaction;
                    InteractionStarted?.Invoke(interaction);
                }
            }
        }

        /// <summary>
        /// Close the current IMD client and dispose all streams.
        /// </summary>
        public void CloseClient()
        {
            client?.CloseAndCancelAllSubscriptions();
            client?.Dispose();
            client = null;

            InteractionStreams?.CloseAsync();
            InteractionStreams?.Dispose();
            InteractionStreams = null;

            interactions.Clear();
        }

        public void PushInteraction(Interaction interaction)
        {
            var id = $"interaction.{interaction.InteractionId}";
            client.SharedState.SetValue(id, Serialization.ToDataStructure(interaction));
        }
        
        public void RemoveInteraction(string id)
        {
            var key = $"interaction.{id}";
            client.SharedState.RemoveKey(key);
        }

        /// <inheritdoc cref="IDisposable.Dispose" />
        public void Dispose()
        {
            CloseClient();
        }
    }
}