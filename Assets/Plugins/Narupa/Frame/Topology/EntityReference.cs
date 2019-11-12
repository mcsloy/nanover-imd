using System.Collections.Generic;
using System.Linq;

namespace Narupa.Frame.Topology
{
    public class EntityReference
    {
        private readonly Frame frame;

        private readonly int index;
        
        private List<int> residueIndices = new List<int>();

        public EntityReference(Frame frame, int index)
        {
            this.frame = frame;
            this.index = index;
        }

        public string Name => frame.ResidueNames[index];

        public IEnumerable<ResidueReference> Residues =>
            residueIndices.Select(frame.GetResidue);

        public void AddResidueIndex(int i)
        {
            residueIndices.Add(i);
        }
    }
}