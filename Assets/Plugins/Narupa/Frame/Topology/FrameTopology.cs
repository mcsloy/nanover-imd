using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Frame.Event;
using Narupa.Protocol.Trajectory;

namespace Narupa.Frame.Topology
{
    /// <summary>
    /// A representation of the n-body system derived from data provided in a
    /// <see cref="Frame" />.
    /// </summary>
    public class FrameTopology : IReadOnlyTopology
    {
        /// <inheritdoc cref="IReadOnlyTopology.Bonds" />
        IReadOnlyList<IReadOnlyBond> IReadOnlyTopology.Bonds => bonds;

        /// <inheritdoc cref="IReadOnlyTopology.Particles" />
        IReadOnlyList<IReadOnlyParticle> IReadOnlyTopology.Particles => particles;

        /// <inheritdoc cref="IReadOnlyTopology.Residues" />
        IReadOnlyList<IReadOnlyResidue> IReadOnlyTopology.Residues => residues;

        /// <inheritdoc cref="IReadOnlyTopology.Entities" />
        IReadOnlyList<IReadOnlyEntity> IReadOnlyTopology.Entities => entities;

        /// <summary>
        /// Create an empty topology.
        /// </summary>
        public FrameTopology()
        {
            
        }
        
        
        /// <summary>
        /// Create a topology representation of a given frame.
        /// </summary>
        public FrameTopology(Frame frame)
        {
            OnFrameUpdate(frame, null);
        }

        /// <summary>
        /// The frame that this topology is derived form.
        /// </summary>
        public Frame Frame { get; private set; }

        private FrameResidue[] residues = new FrameResidue[0];

        private FrameParticle[] particles = new FrameParticle[0];

        private FrameEntity[] entities = new FrameEntity[0];

        private FrameBond[] bonds = new FrameBond[0];

        /// <summary>
        /// Reset all arrays and fill them with blank references.
        /// </summary>
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
            for (var i = 0; i < bondCount; i++)
                bonds[i] = new FrameBond(this, i);
            for (var i = 0; i < particleCount; i++)
                particles[i] = new FrameParticle(this, i);
            for (var i = 0; i < residueCount; i++)
                residues[i] = new FrameResidue(this, i);
            for (var i = 0; i < entityCount; i++)
                entities[i] = new FrameEntity(this, i);
        }

        /// <summary>
        /// Update the references for all the parts of the topology.
        /// </summary>
        public void UpdateTopology()
        {
            AssignBonds();
            AssignParticleResidues();
            AssignResidueEntities();
        }

        /// <summary>
        /// Assign bonds between particles.
        /// </summary>
        private void AssignBonds()
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

        /// <summary>
        /// Assign particles to their residues.
        /// </summary>
        private void AssignParticleResidues()
        {
            foreach (var residue in residues)
                residue.ClearParticles();

            for (var i = 0; i < particles.Length; i++)
            {
                var particle = particles[i];
                var residue = residues[Frame.ParticleResidues[i]];
                residue.AddParticle(particle);
                particle.SetResidue(residue);
            }
        }

        /// <summary>
        /// Assign residues to their entities.
        /// </summary>
        private void AssignResidueEntities()
        {
            foreach (var entity in entities)
                entity.ClearResidues();

            for (var i = 0; i < residues.Length; i++)
            {
                var residue = residues[i];
                var entity = entities[Frame.ResidueEntities[i]];
                entity.AddResidue(residue);
                residue.SetEntity(entity);
            }
        }

        /// <summary>
        /// Update the topology based on frame changes.
        /// </summary>
        public void OnFrameUpdate(Frame frame, FrameChanges changes)
        {
            Frame = frame;
            if (changes == null
             || changes.GetIsChanged(FrameData.BondArrayKey)
             || changes.GetIsChanged(FrameData.ParticleResidueArrayKey)
             || changes.GetIsChanged(FrameData.ResidueChainArrayKey))
            {
                ResetTopology();
                UpdateTopology();
            }
        }
    }
}