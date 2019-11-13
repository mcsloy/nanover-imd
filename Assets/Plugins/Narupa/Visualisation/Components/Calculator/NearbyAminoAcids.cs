namespace Narupa.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="Narupa.Visualisation.Node.Calculator.NearbyAminoAcids" />
    public class NearbyAminoAcids : VisualisationComponent<Node.Calculator.NearbyAminoAcids>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}