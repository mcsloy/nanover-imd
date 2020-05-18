using Narupa.Core.Math;

namespace Narupa.Frontend.Manipulation
{
    public interface IInteractableParticles
    {
        InteractionParticleGrab GetParticleGrab(Transformation grabber);
    }
}