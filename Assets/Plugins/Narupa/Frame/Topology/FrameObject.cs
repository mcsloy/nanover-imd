namespace Narupa.Frame.Topology
{
    /// <summary>
    /// A general object (bond, particle, etc.) that is derived from a
    /// <see cref="Frame" />.
    /// </summary>
    internal abstract class FrameObject
    {
        private readonly FrameTopology topology;
        private readonly Frame frame;
        private readonly int index;

        protected FrameObject(FrameTopology topology, int index)
        {
            this.topology = topology;
            frame = topology.Frame;
            this.index = index;
        }

        /// <summary>
        /// The <see cref="Frame" /> this is derived from.
        /// </summary>
        public Frame Frame => frame;

        /// <summary>
        /// The index of this object in the frame.
        /// </summary>
        public int Index => index;

        /// <summary>
        /// The topology this object belongs to.
        /// </summary>
        public FrameTopology Topology => topology;
    }
}