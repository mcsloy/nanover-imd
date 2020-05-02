using System;
using System.Collections.Generic;
using Narupa.Visualisation.Properties;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Fraction
{
    /// <summary>
    /// Calculates the relative (0-1) fraction of each residue in an entity.
    /// </summary>
    [Serializable]
    public class ResidueInEntityFractionNode : GenericFractionNode
    {
        [SerializeField]
        private IntArrayProperty particleResidues;

        [SerializeField]
        private IntArrayProperty residueEntities;

        [SerializeField]
        private IntProperty entityCount;

        protected override bool IsInputValid => particleResidues.HasNonEmptyValue()
                                             && residueEntities.HasNonEmptyValue()
                                             && entityCount.HasNonNullValue();

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
            var entitySize = new int[entityCount];
            for (var i = 0; i < entityCount; i++)
                entitySize[i] = 0;

            var residueRelativeIndex = new int[residueEntities.Length];

            for (var i = 0; i < residueEntities.Length; i++)
            {
                residueRelativeIndex[i] = entitySize[residueEntities[i]]++;
            }

            Array.Resize(ref array, particleResidues.Length);
            for (var i = 0; i < particleResidues.Length; i++)
            {
                var residue = particleResidues[i];
                array[i] = residueRelativeIndex[residue] / (entitySize[residueEntities[residue]] - 0.9999f);
            }
        }
    }
}