using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Core.Math;
using Narupa.Frontend.Manipulation;
using Narupa.Visualisation;
using Narupa.Visualisation.Property;
using NarupaIMD.Selection;
using NarupaIMD.UI;
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

        [Header("The object which provides the selection information.")]
        [SerializeField]
        private VisualisationScene visualisationScene;

        [SerializeField]
        private ConnectedApplicationState prototype;
        
        public IntArrayProperty interactedParticles = new IntArrayProperty();

        private void Update()
        {
            var interactions = prototype.Sessions.Imd.Interactions;
            var pts = new List<int>();
            foreach(var interaction in interactions)
                pts.AddRange(interaction.Value.ParticleIds);
            interactedParticles.Value = pts.ToArray();

        }

        /// <summary>
        /// Attempt to grab the nearest particle, returning null if no interaction is possible.
        /// </summary>
        /// <param name="grabberPose">The transformation of the grabbing pivot.</param>
        public ActiveParticleGrab GetParticleGrab(Transformation grabberPose)
        {
            var particleIndex = GetClosestParticleToWorldPosition(grabberPose.Position);
            var selection = visualisationScene.GetSelectionForParticle(particleIndex);

            if (selection.Selection.InteractionMethod == ParticleSelection.InteractionMethodNone)
                return null;

            var indices = GetSelectedIndices(selection, particleIndex);

            var grab = new ActiveParticleGrab(indices);
            if (selection.Selection.ResetVelocities)
                grab.ResetVelocities = true;
            return grab;
        }
        
        /// <summary>
        /// Get the particle indices to select, given the nearest particle index.
        /// </summary>
        private IReadOnlyList<int> GetSelectedIndices(VisualisationSelection selection,
                                                      int particleIndex)
        {
            switch (selection.Selection.InteractionMethod)
            {
                case ParticleSelection.InteractionMethodGroup:
                    if (selection.FilteredIndices == null)
                        return Enumerable.Range(0, frameSource.CurrentFrame.ParticleCount)
                                         .ToArray();
                    else
                        return selection.FilteredIndices.Value;
                default:
                    return new[] { particleIndex };
            }
        }

        /// <summary>
        /// Get the closest particle to a given point in world space.
        /// </summary>
        private int GetClosestParticleToWorldPosition(Vector3 worldPosition)
        {
            var position = transform.InverseTransformPoint(worldPosition);

            var frame = frameSource.CurrentFrame;

            var bestSqrDistance = Mathf.Infinity;
            var bestParticleIndex = 0;

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