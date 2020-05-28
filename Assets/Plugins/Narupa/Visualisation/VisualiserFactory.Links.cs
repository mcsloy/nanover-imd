using System.Collections.Generic;
using System.Linq;
using Narupa.Visualisation.Components;
using Narupa.Visualisation.Node.Input;
using Narupa.Visualisation.Node.Output;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Narupa.Visualisation
{
    public partial class VisualiserFactory
    {
        /// <summary>
        /// Try to find an input node with the given name on <paramref name="subgraph"/>
        /// </summary>
        private static bool HasSubgraphInput(GameObject subgraph, string name)
        {
            return subgraph.GetVisualisationNodes<IInputNode>()
                           .Any(node => node.Name == name);
        }

        private static IOutputNode GetSubgraphOutput(GameObject subgraph, string name)
        {
            return subgraph.GetVisualisationNodes<IOutputNode>()
                           .FirstOrDefault(node => node.Name == name);
        }
        
        private static IEnumerable<IInputNode> GetSubgraphInputs(GameObject subgraph)
        {
            return subgraph.GetVisualisationNodes<IInputNode>();
        }
        
        /// <summary>
        /// Find an <see cref="IDynamicPropertyProvider" /> present in a given <see cref="GameObject" />.
        /// </summary>
        private static IDynamicPropertyProvider GetLastAdaptorInSubgraph(GameObject existing)
        {
            return existing.GetVisualisationNodes<IDynamicPropertyProvider>().LastOrDefault();
        }

    }
}