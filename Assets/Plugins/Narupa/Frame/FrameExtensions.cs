using System.Linq;
using Narupa.Core.Science;

namespace Narupa.Frame
{
    public static class FrameExtensions
    {
        /// <summary>
        /// Recenter particles about the origin.
        /// </summary>
        public static void RecenterAroundOrigin(this Frame frame)
        {
            if (frame.ParticlePositions.Length == 0)
                return;

            var total = frame.ParticlePositions
                             .Aggregate((v, w) => v + w);
            total /= frame.ParticlePositions.Length;
            for (var i = 0; i < frame.ParticlePositions.Length; i++)
                frame.ParticlePositions[i] -= total;
        }
    }
}