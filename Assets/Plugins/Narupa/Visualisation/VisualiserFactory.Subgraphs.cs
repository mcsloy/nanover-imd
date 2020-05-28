using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Core;
using UnityEngine;

namespace Narupa.Visualisation
{
    public partial class VisualiserFactory
    {
        /// <summary>
        /// Find all the subgraphs that are defined in the root dictionary.
        /// </summary>
        private void FindAllSubgraphsInRootDictionary()
        {
            var sequenceSubgraph = FindSubgraphInRootDictionary(SequenceKeyword, GetSequenceSubgraph);
            
            FindSubgraphInRootDictionary(ColorKeyword, GetColorSubgraph);

            FindSubgraphInRootDictionary(WidthKeyword, GetWidthSubgraph);

            FindSubgraphInRootDictionary(ScaleKeyword, GetScaleSubgraph);

            subgraphs.Add(GetColorSubgraph("color pulser"));
            
            var renderSubgraph = FindSubgraphInRootDictionary(RenderKeyword, GetRenderSubgraph);
            if (renderSubgraph == null)
                subgraphs.Add(GetRenderSubgraph(DefaultRenderSubgraph));

            // If a subgraph requires a set of sequence lengths, a sequence provider is required.
            // If one hasn't already been provided, the default is one that generates sequences
            // based on entities.
            if (sequenceSubgraph == null
             && subgraphs.Any(subgraph => HasSubgraphInput(subgraph, SequenceLengthsKey)))
            {
                var subgraph = GetSequenceSubgraph(DefaultSequenceSubgraph);
                subgraphs.Insert(0, subgraph);
            }
        }
        
        /// <summary>
        /// Look in <paramref name="dict"/> to see if it contains key. If it does and its a string,
        /// see if a subgraph with that name exists (using <paramref name="findSubgraph"/> and
        /// return it. If it's a dictionary with a 'type' key, look up a subgraph with this id,
        /// and treat the rest of the dictionary as parameters.
        /// </summary>
        /// <param name="dict">The root dictionary in which the key could be found</param>
        /// <param name="key">The key that defines this kind of subgraph</param>
        /// <param name="findSubgraph">A function to find a subgraph with the given name</param>
        private static (GameObject subgraph, Dictionary<string, object> parameters)
            GetSubgraph(
                Dictionary<string, object> dict,
                string key,
                Func<string, GameObject> findSubgraph)
        {
            if (dict.TryGetValue<Dictionary<string, object>>(key, out var strut))
            {
                if (strut.TryGetValue<string>(TypeKeyword, out var type))
                {
                    var subgraph = findSubgraph(type);
                    if (subgraph != null)
                    {
                        return (subgraph, strut);
                    }
                }
            }
            else if (dict.TryGetValue<string>(key, out var t))
            {
                var subgraph = findSubgraph(t);
                if (subgraph != null)
                {
                    return (subgraph, null);
                }
            }

            return (null, null);
        }

        /// <summary>
        /// Add the subgraph and its parameters to the local list of them
        /// </summary>
        private GameObject AddSubgraph(GameObject subgraph,
                                       Dictionary<string, object> parameters)
        {
            subgraphs.Add(subgraph);
            if (parameters != null)
                subgraphParameters.Add(subgraph, parameters);
            return subgraph;
        }

        /// <summary>
        /// Find a subgraph with the given key, and if found add it to our list of subgraphs.
        /// </summary>
        private GameObject FindSubgraphInRootDictionary(string key,
                                                        Func<string, GameObject> findSubgraph)
        {
            var (subgraph, parameters) = GetSubgraph(rootParameters, key, findSubgraph);
            return subgraph == null ? null : AddSubgraph(subgraph, parameters);
        }

        /// <summary>
        /// Get a visualisation subgraph which is responsible for rendering information.
        /// </summary>
        private static GameObject GetRenderSubgraph(string name)
        {
            return Resources.Load<GameObject>($"{RenderSubgraphPath}/{name}");
        }

        /// <summary>
        /// Get a visualisation subgraph which is responsible for coloring particles.
        /// </summary>
        private static GameObject GetColorSubgraph(string name)
        {
            return Resources.Load<GameObject>($"{ColorSubgraphPath}/{name}");
        }

        /// <summary>
        /// Get a visualisation subgraph which is responsible for the scale of particles.
        /// </summary>
        private static GameObject GetScaleSubgraph(string name)
        {
            return Resources.Load<GameObject>($"{ScaleSubgraphPath}/{name}");
        }
        
        /// <summary>
        /// Get a visualisation subgraph which is responsible for the width of particles in
        /// splines.
        /// </summary>
        private static GameObject GetWidthSubgraph(string name)
        {
            return Resources.Load<GameObject>($"{WidthSubgraphPath}/{name}");
        }

        /// <summary>
        /// Get a visualisation subgraph which is responsible for calculating sequences.
        /// </summary>
        private static GameObject GetSequenceSubgraph(string name)
        {
            return Resources.Load<GameObject>($"{SequenceSubgraphPath}/{name}");
        }
        
        /// <summary>
        /// Path in the resources folder(s) where color subgraphs exist
        /// </summary>
        private const string ColorSubgraphPath = "Subgraph/Color";

        /// <summary>
        /// Path in the resources folder(s) where color subgraphs exist
        /// </summary>
        private const string RenderSubgraphPath = "Subgraph/Render";

        /// <summary>
        /// Path in the resources folder(s) where color subgraphs exist
        /// </summary>
        private const string ScaleSubgraphPath = "Subgraph/Scale";

        /// <summary>
        /// Path in the resources folder(s) where color subgraphs exist
        /// </summary>
        private const string WidthSubgraphPath = "Subgraph/Width";

        /// <summary>
        /// Path in the resources folder(s) where color subgraphs exist
        /// </summary>
        private const string SequenceSubgraphPath = "Subgraph/Sequence";
    }
}