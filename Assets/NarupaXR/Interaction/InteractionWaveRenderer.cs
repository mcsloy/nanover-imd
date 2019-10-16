// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using UnityEngine;

namespace NarupaXR.Interaction
{
    /// <summary>
    /// Renders Mike's pretty sine wave between two points. (From Narupa 1)
    /// </summary>
    public class InteractionWaveRenderer : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private NarupaXRPrototype narupaXR;
        [SerializeField]
        private LineRenderer lineRenderer;

        [Header("Config")]
        [SerializeField]
        private float sectionsPerNm = 200f;
        [SerializeField]
        private float speedMultiplier = 5f;
        [SerializeField]
        private float maxLineWidth = 0.1f;
        [SerializeField]
        private float minLineWidth = 0.01f;
        [SerializeField]
        private float height = 0.2f;
        [SerializeField]
        private float frequency = 2f;
        [SerializeField]
        private Gradient forceGradient;
#pragma warning restore 0649

        public Vector3 StartPosition, EndPosition;
        public float CurrentForceMagnitude;

        private Vector3 direction;
        private float MaxForceMagnitude;

        private void Update()
        {
            UpdateLineColors();
            UpdateLinePositions();
        }

        private void UpdateLinePositions()
        {
            var intensity = Mathf.Clamp01(CurrentForceMagnitude / MaxForceMagnitude);
            var width = Mathf.Clamp(intensity * maxLineWidth, minLineWidth, maxLineWidth);
            lineRenderer.widthMultiplier = width;

            var direction = StartPosition - EndPosition;
            var positionCount = (int) Mathf.Clamp(direction.magnitude * sectionsPerNm, 2f, 50f);
            lineRenderer.positionCount = positionCount;

            for (var i = 0; i < positionCount; i++)
            {
                var posOnLine = EndPosition + (float) i / lineRenderer.positionCount * direction;
                var sineGoodness = height * Mathf.Sin(frequency * i + speedMultiplier * Time.time);
                posOnLine.y += sineGoodness;
                lineRenderer.SetPosition(i, posOnLine);
            }
        }

        private void UpdateLineColors()
        {
            if (CurrentForceMagnitude > MaxForceMagnitude)
                MaxForceMagnitude = 1.2f * CurrentForceMagnitude;

            var gradient = lineRenderer.colorGradient;
            var colorKeys = gradient.colorKeys;

            var intensity = Mathf.Clamp01(CurrentForceMagnitude / MaxForceMagnitude);

            for (var i = 0; i < gradient.colorKeys.Length; i++)
            {
                var minColor = gradient.colorKeys[i].color;
                var maxColor = forceGradient.colorKeys[i].color;

                colorKeys[i].color = Color.Lerp(minColor, maxColor, intensity);
            }

            gradient.SetKeys(colorKeys, gradient.alphaKeys);
            lineRenderer.colorGradient = gradient;
        }
    }
}
