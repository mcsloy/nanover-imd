// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using Narupa.Frame;
using Narupa.Visualisation.Property;
using Narupa.Visualisation.Utility;
using UnityEngine;

namespace Narupa.Visualisation.Node.Renderer
{
    /// <summary>
    /// Visualisation node for rendering bonds between particles.
    /// </summary>
    [Serializable]
    public class ParticleTripleRenderer : IDisposable
    {
        private readonly IndirectMeshDrawCommand drawCommand = new IndirectMeshDrawCommand();

#pragma warning disable 0649
        [SerializeField]
        private MaterialProperty material = new MaterialProperty();

        [SerializeField]
        private MeshProperty mesh = new MeshProperty();
#pragma warning restore 0649

        public Transform Transform { get; set; }

        #region Input Properties

        /// <summary>
        /// Positions of particles which will be connected by bonds.
        /// </summary>
        public IProperty<Vector3[]> ParticlePositions => particlePositions;

        public IProperty<Mesh> Mesh => mesh;

        public IProperty<Material> Material => material;

        [SerializeField]
        private TripleArrayProperty triples = new TripleArrayProperty();

        [SerializeField]
        private Vector3ArrayProperty particlePositions = new Vector3ArrayProperty();

        #endregion

        private bool ShouldRender => mesh.HasNonNullValue()
                                  && material.HasNonNullValue()
                                  && triples.HasNonEmptyValue()
                                  && particlePositions.HasNonEmptyValue();

        /// <summary>
        /// Render the provided bonds
        /// </summary>
        public void Render(Camera camera = null)
        {
            if (ShouldRender)
            {
                UpdateMeshAndMaterials();

                InstancingUtility.SetTransform(drawCommand, Transform);

                drawCommand.SetInstanceCount(triples.Value.Length);

                UpdatePositionsIfDirty();
                UpdateTriplesIfDirty();

                drawCommand.MarkForRenderingThisFrame(camera);
            }
        }

        /// <inheritdoc cref="IDisposable.Dispose" />
        public void Dispose()
        {
            drawCommand.Dispose();
        }

        private void UpdatePositionsIfDirty()
        {
            if (particlePositions.IsDirty && particlePositions.HasNonEmptyValue())
            {
                InstancingUtility.SetPositions(drawCommand, particlePositions.Value);

                particlePositions.IsDirty = false;
            }
        }

        private void UpdateTriplesIfDirty()
        {
            if (triples.IsDirty && triples.HasNonEmptyValue())
            {
                InstancingUtility.SetTriples(drawCommand, triples.Value);

                triples.IsDirty = false;
            }
        }

        private void UpdateMeshAndMaterials()
        {
            drawCommand.SetMesh(mesh);
            drawCommand.SetMaterial(material);
        }
        
        /// <summary>
        /// Indicate that a deserialisation or other event which resets buffers has occured.
        /// </summary>
        public void ResetBuffers()
        {
            drawCommand.ResetCommand();
            particlePositions.IsDirty = true;
            triples.IsDirty = true;
        }

    }
}