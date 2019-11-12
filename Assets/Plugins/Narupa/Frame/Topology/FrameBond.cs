namespace Narupa.Frame.Topology
{
    /// <summary>
    /// A bond derived from data provided in a <see cref="Frame" />.
    /// </summary>
    internal class FrameBond : FrameObject, IReadOnlyBond
    {
        /// <inheritdoc cref="IReadOnlyBond.Index" />
        int IReadOnlyBond.Index => Index;

        /// <inheritdoc cref="IReadOnlyBond.Order" />
        public int Order => Frame.BondOrders?[Index] ?? 1;

        /// <inheritdoc cref="IReadOnlyBond.A" />
        IReadOnlyParticle IReadOnlyBond.A => A;

        /// <inheritdoc cref="IReadOnlyBond.B" />
        IReadOnlyParticle IReadOnlyBond.B => B;

        internal FrameBond(FrameTopology topology, int index) : base(topology, index)
        {
        }

        /// <inheritdoc cref="IReadOnlyBond.A" />
        public FrameParticle A { get; private set; }

        /// <inheritdoc cref="IReadOnlyBond.B" />
        public FrameParticle B { get; private set; }

        /// <summary>
        /// Assign the two particles involved in this bond.
        /// </summary>
        public void AssignParticles(FrameParticle a, FrameParticle b)
        {
            A = a;
            B = b;
        }
    }
}