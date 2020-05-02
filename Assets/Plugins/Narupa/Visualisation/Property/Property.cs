using System;

namespace Narupa.Visualisation.Property
{
    /// <summary>
    /// Abstract base class for an implementation of <see cref="IProperty"/>.
    /// </summary>
    [Serializable]
    public abstract class Property : IProperty
    {
        IReadOnlyProperty IProperty.LinkedProperty => NonGenericLinkedProperty;
        
        object IReadOnlyProperty.Value => NonGenericValue;

        protected abstract IReadOnlyProperty NonGenericLinkedProperty { get; }

        protected abstract object NonGenericValue { get; }
        
        /// <summary>
        /// Callback for when the value is changed or undefined.
        /// </summary>
        public event Action ValueChanged;

        /// <summary>
        /// Internal method, called when the value is changed or undefined.
        /// </summary>
        protected void OnValueChanged()
        {
            ValueChanged?.Invoke();
        }

        /// <inheritdoc cref="IReadOnlyProperty.HasValue"/>
        public abstract bool HasValue { get; }

        /// <inheritdoc cref="IProperty.UndefineValue"/>
        public abstract void UndefineValue();

        /// <inheritdoc cref="IProperty.PropertyType"/>
        public abstract Type PropertyType { get; }

        /// <summary>
        /// Has the value of the property changed since the last time the <see cref="IsDirty"/> flag was cleared.
        /// </summary>
        public bool IsDirty { get; set; } = true;

        /// <summary>
        /// Mark the value as having changed.
        /// </summary>
        public virtual void MarkValueAsChanged()
        {
            IsDirty = true;
        }

        /// <inheritdoc cref="IProperty.HasLinkedProperty"/>
        public abstract bool HasLinkedProperty { get; }

        /// <inheritdoc cref="IProperty.TrySetValue"/>
        public abstract void TrySetValue(object value);

        /// <inheritdoc cref="IProperty.TrySetLinkedProperty"/>
        public abstract void TrySetLinkedProperty(object property);
    }
    
    /// <inheritdoc cref="Property"/>
    public abstract class Property<TValue> : Property, IProperty<TValue> {
    
        /// <summary>
        /// Implicit conversion of the property to its value
        /// </summary>
        public static implicit operator TValue(Property<TValue> property)
        {
            return property.Value;
        }

        public abstract TValue Value { get; set; }
        
        public abstract IReadOnlyProperty<TValue> LinkedProperty { get; set; }

        /// <inheritdoc cref="Property.PropertyType" />
        public override Type PropertyType => typeof(TValue);

        /// <inheritdoc cref="Property.TrySetValue" />
        public override void TrySetValue(object value)
        {
            if (value is TValue validValue)
                Value = validValue;
            else if (value == default)
                Value = default;
            else
                throw new ArgumentException($"Tried to set property of type {PropertyType} to {value}.");
        }

    }
}