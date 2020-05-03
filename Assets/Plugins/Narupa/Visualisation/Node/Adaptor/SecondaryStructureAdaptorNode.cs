using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Narupa.Visualisation.Components;
using Narupa.Visualisation.Node.Protein;
using Narupa.Visualisation.Node.Sequence;
using Narupa.Visualisation.Properties.Collections;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Adaptor
{
    /// <summary>
    /// An <see cref="BaseAdaptorNode"/> which injects a 'residue.secondarystructures' key which provides per residue secondary structure based on the DSSP algorithm.
    /// </summary>
    [Serializable]
    public class SecondaryStructureAdaptorNode : VisualisationNode
    {
        internal PolypeptideSequenceNode polypeptideSequence = new PolypeptideSequenceNode();
        internal SecondaryStructureNode secondaryStructure = new SecondaryStructureNode();

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
                VisualisationFrameKeys.ResidueSecondaryStructure.Key);
        }

        /// <inheritdoc cref="BaseAdaptorNode.Refresh"/>
        public override void Refresh()
        {
            base.Refresh();
            polypeptideSequence.Refresh();
            secondaryStructure.Refresh();
        }
    }
}