using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Narupa.Core;
using Narupa.Core.Science;
using Narupa.Visualisation.Components;
using Narupa.Visualisation.Components.Adaptor;
using Narupa.Visualisation.Components.Input;
using Narupa.Visualisation.Node.Color;
using Narupa.Visualisation.Node.Input;
using UnityEngine;

namespace NarupaIMD.Selection
{
    /// <summary>
    /// Construction methods for creating visualisers from nested data structures.
    /// </summary>
    public static class VisualiserFactory
    {
        /// <summary>
        /// Parse a gradient from a generic C# object. Possible values include a Unity
        /// gradient or a list of items which can be interpreted as colors using
        /// <see cref="TryParseColor" />.
        /// </summary>
        private static bool TryParseGradient(object value, out Gradient gradient)
        {
            gradient = null;

            switch (value)
            {
                // Object is already a Gradient
                case Gradient g:
                    gradient = g;
                    return true;

                // Object is a list of colors
                case IReadOnlyList<object> list:
                {
                    var colors = new List<Color>();

                    // Parse each item of the list as a color
                    foreach (var item in list)
                    {
                        if (!TryParseColor(item, out var color))
                            return false;
                        colors.Add(color);
                    }

                    // Construct a gradient from two or more colors
                    if (colors.Count >= 2)
                    {
                        gradient = new Gradient();
                        gradient.SetKeys(
                            colors.Select((c, i) => new GradientColorKey(
                                              c, (float) i / (colors.Count - 1)))
                                  .ToArray(),
                            new[]
                            {
                                new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1)
                            });
                        return true;
                    }

                    break;
                }
            }

            return false;
        }

        /// <summary>
        /// Parse an <see cref="IMapping{TFrom,TTo}" /> from a C# object.
        /// </summary>
        private static bool TryParseElementColorMapping(object value,
                                                        out IMapping<Element, Color> mapping)
        {
            mapping = null;

            switch (value)
            {
                // Object is already a mapping of elements to colors
                case IMapping<Element, Color> actualValue:
                    mapping = actualValue;
                    return true;

                // Object is a string name of a predefined mapping
                case string str:
                    mapping = Resources.Load<ElementColorMapping>($"CPK/{str}");
                    return true;

                // Object is a dictionary, to be interpreted as element symbols to colors
                case Dictionary<string, object> dict:
                {
                    var mappingDictionary = new Dictionary<Element, Color>();
                    foreach (var (key, val) in dict)
                        if (TryParseElement(key, out var element)
                         && TryParseColor(val, out var color))
                            mappingDictionary.Add(element, color);
                        else
                            return false;

                    mapping = mappingDictionary.AsMapping();
                    return true;
                }

                default:
                    return false;
            }
        }

        /// <summary>
        /// Parse an element from either a string symbol or an integer atomic number.
        /// </summary>
        public static bool TryParseElement(object value, out Element element)
        {
            element = Element.Virtual;

            switch (value)
            {
                // Object is potentially an atomic symbol
                case string str:
                {
                    var potentialElement = ElementSymbols.GetFromSymbol(str);
                    if (potentialElement.HasValue)
                    {
                        element = potentialElement.Value;
                        return true;
                    }

                    break;
                }

                // Object is an atomic number
                case int atomicNumber:
                    if (atomicNumber >= 1 && atomicNumber <= 118)
                    {
                        element = (Element) atomicNumber;
                        return true;
                    }

                    break;

                // Object is an atomic number, but in float form
                case double d:
                {
                    var atomicNumber = (int) d;
                    if (atomicNumber >= 1 && atomicNumber <= 118)
                    {
                        element = (Element) atomicNumber;
                        return true;
                    }

                    break;
                }
            }

            return false;
        }

        /// <summary>
        /// Regex for parsing strings such as E4F924, #24fac8 and 0x2Ef9e2
        /// </summary>
        private const string RegexRgbHex =
            @"^(?:#|0x)?([A-Fa-f0-9][A-Fa-f0-9])([A-Fa-f0-9][A-Fa-f0-9])([A-Fa-f0-9][A-Fa-f0-9])$";

        /// <summary>
        /// Attempt to parse a color, from a name, hex code or array of rgba values.
        /// </summary>
        public static bool TryParseColor(object value, out Color color)
        {
            color = Color.white;

            switch (value)
            {
                // Object is already a color
                case Color c:
                    color = c;
                    return true;

                // Object could be either a predefined color or a hex string
                case string str:
                {
                    if (Colors.GetColorFromName(str) is Color c)
                    {
                        color = c;
                        return true;
                    }

                    // Match hex color
                    var match = Regex.Match(str, RegexRgbHex);

                    if (match.Success)
                    {
                        color = new Color(int.Parse(match.Groups[1].Value,
                                                    NumberStyles.HexNumber) / 255f,
                                          int.Parse(match.Groups[2].Value,
                                                    NumberStyles.HexNumber) / 255f,
                                          int.Parse(match.Groups[3].Value,
                                                    NumberStyles.HexNumber) / 255f,
                                          1);
                        return true;
                    }

                    break;
                }

                // Object could be a list of RGB values
                case IReadOnlyList<object> list
                    when list.Count == 3 && list.All(item => item is double):
                    color = new Color((float) (double) list[0],
                                      (float) (double) list[1],
                                      (float) (double) list[2]);
                    return true;

                // Object could be a list of RGBA values
                case IReadOnlyList<object> list
                    when list.Count == 4 && list.All(item => item is double):
                    color = new Color((float) (double) list[0],
                                      (float) (double) list[1],
                                      (float) (double) list[2],
                                      (float) (double) list[3]);
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Attempt to parse a float from a generic object.
        /// </summary>
        private static bool TryParseFloat(object value, out float number)
        {
            number = default;

            switch (value)
            {
                // Object is a float
                case float flt:
                    number = flt;
                    return true;

                // Object is a double
                case double dbl:
                    number = (float) dbl;
                    return true;

                // Object is an int
                case int it:
                    number = it;
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Get a prefab of a predefined visualiser with the given name.
        /// </summary>
        public static GameObject GetPredefinedVisualiser(string name)
        {
            return Resources.Load<GameObject>($"Visualiser/Prefab/{name}");
        }

        /// <summary>
        /// Get a visualisation subgraph which is responsible for rendering information.
        /// </summary>
        public static GameObject GetRenderSubgraph(string name)
        {
            return Resources.Load<GameObject>($"Subgraph/Render/{name}");
        }

        /// <summary>
        /// Get a visualisation subgraph which is responsible for coloring particles.
        /// </summary>
        public static GameObject GetColorSubgraph(string name)
        {
            return Resources.Load<GameObject>($"Subgraph/Color/{name}");
        }

        /// <summary>
        /// Get a visualisation subgraph which is responsible for calculating sequences.
        /// </summary>
        public static GameObject GetSequenceSubgraph(string name)
        {
            return Resources.Load<GameObject>($"Subgraph/Sequence/{name}");
        }

        /// <summary>
        /// Construct a visualiser from the provided arbitrary C# data.
        /// </summary>
        public static (GameObject visualiser, bool isPrefab) ConstructVisualiser(object data)
        {
            GameObject visualiser = null;
            var prefab = true;

            if (data is string visName)
            {
                // A renderer specified by a single string is assumed
                // to be a predefined visualiser
                visualiser = GetPredefinedVisualiser(visName);
            }
            else if (data is Dictionary<string, object> dict)
            {
                // Create a basic dynamic visualiser, with a FrameAdaptor to
                // read in frames and a DynamicVisualiserSubgraphs to contain the dynamic nodes
                visualiser = new GameObject();

                var adaptor = visualiser.AddComponent<FilteredAdaptor>();

                var dynamic = visualiser.AddComponent<DynamicVisualiserSubgraphs>();

                dynamic.FrameAdaptor = adaptor;

                // Set of subgraphs to be used in the visualiser
                var subgraphs = new List<GameObject>();

                // Mapping of subgraph object to its dictionary representation
                var subgraphParameters = new Dictionary<GameObject, Dictionary<string, object>>();

                // The root dictionary
                var globalParameters = dict;

                bool GetSubgraph(string key, Func<string, GameObject> findSubgraph)
                {
                    if (dict.TryGetValue<Dictionary<string, object>>(key, out var strut))
                    {
                        if (strut.TryGetValue<string>("type", out var type))
                        {
                            var subgraph = findSubgraph(type);
                            if (subgraph != null)
                            {
                                subgraphs.Add(subgraph);
                                subgraphParameters.Add(subgraph, strut);
                                return true;
                            }
                        }
                    }
                    else if (dict.TryGetValue<string>(key, out var t))
                    {
                        var subgraph = findSubgraph(t);
                        if (subgraph != null)
                        {
                            subgraphs.Add(subgraph);
                            return true;
                        }
                    }

                    return false;
                }

                var subgraphIndex = subgraphs.Count;
                // Parse the sequence subgraph
                var hasSequenceSubgraph = GetSubgraph("sequence", GetSequenceSubgraph);


                // Parse the color keyword if it is a struct with the 'type' field, and hence
                // describes a color subgraph
                GetSubgraph("color", GetColorSubgraph);

                // Get the render subgraph from the render key
                var renderName = dict.GetValueOrDefault<string>("render");
                var render = GetRenderSubgraph(renderName ?? "ball and stick");

                if (render != null)
                    subgraphs.Add(render);

                if (!hasSequenceSubgraph
                 && FindInputNodeWithName<IntArrayInputNode>(render, "sequences.lengths") != null)
                {
                    var subgraph = GetSequenceSubgraph("entities");
                    subgraphs.Insert(subgraphIndex, subgraph);
                }

                // For each input of each subgraph, see if either the subgraph's dictionary
                // or the root dictionary have an item which could be the value for that input
                foreach (var subgraph in subgraphs)
                foreach (var input in subgraph.GetVisualisationNodes<IInputNode>())
                {
                    if (subgraphParameters.ContainsKey(subgraph)
                     && FindParameter(input, subgraphParameters[subgraph], visualiser))
                        continue;
                    if (FindParameter(input, globalParameters, visualiser))
                        continue;
                }

                // Only return a valid visualiser if it has a render subgraph
                if (render != null)
                {
                    dynamic.SetSubgraphs(subgraphs.ToArray());
                    prefab = false;
                }
                else
                {
                    visualiser = null;
                }
            }

            return (visualiser, prefab);
        }

        private delegate bool TryParseObject<TValue>(object obj, out TValue value);

        /// <summary>
        /// Given that there is an input to a subgraph with a given name, and provided with
        /// a set of parameters, link up the input of the subgraph to a global input with a
        /// value provided in the parameters.
        /// </summary>
        private static bool FindParameter<TInputType, TInputNodeType, TInputComponentType>(
            string name,
            IReadOnlyDictionary<string, object> parameters,
            TryParseObject<TInputType> tryParseObject,
            GameObject visualiser)
            where TInputNodeType : IInputNode
            where TInputComponentType : Component, IVisualisationComponent<TInputNodeType>
        {
            if (FindInputNodeWithName<TInputNodeType>(visualiser, name) != null)
                return true;

            if (parameters.TryGetValue(name, out var valueObject)
             && tryParseObject(valueObject, out var value))
            {
                var inputNode = visualiser.AddComponent<TInputComponentType>();
                inputNode.Node.Name = name;
                inputNode.Node.Input.TrySetValue(value);
                return true;
            }

            return false;
        }

        /// <summary>
        /// For an input node, look through the dictionary of parameters to see if a value
        /// was provided.
        /// </summary>
        private static bool FindParameter(IInputNode input,
                                          IReadOnlyDictionary<string, object> parameters,
                                          GameObject visualiser)
        {
            switch (input)
            {
                case GradientInputNode _:
                    return FindParameter<Gradient, GradientInputNode, GradientInput>(
                        input.Name,
                        parameters,
                        TryParseGradient,
                        visualiser
                    );

                case ColorInputNode _:
                    return FindParameter<Color, ColorInputNode, ColorInput>(
                        input.Name,
                        parameters,
                        TryParseColor,
                        visualiser
                    );

                case FloatInputNode _:
                    return FindParameter<float, FloatInputNode, FloatInput>(
                        input.Name,
                        parameters,
                        TryParseFloat,
                        visualiser
                    );

                case ElementColorMappingInputNode _:
                    return FindParameter<IMapping<Element, Color>, ElementColorMappingInputNode,
                        ElementColorMappingInput>(
                        input.Name,
                        parameters,
                        TryParseElementColorMapping,
                        visualiser
                    );
            }

            return false;
        }

        /// <summary>
        /// Find the visualisation node which wraps an input node type of TType and the
        /// provided name.
        /// </summary>
        private static TType FindInputNodeWithName<TType>(
            GameObject obj,
            string name) where TType : IInputNode
        {
            return obj.GetVisualisationNodes<TType>().FirstOrDefault(vis => vis.Name == name);
        }
    }
}