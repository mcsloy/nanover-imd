// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using Narupa.Visualisation.Components;
using Narupa.Visualisation.Properties;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Input
{
    /// <summary>
    /// Generic input for the visualisation system that provides some value with a
    /// given key.
    /// </summary>
    [Serializable]
    public abstract class InputNode<TProperty, TValue> : IInputNode<TValue>
        where TProperty : Property<TValue>, new()
    {
        IProperty IInputNode.Input => input;

        public IProperty<TValue> Input => input;

        public Type InputType => input.PropertyType;

        [SerializeField]
        private StringProperty name = new StringProperty()
        {
            Value = "KEY"
        };

        [SerializeField]
        private TProperty input = new TProperty();

        public string Name
        {
            get => !name.HasValue ? "$MISSING" : name;
            set => name.Value = value;
        }

        public void Setup()
        {
        }

        public void Refresh()
        {
        }
    }

    public interface IInputNode : IVisualisationNode
    {
        string Name { get; set; }

        IProperty Input { get; }

        Type InputType { get; }
    }

    public interface IInputNode<TValue> : IInputNode
    {
        new IProperty<TValue> Input { get; }
    }
}