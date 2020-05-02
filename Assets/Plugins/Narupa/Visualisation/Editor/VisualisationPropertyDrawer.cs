// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Narupa.Core;
using Narupa.Visualisation.Property;
using UnityEditor;
using UnityEngine;

namespace Narupa.Visualisation.Editor
{
    /// <summary>
    /// Custom UI for drawing a <see cref="Property" /> in the Editor.
    /// </summary>
    [CustomPropertyDrawer(typeof(Property.Property), true)]
    public sealed class VisualisationPropertyDrawer : PropertyDrawer
    {
        /// <summary>
        /// Callback for overriding this property drawer. Returns true if this specific
        /// property was overriden.
        /// </summary>
        public delegate bool OnGuiOverride(ref Rect position,
                                           SerializedProperty property,
                                           GUIContent label);

        private static readonly List<OnGuiOverride> overrides = new List<OnGuiOverride>();

        /// <summary>
        /// Add an override for drawing <see cref="Property" /> fields, for example to
        /// allow linking in the Editor for Visualisation Components
        /// </summary>
        public static void AddOverride(OnGuiOverride guiOverride)
        {
            overrides.Add(guiOverride);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Standard practice of prefixing the name of the property
            position = EditorGUI.PrefixLabel(position, label);

            // Check if any overrides apply here, and if so return
            if (overrides.Any(overrideOnGui => overrideOnGui(ref position, property, label)))
            {
                return;
            }

            DrawDefaultPropertyGui(position, property);
        }

        /// <summary>
        /// Draw a standard representation of a <see cref="Property" />, which is the
        /// standard input for the value that property wraps with a small tick box to
        /// indicate if a value should be provided.
        /// </summary>
        private void DrawDefaultPropertyGui(Rect position, SerializedProperty property)
        {
            var valueProperty = GetValueProperty(property);
            var referencedObjectProperty = GetReferencedObjectProperty(property);

            if (referencedObjectProperty == null && (valueProperty == null ||
                                                     (valueProperty.isArray &&
                                                      valueProperty.type != "string")))
            {
                // Don't want to be able to edit arrays in the Editor.
                EditorGUI.HelpBox(position, "Set array input from within code.", MessageType.None);
            }
            else
            {
                var isProvidedSerializedProperty = GetIsProvidedProperty(property);
                var isValueProvided = isProvidedSerializedProperty.boolValue;

                var valueRect = position;
                var togglePosition = new Rect(valueRect);
                valueRect.xMin += 20f;

                if (isValueProvided)
                {
                    DrawValueGui(property, valueRect);
                }
                else
                {
                    DrawMissingValueGui(property, valueRect);
                }

                EditorGUI.PropertyField(togglePosition, isProvidedSerializedProperty,
                                        GUIContent.none);
            }
        }

        /// <summary>
        /// Draw a box indicating a missing value, indicating if the requirement is
        /// required
        /// </summary>
        private void DrawMissingValueGui(SerializedProperty property, Rect rect)
        {
            var field = property.GetField();
            if (field?.GetCustomAttribute<RequiredPropertyAttribute>() != null)
            {
                EditorGUI.HelpBox(rect, "Missing input!", MessageType.Error);
            }
            else
            {
                EditorGUI.HelpBox(rect, "No input", MessageType.None);
            }
        }

        /// <summary>
        /// Draw the value of the property in the given rect
        /// </summary>
        private void DrawValueGui(SerializedProperty property, Rect rect)
        {
            var valueSerializedProperty = GetValueProperty(property);
            var objectSerializedProperty = GetReferencedObjectProperty(property);

            // Draw a property field, wrapping in a ChangeCheck to 
            // check if its been changed, and dirty the underlying
            // field if so
            EditorGUI.BeginChangeCheck();

            if (valueSerializedProperty != null)
                EditorGUI.PropertyField(rect, valueSerializedProperty, GUIContent.none, true);
            else if (objectSerializedProperty != null)
                EditorGUI.PropertyField(rect, objectSerializedProperty, GUIContent.none, true);

            if (EditorGUI.EndChangeCheck())
            {
                if (property.TryGetObject<Property.Property>(out var narupaProperty))
                {
                    narupaProperty.MarkValueAsChanged();
                }
                else
                {
                    Debug.LogWarning("Visualisation Property not found");
                }
            }
        }

        /// <summary>
        /// For a given <see cref="SerializedProperty" />, get the underlying
        /// <see cref="FieldInfo" />.
        /// </summary>
        private static FieldInfo GetField(SerializedProperty property)
        {
            // TODO: Support nested objects
            return property.serializedObject
                           .targetObject
                           .GetType()
                           .GetFieldInSelfOrParents(property.propertyPath,
                                                    BindingFlags.Public
                                                  | BindingFlags.Instance
                                                  | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Get the <see cref="SerializedProperty" /> representing the internal input
        /// value.
        /// </summary>
        internal static SerializedProperty GetValueProperty(SerializedProperty inputProperty)
        {
            return inputProperty.FindPropertyRelative("value");
        }

        /// <summary>
        /// Get the <see cref="SerializedProperty" /> representing a Unity object
        /// referenced by this property, indicating it is a
        /// <see cref="InterfaceProperty" />.
        /// </summary>
        private SerializedProperty GetReferencedObjectProperty(SerializedProperty inputProperty)
        {
            return inputProperty.FindPropertyRelative("unityObject");
        }

        /// <summary>
        /// Get the <see cref="SerializedProperty" /> representing the internal input
        /// value.
        /// </summary>
        private SerializedProperty GetIsProvidedProperty(SerializedProperty inputProperty)
        {
            return inputProperty.FindPropertyRelative("isValueProvided");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var provided = GetIsProvidedProperty(property);
            if (provided == null || provided.boolValue)
            {
                var valueProperty = GetValueProperty(property);
                if (valueProperty != null)
                    return EditorGUI.GetPropertyHeight(valueProperty);
                var objectProperty = GetReferencedObjectProperty(property);
                if (objectProperty != null)
                    return EditorGUI.GetPropertyHeight(objectProperty);
            }

            return base.GetPropertyHeight(property, label);
        }
    }
}
