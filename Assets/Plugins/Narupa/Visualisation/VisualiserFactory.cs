using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Core;
using Narupa.Core.Science;
using Narupa.Frame;
using Narupa.Visualisation.Components;
using Narupa.Visualisation.Components.Adaptor;
using Narupa.Visualisation.Node.Adaptor;
using Narupa.Visualisation.Node.Input;
using Narupa.Visualisation.Node.Protein;
using Narupa.Visualisation.Property;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Narupa.Visualisation
{
    /// <summary>
    /// Construction methods for creating visualisers from nested data structures.
    /// </summary>
    public partial class VisualiserFactory
    {
        /// <summary>
        /// Construct a visualiser from the provided arbitrary C# data.
        /// </summary>
        public static GameObject ConstructVisualiser(object data)
        {
            GameObject visualiser = null;
            
            var gameObject = new GameObject();

            if (data is string visName)
            {
                // A renderer specified by a single string is assumed
                // to be a predefined visualiser
                data = GetPredefinedVisualiser(visName);
            }

            if (data is Dictionary<string, object> dict)
            {
                // A dictionary indicates the visualiser should be created from
                // fields in dict
                var factory = new VisualiserFactory(dict, gameObject);
                visualiser = factory.visualiser;
            }

            return visualiser;
        }

        
        /// <summary>
        /// Generate a visualiser from a dictionary describing subgraphs and parameters.
        /// </summary>
        private VisualiserFactory(Dictionary<string, object> dict, GameObject gameObject)
        {
            visualiser = gameObject;
            rootParameters = dict;

            baseAdaptor = AddComponent<ParentedAdaptor>().Node;
            filterAdaptor = AddComponent<ParticleFilteredAdaptor>().Node;
            filterAdaptor.ParentAdaptor.Value = baseAdaptor;
            
            FindAllSubgraphsInRootDictionary();

            CheckIfSecondaryStructureIsRequired();

            InstantiateSubgraphs();

            ResolveSubgraphConnections();
        }

        private GameObject visualiser;

        private Dictionary<string, object> rootParameters;

        private Dictionary<GameObject, Dictionary<string, object>> subgraphParameters =
            new Dictionary<GameObject, Dictionary<string, object>>();

        private List<GameObject> subgraphs = new List<GameObject>();

        private ParentedAdaptorNode baseAdaptor;
        private ParticleFilteredAdaptorNode filterAdaptor;

        private T AddComponent<T>() where T : VisualisationComponent
        {
            return visualiser.AddComponent<T>();
        }
            
        public static readonly (string Key, Type Type) ResidueSecondaryStructure
            = ("residue.secondarystructures", typeof(SecondaryStructureAssignment[]));

        private void CheckIfSecondaryStructureIsRequired()
        {
            // If a subgraph requires secondary structure, then a secondary structure adaptor is inserted into the chain before the particle filter.
            if (subgraphs.Any(subgraph => HasSubgraphInput(subgraph,
                                                                        ResidueSecondaryStructure.Key)))
            {
                var secondaryStructureAdaptor = AddComponent<SecondaryStructureCalculator>().Node;
                secondaryStructureAdaptor.LinkToAdaptor(baseAdaptor);
            }
        }

        /// <summary>
        /// Go through each input node of each subgraph, attempting to resolve the value of the input node.
        /// </summary>
        private void ResolveSubgraphConnections()
        {
            var subgraphIndex = 0;
            foreach (var subgraph in subgraphs)
            {
                foreach (var input in GetSubgraphInputs(subgraph))
                {
                    ResolveSubgraphConnections(input, subgraph);
                }

                subgraphIndex++;
            }
        }

        private void ResolveSubgraphConnections(IInputNode input,
                                                GameObject subgraph)
        {
            var subgraphIndex = subgraphs.IndexOf(subgraph);

            // If for an input "some.input" there's a provided "some.input": "$new.input", change
            // the input name to "new.input"
            if (subgraphParameters.ContainsKey(subgraph) &&
                subgraphParameters[subgraph].ContainsKey(input.Name) &&
                subgraphParameters[subgraph][input.Name] is string replacement &&
                replacement.StartsWith("$"))
                input.Name = replacement.Substring(1);

            // Is the value provided specifically for this subgraph in its parameters
            if (subgraphParameters.ContainsKey(subgraph)
             && FindParameterAndSetInputNode(input, subgraphParameters[subgraph]))
                return;

            // It there a parameter in the root space which can provide a value
            if (FindParameterAndSetInputNode(input, rootParameters))
                return;

            // Is there an output node in a preceding graph which is relevant
            for (var i = subgraphIndex - 1; i >= 0; i--)
            {
                var precedingSubgraph = subgraphs[i];

                if (GetSubgraphOutput(precedingSubgraph, input.Name)?.Output is
                        IReadOnlyProperty output)
                {
                    input.Input.TrySetLinkedProperty(output);
                    return;
                }
            }

            // If there's a default value, that's okay
            if (input.Input.HasValue)
                return;

            // Look for adaptors further up the chain
            for (var i = subgraphIndex - 1; i >= 0; i--)
            {
                var precedingSubgraph = subgraphs[i];

                if (GetLastAdaptorInSubgraph(precedingSubgraph) is IDynamicPropertyProvider adaptor)
                {
                    var outputProperty = adaptor.GetOrCreateProperty(input.Name, input.InputType);
                    input.Input.TrySetLinkedProperty(outputProperty);
                    return;
                }
            }

            // Search for the key in the frame
            input.Input.TrySetLinkedProperty(filterAdaptor.GetOrCreateProperty(input.Name,
                                                                               input
                                                                                   .Input
                                                                                   .PropertyType));
        }

        private void InstantiateSubgraphs()
        {
            var newSubgraphs = new List<GameObject>();
            foreach (var subgraph in subgraphs)
            {
                var newSubgraph = Object.Instantiate(subgraph, visualiser.transform);
                if (subgraphParameters.ContainsKey(subgraph))
                    subgraphParameters[newSubgraph] = subgraphParameters[subgraph];
                newSubgraphs.Add(newSubgraph);
            }

            subgraphs = newSubgraphs;
        }
        
        /// <summary>
        /// For an input node <paramref name="input" />, look for a parameter in the
        /// dictionary <paramref name="parameters" /> with the key corresponding to the
        /// name of the node, and if present attempt to set the value of the input node to
        /// the value of the parameter. Returns true if this is successful, and false if
        /// there is no key in <paramref name="parameters" /> or the parameter value cannot
        /// be parsed.
        /// </summary>
        private static bool FindParameterAndSetInputNode(IInputNode input,
                                                         IReadOnlyDictionary<string, object>
                                                             parameters)
        {
            if (!parameters.TryGetValue(input.Name, out var parameter))
                return false;

            switch (input)
            {
                case GradientInputNode gradientInput:
                    if (VisualisationParser.TryParseGradient(parameter, out var gradient))
                    {
                        gradientInput.Input.Value = gradient;
                        return true;
                    }

                    break;

                case ColorInputNode colorInput:
                    if (VisualisationParser.TryParseColor(parameter, out var color))
                    {
                        colorInput.Input.Value = color;
                        return true;
                    }

                    break;

                case FloatInputNode floatInput:
                    if (VisualisationParser.TryParseFloat(parameter, out var @float))
                    {
                        floatInput.Input.Value = @float;
                        return true;
                    }

                    break;

                case ElementColorMappingInputNode mappingInput:
                    if (VisualisationParser.TryParseElementColorMapping(parameter, out var mapping))
                    {
                        mappingInput.Input.Value = mapping;
                        return true;
                    }

                    break;
            }

            return false;
        }

        /// <summary>
        /// Key in a visualiser subgraph that indicates the subgraph to use.
        /// </summary>
        private static string TypeKeyword = "type";

        /// <summary>
        /// Key in the root visualiser dictionary that indicates a sequence calculator.
        /// Will default to <see cref="DefaultSequenceSubgraph" /> if not provided and a
        /// sequence is required.
        /// </summary>
        private const string SequenceKeyword = "sequence";

        /// <summary>
        /// Key in the root visualiser that can indicate either a subgraph (as a name or a
        /// dict with a type field) or an actual color.
        /// </summary>
        private const string ColorKeyword = "color";

        /// <summary>
        /// Key in the root visualiser that can indicate either a subgraph (as a name or a
        /// dict with a type field) or an actual scale.
        /// </summary>
        private const string ScaleKeyword = "scale";

        /// <summary>
        /// Key in the root visualiser that can indicate either a subgraph (as a name or a
        /// dict with a type field) or an actual width.
        /// </summary>
        private const string WidthKeyword = "width";
        
        /// <summary>
        /// Key in the root visualiser that can indicate either a subgraph (as a name or a
        /// dict with a type field) or an actual color.
        /// </summary>
        private const string RenderKeyword = "render";

        /// <summary>
        /// The default render subgraph to use if one is not provided.
        /// </summary>
        private const string DefaultRenderSubgraph = "ball and stick";

        /// <summary>
        /// The key for sequence lengths. This indicates that a sequence subgraph is
        /// required earlier in the chain to provide this.
        /// </summary>
        public const string SequenceLengthsKey = "sequence.lengths";

        /// <summary>
        /// The default subgraph for generating sequences.
        /// </summary>
        private const string DefaultSequenceSubgraph = "entities";
    }
}