using System;
using Narupa.Visualisation.Properties;
using Narupa.Visualisation.Properties.Collections;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Color
{
    /// <summary>
    /// Visualiser node which generates colors based upon residue names.
    /// </summary>
    [Serializable]
    public class ResidueNameColorNode : VisualiserColorNode
    {
#pragma warning disable 0649
        [SerializeField]
        private StringArrayProperty residueNames = new StringArrayProperty();

        [SerializeField]
        private IntArrayProperty particleResidues = new IntArrayProperty();

        [SerializeField]
        private StringColorMappingProperty mapping;
#pragma warning restore 0649

        protected override bool IsInputValid => residueNames.HasNonEmptyValue() 
                                             && particleResidues.HasNonEmptyValue();

        protected override bool IsInputDirty => residueNames.IsDirty
                                             || particleResidues.IsDirty;
        protected override void ClearDirty()
        {
            residueNames.IsDirty = false;
            particleResidues.IsDirty = false;
        }

        protected override void UpdateOutput()
        {
            var particleResidues = this.particleResidues.Value;
            var residueNames = this.residueNames.Value;
            output.Resize(particleResidues.Length);
            for (var i = 0; i < particleResidues.Length; i++)
                output.Value[i] = mapping.Value.Map(residueNames[particleResidues[i]]);
            output.MarkValueAsChanged();
        }
    }
}