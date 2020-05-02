using System;
using Narupa.Visualisation.Node.Protein;
using Narupa.Visualisation.Properties;
using Narupa.Visualisation.Properties.Collections;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Color
{
    [Serializable]
    public class SecondaryStructureColor : VisualiserColorNode
    {
        [SerializeField]
        private SecondaryStructureArrayProperty residueSecondaryStructure;

        [SerializeField]
        private IntArrayProperty particleResidues;

        [SerializeField]
        private StringColorMappingProperty scheme;

        private UnityEngine.Color GetSecondaryStructureColor(SecondaryStructureAssignment i)
        {
            return scheme.Value.Map(i.AsSymbol());
        }

        protected override bool IsInputValid => residueSecondaryStructure.HasNonEmptyValue() 
                                             && particleResidues.HasNonNullValue();

        protected override bool IsInputDirty => residueSecondaryStructure.IsDirty
                                             || particleResidues.IsDirty;

        protected override void ClearDirty()
        {
            residueSecondaryStructure.IsDirty = false;
            particleResidues.IsDirty = false;
        }

        protected override void UpdateOutput()
        {
            var secondaryStructure = this.residueSecondaryStructure.Value;
            var residues = this.particleResidues.Value;
            output.Resize(residues.Length);
            for (var i = 0; i < residues.Length; i++)
                output.Value[i] = GetSecondaryStructureColor(secondaryStructure[residues[i]]);
            output.MarkValueAsChanged();
        }
    }
}