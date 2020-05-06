using System;
using JetBrains.Annotations;
using Narupa.Visualisation.Properties;
using Narupa.Visualisation.Properties.Collections;
using UnityEngine;

namespace Narupa.Visualisation.Node.Spline
{
    [Serializable]
    public class CurvedBondNode : GenericOutputNode
    {
        [SerializeField]
        private Vector3ArrayProperty particlePositions = new Vector3ArrayProperty();

        [SerializeField]
        private ColorProperty defaultColor = new ColorProperty();
        
        [SerializeField]
        private ColorArrayProperty particleColors = new ColorArrayProperty();

        [SerializeField]
        private FloatProperty particleScale = new FloatProperty();
        
        [SerializeField]
        private FloatArrayProperty particleScales = new FloatArrayProperty();
        
        [SerializeField]
        private BondArrayProperty bondPairs = new BondArrayProperty();

        [NotNull]
        private SplineArrayProperty splineSegment = new SplineArrayProperty();

        protected override bool IsInputValid => particlePositions.HasValue && bondPairs.HasValue;

        protected override bool IsInputDirty =>
            particlePositions.IsDirty || bondPairs.IsDirty || particleColors.IsDirty || defaultColor.IsDirty || particleScale.IsDirty || particleScales.IsDirty;

        private Vector3[] offsetArray = new Vector3[0];
        private int[] offsetCount = new int[0];

        protected override void ClearDirty()
        {
            particlePositions.IsDirty = false;
            bondPairs.IsDirty = false;
        }

        protected override void UpdateOutput()
        {
            var particlePositions = this.particlePositions.Value;
          
            if (bondPairs.IsDirty)
            {
                Array.Resize(ref offsetArray, particlePositions.Length);
                Array.Resize(ref offsetCount, particlePositions.Length);
            }

            var bondcount = bondPairs.Value.Length;
            var particleCount = particlePositions.Length;
            
            for (var i = 0; i < particleCount; i++)
            {
                offsetArray[i] = Vector3.zero;
                offsetCount[i] = 0;
            }

            foreach (var bond in bondPairs)
            {
                var a = bond.A;
                var b = bond.B;
                var dir = particlePositions[b] - particlePositions[a];
                offsetArray[a] += dir;
                offsetArray[b] -= dir;
                offsetCount[a]++;
                offsetCount[b]++;
            }

            this.splineSegment.Resize(bondcount);
            var splineSegment = this.splineSegment.Value;
           
            var hasColors = this.particleColors.HasValue;
            var particleColors = hasColors ?  this.particleColors.Value : null;

            var hasScales = this.particleScales.HasValue;
            var particleScales = hasScales ?  this.particleScales.Value : null;

            
            var defaultColor = this.defaultColor.HasValue ? this.defaultColor.Value : UnityEngine.Color.white;

            var defaultScale =
                particleScale.HasValue ? particleScale.Value : 1f;
            
            var l = 0;
            foreach (var bond in bondPairs)
            {
                var offset1 = offsetArray[bond.A];
                var offset2 = offsetArray[bond.B];
                var offsetC1 = offsetCount[bond.A];
                var offsetC2 = offsetCount[bond.B];

                var pos1 = particlePositions[bond.A];
                var pos2 = particlePositions[bond.B];
                var dir = pos2 - pos1;

                var tan1 = dir;
                var tan2 = dir;

                if (offsetC1 > 1)
                {
                    var mag = offset1.magnitude / offsetC1;
                    tan1 -= Mathf.Clamp01(15f * mag) * Vector3.Project(tan1, offset1 / offsetC1);
                }

                if (offsetC2 > 1)
                {
                    var mag = offset2.magnitude / offsetC2;
                    tan2 -= Mathf.Clamp01(15f * mag) *  Vector3.Project(tan2, offset2 / offsetC2);
                }

                var norm1 = Vector3.Cross(Vector3.up, tan1).normalized;
                var norm2 = Vector3.Cross(Vector3.up, tan2).normalized;

                var color1 = hasColors
                                 ? particleColors[bond.A]
                                 : defaultColor;

                var color2 = hasColors
                                 ? particleColors[bond.B]
                                 : defaultColor;

                var scale1 = hasScales
                                 ? particleScales[bond.A]
                                 : defaultScale;

                var scale2 = hasScales
                                 ? particleScales[bond.B]
                                 : defaultScale;
                
                splineSegment[l] = new SplineSegment
                {
                    StartPoint = pos1,
                    EndPoint = pos2,
                    StartTangent = tan1,
                    EndTangent = tan2,
                    StartNormal = norm1,
                    EndNormal = norm2,
                    StartColor = color1,
                    EndColor = color2,
                    StartScale = scale1 * Vector3.one,
                    EndScale = scale2 * Vector3.one
                };

                l++;
            }

            this.splineSegment.Value = splineSegment;
        }

        protected override void ClearOutput()
        {
            splineSegment.UndefineValue();
        }
    }
}