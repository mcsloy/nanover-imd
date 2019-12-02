using Narupa.Visualisation.Node.Color;

namespace Narupa.Visualisation.Components.Color
{
    /// <inheritdoc cref="SecondaryStructureColorNode" />
    public class SecondaryStructureColor :
        VisualisationComponent<SecondaryStructureColorNode>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}