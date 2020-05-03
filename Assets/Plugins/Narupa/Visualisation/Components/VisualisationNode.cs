using System;
using System.Reflection;
using JetBrains.Annotations;
using Narupa.Core;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Components
{
    public abstract class VisualisationNode : IVisualisationNode, ISerializationCallbackReceiver
    {
        public virtual void Refresh()
        {
        }

        public virtual void OnBeforeSerialize()
        {
        }

        public virtual void OnAfterDeserialize()
        {
            foreach (var field in ReflectionUtility.GetFieldsInSelfOrParents(this.GetType(),
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic))
            {
                if (field.GetCustomAttribute<NotNullAttribute>() != null)
                {
                    var val = field.GetValue(this);
                    if (val == null)
                    {
                        Debug.LogWarning("Found null property");
                        if (field.FieldType.IsArray && field.FieldType.HasElementType)
                        {
                            field.SetValue(this, Array.CreateInstance(field.FieldType.GetElementType(), 0));
                        }
                        else
                        {
                            field.SetValue(this, Activator.CreateInstance(field.FieldType));
                        }
                    }
                }
            }
        }
    }
}