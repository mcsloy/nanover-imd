using System;
using System.Collections.Generic;

namespace NarupaIMD.Selection
{
    public class DefaultVisualisation : IParticleVisualisation
    {
        public float Priority => 0;
        public event Action SelectionUpdated;
        public event Action Removed;
        public IReadOnlyList<int> ParticleIndices => null;
        public bool Hide => false;
        public object Visualiser => "ball and stick";
        public string DisplayName => "Root Visualisation";
        public int Layer => 0;
    }
}