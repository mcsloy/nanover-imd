namespace Narupa.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="Narupa.Visualisation.Node.Calculator.HydrogenBonds" />
    public class HydrogenBonds : VisualisationComponent<Node.Calculator.HydrogenBonds>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}