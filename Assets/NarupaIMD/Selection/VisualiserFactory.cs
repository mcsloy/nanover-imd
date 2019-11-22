using System.Collections.Generic;
using System.Linq;
using Narupa.Core;
using Narupa.Core.Science;
using Narupa.Visualisation.Components;
using Narupa.Visualisation.Components.Adaptor;
using Narupa.Visualisation.Components.Input;
using Narupa.Visualisation.Node.Color;
using UnityEngine;

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
                        new GradientAlphaKey(1, 0),
                        new GradientAlphaKey(1, 1)
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
                // Look for a visualiser prefab
                visualiser = GetPredefinedVisualiser(visName);
            }
            else if (data is Dictionary<string, object> dict)
            {
                visualiser = new GameObject();
                var adaptor = visualiser.AddComponent<FrameAdaptor>();
                var dynamic = visualiser.AddComponent<DynamicComponent>();
                dynamic.FrameAdaptor = adaptor;

                var list = new List<GameObject>();

                // Particle filter for selections
                var filterInput = visualiser.AddComponent<IntArrayInput>();
                filterInput.Node.Name = "particle.filter";

                if (dict.ContainsKey("color"))
                {
                    if (TryParseColor(dict["color"], out var color))
                    {
                        var input = visualiser.AddComponent<ColorInput>();
                        input.Node.Name = "color";
                        input.Node.Input.Value = color;
                    }
                    else if (dict["color"] is Dictionary<string, object> colorStruct)
                    {
                        if (colorStruct.TryGetValue("type", out var type) && type is string str)
                        {
                            var colorSubgraph = GetColorSubgraph(str);
                            if (colorSubgraph != null)
                            {
                                list.Add(colorSubgraph);
                            }

                            if (colorStruct.TryGetValue("gradient", out var gradientObject)
                             && TryParseGradient(gradientObject, out var gradient))
                            {
                                var input = visualiser.AddComponent<GradientInput>();
                                input.Node.Name = "gradient";
                                input.Node.Input.Value = gradient;
                            }

                            if (str == "cpk"
                             && colorStruct.TryGetValue("scheme", out var schemeObject)
                             && TryParseElementColorMapping(schemeObject, out var scheme))
                            {
                                var input = visualiser.AddComponent<ElementColorMappingInput>();
                                input.Node.Name = "scheme";
                                input.Node.Input.Value = scheme;
                            }
                        }
                    }
                }

                if (dict.TryGetValue("scale", out var scaleValue) && scaleValue is double scale)
                {
                    var input = visualiser.AddComponent<FloatInput>();
                    input.Node.Name = "scale";
                    input.Node.Input.Value = (float) scale;
                }

                var renderName = dict.GetValueOrDefault<string>("render");
                var render = GetRenderSubgraph(renderName ?? "ball and stick");

                if (render != null)
                {
                    list.Add(render);

                    dynamic.SetSubgraphs(list.ToArray());

                    prefab = false;
                }
                else
                {
                    visualiser = null;
                }
            }

            return (visualiser, prefab);
        }
    }
}