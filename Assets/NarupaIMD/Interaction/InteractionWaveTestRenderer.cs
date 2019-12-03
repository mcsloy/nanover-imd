// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Frontend.Manipulation;
using Narupa.Session;
using Narupa.Visualisation;
using NarupaIMD.UI;
using UnityEngine;

namespace NarupaXR.Interaction
{
    /// <summary>
    /// Manage instances of InteractionWaveRenderer so that all known 
    /// interactions are rendered using Mike's pretty sine wave method from 
    /// Narupa 1
    /// </summary>
    public class InteractionWaveTestRenderer : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private ConnectedApplicationState narupaXR;
        [SerializeField]
        private InteractionWaveRenderer waveTemplate;
#pragma warning restore 0649


        [SerializeField]
        private SynchronisedFrameSource frameSource;

        private Dictionary<string, InteractionWaveRenderer> renderers
            = new Dictionary<string, InteractionWaveRenderer>();

        private void Update()
        {
            if (!narupaXR.IsConnected)
                return;
            
            var interactions = narupaXR.Sessions.Imd.Interactions;
            var frame = frameSource.CurrentFrame;
            
            foreach (var interaction in interactions.Values)
            {
                var renderer = GetRenderer(interaction.InteractionId);
                renderer.StartPosition = transform.TransformPoint(interaction.Position);

                var particlePositionSim = computeParticleCentroid(interaction.ParticleIds);
                var particlePositionWorld = transform.TransformPoint(particlePositionSim);
                renderer.EndPosition = particlePositionWorld;

                renderer.CurrentForceMagnitude = .5f;
            }

            var remove = renderers.Keys.Where(interactionId => !interactions.ContainsKey(interactionId)).ToList();

            foreach (var interactionId in remove)
            {
                Destroy(renderers[interactionId].gameObject);
                renderers.Remove(interactionId);
            }

            Vector3 computeParticleCentroid(int[] particleIds)
            {
                var centroid = Vector3.zero;

                for (int i = 0; i < particleIds.Length; ++i)
                    centroid += frame.ParticlePositions[particleIds[i]];

                return centroid / particleIds.Length;
            }
        }

        private InteractionWaveRenderer GetRenderer(string interactionId)
        {
            if (!renderers.TryGetValue(interactionId, out var renderer))
            {
                renderer = Instantiate(waveTemplate);
                renderer.gameObject.SetActive(true);
                renderer.transform.SetParent(transform);
                renderers.Add(interactionId, renderer);
            }

            return renderer;
        }
    }
}
