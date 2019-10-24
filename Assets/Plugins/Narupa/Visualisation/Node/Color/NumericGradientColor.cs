using System;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Color
{
    [Serializable]
    public class NumericGradientColor : VisualiserColor
    {
        [SerializeField]
        private FloatArrayProperty input;

        [SerializeField]
        private GradientProperty gradient;

        [SerializeField]
        private FloatProperty minimumValue;

        [SerializeField]
        private FloatProperty maximumValue;

        protected override bool IsInputValid => input.HasNonEmptyValue()
                                             && gradient.HasNonNullValue()
                                             && maximumValue.HasNonNullValue()
                                             && maximumValue.HasNonNullValue();

        protected override bool IsInputDirty => input.IsDirty
                                             || gradient.IsDirty
                                             || maximumValue.IsDirty
                                             || minimumValue.IsDirty;

        protected override void ClearDirty()
        {
            input.IsDirty = false;
            gradient.IsDirty = false;
            minimumValue.IsDirty = false;
            maximumValue.IsDirty = false;
        }

        protected override void UpdateOutput()
        {
            var colorArray = colors.HasValue ? colors.Value : new UnityEngine.Color[0];
            var input = this.input.Value;
            var gradient = this.gradient.Value;
            var minimumValue = this.minimumValue.Value;
            var range = (maximumValue.Value - minimumValue);
            Array.Resize(ref colorArray, input.Length);
            for (var i = 0; i < input.Length; i++)
                colorArray[i] = gradient.Evaluate((input[i] - minimumValue) / range);
            colors.Value = colorArray;
        }

        protected override void ClearOutput()
        {
            colors.Value = null;
        }
    }
}