// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Core.Collections;
using Narupa.Visualisation.Components;
using Narupa.Visualisation.Node.Input;
using Narupa.Visualisation.Node.Output;
using Narupa.Visualisation.Property;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Narupa.Visualisation.Editor
{
    /// <summary>
    /// Property drawer override for drawing <see cref="Property" /> fields
    /// which belong to a <see cref="VisualisationComponent" />.
    /// </summary>
    [InitializeOnLoad]
    public static class VisualiserComponentsPropertyDrawer
    {
        static VisualiserComponentsPropertyDrawer()
        {
            VisualisationPropertyDrawer.AddOverride(OnGui);
        }

        /// <summary>
        /// Draw the GUI for a <see cref="Property.Property" /> that is wrapped by a
        /// <see cref="VisualisationComponent" /> which provides information on links.
        /// </summary>
        /// <returns>
        /// True if this should override the default property GUI, false if it
        /// shouldn't.
        /// </returns>
        private static bool OnGui(ref Rect rect, SerializedProperty property, GUIContent label)
        {
            // Find the list of links stored in the VisualisationComponent
            var collection = property.serializedObject.FindProperty("inputLinkCollection");
            if (collection == null) return false;

            // Get link which is stored in the VisualisationComponent, which
            // describes if this property is linked to anything
            var link = FindVisualisationLink(collection, property.name);

            var linkToggleRect = new Rect(rect)
            {
                width = 40
            };
            linkToggleRect.x = rect.xMax - linkToggleRect.width;
            rect.width -= 44;

            // Is this property linked
            var isLinked = link.HasValue;

            if (GUI.Button(linkToggleRect, isLinked ? "Unlink" : "Link", EditorStyles.miniButton))
            {
                if (isLinked)
                {
                    // Unlink by destroying the link in the VisualisationComponent
                    collection.DeleteArrayElementAtIndex(link.Value.Index);
                    isLinked = false;
                }
                else
                {
                    // Create a link in the VisualisationComponent
                    var index = collection.arraySize;
                    collection.InsertArrayElementAtIndex(index);
                    var linkProperty = collection.GetArrayElementAtIndex(index);
                    var destProperty = linkProperty.FindPropertyRelative(
                        nameof(VisualisationComponent.InputPropertyLink.destinationFieldName));
                    destProperty.stringValue = property.name;
                }
            }

            if (isLinked)
            {
                var linkRect = new Rect(rect);
                linkRect.yMin = linkRect.yMax - 16f;
                DrawSourceForLinkedProperty(linkRect, link.Value);
                rect.height -= 16;
            }
            else
            {
                var obj = property.serializedObject.targetObject as VisualisationComponent;
                if (obj != null)
                {
                    if (obj.GetWrappedVisualisationNode() is IInputNode node &&
                        property.name == "name")
                    {
                        var valueProperty = VisualisationPropertyDrawer.GetValueProperty(property);
                        FramePropertyDrawer.DrawFrameKeyProperty(
                            rect, valueProperty, node.InputType);
                        return true;
                    }

                    if (obj.GetWrappedVisualisationNode() is IOutputNode output &&
                        property.name == "name")
                    {
                        var valueProperty = VisualisationPropertyDrawer.GetValueProperty(property);
                        FramePropertyDrawer.DrawFrameKeyProperty(
                            rect, valueProperty, output.OutputType);
                        return true;
                    }
                }

                return false;
            }

            return true;
        }

        private static SerializedInputLink? FindVisualisationLink(
            SerializedProperty linkCollectionSerProp,
            string name)
        {
            for (var i = 0; i < linkCollectionSerProp.arraySize; i++)
            {
                var currentLinkSerProp = linkCollectionSerProp.GetArrayElementAtIndex(i);
                var link = new SerializedInputLink
                {
                    Index = i,
                    DestinationProperty = currentLinkSerProp.FindPropertyRelative(
                        nameof(VisualisationComponent.InputPropertyLink.destinationFieldName))
                };

                if (name != link.DestinationProperty.stringValue)
                    continue;

                link.SourceProperty =
                    currentLinkSerProp.FindPropertyRelative(
                        nameof(VisualisationComponent.InputPropertyLink.sourceFieldName));
                link.SourceComponent =
                    currentLinkSerProp.FindPropertyRelative(
                        nameof(VisualisationComponent.InputPropertyLink.sourceComponent));

                return link;
            }

            return null;
        }

        private struct SerializedInputLink
        {
            public int Index;

            public SerializedProperty SourceComponent;

            public SerializedProperty SourceProperty;

            public SerializedProperty DestinationProperty;
        }

        private class ValidShortcut
        {
            public GUIContent Label;
            public Object Source;
            public string Name;
        }

        private static IEnumerable<ValidShortcut> GetShortcuts(MonoBehaviour behaviour,
                                                               Type destinationType)
        {
            if (behaviour == null)
                yield break;
            var go = behaviour.gameObject;
            foreach (var shortcut in GetShortcuts(go, destinationType))
                yield return shortcut;
        }

        private static IEnumerable<ValidShortcut> GetShortcuts(GameObject go, Type destinationType)
        {
            foreach (var obj in go.GetComponents<VisualisationComponent>())
            {
                var fields = GetPropertiesOfType(obj, destinationType);
                foreach (var field in fields)
                {
                    yield return new ValidShortcut
                    {
                        Label = new GUIContent(GetLabelForFieldOfComponent(obj, field)),
                        Source = obj,
                        Name = field
                    };
                }
            }

            if (go.transform.parent != null)
                foreach (var shortcut in GetShortcuts(go.transform.parent.gameObject,
                                                      destinationType))
                    yield return shortcut;
        }

        /// <summary>
        /// User readable name for a field of a visualisation component.
        /// </summary>
        private static string GetLabelForFieldOfComponent(VisualisationComponent obj, string field)
        {
            if (obj.GetWrappedVisualisationNode() is IInputNode input && field == "input")
            {
                var property =
                    VisualisationFrameProperties.All.FirstOrDefault(k => k.Key == input.Name);
                return $"> {(property != null ? property.DisplayName : input.Name)}";
            }

            return
                $"{ObjectNames.NicifyVariableName(obj.GetType().Name)} > {ObjectNames.NicifyVariableName(field)}";
        }

        private static IEnumerable<string> GetPropertiesOfType(
            IPropertyProvider provider,
            Type type)
        {
            var names = new List<string>();
            names.AddRange(provider.GetPotentialProperties().Where(a => a.type == type)
                                   .Select(a => a.name));
            names.AddRange(provider.GetProperties().Where(a => a.property.PropertyType == type)
                                   .Select(a => a.name));
            return names.Distinct();
        }

        private static void DrawSourceForLinkedProperty(Rect rect, SerializedInputLink link)
        {
            var sourceFieldWidth = 144;

            var destinationObject =
                link.DestinationProperty.serializedObject.targetObject as IPropertyProvider;
            var destinationType = destinationObject
                                  .GetProperty(link.DestinationProperty.stringValue).PropertyType;

            var shortcuts =
                GetShortcuts(
                    link.DestinationProperty.serializedObject.targetObject as MonoBehaviour,
                    destinationType).ToArray();

            var currentComponent = link.SourceComponent.objectReferenceValue;

            var currentField = link.SourceProperty.stringValue;

            var currentShortcutIndex = shortcuts.IndexOf(s => s.Source == currentComponent
                                                           && s.Name == currentField);

            var shortcutIndex = EditorGUI.Popup(rect, currentShortcutIndex, shortcuts
                                                                            .Select(shortcut =>
                                                                                        shortcut
                                                                                            .Label)
                                                                            .ToArray());

            if (shortcutIndex != currentShortcutIndex)
            {
                var shortcut = shortcuts[shortcutIndex];
                link.SourceComponent.objectReferenceValue = shortcut.Source;
                link.SourceProperty.stringValue = shortcut.Name;
            }
        }
    }
}
