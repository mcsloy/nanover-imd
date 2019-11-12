using System.Collections.Generic;

namespace Narupa.Frame.Topology
{
    public interface IReadOnlyTopology
    {
        IReadOnlyList<IReadOnlyBond> Bonds { get; }

        IReadOnlyList<IReadOnlyParticle> Particles { get; }

        IReadOnlyList<IReadOnlyResidue> Residues { get; }

        IReadOnlyList<IReadOnlyEntity> Entities { get; }
    }
}