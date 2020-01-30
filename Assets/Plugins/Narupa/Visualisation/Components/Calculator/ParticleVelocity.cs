using Narupa.Visualisation.Node.Calculator;

namespace Narupa.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="ParticleVelocityNode" />
    public class ParticleVelocity : VisualisationComponent<ParticleVelocityNode>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}