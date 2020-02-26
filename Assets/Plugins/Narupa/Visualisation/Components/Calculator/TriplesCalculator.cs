namespace Narupa.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="Narupa.Visualisation.Node.Calculator.TriplesCalculator" />
    public class TriplesCalculator : VisualisationComponent<Node.Calculator.TriplesCalculator>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}