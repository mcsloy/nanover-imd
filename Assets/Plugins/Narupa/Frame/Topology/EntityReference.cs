using System.Collections.Generic;

namespace Narupa.Frame.Topology
{
    internal class EntityReference : ObjectReference, IReadOnlyEntity
    {
        IReadOnlyCollection<IReadOnlyResidue> IReadOnlyEntity.Residues => residues;

        string IReadOnlyEntity.Name => Name;

        int IReadOnlyEntity.Index => Index;

        public EntityReference(FrameTopology topology, int index) : base(topology, index)
        {
        }

        private List<ResidueReference> residues = new List<ResidueReference>();

        public string Name => Frame.ResidueNames[Index];

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