using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Core.Science;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Protein
{
    /// <summary>
    /// Calculates sequences of subsequent residues in entities which are standard
    /// amino acids, hence forming polypeptide chains.
    /// </summary>
    [Serializable]
    public class PolypeptideSequenceNode : GenericOutputNode
    {
        /// <summary>
        /// The name of each atom.
        /// </summary>
        [SerializeField]
        private StringArrayProperty atomNames = new StringArrayProperty();

        /// <summary>
        /// The residue index for each atom.
        /// </summary>
        [SerializeField]
        private IntArrayProperty atomResidues = new IntArrayProperty();

        /// <summary>
        /// The name of each residue.
        /// </summary>
        [SerializeField]
        private StringArrayProperty residueNames = new StringArrayProperty();

        /// <summary>
        /// The entity (chain) index for each residue.
        /// </summary>
        [SerializeField]
        private IntArrayProperty residueEntities = new IntArrayProperty();

        /// <summary>
        /// A set of sequences of residue indices that form polypeptide chains.
        /// </summary>
        private readonly SelectionArrayProperty residueSequences = new SelectionArrayProperty();

        /// <summary>
        /// A set of sequences of alpha carbon atom indices that form polypeptide chains.
        /// </summary>
        private readonly SelectionArrayProperty alphaCarbonSequences = new SelectionArrayProperty();

        /// <inheritdoc cref="IsInputValid" />
        protected override bool IsInputValid => atomNames.HasNonNullValue()
                                             && atomResidues.HasNonNullValue()
                                             && residueNames.HasNonNullValue()
                                             && residueEntities.HasNonNullValue();

        /// <inheritdoc cref="IsInputDirty" />
        protected override bool IsInputDirty => atomNames.IsDirty
                                             || atomResidues.IsDirty
                                             || residueNames.IsDirty
                                             || residueEntities.IsDirty;

        /// <inheritdoc cref="ClearDirty" />
        protected override void ClearDirty()
        {
            atomNames.IsDirty = false;
            atomResidues.IsDirty = false;
            residueNames.IsDirty = false;
            residueEntities.IsDirty = false;
        }

        /// <inheritdoc cref="UpdateOutput" />
        protected override void UpdateOutput()
        {
            var residueSequences = new List<List<int>>();
            var alphaCarbonSequences = new List<List<int>>();
            var residueNames = this.residueNames.Value;
            var atomNames = this.atomNames.Value;
            var atomResidues = this.atomResidues.Value;
            var residueEntities = this.residueEntities.Value;
            var currentResidues = new List<int>();
            var currentAlphaCarbons = new List<int>();
            int currentEntity = -1;
            for (var atom = 0; atom < atomNames.Length; atom++)
            {
                if (!string.Equals(atomNames[atom], "ca",
                                   StringComparison.InvariantCultureIgnoreCase))
                    continue;
                var residue = atomResidues[atom];
                if (!AminoAcid.IsStandardAminoAcid(residueNames[residue]))
                    continue;
                var entity = residueEntities[residue];
                if (currentEntity != entity && currentResidues.Any())
                {
                    residueSequences.Add(currentResidues);
                    alphaCarbonSequences.Add(currentAlphaCarbons);
                    currentResidues = new List<int>();
                    currentAlphaCarbons = new List<int>();
                }

                currentEntity = entity;
                currentResidues.Add(residue);
                currentAlphaCarbons.Add(atom);
            }

            if (currentResidues.Any())
            {
                residueSequences.Add(currentResidues);
                alphaCarbonSequences.Add(currentAlphaCarbons);
            }

            this.residueSequences.Value = residueSequences.ToArray();
            this.alphaCarbonSequences.Value = alphaCarbonSequences.ToArray();
        }

        /// <inheritdoc cref="ClearOutput" />
        protected override void ClearOutput()
        {
            residueSequences.UndefineValue();
            alphaCarbonSequences.UndefineValue();
        }
    }
}