namespace Narupa.Frame.Topology
{
    internal abstract class ObjectReference
    {
        private readonly Topology topology;
        private readonly Frame frame;
        private readonly int index;

        protected ObjectReference(Topology topology, int index)
        {
            this.topology = topology;
            this.frame = topology.Frame;
            this.index = index;
        }

        public Frame Frame => frame;

        public int Index => index;

        public Topology Topology => topology;
    }
}