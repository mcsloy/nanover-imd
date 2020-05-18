// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Narupa.Core.Math;
using Narupa.Grpc.Interactive;
using Narupa.Session;
using UnityEngine;

namespace Narupa.Frontend.Manipulation
{
    /// <summary>
    /// Situates and IMD simulation in a Unity transform and allows grab
    /// manipulations to begin and end particle interactions.
    /// </summary>
    public class ManipulableParticles
    {
        /// <summary>
        /// The force multiplier.
        /// </summary>
        public float ForceScale { get; set; } = 100f;

        private readonly Transform transform;
        private readonly ImdSession imdSession;

        public IInteractableParticles InteractableParticles { get; set; }

        private readonly HashSet<ActiveParticleGrab> activeGrabs
            = new HashSet<ActiveParticleGrab>();

        public ManipulableParticles(Transform transform,
                                    ImdSession imdSession,
                                    IInteractableParticles interactableParticles)
        {
            this.transform = transform;
            this.imdSession = imdSession;
            this.InteractableParticles = interactableParticles;
        }

        /// <summary>
        /// Start a particle grab on whatever particle falls close to the position
        /// of the given grabber. Return either the manipulation or null if there was
        /// nothing grabbable.
        /// </summary>
        public IActiveManipulation StartParticleGrab(UnitScaleTransformation grabberPose)
        {
            if (InteractableParticles.GetParticleGrab(grabberPose) is InteractionParticleGrab grab)
                return StartParticleGrab(grabberPose, grab);
            return null;
        }

        private ActiveParticleGrab StartParticleGrab(UnitScaleTransformation grabberPose,
                                                     InteractionParticleGrab grab)
        {
            grab.UpdateManipulatorPose(grabberPose);
            grab.ParticleGrabUpdated += () => OnParticleGrabUpdated(grab);
            grab.ManipulationEnded += () => EndParticleGrab(grab);
            OnParticleGrabUpdated(grab);
            activeGrabs.Add(grab);
            return grab;
        }

        private void OnParticleGrabUpdated(InteractionParticleGrab grab)
        {
            var position = transform.InverseTransformPoint(grab.GrabPosition);

            var interaction = grab.Interaction;
            interaction.Position = position;
            interaction.Properties.Scale = ForceScale;
                    
            imdSession.PushInteraction(interaction);
        }

        private void EndParticleGrab(InteractionParticleGrab grab)
        {
            activeGrabs.Remove(grab);
            imdSession.RemoveInteraction(grab.Id);
        }
    }
}