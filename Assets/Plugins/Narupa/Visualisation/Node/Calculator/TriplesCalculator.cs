using System;
using System.Collections.Generic;
using Narupa.Frame;
using Narupa.Visualisation.Node.Renderer;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Calculator
{
    [Serializable]
    public class TriplesCalculator : GenericOutputNode
    {
        [SerializeField]
        private Vector3ArrayProperty positions = new Vector3ArrayProperty();

        private TripleArrayProperty triples = new TripleArrayProperty();

        private BondArrayProperty bonds = new BondArrayProperty();
        
        public float cutoff;
        
        protected override bool IsInputValid => positions.HasValue;
        protected override bool IsInputDirty => positions.IsDirty;
        protected override void ClearDirty()
        {
            positions.IsDirty = false;
        }

        protected override void UpdateOutput()
        {
            var c2 = cutoff * cutoff;
            var positions = this.positions.Value;
            var tri = new List<Triple>();
            var bnds = new List<BondPair>();
            for (var i = 0; i < positions.Length; i++)
            {
                var p0 = positions[i];
                for (var j = i + 1; j < positions.Length; j++)
                {
                    var p1 = positions[j];
                    if ((p1 - p0).sqrMagnitude > c2)
                        continue;
                    bnds.Add(new BondPair(i, j));
                    for (var k = j + 1; k < positions.Length; k++)
                    {
                        var p2 = positions[k];
                        if ((p2 - p0).sqrMagnitude > c2)
                            continue;
                        if ((p2 - p1).sqrMagnitude > c2)
                            continue;
                        tri.Add(new Triple
                        {
                            A = i,
                            B = j,
                            C = k
                        });
                    }
                }
            }

            triples.Value = tri.ToArray();
            bonds.Value = bnds.ToArray();
        }

        protected override void ClearOutput()
        {
            triples.UndefineValue();
            bonds.UndefineValue();
        }
    }
}