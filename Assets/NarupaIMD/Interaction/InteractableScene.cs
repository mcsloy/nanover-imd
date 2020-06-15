using System.Collections.Generic;
using System.Linq;
using Narupa.Core.Math;
using Narupa.Frontend.Manipulation;
using Narupa.Visualisation;
using Narupa.Visualisation.Properties;
using Narupa.Visualisation.Property;
using NarupaIMD.Selection;
using UnityEngine;

namespace NarupaXR.Interaction
{
    /// <summary>
    /// Exposes a <see cref="SynchronisedFrameSource"/> that allows particles to be grabbed, accounting for the interaction method of the selections.
    /// </summary>
    public class InteractableScene : MonoBehaviour, IInteractableParticles
    {
        [Header("The provider of the frames which can be grabbed.")]
        [SerializeField]
        private SynchronisedFrameSource frameSource;

        [SerializeField]
        private NarupaXRPrototype prototype;

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

        private InteractionGroup GetGroupForParticle(int index)
        {
            return null;
        }

        private ParticleSelection GetSelection(InteractionGroup group)
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
            
            var selection = GetGroupForParticle(particleIndex.Value);

            if (selection.Method == InteractionGroupMethod.None)
                return null;

            var indices = GetIndicesInSelection(selection, particleIndex.Value);

            var grab = new ActiveParticleGrab(indices);
            if (selection.ResetVelocities)
                grab.ResetVelocities = true;
            return grab;
        }
        
        /// <summary>
        /// Get the particle indices to select, given the nearest particle index.
        /// </summary>
        private IReadOnlyList<int> GetIndicesInSelection(InteractionGroup instance,
                                                      int particleIndex)
        {
            switch (instance.Method)
            {
                case InteractionGroupMethod.Group:
                    var selection = GetSelection(instance);
                    if (selection == null) 
                        return Enumerable.Range(0, frameSource.CurrentFrame.ParticleCount).ToArray();
                    else
                        return selection.ParticleIds;
                default:
                    return new[] { particleIndex };
            }
        }

        /// <summary>
        /// Get the closest particle to a given point in world space.
        /// </summary>
        private int? GetClosestParticleToWorldPosition(Vector3 worldPosition, float cutoff = Mathf.Infinity)
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