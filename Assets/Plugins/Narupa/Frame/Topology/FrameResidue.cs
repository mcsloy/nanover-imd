using System.Collections.Generic;

namespace Narupa.Frame.Topology
{
    /// <summary>
    /// An entity (group of residues) derived from data provided in a
    /// <see cref="Frame" />.
    /// </summary>
    internal class FrameResidue : FrameObject, IReadOnlyResidue
    {
        /// <inheritdoc cref="IReadOnlyResidue.Index" />
        int IReadOnlyResidue.Index => Index;

        /// <inheritdoc cref="IReadOnlyResidue.Entity" />
        IReadOnlyEntity IReadOnlyResidue.Entity => Entity;

        /// <inheritdoc cref="IReadOnlyResidue.Particles" />
        IReadOnlyList<IReadOnlyParticle> IReadOnlyResidue.Particles => particles;

        /// <inheritdoc cref="IReadOnlyResidue.FindParticleByName" />
        public IReadOnlyParticle FindParticleByName(string name)
        {
            return particlesByName[name];
        }

        internal FrameResidue(FrameTopology topology, int index) : base(topology, index)
        {
        }

        private readonly List<FrameParticle> particles = new List<FrameParticle>();
        private readonly Dictionary<string, FrameParticle> particlesByName = new Dictionary<string, FrameParticle>();

        /// <inheritdoc cref="IReadOnlyResidue.Name" />
        public string Name => Frame.ResidueNames[Index];

        /// <inheritdoc cref="IReadOnlyResidue.Entity" />
        public FrameEntity Entity { get; private set; }

        /// <summary>
        /// Add a particle to this residue.
        /// </summary>
        public void AddParticle(FrameParticle i)
        {
            particles.Add(i);
            particlesByName[i.Name] = i;
        }

        /// <summary>
        /// Set the containing entity for this residue.
        /// </summary>
        public void SetEntity(FrameEntity entity)
        {
            Entity = entity;
        }

        /// <summary>
        /// Remove all particles from this residue.
        /// </summary>
        public void ClearParticles()
        {
            particles.Clear();
            particlesByName.Clear();
        }
    }
}