// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using UnityEngine;
using UnityEngine.Serialization;

namespace NarupaIMD.Interaction
{
    /// <summary>
    /// Renders Mike's pretty sine wave between two points. (From Narupa 1)
    /// </summary>
    public class InteractionWaveRenderer : MonoBehaviour
    {
#pragma warning disable 0649
        [FormerlySerializedAs("narupaXR")]
        [SerializeField]
        private NarupaIMDPrototype narupa;
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

        private Vector3 startPosition, endPosition;
        private float currentForceMagnitude;

        private Vector3 direction;
        private float maxForceMagnitude;

        private void Update()
        {
           UpdateLineColors();
                UpdateLinePositions();
        }

        private void UpdateLinePositions()
        {
            var intensity = Mathf.Clamp01(currentForceMagnitude / maxForceMagnitude);
            var width = Mathf.Clamp(intensity * maxLineWidth, minLineWidth, maxLineWidth);
            lineRenderer.widthMultiplier = width;

            var direction = startPosition - endPosition;
            var positionCount = (int) Mathf.Clamp(direction.magnitude * sectionsPerNm, 2f, 50f);
            lineRenderer.positionCount = positionCount;

            for (var i = 0; i < positionCount; i++)
            {
                var posOnLine = endPosition + (float) i / lineRenderer.positionCount * direction;
                var sineGoodness = height * Mathf.Sin(frequency * i + speedMultiplier * Time.time);
                posOnLine.y += sineGoodness;
                lineRenderer.SetPosition(i, posOnLine);
            }
        }

        private void UpdateLineColors()
        {
            if (currentForceMagnitude > maxForceMagnitude)
                maxForceMagnitude = 1.2f * currentForceMagnitude;

            var gradient = lineRenderer.colorGradient;
            var colorKeys = gradient.colorKeys;

            var intensity = Mathf.Clamp01(currentForceMagnitude / maxForceMagnitude);

            for (var i = 0; i < gradient.colorKeys.Length; i++)
            {
                var minColor = gradient.colorKeys[i].color;
                var maxColor = forceGradient.colorKeys[i].color;

                colorKeys[i].color = Color.Lerp(minColor, maxColor, intensity);
            }

            gradient.SetKeys(colorKeys, gradient.alphaKeys);
            lineRenderer.colorGradient = gradient;
        }

        public void SetPositionAndForce(Vector3 startPoint, Vector3 endPoint, float force)
        {
            startPosition = startPoint;
            endPosition = endPoint;
            currentForceMagnitude = force;
        }
    }
}
