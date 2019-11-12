using System.Collections.Generic;

namespace Narupa.Frame.Topology
{
    internal class ResidueReference : ObjectReference
    {
        public ResidueReference(Topology topology, int index) : base(topology, index)
        {
        }

        private readonly List<ParticleReference> particles = new List<ParticleReference>();

        public string Name => Frame.ResidueNames[Index];

        public IEnumerable<ParticleReference> Particles => particles;
        
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