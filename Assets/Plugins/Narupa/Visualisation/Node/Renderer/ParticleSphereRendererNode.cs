// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Narupa.Visualisation.Components;
using Narupa.Visualisation.Properties;
using Narupa.Visualisation.Properties.Collections;
using Narupa.Visualisation.Property;
using Narupa.Visualisation.Utility;
using Plugins.Narupa.Visualisation.Node.Renderer;
using UnityEngine;
using UnityEngine.Rendering;

namespace Narupa.Visualisation.Node.Renderer
{
    /// <summary>
    /// Visualisation node for rendering particles using spheres.
    /// </summary>
    /// <remarks>
    /// A call to <see cref="UpdateRenderer" /> should be made every frame. Depending
    /// on use case, either call <see cref="Render" /> to draw for a single frame, or
    /// add to a <see cref="CommandBuffer" /> using
    /// <see cref="AppendToCommandBuffer" />.
    /// </remarks>
    [Serializable]
    public class ParticleSphereRendererNode : IndirectMeshRenderer, IDisposable, IVisualisationNode,
                                              IRenderNode, ISerializationCallbackReceiver
    {
        [NotNull]
        private IndirectMeshDrawCommand drawCommand = new IndirectMeshDrawCommand();

        /// <summary>
        /// Positions of particles.
        /// </summary>
        public IProperty<Vector3[]> ParticlePositions => particlePositions;

        /// <summary>
        /// Color of particles.
        /// </summary>
        public IProperty<UnityEngine.Color[]> ParticleColors => particleColors;

        /// <summary>
        /// Scale of particles.
        /// </summary>
        public IProperty<float[]> ParticleScales => particleScales;

        /// <summary>
        /// Color to use in the absence of per particle colors
        /// </summary>
        public IProperty<UnityEngine.Color> ParticleColor => particleColor;

        /// <summary>
        /// Scale to use in the absence of per particle scales
        /// </summary>
        public IProperty<float> ParticleScale => particleScale;

        public IProperty<Mesh> Mesh => mesh;

        public IProperty<Material> Material => material;

#pragma warning disable 0649
        [SerializeField]
        private Vector3ArrayProperty particlePositions = new Vector3ArrayProperty();

        [SerializeField]
        private ColorArrayProperty particleColors = new ColorArrayProperty();

        [SerializeField]
        private FloatArrayProperty particleScales = new FloatArrayProperty();

        [SerializeField]
        private ColorProperty particleColor = new ColorProperty();

        [SerializeField]
        private FloatProperty particleScale = new FloatProperty();

        [SerializeField]
        private MeshProperty mesh = new MeshProperty();

        [SerializeField]
        private MaterialProperty material = new MaterialProperty();
#pragma warning restore 0649

        public override bool ShouldRender => drawCommand != null
                                          && mesh.HasNonNullValue()
                                          && material.HasNonNullValue()
                                          && particlePositions.HasNonEmptyValue()
                                          && particleColor.HasValue
                                          && particleScale.HasValue
                                          && Transform != null
                                          && InstanceCount > 0;

        private int InstanceCount => particlePositions.Value.Length;

        public override bool IsInputDirty => mesh.IsDirty
                                          || material.IsDirty
                                          || particleColor.IsDirty
                                          || particleScale.IsDirty
                                          || particlePositions.IsDirty
                                          || particleColors.IsDirty
                                          || particleScales.IsDirty;


        public override void UpdateInput()
        {
            UpdateMeshAndMaterials();

            drawCommand.SetInstanceCount(InstanceCount);
            drawCommand.SetFloat("_Scale", particleScales.HasValue ? 1f : particleScale.Value);
            drawCommand.SetColor("_Color", particleColors.HasValue ? UnityEngine.Color.white : particleColor.Value);
            particleScale.IsDirty = false;
            particleColor.IsDirty = false;

            UpdateBuffers();
        }

        protected virtual void UpdateBuffers()
        {
            UpdatePositionsIfDirty();
            UpdateColorsIfDirty();
            UpdateScalesIfDirty();
        }

        private void UpdatePositionsIfDirty()
        {
            if (particlePositions.IsDirty && particlePositions.HasNonEmptyValue())
            {
                InstancingUtility.SetPositions(drawCommand, particlePositions.Value);

                particlePositions.IsDirty = false;
            }
        }

        private void UpdateColorsIfDirty()
        {
            if (particleColors.IsDirty && particleColors.HasNonEmptyValue())
            {
                InstancingUtility.SetColors(drawCommand, particleColors.Value);

                particleColors.IsDirty = false;
            }
        }

        private void UpdateScalesIfDirty()
        {
            if (particleScales.IsDirty && particleScales.HasNonEmptyValue())
            {
                InstancingUtility.SetScales(drawCommand, particleScales.Value);

                particleScales.IsDirty = false;
            }
        }

        private void UpdateMeshAndMaterials()
        {
            drawCommand.SetMesh(mesh);
            drawCommand.SetMaterial(material);
            mesh.IsDirty = false;
            material.IsDirty = false;
        }

        protected override IndirectMeshDrawCommand DrawCommand => drawCommand;

        public override void ResetBuffers()
        {
            base.ResetBuffers();
            particleColors.IsDirty = true;
            particlePositions.IsDirty = true;
            particleScales.IsDirty = true;
        }

        public void Setup()
        {
            drawCommand = new IndirectMeshDrawCommand();
        }

        public void Refresh()
        {
            if (!Application.isPlaying)
                ResetBuffers();
        }

        public void OnBeforeSerialize()
        {
            
        }
    }
}