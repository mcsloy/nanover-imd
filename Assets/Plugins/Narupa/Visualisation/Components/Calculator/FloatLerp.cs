using Narupa.Visualisation.Node.Calculator;

namespace Narupa.Visualisation.Components.Calculator
{
    public class FloatLerp : VisualisationComponent<FloatLerpNode>
    {
        public void Update()
        {
            node.Refresh();
        }
    }
}