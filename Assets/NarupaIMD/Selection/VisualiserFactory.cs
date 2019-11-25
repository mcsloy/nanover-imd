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
        /// Parse a gradient from a generic C# object. Possible values include a Unity gradient or a list of items which can be interpreted as colors using <see cref="TryParseColor"/>.
        /// </summary>
        private static bool TryParseGradient(object value, out Gradient gradient)
        {
            gradient = null;

            switch (value)
            {
                case Gradient g:
                    gradient = g;
                    return true;
                
                case IReadOnlyList<object> list:
                {
                    var colors = new List<Color>();
                    
                    foreach (var item in list)
                    {
                        if (!TryParseColor(item, out var color))
                            return false;
                        colors.Add(color);
                    }

                    if (colors.Count > 0)
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
                case IMapping<Element, Color> actualValue:
                    mapping = actualValue;
                    return true;
                
                case string str:
                    mapping = Resources.Load<ElementColorMapping>($"CPK/{str}");
                    return true;
                
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
        private static bool TryParseElement(object value, out Element element)
        {
            element = Element.Virtual;

            switch (value)
            {
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

        private const string RegexRgbHex =
            @"^#?([A-Fa-f0-9][A-Fa-f0-9])([A-Fa-f0-9][A-Fa-f0-9])([A-Fa-f0-9][A-Fa-f0-9])$";

        /// <summary>
        /// Attempt to parse a color, from a name, hex code or array of rgba values.
        /// </summary>
        private static bool TryParseColor(object value, out Color color)
        {
            color = Color.white;

            switch (value)
            {
                case string str:
                {
                    var htmlColor = System.Drawing.Color.FromName(str);
                    if (htmlColor.A > 0)
                    {
                        color = new Color(htmlColor.R / 255f,
                                          htmlColor.G / 255f,
                                          htmlColor.B / 255f,
                                          htmlColor.A / 255f);
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
                case List<object> list when list.Count == 3 && list.All(item => item is double):
                    color = new Color((float) (double) list[0],
                                      (float) (double) list[1],
                                      (float) (double) list[2]);
                    return true;
                case List<object> list when list.Count == 4 && list.All(item => item is double):
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
                case float flt:
                    number = flt;
                    return true;
                case double dbl:
                    number = (float) dbl;
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
                // read in frames and a DynamicComponent to contain the dynamic nodes
                visualiser = new GameObject();
                var adaptor = visualiser.AddComponent<FrameAdaptor>();
                var dynamic = visualiser.AddComponent<DynamicComponent>();
                dynamic.FrameAdaptor = adaptor;

                var subgraphs = new List<GameObject>();

                // Particle filter for selections
                var filterInput = visualiser.AddComponent<IntArrayInput>();
                filterInput.Node.Name = "particle.filter";

                var subgraphParameters = new Dictionary<GameObject, Dictionary<string, object>>();
                var globalParameters = dict;

                // Parse the color keyword if it is a struct, and hence describes 
                // a color subgraph
                if (dict.TryGetValue<Dictionary<string, object>>("color", out var colorStruct))
                    if (colorStruct.TryGetValue<string>("type", out var type))
                    {
                        var colorSubgraph = GetColorSubgraph(type);
                        if (colorSubgraph != null)
                        {
                            subgraphs.Add(colorSubgraph);
                            subgraphParameters.Add(colorSubgraph, colorStruct);
                        }
                    }

                var renderName = dict.GetValueOrDefault<string>("render");
                var render = GetRenderSubgraph(renderName ?? "ball and stick");

                if (render != null)
                    subgraphs.Add(render);

                // Find all possible inputs for the subgraphs, and then check to see if
                foreach (var subgraph in subgraphs)
                foreach (var input in FindInputNodes(subgraph))
                {
                    var name = input.Name;
                    if (subgraphParameters.ContainsKey(subgraph)
                        && FindParameter(name, subgraphParameters[subgraph], input, visualiser))
                        continue;
                    if (FindParameter(name, globalParameters, input, visualiser))
                        continue;
                }

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
            if (FindInputNodeWithName<TInputNodeType>(visualiser, name))
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
        /// Create an InputNode on the root visualiser.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parameters"></param>
        /// <param name="input"></param>
        /// <param name="visualiser"></param>
        /// <returns></returns>
        private static bool FindParameter(string name,
                                          IReadOnlyDictionary<string, object> parameters,
                                          IInputNode input,
                                          GameObject visualiser)
        {
            switch (input)
            {
                case GradientInputNode _:
                    return FindParameter<Gradient, GradientInputNode, GradientInput>(
                        name,
                        parameters,
                        TryParseGradient,
                        visualiser
                    );

                case ColorInputNode _:
                    return FindParameter<Color, ColorInputNode, ColorInput>(
                        name,
                        parameters,
                        TryParseColor,
                        visualiser
                    );

                case FloatInputNode _:
                    return FindParameter<float, FloatInputNode, FloatInput>(
                        name,
                        parameters,
                        TryParseFloat,
                        visualiser
                    );

                case ElementColorMappingInputNode _:
                    return FindParameter<IMapping<Element, Color>, ElementColorMappingInputNode,
                        ElementColorMappingInput>(
                        name,
                        parameters,
                        TryParseElementColorMapping,
                        visualiser
                    );
            }

            return false;
        }

        private static IEnumerable<IInputNode> FindInputNodes(GameObject subgraph)
        {
            return subgraph.GetComponents<VisualisationComponent>()
                .Where(vis => vis.GetWrappedVisualisationNode() is IInputNode)
                .Select(vis => vis.GetWrappedVisualisationNode() as IInputNode);
        }

        /// <summary>
        /// Find the visualisation node which wraps an input node type of TType and the
        /// provided name.
        /// </summary>
        private static VisualisationComponent FindInputNodeWithName<TType>(
            GameObject obj,
            string name) where TType : IInputNode
        {
            return obj.GetComponents<VisualisationComponent>()
                .FirstOrDefault(vis => vis.GetWrappedVisualisationNode() is TType type
                                       && type.Name == name);
        }
    }
}