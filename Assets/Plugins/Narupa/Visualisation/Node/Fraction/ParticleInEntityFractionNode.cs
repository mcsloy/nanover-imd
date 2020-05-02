using System;
using Narupa.Visualisation.Properties;
using UnityEngine;

namespace Narupa.Visualisation.Node.Fraction
{
    /// <summary>
    /// Calculates the relative (0-1) fraction of each particle in its entity.
    /// </summary>
    [Serializable]
    public class ParticleInEntityFractionNode : GenericFractionNode
    {
        [SerializeField]
        private IntArrayProperty particleResidues;

        [SerializeField]
        private IntArrayProperty residueEntities;

        [SerializeField]
        private IntProperty entityCount;

        protected override bool IsInputValid => particleResidues.HasValue
                                             && residueEntities.HasValue
                                             && entityCount.HasValue;

        protected override bool IsInputDirty => particleResidues.IsDirty
                                             || residueEntities.IsDirty
                                             || entityCount.IsDirty;

        protected override void ClearDirty()
        {
            particleResidues.IsDirty = false;
            residueEntities.IsDirty = false;
            entityCount.IsDirty = false;
        }

        protected override void GenerateArray(ref float[] array)
        {
            var entitySizes = new int[entityCount];
            for (var i = 0; i < entityCount; i++)
                entitySizes[i] = 0;

            var particleInEntityIndex = new int[particleResidues.Length];

            for (var i = 0; i < particleResidues.Length; i++)
            {
                particleInEntityIndex[i] = entitySizes[residueEntities[particleResidues[i]]]++;
            }

            Array.Resize(ref array, particleResidues.Length);
            for (var i = 0; i < particleResidues.Length; i++)
            {
                var entitySize = entitySizes[residueEntities[particleResidues[i]]];
                if (entitySize == 1)
                    array[i] = 0.5f;
                else
                    array[i] = particleInEntityIndex[i] / (entitySize - 1f);
            }
        }
    }
}