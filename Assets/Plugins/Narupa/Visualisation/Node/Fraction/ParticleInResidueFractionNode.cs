using System;
using Narupa.Visualisation.Properties;
using UnityEngine;

namespace Narupa.Visualisation.Node.Fraction
{
    /// <summary>
    /// Calculates the relative (0-1) fraction of each particle in its residue.
    /// </summary>
    [Serializable]
    public class ParticleInResidueFractionNode : GenericFractionNode
    {
        [SerializeField]
        private IntArrayProperty particleResidues;

        [SerializeField]
        private IntProperty residueCount;

        protected override bool IsInputValid => particleResidues.HasValue
                                             && residueCount.HasValue;

        protected override bool IsInputDirty => particleResidues.IsDirty
                                             || residueCount.IsDirty;

        protected override void ClearDirty()
        {
            particleResidues.IsDirty = false;
            residueCount.IsDirty = false;
        }

        protected override void GenerateArray(ref float[] array)
        {
            var residueSizes = new int[residueCount];
            for (var i = 0; i < residueCount; i++)
                residueSizes[i] = 0;

            var particleInResidueIndex = new int[particleResidues.Length];

            for (var i = 0; i < particleResidues.Length; i++)
            {
                particleInResidueIndex[i] = residueSizes[particleResidues[i]]++;
            }

            Array.Resize(ref array, particleResidues.Length);
            for (var i = 0; i < particleResidues.Length; i++)
            {
                var residueSize = residueSizes[particleResidues[i]];
                if (residueSize == 1)
                    array[i] = 0.5f;
                else
                    array[i] = particleInResidueIndex[i] / (residueSize - 1f);
            }
        }
    }
}