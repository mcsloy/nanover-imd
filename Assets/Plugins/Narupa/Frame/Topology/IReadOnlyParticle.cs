using System.Collections.Generic;
using Narupa.Core.Science;
using UnityEngine;

namespace Narupa.Frame.Topology
{
    public interface IReadOnlyParticle
    {
        int Index { get; }

        string Type { get; }

        string Name { get; }

        Vector3 Position { get; }

        Element? Element { get; }

        IReadOnlyCollection<IReadOnlyBond> Bonds { get; }

        IReadOnlyCollection<IReadOnlyParticle> BondedParticles { get; }

        IReadOnlyResidue Residue { get; }
    }
}