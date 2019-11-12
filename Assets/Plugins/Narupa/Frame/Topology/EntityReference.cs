using System.Collections.Generic;
using System.Linq;

namespace Narupa.Frame.Topology
{
    internal class EntityReference : ObjectReference
    {
        public EntityReference(Topology topology, int index) : base(topology, index)
        {
        }
        
        private List<ResidueReference> residues = new List<ResidueReference>();

        public string Name => Frame.ResidueNames[Index];

        public IEnumerable<ResidueReference> Residues => residues;

        public void AddResidueIndex(ResidueReference i)
        {
            residues.Add(i);
        }

        public void ClearResidues()
        {
            residues.Clear();
        }

        
    }
}