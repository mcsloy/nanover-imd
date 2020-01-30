using System;
using Narupa.Visualisation.Properties.Collections;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Calculator
{
    [Serializable]
    public class VectorMagnitudeNode : GenericOutputNode
    {
        [SerializeField]
        private Vector3ArrayProperty input = new Vector3ArrayProperty();
        
        private FloatArrayProperty output = new FloatArrayProperty();

        protected override bool IsInputValid => input.HasNonNullValue();
        protected override bool IsInputDirty => input.IsDirty;
        protected override void ClearDirty()
        {
            input.IsDirty = false;
        }

        protected override void UpdateOutput()
        {
            output.Resize(input.Value.Length);
            for (var i = 0; i < input.Value.Length; i++)
            {
                output.Value[i] = input.Value[i].magnitude;
            }
            output.MarkValueAsChanged();
        }

        protected override void ClearOutput()
        {
            output.UndefineValue();
        }
    }
}