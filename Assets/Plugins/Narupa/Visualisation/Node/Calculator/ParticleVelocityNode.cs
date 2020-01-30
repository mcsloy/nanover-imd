using System;
using Narupa.Visualisation.Properties;
using Narupa.Visualisation.Properties.Collections;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Calculator
{
    /// <summary>
    /// Calculate the velocity of the particles using the time on the client.
    /// </summary>
    [Serializable]
    public class ParticleVelocityNode : GenericOutputNode
    {
        [SerializeField]
        private Vector3ArrayProperty particlePositions = new Vector3ArrayProperty();
        
        private Vector3ArrayProperty particleVelocities = new Vector3ArrayProperty();

        private Vector3[] previousPositions = null;
        
        protected override bool IsInputValid => particlePositions.HasNonNullValue();
        protected override bool IsInputDirty => particlePositions.IsDirty;
        protected override void ClearDirty()
        {
            particlePositions.IsDirty = false;
        }

        protected override void UpdateOutput()
        {
            var count = particlePositions.Value.Length;
            
            if (previousPositions != null && previousPositions.Length != count)
                previousPositions = null;
            
            if (previousPositions == null)
            {
                previousPositions = new Vector3[count];
                particleVelocities.Resize(count);
                for (var i = 0; i < count; i++)
                    particleVelocities.Value[i] = Vector3.zero;
            }
            else
            {
                for (var i = 0; i < count; i++)
                    particleVelocities.Value[i] = particlePositions.Value[i] - previousPositions[i];
                Array.Copy(particlePositions.Value, previousPositions, count);
            }
            
            particleVelocities.MarkValueAsChanged();
        }

        protected override void ClearOutput()
        {
            particleVelocities.UndefineValue();
            previousPositions = null;
        }
    }
}