using System.Collections.Generic;

namespace Narupa.Frame.Topology
{
    public interface IReadOnlyResidue
    {
        int Index { get; }

        string Name { get; }

        IReadOnlyEntity Entity { get; }

        IReadOnlyList<IReadOnlyParticle> Particles { get; }
    }
}