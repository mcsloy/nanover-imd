using System.Collections.Generic;

namespace Narupa.Frame.Topology
{
    public interface IReadOnlyEntity
    {
        int Index { get; }

        string Name { get; }

        IReadOnlyCollection<IReadOnlyResidue> Residues { get; }
    }
}