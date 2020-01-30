using Narupa.Visualisation.Node.Calculator;

namespace Narupa.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="VectorMagnitudeNode" />
    public class VectorMagnitude : VisualisationComponent<VectorMagnitudeNode>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}