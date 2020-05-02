using Narupa.Visualisation.Node.Input;
using Narupa.Visualisation.Node.Output;

namespace Narupa.Visualisation.Components
{
    public static class VisualisationSubgraphExtensions
    {
        public static IInputNode<TType> GetInputNode<TType>(
            this VisualisationSubgraph subgraph,
            string name)
        {
            foreach (var node in subgraph.Nodes)
            {
                if (node is IInputNode<TType> input && input.Name.Equals(name))
                    return input;
            }

            return null;
        }

        public static IOutputNode<TType> GetOutputNode<TType>(this VisualisationSubgraph subgraph,
                                                              string name)
        {
            foreach (var node in subgraph.Nodes)
            {
                if (node is IOutputNode<TType> output && output.Name.Equals(name))
                    return output;
            }

            return null;
        }

        public static IOutputNode GetOutputNode(this VisualisationSubgraph subgraph,
                                                string name)
        {
            foreach (var node in subgraph.Nodes)
            {
                if (node is IOutputNode output && output.Name.Equals(name))
                    return output;
            }

            return null;
        }
    }
}