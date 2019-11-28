using System;
using System.Linq;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Color
{
    /// <summary>
    /// Applies an oscillating color change to a subset of particles.
    /// </summary>
    [Serializable]
    public class ColorPulserNode
    {
        [SerializeField]
        private ColorArrayProperty inputColors = new ColorArrayProperty();

        private ColorArrayProperty outputColors = new ColorArrayProperty();

        [SerializeField]
        private IntArrayProperty filter = new IntArrayProperty();

        [SerializeField]
        private float speed = 6f;

        [SerializeField]
        private float maximum = 1f;

        [SerializeField]
        private float minimum = 0f;

        private float t = 0;

        private bool isActive = false;
        private bool isValid = false;

        public bool IsInputDirty => inputColors.IsDirty
                                 || filter.IsDirty;

        private float darken = 1f;

        private float targetDarken = 1f;
        
        public void Refresh()
        {
            if (IsInputDirty)
            {
                isActive = (filter.HasNonEmptyValue() || !filter.HasValue);
                isValid = inputColors.HasNonNullValue();

                // Reset counter when there are no highlights to draw
                if (filter.IsDirty && !filter.HasNonEmptyValue())
                    t = 0;

                if (isValid)
                {
                    var arr = new UnityEngine.Color[inputColors.Value.Length];
                    outputColors.Value = arr;
                    Array.Copy(inputColors.Value, outputColors.Value, inputColors.Value.Length);
                    outputColors.MarkValueAsChanged();
                }
                else
                {
                    outputColors.UndefineValue();
                }

                filter.IsDirty = false;
                inputColors.IsDirty = false;
            }

            targetDarken = isActive ? 0.8f : 1f;

            if (isValid && (isActive || targetDarken != darken))
            {
                darken = Mathf.MoveTowards(darken, targetDarken, Time.deltaTime * 5f);
                t += Time.deltaTime * speed;
                var intensity = Mathf.Lerp(minimum, maximum, 0.5f + 0.5f * Mathf.Sin(t));
                Array.Copy(inputColors.Value, outputColors.Value, inputColors.Value.Length);

                // Only change colors when running.
                if (Application.isPlaying)
                {
                    for (var i = 0; i < inputColors.Value.Length; i++)
                        outputColors.Value[i] *= darken;

                    var filter = this.filter.HasNonNullValue()
                                     ? this.filter.Value
                                     : Enumerable.Range(0, inputColors.Value.Length);
                    foreach (var i in filter)
                    {
                        outputColors.Value[i] += intensity * UnityEngine.Color.white;
                    }
                }

                outputColors.MarkValueAsChanged();
            }
        }
    }
}