using System;
using Narupa.Visualisation.Properties;
using UnityEngine;

namespace Narupa.Visualisation.Node.Transform
{
    /// <summary>
    /// Transform a float linearly by applying a scale and an offset.
    /// </summary>
    [Serializable]
    public class LinearTransformFloatNode : SingleOutputNode<FloatProperty>
    {
        [SerializeField]
        private FloatProperty input;

        [SerializeField]
        private FloatProperty scale;

        [SerializeField]
        private FloatProperty offset;

        protected override bool IsInputValid => input.HasValue;
        protected override bool IsInputDirty => input.IsDirty || scale.IsDirty || offset.IsDirty;

        protected override void ClearDirty()
        {
            input.IsDirty = false;
            scale.IsDirty = false;
            offset.IsDirty = false;
        }

        protected override void UpdateOutput()
        {
            var value = input.Value;
            if (scale.HasValue)
                value *= scale.Value;
            if (offset.HasValue)
                value += offset.Value;
            output.Value = value;
        }
    }
}