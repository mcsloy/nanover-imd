using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Narupa.Core.Math;
using Narupa.Frontend.Manipulation;
using Narupa.Visualisation;
using Narupa.Visualisation.Properties;
using Narupa.Visualisation.Property;
using NarupaIMD.Selection;
using UnityEngine;
using UnityEngine.Assertions;

namespace NarupaXR.Interaction
{
    /// <summary>
    /// Combines frame information from a  <see cref="SynchronisedFrameSource"/> with
    /// interaction information for the system. This both exposes the set of interacted particle
    /// ids as <see cref="InteractedParticles"/> and can provide an <see cref="ActiveParticleGrab"/>
    /// which accounts for interaction group settings such as group interactions and reset
    /// velocity flags.
    /// </summary>
    public class InteractableScene : MonoBehaviour, IInteractableParticles
    {
        [Header("The provider of the frames which can be grabbed.")]
        [SerializeField]
        private SynchronisedFrameSource frameSource;

        [SerializeField]
        private NarupaXRPrototype prototype;

        private void Awake()
        {
            Assert.IsNotNull(frameSource, $"{nameof(InteractableScene)} is missing " +
                                          $"{nameof(SynchronisedFrameSource)} {nameof(frameSource)}");
            Assert.IsNotNull(prototype, $"{nameof(InteractableScene)} is missing " +
                                        $"{nameof(NarupaXRPrototype)} {nameof(prototype)}");
        }

        /// <inheritdoc cref="InteractedParticles"/>
        private readonly IntArrayProperty interactedParticles = new IntArrayProperty();

        /// <summary>
        /// The set of particles which are currently being interacted with.
        /// </summary>
        public IReadOnlyProperty<int[]> InteractedParticles => interactedParticles;

        private void Update()
        {
            var interactions = prototype.Sessions.Imd.Interactions;
            var pts = new List<int>();
            foreach (var interaction in interactions)
                pts.AddRange(interaction.Value.ParticleIds);
            interactedParticles.Value = pts.ToArray();
        }

        private InteractionGroupData GetInteractionGroupForParticle(int index)
        {
            return null;
        }

        private ParticleSelectionData GetSelectionForInteractionGroup(InteractionGroupData group)
        {
            return null;
        }

        /// <summary>
        /// Attempt to grab the nearest particle, returning null if no interaction is possible.
        /// </summary>
        /// <param name="grabberPose">The transformation of the grabbing pivot.</param>
        public ActiveParticleGrab GetParticleGrab(Transformation grabberPose)
        {
            var particleIndex = GetClosestParticleToWorldPosition(grabberPose.Position);

            if (!particleIndex.HasValue)
                return null;

            var interactionGroup = GetInteractionGroupForParticle(particleIndex.Value);

            var indices = GetIndicesInSelection(interactionGroup, particleIndex.Value);

            var grab = new ActiveParticleGrab(indices);
            if (interactionGroup.ResetVelocities)
                grab.ResetVelocities = true;

            return grab;
        }

        /// <summary>
        /// Get the particle indices to select, given the nearest particle index.
        /// </summary>
        private IReadOnlyList<int> GetIndicesInSelection([CanBeNull] InteractionGroupData group,
                                                         int particleIndex)
        {
            switch (group?.Method ?? InteractionGroupMethod.Single)
            {
                case InteractionGroupMethod.None:
                    return new int[0];
                case InteractionGroupMethod.Group:
                    var selection = GetSelectionForInteractionGroup(group);
                    if (selection == null)
                        return Enumerable.Range(0, frameSource.CurrentFrame.ParticleCount)
                                         .ToArray();
                    else
                        return selection.ParticleIds;
                case InteractionGroupMethod.Single:
                    return new[]
                    {
                        particleIndex
                    };
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Get the closest particle to a given point in world space.
        /// </summary>
        private int? GetClosestParticleToWorldPosition(Vector3 worldPosition,
                                                       float cutoff = Mathf.Infinity)
        {
            var position = transform.InverseTransformPoint(worldPosition);

            var frame = frameSource.CurrentFrame;

            var bestSqrDistance = cutoff * cutoff;
            int? bestParticleIndex = null;

            for (var i = 0; i < frame.ParticlePositions.Length; ++i)
            {
                var particlePosition = frame.ParticlePositions[i];
                var sqrDistance = Vector3.SqrMagnitude(position - particlePosition);

                if (sqrDistance < bestSqrDistance)
                {
                    bestSqrDistance = sqrDistance;
                    bestParticleIndex = i;
                }
            }

            return bestParticleIndex;
        }
    }
}