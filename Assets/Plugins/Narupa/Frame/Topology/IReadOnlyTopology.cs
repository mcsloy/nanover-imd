using System.Collections.Generic;

namespace Narupa.Frame.Topology
{
    /// <summary>
    /// Description of an n-body system of particles, which are connected by bonds and
    /// grouped into residues and entities.
    /// </summary>
    public interface IReadOnlyTopology
    {
        /// <summary>
        /// The bonds between the particles in the system.
        /// </summary>
        IReadOnlyList<IReadOnlyBond> Bonds { get; }

        /// <summary>
        /// The particles in the system.
        /// </summary>
        IReadOnlyList<IReadOnlyParticle> Particles { get; }

        /// <summary>
        /// The residues (groups of particles) in the system.
        /// </summary>
        IReadOnlyList<IReadOnlyResidue> Residues { get; }

        /// <summary>
        /// The entities (groups of residues) in the system.
        /// </summary>
        IReadOnlyList<IReadOnlyEntity> Entities { get; }
    }
}