// Copyright (c) Intangible Realities Laboratory. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Narupa.Core.Science;
using Narupa.Frame.Topology;
using UnityEngine;

namespace Narupa.Frame
{
    /// <summary>
    /// A particle which exists only as an index, pointing to various arrays which
    /// contain information such as name and type
    /// </summary>
    public class ParticleReference : IParticle
    {
        private readonly Frame frame;
        private readonly int index;

        /// <summary>
        /// Create an ParticleReference that represents the particle at index in a frame
        /// </summary>
        public ParticleReference([NotNull] Frame frame, int index)
        {
            this.frame = frame;
            this.index = index;
        }

        /// <summary>
        /// The topology the particle is a part of
        /// </summary>
        [NotNull]
        public Frame Frame => frame;

        /// <inheritdoc />
        public int Index => index;

        /// <inheritdoc />
        [CanBeNull]
        public string Type => Frame.ParticleTypes?[index];

        /// <inheritdoc />
        public Vector3 Position => Frame.ParticlePositions[index];

        public Element? Element => Frame.ParticleElements?[index];

        public ResidueReference Residue => frame.GetResidue(Frame.ParticleResidues[index]);
    }
}