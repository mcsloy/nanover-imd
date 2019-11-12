// Copyright (c) Intangible Realities Laboratory. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System.Collections.Generic;
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
    internal class ParticleReference : ObjectReference, IParticle
    {
        public ParticleReference(Topology.Topology topology, int index) : base(topology, index)
        {
        }
        
        private List<ParticleReference> bondedParticles = new List<ParticleReference>();
        private List<BondReference> bonds = new List<BondReference>();

        /// <inheritdoc />
        [CanBeNull]
        public string Type => Frame.ParticleTypes?[Index];

        /// <inheritdoc />
        public Vector3 Position => Frame.ParticlePositions[Index];

        public Element? Element => Frame.ParticleElements?[Index];

        public ResidueReference Residue { get; private set; }

        public void SetResidue(ResidueReference reference)
        {
            Residue = reference;
        }

        public void ClearBonds()
        {
            bonds.Clear();
            bondedParticles.Clear();
        }

        public void AddBond(BondReference bond)
        {
            bonds.Add(bond);
            bondedParticles.Add(bond.A == this ? bond.B : bond.A);
        }
    }
}