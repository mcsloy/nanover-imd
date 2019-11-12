using System.Collections.Generic;

namespace Narupa.Frame.Topology
{
    /// <summary>
    /// A collection of related particles in an n-body system.
    /// </summary>
    public interface IReadOnlyResidue
    {
        /// <summary>
        /// The index of this residue in the topology.
        /// </summary>
        int Index { get; }

        /// <summary>
        /// The user-defined name of this residue.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The entity (group of residues) to which this residue belongs to.
        /// </summary>
        IReadOnlyEntity Entity { get; }

        /// <summary>
        /// The particles which are members of this residue.
        /// </summary>
        IReadOnlyList<IReadOnlyParticle> Particles { get; }

        /// <summary>
        /// Get a particle by the given user-defined name.
        /// </summary>
        IReadOnlyParticle FindParticleByName(string name);
    }
}