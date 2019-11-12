using System.Collections.Generic;

namespace Narupa.Frame.Topology
{
    /// <summary>
    /// An entity (group of residues) derived from data provided in a
    /// <see cref="Frame" />.
    /// </summary>
    internal class FrameEntity : FrameObject, IReadOnlyEntity
    {
        /// <inheritdoc cref="IReadOnlyEntity.Residues" />
        IReadOnlyCollection<IReadOnlyResidue> IReadOnlyEntity.Residues => residues;

        /// <inheritdoc cref="IReadOnlyEntity.Name" />
        string IReadOnlyEntity.Name => Name;

        /// <inheritdoc cref="IReadOnlyEntity.Index" />
        int IReadOnlyEntity.Index => Index;

        internal FrameEntity(FrameTopology topology, int index) : base(topology, index)
        {
        }

        private List<FrameResidue> residues = new List<FrameResidue>();

        /// <inheritdoc cref="IReadOnlyEntity.Name" />
        public string Name => Frame.ResidueNames[Index];

        /// <summary>
        /// Add the given residue to this entity.
        /// </summary>
        internal void AddResidue(FrameResidue i)
        {
            residues.Add(i);
        }

        /// <summary>
        /// Clear all residues from this entity.
        /// </summary>
        internal void ClearResidues()
        {
            residues.Clear();
        }
    }
}