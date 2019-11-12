namespace Narupa.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="Narupa.Visualisation.Node.Calculator.HydrophobicBonds" />
    public class HydrophobicBonds : VisualisationComponent<Node.Calculator.HydrophobicBonds>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}