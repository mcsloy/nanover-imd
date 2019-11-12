using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Core.Science;
using Narupa.Frame;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Calculator
{
    [Serializable]
    public class HydrophobicBonds : GenericOutputNode
    {
        [SerializeField]
        private TopologyProperty topology = new TopologyProperty();

        private BondArrayProperty bonds = new BondArrayProperty();

        [SerializeField]
        private IntProperty ligandEntityIndex = new IntProperty
        {
            Value = 1
        };

        [SerializeField]
        private Vector3ArrayProperty particlePositions = new Vector3ArrayProperty();

        protected override bool IsInputValid => topology.HasNonNullValue()
                                             && ligandEntityIndex.HasValue
                                             && particlePositions.HasNonNullValue();

        protected override bool IsInputDirty => topology.IsDirty
                                             || ligandEntityIndex.IsDirty
                                             || particlePositions.IsDirty;

        protected override void ClearDirty()
        {
            topology.IsDirty = false;
            ligandEntityIndex.IsDirty = false;
            particlePositions.IsDirty = false;
        }

        protected override void UpdateOutput()
        {
            var bondPairs = new List<BondPair>();
            var ligand = topology.Value.Entities[ligandEntityIndex];
            foreach (var entity in topology.Value.Entities)
            {
                if (entity == ligand)
                    continue;
                foreach (var ligandResidue in ligand.Residues)
                foreach (var ligandParticle in ligandResidue.Particles)
                {
                    if (ligandParticle.Element != Element.Carbon)
                        continue;
                    if (ligandParticle.BondedParticles.Any(p => p.Element != Element.Hydrogen
                                                             && p.Element != Element.Carbon))
                        continue;
                    var ligandPos = particlePositions.Value[ligandParticle.Index];
                    foreach (var proteinResidue in entity.Residues)
                    foreach (var proteinParticle in proteinResidue.Particles)
                    {
                        if (proteinParticle.Element != Element.Carbon)
                            continue;
                        if (proteinParticle.BondedParticles.Any(p => p.Element != Element.Hydrogen
                                                                  && p.Element != Element.Carbon))
                            continue;
                        var proteinPos = particlePositions.Value[proteinParticle.Index];
                        if (Vector3.Distance(ligandPos, proteinPos) < 0.4f)
                            bondPairs.Add(
                                new BondPair(ligandParticle.Index, proteinParticle.Index));
                    }
                }
            }

            bonds.Value = bondPairs.ToArray();
        }

        protected override void ClearOutput()
        {
            bonds.UndefineValue();
        }
    }
}