using System;
using Narupa.Visualisation.Node.Protein;
using Narupa.Visualisation.Properties;
using Narupa.Visualisation.Properties.Collections;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Scale
{
    /// <summary>
    /// Provides scales that depend on the secondary structure.
    /// </summary>
    [Serializable]
    public class SecondaryStructureScaleNode : VisualiserScaleNode
    {
        [SerializeField]
        private SecondaryStructureArrayProperty residueSecondaryStructure;

        [SerializeField]
        private IntArrayProperty particleResidues;

        [SerializeField]
        private StringFloatMappingProperty scheme;

        [SerializeField]
        private FloatProperty scale;

        private float GetSecondaryStructureScale(SecondaryStructureAssignment i)
        {
            return scheme.Value.Map(i.AsSymbol()) * scale.Value;
        }


        protected override bool IsInputValid => residueSecondaryStructure.HasNonEmptyValue()
                                             && particleResidues.HasNonNullValue()
                                             && scheme.HasNonNullValue()
                                             && scale.HasValue;

        protected override bool IsInputDirty => residueSecondaryStructure.IsDirty
                                             || particleResidues.IsDirty
                                             || scheme.IsDirty
                                             || scale.IsDirty;

        protected override void ClearDirty()
        {
            residueSecondaryStructure.IsDirty = false;
            particleResidues.IsDirty = false;
            scheme.IsDirty = false;
            scale.IsDirty = false;
        }

        protected override void ClearOutput()
        {
            scales.UndefineValue();
        }

        protected override void UpdateOutput()
        {
            var secondaryStructure = this.residueSecondaryStructure.Value;
            var residues = this.particleResidues.Value;
            var scaleArray = scales.Resize(residues.Length);
            for (var i = 0; i < residues.Length; i++)
                scaleArray[i] = GetSecondaryStructureScale(secondaryStructure[residues[i]]);

            scales.Value = scaleArray;
        }
    }
}