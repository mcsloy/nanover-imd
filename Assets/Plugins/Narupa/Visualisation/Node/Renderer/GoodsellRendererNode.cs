using System;
using System.Collections.Generic;
using Narupa.Visualisation.Properties;
using Narupa.Visualisation.Properties.Collections;
using Narupa.Visualisation.Property;
using UnityEngine;
using UnityEngine.Rendering;

namespace Narupa.Visualisation.Node.Renderer
{
    /// <summary>
    /// Renderer which draws spheres using a command buffer, allowing them to be
    /// outlined depending on their relative residue indices, reminiscent of the Goodsell style.
    /// </summary>
    [ExecuteAlways]
    [Serializable]
    public class GoodsellRendererNode : CommandBufferRendererNode
    {
#pragma warning disable 0649
        [SerializeField]
        private Vector3ArrayProperty particlePositions = new Vector3ArrayProperty();

        [SerializeField]
        private ColorArrayProperty particleColors = new ColorArrayProperty();

        [SerializeField]
        private FloatArrayProperty particleScales = new FloatArrayProperty();
        
        [SerializeField]
        private IntArrayProperty particleResidues = new IntArrayProperty();
#pragma warning restore 0649

        [SerializeField]
        private ColorProperty rendererColor = new ColorProperty();

        [SerializeField]
        private FloatProperty rendererScale = new FloatProperty();
        
        [SerializeField]
        private MeshProperty sphereMesh = new MeshProperty();
        
        [SerializeField]
        private MaterialProperty sphereMaterial = new MaterialProperty();

        /// <inheritdoc cref="CommandBufferRendererNode.Cleanup"/>
        public override void Dispose()
        {
            base.Dispose();
            renderer.ResetBuffers();
        }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
            renderer.ParticlePositions.LinkedProperty = particlePositions;
            renderer.ParticleColors.LinkedProperty = particleColors;
            renderer.ParticleResidues.LinkedProperty = particleResidues;
            renderer.ParticleColor.LinkedProperty = rendererColor;
            renderer.ParticleScale.LinkedProperty = rendererScale;
            renderer.ParticleScales.LinkedProperty = particleScales;

            renderer.Mesh.LinkedProperty = sphereMesh;
            renderer.Material.LinkedProperty = sphereMaterial;
        }

#pragma warning disable 0649
        [SerializeField]
        private CameraEvent cameraEvent = CameraEvent.AfterForwardOpaque;

        [SerializeField]
        private Shader shader;

        [SerializeField]
        private GoodsellSphereRendererNode renderer = new GoodsellSphereRendererNode();
#pragma warning restore 0649

        private bool isRendering = false;
        
        /// <inheritdoc cref="CommandBufferRendererNode.Render"/>
        public override void Render(Camera cam)
        {
            renderer.Transform = Transform;
            
            if (renderer.IsInputDirty && renderer.ShouldRender != isRendering)
            {
                Dispose();
                isRendering = renderer.ShouldRender;
            }

            base.Render(cam);
            renderer.UpdateRenderer();
        }

        /// <inheritdoc cref="CommandBufferRendererNode.GetBuffers"/>
        protected override IEnumerable<(CameraEvent Event, CommandBuffer Buffer)> GetBuffers(
            Camera camera)
        {
            if (GetColorBuffer(camera) is CommandBuffer colorBuffer)
                yield return (cameraEvent, colorBuffer);
        }

        private CommandBuffer GetColorBuffer(Camera camera)
        {
            var buffer = new CommandBuffer
            {
                name = "Goodsell Pass"
            };

            if (shader == null)
                return null;

            var material = CreateMaterial(shader);

            var colorId = Shader.PropertyToID("_ColorTexture");
            var depthId = Shader.PropertyToID("_DepthTexture");
            var resId = Shader.PropertyToID("_ResidueTexture");

            buffer.GetTemporaryRT(colorId, -1, -1, 24,
                                  FilterMode.Bilinear,
                                  RenderTextureFormat.ARGB32,
                                  RenderTextureReadWrite.Linear);
            buffer.GetTemporaryRT(depthId, -1, -1, 0,
                                  FilterMode.Bilinear,
                                  RenderTextureFormat.RFloat,
                                  RenderTextureReadWrite.Linear);
            buffer.GetTemporaryRT(resId, -1, -1, 0,
                                  FilterMode.Bilinear,
                                  RenderTextureFormat.RFloat,
                                  RenderTextureReadWrite.Linear);

            buffer.SetRenderTarget(new RenderTargetIdentifier[]
            {
                colorId, depthId, resId
            }, colorId);

            buffer.ClearRenderTarget(true, true, UnityEngine.Color.clear);

            renderer.AppendToCommandBuffer(buffer);

            buffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);

            buffer.SetGlobalTexture("_DepthTex", depthId);
            buffer.SetGlobalTexture("_MainTex", colorId);
            buffer.SetGlobalTexture("_ResidueTex", resId);

            buffer.Blit(colorId, BuiltinRenderTextureType.CameraTarget, material);

            buffer.ReleaseTemporaryRT(colorId);
            buffer.ReleaseTemporaryRT(depthId);

            return buffer;
        }
    }
}