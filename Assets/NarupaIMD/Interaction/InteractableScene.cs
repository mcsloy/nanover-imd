using System.Collections.Generic;
using System.Linq;
using Narupa.Core.Math;
using Narupa.Frontend.Manipulation;
using Narupa.Visualisation;
using NarupaIMD.Selection;
using UnityEngine;

namespace NarupaXR.Interaction
{
    /// <summary>
    /// Represents a set of particles that can be interacted with.
    /// </summary>
    public class InteractableScene : MonoBehaviour, IInteractableParticles
    {
        [SerializeField]
        private SynchronisedFrameSource frameSource;

        [SerializeField]
        private VisualisationScene visualisationScene;

        public ActiveParticleGrab GetParticleGrab(Transformation grabberPose)
        {
            var particleIndex = GetClosestParticleToWorldPosition(grabberPose.Position);
            var selection = visualisationScene.GetSelectionForParticle(particleIndex);
            var indices = GetSelectedIndices(selection, particleIndex);

            if (selection.Selection.InteractionMethod == "none")
                return null;
            
            var grab = new ActiveParticleGrab(indices);
            if (selection.Selection.ResetVelocities)
                grab.ResetVelocities = true;
            return grab;
        }
        
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