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

    public interface IReadOnlyBond
    {
        int Index { get; }
        
        int Order { get; }
        
        IReadOnlyParticle A { get; }
        
        IReadOnlyParticle B { get; }
    }

    public interface IReadOnlyResidue
    {
        int Index { get; }
        
        string Name { get; }
        
        IReadOnlyEntity Entity { get; }
    }

    public interface IReadOnlyEntity
    {
        int Index { get; }
        
        string Name { get; }
        
        IReadOnlyCollection<IReadOnlyResidue> Residues { get; }
    }

    public interface IReadOnlyTopology
    {
        IReadOnlyList<IReadOnlyBond> Bonds { get; }
        
        IReadOnlyList<IReadOnlyParticle> Particles { get; }
        
        IReadOnlyList<IReadOnlyResidue> Residues { get; }
        
        IReadOnlyList<IReadOnlyEntity> Entities { get; }
    }
    
}