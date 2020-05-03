// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Narupa.Core;
using Narupa.Frame;
using Narupa.Frame.Event;
using Narupa.Visualisation.Property;

namespace Narupa.Visualisation.Node.Adaptor
{
    /// <summary>
    /// Visualisation node which reads frames using <see cref="IFrameConsumer" /> and
    /// dynamically provides the frame's data as properties for the visualisation
    /// system.
    /// </summary>
    /// <remarks>
    /// This visualisation node acts as the bridge between the underlying trajectory
    /// and the visualisation system.
    /// </remarks>
    [Serializable]
    public class FrameAdaptorNode : BaseAdaptorNode, IFrameConsumer, IDisposable
    {
        /// <inheritdoc cref="BaseAdaptorNode.OnCreateProperty{T}" />
        protected override IReadOnlyProperty<T> OnCreateProperty<T>(string key, IProperty<T> property)
        {
            GetPropertyValueFromFrame(key, property);
            return property;
        }

        /// <summary>
        /// Update a property by getting the value with the given key from the current
        /// frame, ignoring if this property is marked as an override.
        /// </summary>
        private void GetPropertyValueFromFrame(string key, IProperty property)
        {
            if (IsPropertyOverriden(key))
                return;
            if (FrameSource?.CurrentFrame != null
             && FrameSource.CurrentFrame.Data.TryGetValue(key, out var value))
            {
                property.TrySetValue(value);
            }
        }

        /// <summary>
        /// Callback for when the frame is changed. Updates the output properties
        /// selectively depending on if the field is marked as having changed.
        /// </summary>
        private void OnFrameUpdated(IFrame frame, FrameChanges changes = null)
        {
            if (frame == null)
                return;

            foreach (var (key, property) in Properties)
            {
                if (changes?.HasChanged(key) ?? true)
                    GetPropertyValueFromFrame(key, property);
            }
        }

        private ITrajectorySnapshot source;

        /// <inheritdoc cref="IFrameConsumer.FrameSource" />
        public ITrajectorySnapshot FrameSource
        {
            get => source;
            set
            {
                if (source != null)
                    source.FrameChanged -= OnFrameUpdated;
                source = value;
                if (source != null)
                {
                    source.FrameChanged += OnFrameUpdated;
                    OnFrameUpdated(source.CurrentFrame);
                }
            }
        }

        public void Dispose()
        {
            FrameSource = null;
            Refresh();
        }
    }
}