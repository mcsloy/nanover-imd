// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Narupa.Frontend.Manipulation;
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
        private NarupaXRPrototype narupaXR;
        [SerializeField]
        private InteractionWaveRenderer waveTemplate;
#pragma warning restore 0649

        private Dictionary<ManipulableParticles.ActiveParticleGrab, InteractionWaveRenderer> renderers
            = new Dictionary<ManipulableParticles.ActiveParticleGrab, InteractionWaveRenderer>();

        private void Update()
        {
            var grabs = narupaXR.ManipulableParticles.ActiveGrabs.ToList();
            var frame = narupaXR.FrameSynchronizer.CurrentFrame;
            
            foreach (var grab in grabs)
            {
                var renderer = GetRenderer(grab);
                renderer.StartPosition = grab.GrabPosition;

                var particlePositionSim = frame.ParticlePositions[grab.ParticleIndex];
                var particlePositionWorld = transform.TransformPoint(frame.ParticlePositions[grab.ParticleIndex]);
                renderer.EndPosition = particlePositionWorld;

                renderer.CurrentForceMagnitude = 1;
            }

            var remove = renderers.Keys.Where(grab => !grabs.Contains(grab)).ToList();

            foreach (var grab in remove)
            {
                Destroy(renderers[grab].gameObject);
                renderers.Remove(grab);
            }
        }

        private InteractionWaveRenderer GetRenderer(ManipulableParticles.ActiveParticleGrab grab)
        {
            if (!renderers.TryGetValue(grab, out var renderer))
            {
                renderer = Instantiate(waveTemplate);
                renderer.gameObject.SetActive(true);
                renderer.transform.SetParent(transform);
                renderers.Add(grab, renderer);
            }

            return renderer;
        }
    }
}
