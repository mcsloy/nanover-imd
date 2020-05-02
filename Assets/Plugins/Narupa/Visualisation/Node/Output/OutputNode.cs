// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using Narupa.Visualisation.Components;
using Narupa.Visualisation.Properties;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Output
{
    /// <summary>
    /// Generic output for the visualisation system that provides some value with a
    /// given key.
    /// </summary>
    [Serializable]
    public abstract class OutputNode<TProperty, TValue> : IOutputNode<TValue>
        where TProperty : Property.Property, new()
    {
        IReadOnlyProperty IOutputNode.Output => output;

        public Type OutputType => typeof(TProperty);

        [SerializeField]
        private StringProperty name = new StringProperty()
        {
            Value = "$MISSING"
        };

        [SerializeField]
        private TProperty output = new TProperty();

        public string Name => name.HasValue ? name : "$MISSING";

        public void Setup()
        {
        }

        public void Refresh()
        {
        }
    }

    public interface IOutputNode : IVisualisationNode
    {
        string Name { get; }

        IReadOnlyProperty Output { get; }

        Type OutputType { get; }
    }

    public interface IOutputNode<TType> : IOutputNode
    {
    }
}