using Narupa.Visualisation.Node.Protein;

namespace Narupa.Visualisation.Components.Adaptor
{
    /// <inheritdoc cref="SecondaryStructureCalculatorNode"/>
    public class SecondaryStructureCalculator : VisualisationComponent<SecondaryStructureCalculatorNode>
    {
        private void Update()
        {
            Node.Refresh();
        }
    }
}