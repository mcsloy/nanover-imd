using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Narupa.Core;
using Narupa.Visualisation.Property;

namespace Narupa.Visualisation.Components
{
    public class VisualisationUtility
    {
        /// <summary>
        /// Get all fields on an object which are visualisation properties.
        /// </summary>
        public static IEnumerable<(string, IReadOnlyProperty)> GetAllPropertyFields(object obj)
        {
            var allFields = obj.GetType()
                               .GetFieldsInSelfOrParents(BindingFlags.Instance
                                                       | BindingFlags.NonPublic
                                                       | BindingFlags.Public);

            var validFields = allFields.Where(field => typeof(IReadOnlyProperty).IsAssignableFrom(
                                                  field.FieldType
                                              ));

            foreach (var field in validFields)
            {
                yield return (field.Name,
                              field.GetValue(obj) as
                                  IReadOnlyProperty);
            }
        }

        /// <summary>
        /// Get all fields on an object which are visualisation properties.
        /// </summary>
        public static IReadOnlyProperty GetPropertyField(object obj, string key)
        {
            if (obj == null)
                return null;
            var field = obj.GetType()
                           .GetFieldInSelfOrParents(key,
                                                    BindingFlags.Instance
                                                  | BindingFlags.NonPublic
                                                  | BindingFlags.Public);

            if (field == null)
                return null;

            if (!typeof(IReadOnlyProperty).IsAssignableFrom(field.FieldType))
                return null;

            return field.GetValue(obj) as IReadOnlyProperty;
        }
    }
}