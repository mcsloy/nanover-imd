using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Narupa.Core;
using Narupa.Core.Science;
using Narupa.Visualisation.Node.Color;
using UnityEngine;

namespace Narupa.Visualisation
{
    public static class VisualisationParser
    {
        public delegate bool TryParse<T>(object value, out T parsed);

        /// <summary>
        /// Parse a gradient from a generic C# object. Possible values include a Unity
        /// gradient or a list of items which can be interpreted as colors using
        /// <see cref="TryParseColor" />.
        /// </summary>
        public static bool TryParseGradient(object value, out Gradient gradient)
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
        public static bool TryParseElementColorMapping(object value,
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
                    mapping = Resources.Load<ElementColorMapping>($"Mapping/{str}");
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
                    when list.Count == 3 && list.All(item => item is double
                                                          || item is int
                                                          || item is float
                                                          || item is long):
                    color = new Color((float) (double) list[0],
                                      (float) (double) list[1],
                                      (float) (double) list[2]);
                    return true;

                // Object could be a list of RGBA values
                case IReadOnlyList<object> list
                    when list.Count == 4 && list.All(item => item is double
                                                          || item is int
                                                          || item is float
                                                          || item is long):
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
        public static bool TryParseFloat(object value, out float number)
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

                case long lng:
                    number = lng;
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Attempt to parse a string from a generic object.
        /// </summary>
        public static bool TryParseString(object value, out string strng)
        {
            strng = default;

            switch (value)
            {
                // Object is a string
                case string str:
                    strng = str;
                    return true;

                default:
                    return false;
            }
        }
    }
}