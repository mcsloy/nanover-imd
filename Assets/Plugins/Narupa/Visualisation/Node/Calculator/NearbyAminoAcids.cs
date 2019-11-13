using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Core.Science;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Calculator
{
    /// <summary>
    /// Calculate the indices of nearby amino acids.
    /// </summary>
    [Serializable]
    public class NearbyAminoAcids : GenericOutputNode
    {
        [SerializeField]
        private TopologyProperty topology = new TopologyProperty();

        [SerializeField]
        private Vector3ArrayProperty positions = new Vector3ArrayProperty();

        [SerializeField]
        private IntProperty ligandEntityIndex = new IntProperty();

        [SerializeField]
        private FloatProperty maximumDistance = new FloatProperty();

        [SerializeField]
        private FloatProperty minimumDistance = new FloatProperty();

        private IntArrayProperty nearbyResidues = new IntArrayProperty();

        protected override bool IsInputValid => topology.HasNonNullValue()
                                             && positions.HasNonEmptyValue()
                                             && ligandEntityIndex.HasNonNullValue()
                                             && maximumDistance.HasNonNullValue()
                                             && minimumDistance.HasNonNullValue();

        protected override bool IsInputDirty => topology.IsDirty
                                             || positions.IsDirty
                                             || ligandEntityIndex.IsDirty
                                             || maximumDistance.IsDirty
                                             || minimumDistance.IsDirty;

        protected void CalculateLigandPosition()
        {
            var entity = topology.Value.Entities[ligandEntityIndex.Value];
            var count = 0;
            var pos = Vector3.zero;
            foreach (var residue in entity.Residues)
            foreach (var particle in residue.Particles)
            {
                pos += Vector3.zero;
                count++;
            }

            pos /= count;
            ligandPosition = pos;
        }

        private Vector3 ligandPosition;

        private List<int> nearbyAminoAcids = new List<int>();

        protected void CalculateNearbyAminoAcids()
        {
            var toAdd = new List<int>();
            var toRemove = new List<int>();
            var maximumDistance = this.maximumDistance.Value;
            var minimumDistance = this.minimumDistance.Value;
            foreach (var entity in topology.Value.Entities)
            {
                if (entity.Index == ligandEntityIndex.Value)
                    continue;

                foreach (var residue in entity.Residues)
                {
                    if (!AminoAcid.IsStandardAminoAcid(residue.Name))
                        continue;

                    var alphaCarbon = residue.FindParticleByName("CA");

                    var aminoAcidPosition = positions.Value[alphaCarbon.Index];

                    var distance = Vector3.Distance(ligandPosition, aminoAcidPosition);

                    if (nearbyAminoAcids.Contains(residue.Index) && distance > maximumDistance)
                    {
                        toRemove.Add(residue.Index);
                    }
                    else if (distance < minimumDistance)
                    {
                        toAdd.Add(residue.Index);
                    }
                }
            }

            if (toAdd.Any() || toRemove.Any())
            {
                foreach (var a in toRemove)
                    nearbyAminoAcids.Remove(a);
                nearbyAminoAcids.AddRange(toAdd);
            }
        }

        protected override void ClearDirty()
        {
            topology.IsDirty = false;
            positions.IsDirty = false;
            maximumDistance.IsDirty = false;
            minimumDistance.IsDirty = false;
            ligandEntityIndex.IsDirty = false;
        }

        protected override void UpdateOutput()
        {
            CalculateLigandPosition();
            CalculateNearbyAminoAcids();
            nearbyResidues.Value = nearbyAminoAcids.ToArray();
        }

        protected override void ClearOutput()
        {
            nearbyResidues.UndefineValue();
        }
    }
}