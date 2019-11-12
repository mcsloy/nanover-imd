namespace Narupa.Frame.Topology
{
    internal class BondReference : ObjectReference, IReadOnlyBond
    {
        int IReadOnlyBond.Index => Index;
        int IReadOnlyBond.Order => Order;
        IReadOnlyParticle IReadOnlyBond.A => A;
        IReadOnlyParticle IReadOnlyBond.B => B;

        public BondReference(FrameTopology topology, int index) : base(topology, index)
        {
        }

        public int Order => Frame.BondOrders?[Index] ?? 1;

        public BondPair Bond => Frame.Bonds[Index];

        public ParticleReference A { get; private set; }

        public ParticleReference B { get; private set; }

        public void ClearParticles()
        {
            A = null;
            B = null;
        }

        public void AssignParticles(ParticleReference a, ParticleReference b)
        {
            A = a;
            B = b;
        }
    }
}