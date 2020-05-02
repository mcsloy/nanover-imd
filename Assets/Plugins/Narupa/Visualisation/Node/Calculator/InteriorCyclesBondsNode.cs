using System;
using System.Collections.Generic;
using Narupa.Frame;
using Narupa.Visualisation.Properties.Collections;
using UnityEngine;

namespace Narupa.Visualisation.Node.Calculator
{
    /// <summary>
    /// Generates all internal bonds requires for a cycle.
    /// </summary>
    [Serializable]
    public class InteriorCyclesBondsNode : SingleOutputNode<BondArrayProperty>
    {
        [SerializeField]
        private SelectionArrayProperty cycles = new SelectionArrayProperty();

        protected override bool IsInputValid => cycles.HasValue;
        protected override bool IsInputDirty => cycles.IsDirty;

        protected override void ClearDirty()
        {
            cycles.IsDirty = false;
        }

        protected override void UpdateOutput()
        {
            var bonds = new List<BondPair>();

            foreach (var cycle in cycles.Value)
            {
                for (var i = 0; i < cycle.Count - 2; i++)
                {
                    for (var j = i + 2; j < cycle.Count; j++)
                    {
                        bonds.Add(new BondPair(cycle[i], cycle[j]));
                    }
                }
            }

            output.Value = bonds.ToArray();
        }
    }
}