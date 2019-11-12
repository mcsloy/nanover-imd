// Copyright (c) Intangible Realities Laboratory. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System.Collections.Generic;
using Narupa.Core.Science;
using Narupa.Frame.Topology;
using UnityEngine;

namespace Narupa.Frame
{
    /// <summary>
    /// A particle derived from data provided in a <see cref="Frame" />.
    /// </summary>
    internal class FrameParticle : FrameObject, IReadOnlyParticle
    {
        /// <inheritdoc cref="IReadOnlyParticle.Bonds" />
        IReadOnlyCollection<IReadOnlyBond> IReadOnlyParticle.Bonds => bonds;

        /// <inheritdoc cref="IReadOnlyParticle.BondedParticles" />
        IReadOnlyCollection<IReadOnlyParticle> IReadOnlyParticle.BondedParticles => bondedParticles;

        /// <inheritdoc cref="IReadOnlyParticle.Residue" />
        IReadOnlyResidue IReadOnlyParticle.Residue => Residue;

        internal FrameParticle(FrameTopology topology, int index) : base(topology, index)
        {
        }

        private List<FrameParticle> bondedParticles = new List<FrameParticle>();
        private List<FrameBond> bonds = new List<FrameBond>();

        /// <inheritdoc cref="IReadOnlyParticle.Type" />
        public string Type => Frame.ParticleTypes?[Index];

        /// <inheritdoc cref="IReadOnlyParticle.Name" />
        public string Name => Frame.ParticleNames?[Index];

        /// <inheritdoc cref="IReadOnlyParticle.Position" />
        public Vector3 Position => Frame.ParticlePositions[Index];

        /// <inheritdoc cref="IReadOnlyParticle.Element" />
        public Element? Element => Frame.ParticleElements?[Index];

        /// <inheritdoc cref="IReadOnlyParticle.Residue" />
        public FrameResidue Residue { get; private set; }

        /// <summary>
        /// Set the residue which contains this particle.
        /// </summary>
        public void SetResidue(FrameResidue reference)
        {
            Residue = reference;
        }

        /// <summary>
        /// Clear the bonds and bonded particles for this particle.
        /// </summary>
        public void ClearBonds()
        {
            bonds.Clear();
            bondedParticles.Clear();
        }

        /// <summary>
        /// Add a bond to this particle.
        /// </summary>
        public void AddBond(FrameBond bond)
        {
            bonds.Add(bond);
            bondedParticles.Add(bond.A == this ? bond.B : bond.A);
        }
    }
}