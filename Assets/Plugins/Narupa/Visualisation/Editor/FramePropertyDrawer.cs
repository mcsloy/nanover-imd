using System;
using System.Linq;
using Narupa.Core.Collections;
using Narupa.Frame;
using Narupa.Visualisation.Node.Input;
using UnityEditor;
using UnityEngine;

namespace Narupa.Visualisation.Editor
{
    /// <summary>
    /// Property drawer to render a string field for selecting a frame property.
    /// </summary>
    public sealed class FramePropertyDrawer : PropertyDrawer
    {
        public static void DrawFrameKeyProperty(Rect position, SerializedProperty property, Type type)
        {
            var value = property.stringValue;

            var oldColor = GUI.color;
            GUI.color = GetColor(value);
            
            var frameProperty = VisualisationFrameProperties.All.FirstOrDefault(p => p.Key == value);
            var isStandardProperty = frameProperty != null;

            var dropdownRect = isStandardProperty
                                   ? position
                                   : new Rect(position.xMax - 16, position.y, 16, position.height);

            var properties = VisualisationFrameProperties.All.Where(p => p.Type == type).ToArray();
            var currentIndex = isStandardProperty
                                   ? properties.FirstIndexOf(p => p.Key == value)
                                   : properties.Length;

            int index = -1;
            if (isStandardProperty)
            {
                index = EditorGUI.Popup(dropdownRect,
                                        "",
                                        currentIndex,
                                        properties.Select(p => $"{p.Key} ({p.DisplayName})")
                                                  .Concat("Custom...")
                                                  .ToArray());
            }
            else
            {
                index = EditorGUI.Popup(dropdownRect,
                                        currentIndex,
                                        properties.Select(p => $"{p.Key} ({p.DisplayName})")
                                                  .Concat("Custom...")
                                                  .ToArray());
            }

            if (isStandardProperty && index == properties.Length) // custom
            {
                property.stringValue = "";
            }
            else if (index < properties.Length) // Chosen a preset
            {
                var newFrameProperty = properties[index];
                property.stringValue = newFrameProperty.Key;
            }

            if (!isStandardProperty)
            {
                {
                    EditorGUI.PropertyField(
                        new Rect(position.x, position.y, position.width - 32, position.height),
                        property, GUIContent.none);
                }
            }

            GUI.color = oldColor;
        }

        private static Color GetColor(string value)
        {
            if (value.StartsWith("particle.") && !value.EndsWith(".count"))
            {
                return GUI.color * new Color(0.9f, 0.95f, 1f);
            }
            
            if (value.StartsWith("residue.") && !value.EndsWith(".count"))
            {
                return GUI.color * new Color(0.95f, 1f, 0.95f);
            }
            
            if (value.StartsWith("entity.") && !value.EndsWith(".count"))
            {
                return GUI.color * new Color(1f, 0.9f, 0.95f);
            }

            return GUI.color;
        }
    }
}
