using System;
using Narupa.Visualisation.Properties;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Fraction
{
    /// <summary>
    /// Calculates the relative (0-1) fraction of each residue in the system.
    /// </summary>
    [Serializable]
    public class EntityInSystemFractionNode : GenericFractionNode
    {
        [SerializeField]
        private IntArrayProperty particleResidues;

        [SerializeField]
        private IntArrayProperty residueEntities;

        [SerializeField]
        private IntProperty entityCount;

        protected override bool IsInputValid => particleResidues.HasNonEmptyValue()
                                             && residueEntities.HasNonNullValue()
                                             && entityCount.HasNonNullValue();

        protected override bool IsInputDirty => particleResidues.IsDirty
                                             || residueEntities.IsDirty
                                             || entityCount.IsDirty;

        protected override void ClearDirty()
        {
            particleResidues.IsDirty = false;
            entityCount.IsDirty = false;
            residueEntities.IsDirty = false;
        }

        protected override void GenerateArray(ref float[] array)
        {
            Array.Resize(ref array, particleResidues.Length);
            if (entityCount == 1)
            {
                for (var i = 0; i < particleResidues.Length; i++)
                    array[i] = 0.5f;
            }
            else
            {
                for (var i = 0; i < particleResidues.Length; i++)
                    array[i] = residueEntities[particleResidues[i]] / (entityCount - 1f);
            }
        }
    }
}