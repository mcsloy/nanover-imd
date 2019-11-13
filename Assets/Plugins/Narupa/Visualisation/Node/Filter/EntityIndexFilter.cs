using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Filter
{
    /// <summary>
    /// Select an entity with the given index.
    /// </summary>
    [Serializable]
    public class EntityIndexFilter : VisualiserFilter
    {
        [SerializeField]
        private TopologyProperty topology;

        [SerializeField]
        private IntProperty entityIndex;

        protected override bool IsInputValid => entityIndex.HasNonNullValue()
                                             && topology.HasNonNullValue();

        protected override bool IsInputDirty => topology.IsDirty
                                             || entityIndex.IsDirty;

        protected override void ClearDirty()
        {
            topology.IsDirty = false;
            entityIndex.IsDirty = false;
        }

        protected override int MaximumFilterCount => topology
                                                     .Value.Entities[entityIndex.Value].Residues
                                                     .Sum(res => res.Particles.Count);

        protected override IEnumerable<int> GetFilteredIndices()
        {
            var entity = topology.Value.Entities[entityIndex.Value];
            foreach(var residue in entity.Residues)
            foreach (var particle in residue.Particles)
                yield return particle.Index;
        }
    }
}