using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;

namespace Narupa.Visualisation.Editor
{
    public static class SerializedPropertyExtensions
    {
        /// <summary>
        /// Get the field that is referenced by a <see cref="SerializedProperty" />
        /// </summary>
        public static FieldInfo GetField(this SerializedProperty property)
        {
            object obj = property.serializedObject.targetObject;

            if (obj == null)
                return null;

            FieldInfo field = null;
            var names = property.propertyPath.Split('.');
            foreach (var name in names)
            {
                field = obj.GetType().GetField(name,
                                               BindingFlags.Instance
                                             | BindingFlags.Public
                                             | BindingFlags.NonPublic);

                if (field == null)
                    return null;

                obj = field.GetValue(obj);
            }

            return field;
        }

        /// <summary>
        /// Try getting the value of a <see cref="SerializedProperty" />.
        /// </summary>
        public static bool TryGetObject(this SerializedProperty property, out object result)
        {
            result = null;
            object obj = property.serializedObject.targetObject;

            if (obj == null)
                return false;

            var names = property.propertyPath.Split('.');

            for (var i = 0; i < names.Length; i++)
            {
                if (names[i] == "Array" && names[i + 1].StartsWith("data"))
                {
                    var arr = obj as IReadOnlyList<object>;
                    var regex = Regex.Match(names[i + 1], @"data\[(\d+)\]");
                    var index = int.Parse(regex.Groups[1].Value);
                    obj = arr[index];
                    i++;
                }
                else
                {
                    var field = obj.GetType().GetField(names[i],
                                                       BindingFlags.Instance
                                                     | BindingFlags.Public
                                                     | BindingFlags.NonPublic);

                    if (field == null)
                        return false;

                    obj = field.GetValue(obj);
                }
            }

            result = obj;
            return true;
        }

        /// <summary>
        /// Try getting the value of a <see cref="SerializedProperty" /> expressed as type
        /// <paramref cref="TType" />.
        /// </summary>
        public static bool TryGetObject<TType>(this SerializedProperty property, out TType result)
        {
            if (property.TryGetObject(out var obj) && obj is TType correctType)
            {
                result = correctType;
                return true;
            }

            result = default;
            return false;
        }
    }
}