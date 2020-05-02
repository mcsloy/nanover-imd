using System;
using Narupa.Visualisation.Properties;
using Narupa.Visualisation.Properties.Collections;
using UnityEngine;

namespace Narupa.Visualisation.Node.Transform
{
    /// <summary>
    /// Transform a float array linearly by applying a scale and an offset.
    /// </summary>
    [Serializable]
    public class LinearTransformFloatArrayNode : SingleOutputNode<FloatArrayProperty>
    {
        [SerializeField]
        private FloatArrayProperty input;

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
            output.Resize(input.Value.Length);
            if (scale.HasValue)
            {
                for (var i = 0; i < output.Value.Length; i++)
                {
                    output.Value[i] *= scale.Value;
                }
            }

            if (offset.HasValue)
            {
                for (var i = 0; i < output.Value.Length; i++)
                {
                    output.Value[i] += offset.Value;
                }
            }

            output.MarkValueAsChanged();
        }
    }
}