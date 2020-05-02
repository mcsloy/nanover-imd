using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Core;
using Narupa.Core.Science;
using Narupa.Frame;
using Narupa.Protocol.Trajectory;
using Narupa.Visualisation.Components;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Adaptor
{
    /// <summary>
    /// An Adaptor node is a node which can provide any property. This allows it to act
    /// as a gateway, for both connecting the visualisation system to external sources
    /// (such as a frame source) and for filtering out values.
    /// </summary>
    public abstract class BaseAdaptorNode : VisualisationNode, IDynamicPropertyProvider
    {
        /// <inheritdoc cref="Properties" />
        private Dictionary<string, IProperty> properties =
            new Dictionary<string, IProperty>();
        
        public override void Setup()
        {
            base.Setup();
            properties = properties ?? new Dictionary<string, IProperty>();
        }

        /// <summary>
        /// Dynamic properties created by the system, with the keys corresponding to the
        /// keys in the frame's data.
        /// </summary>
        public IReadOnlyDictionary<string, IProperty> Properties => properties;

        /// <inheritdoc cref="IDynamicPropertyProvider.GetOrCreateProperty{T}" />
        public virtual IReadOnlyProperty<T> GetOrCreateProperty<T>(string name)
        {
            if (GetProperty(name) is IReadOnlyProperty<T> existing)
                return existing;

            var property = new SerializableProperty<T>();
            properties[name] = property;
            OnCreateProperty(name, property);
            return property;
        }

        /// <inheritdoc cref="IDynamicPropertyProvider.GetPotentialProperties" />
        public IEnumerable<(string name, Type type)> GetPotentialProperties()
        {
            return StandardFrameKeys.All.Select(a => (a.Key, a.Type));
        }

        /// <inheritdoc cref="IDynamicPropertyProvider.CanDynamicallyProvideProperty{T}" />
        public bool CanDynamicallyProvideProperty<T>(string name)
        {
            return true;
        }

        /// <inheritdoc cref="IDynamicPropertyProvider.GetProperty" />
        public virtual IReadOnlyProperty GetProperty(string key)
        {
            return properties.TryGetValue(key, out var value)
                       ? value
                       : null;
        }

        /// <inheritdoc cref="IDynamicPropertyProvider.GetProperties" />
        public virtual IEnumerable<(string name, IReadOnlyProperty property)> GetProperties()
        {
            foreach (var (key, value) in properties)
                yield return (key, value);
        }

        /// <summary>
        /// Callback when a property has been requested, created and added to the
        /// <see cref="Properties" /> dictionary, but has not yet been returned to the
        /// requestor.
        /// </summary>
        protected virtual void OnCreateProperty<T>(string key, IProperty<T> property)
        {
        }

        /// <summary>
        /// Array of elements of the provided frame.
        /// </summary>
        public IReadOnlyProperty<Element[]> ParticleElements =>
            GetOrCreateProperty<Element[]>(FrameData.ParticleElementArrayKey);

        /// <summary>
        /// Array of particle positions of the provided frame.
        /// </summary>
        public IReadOnlyProperty<Vector3[]> ParticlePositions =>
            GetOrCreateProperty<Vector3[]>(FrameData.ParticlePositionArrayKey);

        /// <summary>
        /// Array of bonds of the provided frame.
        /// </summary>
        public IReadOnlyProperty<BondPair[]> BondPairs =>
            GetOrCreateProperty<BondPair[]>(FrameData.BondArrayKey);

        /// <summary>
        /// Array of bond orders of the provided frame.
        /// </summary>
        public IReadOnlyProperty<int[]> BondOrders =>
            GetOrCreateProperty<int[]>(FrameData.BondOrderArrayKey);

        /// <summary>
        /// Array of particle residues of the provided frame.
        /// </summary>
        public IReadOnlyProperty<int[]> ParticleResidues =>
            GetOrCreateProperty<int[]>(FrameData.ParticleResidueArrayKey);

        /// <summary>
        /// Array of particle names of the provided frame.
        /// </summary>
        public IReadOnlyProperty<string[]> ParticleNames =>
            GetOrCreateProperty<string[]>(FrameData.ParticleNameArrayKey);

        /// <summary>
        /// Array of residue names of the provided frame.
        /// </summary>
        public IReadOnlyProperty<string[]> ResidueNames =>
            GetOrCreateProperty<string[]>(FrameData.ResidueNameArrayKey);

        /// <summary>
        /// Array of residue entities of the provided frame.
        /// </summary>
        public IReadOnlyProperty<int[]> ResidueEntities =>
            GetOrCreateProperty<int[]>(FrameData.ResidueChainArrayKey);

        /// <summary>
        /// Array of residue entities of the provided frame.
        /// </summary>
        public IReadOnlyProperty<int> ResidueCount =>
            GetOrCreateProperty<int>(FrameData.ResidueCountValueKey);
    }
}