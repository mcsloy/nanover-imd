using System;
using Narupa.Visualisation.Properties.Collections;

namespace Narupa.Visualisation.Node.Fraction
{
    /// <summary>
    /// Calculates some 0-1 value based on indices.
    /// </summary>
    [Serializable]
    public abstract class GenericFractionNode : SingleOutputNode<FloatArrayProperty>
    {
        protected abstract override bool IsInputValid { get; }

        protected abstract override bool IsInputDirty { get; }

        protected abstract override void ClearDirty();

        protected abstract void GenerateArray(ref float[] array);

        protected override void UpdateOutput()
        {
            var array = output.HasValue ? output.Value : new float[0];
            GenerateArray(ref array);
            output.Value = array;
        }
    }
}