// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using Narupa.Core.Science;
using Narupa.Visualisation.Node.Color;
using UnityEditor;
using UnityEngine;

namespace Narupa.Visualisation.Editor
{
    /// <summary>
    /// Property drawer to render string palette assignments in the editor.
    /// </summary>
    [CustomPropertyDrawer(typeof(StringColorMapping.StringColorAssignment))]
    public sealed class StringColorAssignmentPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var stringProperty =
                property.FindPropertyRelative(
                    nameof(StringColorMapping.StringColorAssignment.value));

            var colorProperty =
                property.FindPropertyRelative(
                    nameof(ElementColorMapping.ElementColorAssignment.color));

            var numberBoxWidth = Mathf.Min(72f, position.width / 2f);

            var numberRect = new Rect(position.x,
                                      position.y,
                                      numberBoxWidth,
                                      position.height);
            var colorRect = new Rect(position.x + numberBoxWidth + 8,
                                     position.y,
                                     position.width - 8 - numberBoxWidth,
                                     position.height);

            EditorGUI.PropertyField(numberRect,
                                    stringProperty,
                                    GUIContent.none);

            EditorGUI.PropertyField(colorRect,
                                    colorProperty,
                                    GUIContent.none);
        }
    }
}