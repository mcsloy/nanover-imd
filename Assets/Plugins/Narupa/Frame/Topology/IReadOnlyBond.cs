namespace Narupa.Frame.Topology
{
    /// <summary>
    /// A bond between particles in an n-body system.
    /// </summary>
    public interface IReadOnlyBond
    {
        /// <summary>
        /// The index of the bond in the topology.
        /// </summary>
        int Index { get; }

        /// <summary>
        /// The order of the bond.
        /// </summary>
        int Order { get; }

        /// <summary>
        /// The first particle involved in the bond.
        /// </summary>
        IReadOnlyParticle A { get; }

        /// <summary>
        /// The second particle involved in the bond.
        /// </summary>
        IReadOnlyParticle B { get; }
    }
}