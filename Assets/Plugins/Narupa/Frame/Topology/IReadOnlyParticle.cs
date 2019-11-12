using System.Collections.Generic;
using Narupa.Core.Science;
using UnityEngine;

namespace Narupa.Frame.Topology
{
    /// <summary>
    /// A particle in an n-body system, such as an atom.
    /// </summary>
    public interface IReadOnlyParticle
    {
        /// <summary>
        /// The index of the particle in the topology.
        /// </summary>
        int Index { get; }

        /// <summary>
        /// A user-defined type of the particle.
        /// </summary>
        string Type { get; }

        /// <summary>
        /// A user-defined name of the particle.
        /// </summary>
        /// <remarks>
        /// This should be unique in the residue this particle belongs to.
        /// </remarks>
        string Name { get; }

        /// <summary>
        /// The position of the particle.
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// The atomic element of the particle, or null if it is not an atom.
        /// </summary>
        Element? Element { get; }

        /// <summary>
        /// The set of bonds which involve this particle.
        /// </summary>
        IReadOnlyCollection<IReadOnlyBond> Bonds { get; }

        /// <summary>
        /// The set of particles which are bonded to this particle.
        /// </summary>
        IReadOnlyCollection<IReadOnlyParticle> BondedParticles { get; }

        /// <summary>
        /// The residue to which this particle belongs.
        /// </summary>
        IReadOnlyResidue Residue { get; }
    }
}