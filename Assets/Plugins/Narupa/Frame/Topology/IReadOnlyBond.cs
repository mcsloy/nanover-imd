namespace Narupa.Frame.Topology
{
    public interface IReadOnlyBond
    {
        int Index { get; }

        int Order { get; }

        IReadOnlyParticle A { get; }

        IReadOnlyParticle B { get; }
    }
}