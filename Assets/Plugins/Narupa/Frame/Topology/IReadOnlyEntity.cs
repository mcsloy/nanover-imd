using System.Collections.Generic;

namespace Narupa.Frame.Topology
{
    /// <summary>
    /// A group of residues (group of particles) in an n-body system, such as a peptide
    /// chain.
    /// </summary>
    public interface IReadOnlyEntity
    {
        /// <summary>
        /// The index of the entity in the topology.
        /// </summary>
        int Index { get; }

        /// <summary>
        /// The user-defined name of the entity.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The residues which belong in this entity.
        /// </summary>
        IReadOnlyCollection<IReadOnlyResidue> Residues { get; }
    }
}