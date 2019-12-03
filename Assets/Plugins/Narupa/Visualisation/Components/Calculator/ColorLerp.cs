using Narupa.Visualisation.Node.Calculator;

namespace Narupa.Visualisation.Components.Calculator
{
    public class ColorLerp : VisualisationComponent<ColorLerpNode>
    {
        public void Update()
        {
            node.Refresh();
        }
    }
}