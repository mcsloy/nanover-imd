using UnityEngine;

namespace Narupa.Visualisation.Components
{
    public abstract class VisualisationNode : IVisualisationNode, ISerializationCallbackReceiver
    {
        public virtual void Setup()
        {
        }

        public virtual void Refresh()
        {
        }

        public virtual void OnBeforeSerialize()
        {
        }

        public virtual void OnAfterDeserialize()
        {
            Setup();
        }
    }
}