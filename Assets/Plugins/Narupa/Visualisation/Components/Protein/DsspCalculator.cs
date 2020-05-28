using Narupa.Visualisation.Node.Protein;

namespace Narupa.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="DsspCalculatorNode" />
    public class DsspCalculator : VisualisationComponent<DsspCalculatorNode>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}