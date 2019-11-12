using System.Collections.Generic;
using System.Linq;

namespace Narupa.Frame.Topology
{
    public class ResidueReference
    {
        private readonly Frame frame;

        private readonly int index;
        
        private List<int> particleIndices = new List<int>();

        public ResidueReference(Frame frame, int index)
        {
            this.frame = frame;
            this.index = index;
        }

        public string Name => frame.ResidueNames[index];

        public IEnumerable<ParticleReference> Particles =>
            particleIndices.Select(frame.GetParticle);

        public void AddParticleIndex(int i)
        {
            particleIndices.Add(i);
        }
    }
}