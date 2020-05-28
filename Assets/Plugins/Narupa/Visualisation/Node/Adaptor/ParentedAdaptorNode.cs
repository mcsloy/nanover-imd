using System;
using Narupa.Core;
using Narupa.Visualisation.Components;
using Narupa.Visualisation.Properties;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Adaptor
{
    /// <summary>
    /// An <see cref="BaseAdaptorNode"/> which is linked to another adaptor. This adaptor contains its own properties, but links them to the parent. This means that the parent can be changed without listeners to this adaptor needing to change their links.
    /// </summary>
    [Serializable]
    public class ParentedAdaptorNode : BaseAdaptorNode
    {
        /// <inheritdoc cref="ParentAdaptor"/>
        [SerializeField]
        private DynamicPropertyProviderNode adaptor = new DynamicPropertyProviderNode();

        /// <summary>
        /// The adaptor that this adaptor inherits from.
        /// </summary>
        public IProperty<IDynamicPropertyProvider> ParentAdaptor => adaptor;

        /// <inheritdoc cref="BaseAdaptorNode.OnCreateProperty{T}"/>
        protected override IReadOnlyProperty<T> OnCreateProperty<T>(string key, IProperty<T> property)
        {
            base.OnCreateProperty(key, property);
            if (adaptor.HasNonNullValue())
                property.LinkedProperty = adaptor.Value.GetOrCreateProperty<T>(key);
            return property;
        }

        public void GetValueFromParent(string key, IProperty property)
        {
            if (IsPropertyOverriden(key))
                return;
            
            if (adaptor.HasNonNullValue())
            {
                property.TrySetLinkedProperty(adaptor.Value.GetOrCreateProperty(key, property.PropertyType));
            }
            else
            {
                property.TrySetLinkedProperty(null);
            }
        }
        
        public override void RemoveOverrideProperty(string name)
        {
            base.RemoveOverrideProperty(name);
            GetValueFromParent(name, GetProperty(name) as IProperty);
        }

        /// <inheritdoc cref="BaseAdaptorNode.Refresh"/>
        public override void Refresh()
        {
            if (adaptor.IsDirty)
            {
                foreach (var (key, property) in Properties)
                    GetValueFromParent(key, property);

                adaptor.IsDirty = false;
            }
        }
    }
}