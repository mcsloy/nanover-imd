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
            var particleIndex = GetNearestParticle(grabberPose.Position);
            var selection = visualisationScene.GetSelectionForParticle(particleIndex);
            var indices = selection.Selection ?? new[]
            {
                particleIndex
            };
            var grab = new ActiveParticleGrab(indices);
            return grab;
        }
        
        private int GetNearestParticle(Vector3 worldPosition)
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