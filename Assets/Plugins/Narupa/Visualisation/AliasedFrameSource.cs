// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Narupa.Frame;
using Narupa.Frame.Event;
using System.Collections.Generic;
using UnityEngine;

namespace Narupa.Visualisation
{
    /// <summary>
    /// Interface between a <see cref="ITrajectorySnapshot" /> and Unity. Frontend
    /// specific tasks such as rendering should utilise this to track the frame, as it
    /// delays frame updating to the main thread.
    /// </summary>
    [DisallowMultipleComponent]
    public class AliasedFrameSource : MonoBehaviour,
                                           ITrajectorySnapshot,
                                           IFrameConsumer
    {
        /// <inheritdoc cref="ITrajectorySnapshot.CurrentFrame" />
        public Frame.Frame CurrentFrame => snapshot?.CurrentFrame;

        public string Alias = "trace.";

        /// <inheritdoc cref="ITrajectorySnapshot.FrameChanged" />
        public event FrameChanged FrameChanged;

        private ITrajectorySnapshot snapshot;

        /// <summary>
        /// Source for the frames to be displayed.
        /// </summary>
        public ITrajectorySnapshot FrameSource
        {
            set
            {
                snapshot = value;

                Dictionary<string, object> changes = new Dictionary<string, object>();

                if (snapshot != null) 
                {
                    foreach (KeyValuePair<string, object> keyValuePair in snapshot.CurrentFrame.Data)
                    {
                        if (keyValuePair.Key.StartsWith(Alias))
                        {
                            string nonAliasedKey = keyValuePair.Key.Remove(0, Alias.Length);
                            
                            if (snapshot.CurrentFrame.Data.ContainsKey(nonAliasedKey))
                                changes[nonAliasedKey] = keyValuePair.Value;
                        }
                    }

                    foreach (KeyValuePair<string, object> keyValuePair in changes) 
                        snapshot.CurrentFrame.Data[keyValuePair.Key] = keyValuePair.Value;
                }
            }
        }

        /// <summary>
        /// Callback when a frame is updated, which can happen outwith the main Unity
        /// thread.
        /// </summary>
        private void SnapshotOnFrameChanged(IFrame frame, FrameChanges changes)
        {

        }

        private void Update()
        {

        }

        /// <summary>
        /// Raise <see cref="FrameChanged" /> if necessary then clear state to
        /// unchanged.
        /// </summary>
        private void FlushChanges()
        {

        }
    }
}