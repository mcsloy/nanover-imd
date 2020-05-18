// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Narupa.Grpc;
using Narupa.Grpc.Interactive;
using Narupa.Grpc.Serialization;

namespace Narupa.Session
{
    /// <summary>
    /// Provides functionality to manage the sending of particle interactions
    /// over an <see cref="ImdClient"/>.
    /// </summary>
    public class ImdSession : GrpcSession<ImdClient>
    {
        private Dictionary<string, Interaction>
            interactions = new Dictionary<string, Interaction>();

        /// <summary>
        /// Dictionary of all currently known interactions.
        /// </summary>
        public IReadOnlyCollection<Interaction> Interactions => interactions.Values;

        public Action<Interaction> InteractionStarted;

        public Action<Interaction> InteractionUpdated;

        public Action<string> InteractionEnded;

        protected override ImdClient CreateClient(GrpcConnection connection)
        {
            return new ImdClient(connection);
        }

        public override void OpenClient(GrpcConnection connection)
        {
            base.OpenClient(connection);
            Client.SharedState.KeyUpdated += SharedStateOnKeyUpdated;
            Client.SharedState.KeyRemoved += SharedStateOnKeyRemoved;
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
        public override void CloseClient()
        {
            base.CloseClient();

            interactions.Clear();
        }

        public void PushInteraction(Interaction interaction)
        {
            var id = $"interaction.{interaction.InteractionId}";
            SharedState.SetValue(id, Serialization.ToDataStructure(interaction));
        }

        public void RemoveInteraction(string id)
        {
            var key = $"interaction.{id}";
            SharedState.RemoveKey(key);
        }
    }
}