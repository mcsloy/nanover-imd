using System;
using JetBrains.Annotations;
using Narupa.Visualisation.Node.Adaptor;
using Narupa.Visualisation.Properties.Collections;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Protein
{
    /// <summary>
    /// A calculator which finds polypeptide sequences and performs secondary structure calculations on them.
    /// </summary>
    [Serializable]
    public class SecondaryStructureCalculatorNode
    {
        internal PolypeptideSequenceNode polypeptideSequence = new PolypeptideSequenceNode();
        internal DsspCalculatorNode secondaryStructure = new DsspCalculatorNode();

        public IReadOnlyProperty<SecondaryStructureAssignment[]> Assignments => assignments;
        
        [NotNull]
        private SecondaryStructureArrayProperty assignments = new SecondaryStructureArrayProperty();

        [SerializeField]
        private DsspOptions options = new DsspOptions();

        public void LinkToAdaptor(ParentedAdaptorNode adaptor)
        {
            polypeptideSequence.AtomNames.LinkedProperty = adaptor.ParticleNames;
            secondaryStructure.AtomPositions.LinkedProperty = adaptor.ParticlePositions;
            polypeptideSequence.AtomResidues.LinkedProperty = adaptor.ParticleResidues;
            polypeptideSequence.ResidueNames.LinkedProperty = adaptor.ResidueNames;
            polypeptideSequence.ResidueEntities.LinkedProperty = adaptor.ResidueEntities;
            secondaryStructure.ResidueCount.LinkedProperty = adaptor.ResidueCount;

            secondaryStructure.AtomNames.LinkedProperty = polypeptideSequence.AtomNames;
            secondaryStructure.AtomResidues.LinkedProperty = polypeptideSequence.AtomResidues;
            secondaryStructure.PeptideResidueSequences.LinkedProperty =
                polypeptideSequence.ResidueSequences;
            assignments.LinkedProperty = secondaryStructure.ResidueSecondaryStructure;

            secondaryStructure.DsspOptions = options;

            adaptor.AddOverrideProperty<SecondaryStructureAssignment[]>(
                VisualiserFactory.ResidueSecondaryStructure.Key);
        }

        /// <inheritdoc cref="BaseAdaptorNode.Refresh"/>
        public void Refresh()
        {
            polypeptideSequence.Refresh();
            secondaryStructure.Refresh();
        }
    }
}