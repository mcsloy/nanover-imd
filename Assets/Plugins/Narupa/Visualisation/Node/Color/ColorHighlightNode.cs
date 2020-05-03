using System;
using JetBrains.Annotations;
using Narupa.Visualisation.Components;
using Narupa.Visualisation.Properties;
using Narupa.Visualisation.Properties.Collections;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Color
{
    /// <summary>
    /// Applies an oscillating color change to a subset of particles.
    /// </summary>
    [Serializable]
    public class ColorHighlightNode : VisualisationNode
    {
        [SerializeField]
        private ColorArrayProperty input = new ColorArrayProperty();

        [SerializeField]
        private IntProperty count = new IntProperty();

        [SerializeField]
        private IntArrayProperty filter = new IntArrayProperty();

        [NotNull]
        private ColorArrayProperty output = new ColorArrayProperty();

        [NotNull]
        private UnityEngine.Color[] cachedArray = new UnityEngine.Color[0];

        [SerializeField]
        private float speed = 6f;

        [SerializeField]
        private float maximum = 1f;

        [SerializeField]
        private float minimum = 0f;


        public bool IsInputDirty => input.IsDirty
                                 || filter.IsDirty
                                 || count.IsDirty;

        private float strength = 1f;
        private float targetStrength = 1f;

        private float darkenAmount = 0.8f;

        public void Refresh()
        {
            var isFilterNonZero = false;
            var isOutputValid = false;
            if (IsInputDirty)
            {
                isFilterNonZero = filter.HasNonEmptyValue();
                isOutputValid = input.HasNonNullValue() || count.HasNonNullValue();

                if (input.HasNonNullValue())
                {
                    Array.Resize(ref cachedArray, input.Value.Length);
                    Array.Copy(input.Value, cachedArray, input.Value.Length);

                    output.Resize(cachedArray.Length);
                    Array.Copy(cachedArray, output.Value, cachedArray.Length);
                    output.MarkValueAsChanged();
                }
                else if (count.HasNonNullValue())
                {
                    Array.Resize(ref cachedArray, count.Value);
                    for (var i = 0; i < count.Value; i++)
                        cachedArray[i] = UnityEngine.Color.white;

                    output.Resize(cachedArray.Length);
                    Array.Copy(cachedArray, output.Value, cachedArray.Length);
                    output.MarkValueAsChanged();
                }
                else
                {
                    output.UndefineValue();
                }

                filter.IsDirty = false;
                input.IsDirty = false;
            }

            targetStrength = isFilterNonZero ? 1 : 0;

            if (isOutputValid && (strength != targetStrength || isFilterNonZero))
            {
                strength = Mathf.MoveTowards(strength, targetStrength, speed);
                var darken = Mathf.Lerp(1f, darkenAmount, targetStrength);
                var intensity = Mathf.Lerp(minimum, maximum,
                                           targetStrength *
                                           (0.5f + 0.5f * Mathf.Sin(speed * Time.time)));

                output.Resize(cachedArray.Length);

                for (var i = 0; i < cachedArray.Length; i++)
                    output.Value[i] = cachedArray[i] * darken;

                if (isFilterNonZero)
                {
                    foreach (var i in filter)
                    {
                        output.Value[i] += intensity * UnityEngine.Color.white;
                    }
                }

                output.MarkValueAsChanged();
            }
        }
    }
}