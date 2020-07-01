using System;
using System.Collections.Generic;

namespace NarupaIMD.Selection
{
    public interface IParticleVisualisation
    {
        float Priority { get; }
        event Action SelectionUpdated;
        event Action Removed;
        IReadOnlyList<int> ParticleIndices { get; }
        bool Hide { get; }
        object Visualiser { get; }
        string DisplayName { get; }
        int Layer { get; }
    }
}