using System;
using System.Linq;

namespace Narupa.Frame.Topology
{
    public class Topology
    {
        private readonly Frame frame;

        public Topology(Frame frame)
        {
            this.frame = frame;
        }

        public Frame Frame => frame;
        
        private ResidueReference[] residues = new ResidueReference[0];
        
        private ParticleReference[] particles = new ParticleReference[0];
        
        private EntityReference[] entities = new EntityReference[0];
        
        private BondReference[] bonds = new BondReference[0];

        internal ParticleReference GetParticle(int index)
        {
            return particles[index];
        }

        internal ResidueReference GetResidue(int index)
        {
            return residues[index];
        }
        
        internal EntityReference GetEntity(int index)
        {
            return entities[index];
        }

        public void ResetTopology()
        {
            var bondCount = Frame.Bonds.Count;
            var particleCount = Frame.ParticlePositions.Length;
            var residueCount = Frame.ParticleResidues.Max() + 1;
            var entityCount = Frame.ResidueEntities.Max() + 1;
            Array.Resize(ref bonds, bondCount);
            Array.Resize(ref particles, particleCount);
            Array.Resize(ref residues, residueCount);
            Array.Resize(ref entities, entityCount);
            for(var i = 0; i < bondCount; i++)
                bonds[i] = new BondReference(this, i);
            for(var i = 0; i < particleCount; i++)
                particles[i] = new ParticleReference(this, i);
            for(var i = 0; i < residueCount; i++)
                residues[i] = new ResidueReference(this, i);
            for(var i = 0; i < entityCount; i++)
                entities[i] = new EntityReference(this, i);
        }

        public void AssignReferences()
        {
            AssignBonds();
            AssignParticleResidues();
            AssignResidueEntities();
        }
        
        public void AssignBonds()
        {
            foreach (var particle in particles)
                particle.ClearBonds();
            
            for (var i = 0; i < bonds.Length; i++)
            {
                var bond = bonds[i];
                var bondPair = Frame.BondPairs[i];
                var particleA = particles[bondPair.A];
                var particleB = particles[bondPair.B];
                bond.AssignParticles(particleA, particleB);
                particleA.AddBond(bond);
                particleB.AddBond(bond);
            }
        }

        public void AssignParticleResidues()
        {
            foreach (var residue in residues)
                residue.ClearParticles();
            
            for (var i = 0; i < particles.Length; i++)
            {
                var particle = particles[i];
                var residue = residues[frame.ParticleResidues[i]];
                residue.AddParticle(particle);
                particle.SetResidue(residue);
            }
        }
        
        public void AssignResidueEntities()
        {
            foreach (var entity in entities)
                entity.ClearResidues();
            
            for (var i = 0; i < residues.Length; i++)
            {
                var residue = residues[i];
                var entity = entities[frame.ResidueEntities[i]];
                entity.AddResidueIndex(residue);
                residue.SetEntity(entity);
            }
        }
    }
}