namespace Narupa.Frame.Topology
{
    internal abstract class ObjectReference
    {
        private readonly FrameTopology topology;
        private readonly Frame frame;
        private readonly int index;

        protected ObjectReference(FrameTopology topology, int index)
        {
            this.topology = topology;
            frame = topology.Frame;
            this.index = index;
        }

        public Frame Frame => frame;

        public int Index => index;

        public FrameTopology Topology => topology;
    }
}