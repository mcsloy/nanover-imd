using System.Collections.Generic;
using System.Linq;
using Narupa.Grpc.Interactive;

namespace Narupa.Frontend.Manipulation
{
    public class InteractionParticleGrab : ActiveParticleGrab
    {
        public Interaction Interaction { get; }
        
        public InteractionParticleGrab(IEnumerable<int> particleIndices) : base(particleIndices)
        {
            Interaction = new Interaction
            {
                InteractionId = Id,
                Particles = ParticleIndices.ToList()
            };
        }
    }
}