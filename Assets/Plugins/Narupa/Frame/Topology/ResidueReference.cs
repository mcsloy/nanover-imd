using System.Collections.Generic;

namespace Narupa.Frame.Topology
{
    internal class ResidueReference : ObjectReference, IReadOnlyResidue
    {
        int IReadOnlyResidue.Index => Index;

        string IReadOnlyResidue.Name => Name;

        IReadOnlyEntity IReadOnlyResidue.Entity => Entity;

        IReadOnlyList<IReadOnlyParticle> IReadOnlyResidue.Particles => particles;

        public ResidueReference(FrameTopology topology, int index) : base(topology, index)
        {
        }

        private readonly List<ParticleReference> particles = new List<ParticleReference>();

        public string Name => Frame.ResidueNames[Index];

        public EntityReference Entity { get; private set; }

        public void AddParticle(ParticleReference i)
        {
            particles.Add(i);
        }

        public void SetEntity(EntityReference entity)
        {
            Entity = entity;
        }

        public void ClearParticles()
        {
            particles.Clear();
        }
    }
}