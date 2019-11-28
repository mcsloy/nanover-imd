using System;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Color
{
    /// <summary>
    /// Fills an array with a solid color.
    /// </summary>
    [Serializable]
    public class SolidColorNode : VisualiserColorNode
    {
        [SerializeField]
        private IntProperty count;

        [SerializeField]
        private ColorProperty color;

        protected override bool IsInputValid => count.HasValue && color.HasValue;
        protected override bool IsInputDirty => count.IsDirty || color.IsDirty;

        protected override void ClearDirty()
        {
            count.IsDirty = false;
            color.IsDirty = false;
        }

        protected override void UpdateOutput()
        {
            var arr = colors.HasValue ? colors.Value : new UnityEngine.Color[0];
            var count = this.count.Value;
            Array.Resize(ref arr, count);
            for (var i = 0; i < count; i++)
                arr[i] = color.Value;
            colors.Value = arr;
        }

        protected override void ClearOutput()
        {
            colors.UndefineValue();
        }
    }
}