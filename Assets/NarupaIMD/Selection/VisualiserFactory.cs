using System.Collections.Generic;
using System.Linq;
using Narupa.Core;
using Narupa.Core.Science;
using Narupa.Visualisation.Components;
using Narupa.Visualisation.Components.Adaptor;
using Narupa.Visualisation.Node.Color;
using Narupa.Visualisation.Node.Input;
using UnityEngine;
using ColorInput = Narupa.Visualisation.Components.Input.ColorInput;
using ElementColorMappingInput = Narupa.Visualisation.Components.Input.ElementColorMappingInput;
using FloatInput = Narupa.Visualisation.Components.Input.FloatInput;
using GradientInput = Narupa.Visualisation.Components.Input.GradientInput;
using IntArrayInput = Narupa.Visualisation.Components.Input.IntArrayInput;

namespace NarupaIMD.Selection
{
    /// <summary>
    /// Construction methods for creating visualisers from nested data structures.
    /// </summary>
    public static class VisualiserFactory
    {
        /// <summary>
        /// Parse a gradient from a C# object.
        /// </summary>
        private static bool TryParseGradient(object value, out Gradient gradient)
        {
            gradient = null;
            if (!(value is List<object> list))
                return false;
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
                                      c, ((float) i) / (colors.Count - 1)))
                          .ToArray(),
                    new[]
                    {
                        new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1)
                    });
                return true;
            }

            return false;
        }

        /// <summary>
        /// Parse an <see cref="IMapping{TFrom,TTo}"/> from a C# object.
        /// </summary>
        private static bool TryParseElementColorMapping(object value,
                                                        out IMapping<Element, Color> mapping)
        {
            mapping = null;

            if (value is string str)
            {
                mapping = Resources.Load<ElementColorMapping>($"CPK/{str}");
                return true;
            }

            if (value is Dictionary<string, object> dict)
            {
                var mappingDictionary = new Dictionary<Element, Color>();
                foreach (var (key, val) in dict)
                {
                    if (TryParseElement(key, out var element)
                     && TryParseColor(val, out var color))
                    {
                        mappingDictionary.Add(element, color);
                    }
                    else
                    {
                        return false;
                    }
                }

                mapping = mappingDictionary.AsMapping();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Parse an element from either a string symbol or an integer atomic number.
        /// </summary>
        private static bool TryParseElement(object value, out Element element)
        {
            element = Element.Virtual;

            if (value is string str)
            {
                var potentialElement = ElementSymbols.GetFromSymbol(str);
                if (potentialElement.HasValue)
                {
                    element = potentialElement.Value;
                    return true;
                }
            }
            else if (value is double d)
            {
                var atomicNumber = (int) d;
                if (atomicNumber >= 1 && atomicNumber <= 118)
                {
                    element = (Element) atomicNumber;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Attempt to parse a color, from a name, hex code or array of rgba values.
        /// </summary>
        private static bool TryParseColor(object value, out Color color)
        {
            color = Color.white;

            if (value is string str)
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

                // TODO: conversion from hex code
            }
            else if (value is List<object> list)
            {
                if (list.Count == 3 && list[0] is double
                                    && list[1] is double
                                    && list[2] is double)
                {
                    color = new Color((float) (double) list[0],
                                      (float) (double) list[1],
                                      (float) (double) list[2]);
                    return true;
                }

                if (list.Count == 4 && list[0] is double
                                    && list[1] is double
                                    && list[2] is double
                                    && list[3] is double)
                {
                    color = new Color((float) (double) list[0],
                                      (float) (double) list[1],
                                      (float) (double) list[2],
                                      (float) (double) list[3]);
                    return true;
                }
            }

            return false;
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
                if (dict.ContainsKey("color"))
                {
                    if (dict["color"] is Dictionary<string, object> colorStruct)
                    {
                        if (colorStruct.TryGetValue("type", out var type) && type is string str)
                        {
                            var colorSubgraph = GetColorSubgraph(str);
                            if (colorSubgraph != null)
                            {
                                subgraphs.Add(colorSubgraph);
                                subgraphParameters.Add(colorSubgraph, colorStruct);
                            }
                        }
                    }
                }

                var renderName = dict.GetValueOrDefault<string>("render");
                var render = GetRenderSubgraph(renderName ?? "ball and stick");

                if (render != null)
                    subgraphs.Add(render);

                // Find all possible inputs for the subgraphs, and then check to see if
                foreach (var subgraph in subgraphs)
                {
                    foreach (var input in FindInputNodes(subgraph))
                    {
                        var name = input.Name;
                        if (subgraphParameters.ContainsKey(subgraph)
                         && FindParameter(name, subgraphParameters[subgraph], input, visualiser))
                            continue;
                        if (FindParameter(name, globalParameters, input, visualiser))
                            continue;
                    }
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
                case Narupa.Visualisation.Node.Input.GradientInput _:
                    if (FindInputNodeWithName<Narupa.Visualisation.Node.Input.GradientInput>(
                        visualiser, name))
                        return true;
                    if (parameters.TryGetValue(name, out var gradientObject)
                     && TryParseGradient(gradientObject, out var gradient))
                    {
                        var inputNode = visualiser.AddComponent<GradientInput>();
                        inputNode.Node.Name = name;
                        inputNode.Node.Input.Value = gradient;
                    }

                    return true;

                case Narupa.Visualisation.Node.Input.ColorInput _:
                    if (FindInputNodeWithName<Narupa.Visualisation.Node.Input.ColorInput>(
                        visualiser, name))
                        return true;
                    if (parameters.TryGetValue(name, out var colorObject)
                     && TryParseColor(colorObject, out var color))
                    {
                        var inputNode = visualiser.AddComponent<ColorInput>();
                        inputNode.Node.Name = name;
                        inputNode.Node.Input.Value = color;
                    }

                    return true;

                case Narupa.Visualisation.Node.Input.FloatInput _:
                    if (FindInputNodeWithName<Narupa.Visualisation.Node.Input.FloatInput>(
                        visualiser, name))
                        return true;
                    if (parameters.TryGetValue(name, out var scaleObject)
                     && scaleObject is double scale)
                    {
                        var inputNode = visualiser.AddComponent<FloatInput>();
                        inputNode.Node.Name = name;
                        inputNode.Node.Input.Value = (float) scale;
                    }

                    return true;

                case Narupa.Visualisation.Node.Input.ElementColorMappingInput _:
                    if (FindInputNodeWithName<Narupa.Visualisation.Node.Input.ElementColorMappingInput>(
                        visualiser, name))
                        return true;
                    if (parameters.TryGetValue(name, out var schemeObject)
                     && TryParseElementColorMapping(schemeObject, out var scheme))
                    {
                        var inputNode = visualiser.AddComponent<ElementColorMappingInput>();
                        inputNode.Node.Name = name;
                        inputNode.Node.Input.Value = scheme;
                    }

                    return true;
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
        /// Find the visualisation node which wraps an input node type of TType and the provided name.
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