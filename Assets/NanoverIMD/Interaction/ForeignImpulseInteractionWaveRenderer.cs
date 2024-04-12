
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Nanover.Frontend.Utility;
using Nanover.Core.Utility;

namespace NanoverImd.Interaction
{
    /// <summary>
    /// This class is responsible for instantiating and managing the connected sine wave renderer
    /// instances used to visualise the impulse interactor events resulting form non-local users.
    /// </summary>
    public class ForeignImpulseInteractionWaveRenderer : MonoBehaviour
    {

        /// <summary>
        /// Sine connected renderer template.
        /// </summary>
        /// <remarks>
        /// This prefab/object will be used as a template when creating new sine connected renderer
        /// instances.
        /// </remarks>
        [SerializeField]
        private SineConnectorRenderer waveTemplate;


        /// <summary>
        /// Current NanoVer IMD simulation instance.
        /// </summary>
        /// <remarks>
        /// This is necessary to provide quick and easy access to the current simulation frame, the
        /// impulse interaction collection, and the simulation's inner space.
        /// </remarks>
        [SerializeField]
        private NanoverImdSimulation simulation;

        /// <summary>
        /// Indexed pool of sine connected renderers.
        /// </summary>
        /// <remarks>
        /// This prevents having to repeatedly create and destroy new objects to render impulse
        /// interactions every time a new interaction event starts and finishes.
        /// </remarks>
        private IndexedPool<SineConnectorRenderer> wavePool;

        private void Awake()
        {
            // Create a new indexed pool of sine connected renderers When first starting up.
            wavePool = new IndexedPool<SineConnectorRenderer>(
                CreateInstanceCallback, ActivateInstanceCallback, DeactivateInstanceCallback);
        }

        /// <summary>
        /// Callback method used by the indexed pool to activate a sine connector renderer instance.
        /// </summary>
        /// <param name="waveRenderer">Renderer instance to activate.</param>
        private void ActivateInstanceCallback(SineConnectorRenderer waveRenderer) =>
            waveRenderer.gameObject.SetActive(true);

        /// <summary>
        /// Callback method used by the indexed pool to deactivate a sine connector renderer instance.
        /// </summary>
        /// <param name="waveRenderer">Renderer instance to deactivate.</param>
        private void DeactivateInstanceCallback(SineConnectorRenderer waveRenderer) =>
            waveRenderer.gameObject.SetActive(false);

        /// <summary>
        /// Method used by the indexed pool to create a new sine connected renderer instance.
        /// </summary>
        /// <returns>Newly instantiated and active sine connected renderer instance.</returns>
        private SineConnectorRenderer CreateInstanceCallback()
        {
            var waveRenderer = Instantiate(waveTemplate, transform, true);
            waveRenderer.gameObject.SetActive(true);
            return waveRenderer;
        }
        


        private void Update()
        {
            // This method will ensure that there is one active sine renderer instance for each
            // foreign impulse interaction. It will also set the start and end points for said
            // renderers. Note that sine renderers for locally crated impulse interactions are
            // dealt with by the relevant input handler.

            var frame = simulation.FrameSynchroniser.CurrentFrame;


            // The "inner" space transform of the simulation, formally known as "Right Handed Space".
            Transform innerSpaceTransform = simulation.SimulationSpaceTransforms.Item2;

            // Search the particle interaction collection for keys associated with impulse interactions
            // that are associated with external agents. Each client includes its own globally unique
            // application instance identifier within the interaction's ID. This means any interaction's
            // whose ID key does not include `GlobalSettings.ApplicationGUID` must have been created
            // either by the server or another user. Interactions resulting from the local client will
            // have their visuals handled by their associated `ImpulseMonoInputInputHandler` instance.
            var keys = simulation.InteractionCollection.Keys.Where(
                key => !key.Contains(GlobalSettings.ApplicationGUID.ToString()));

            // Now that the keys of the remote interactions have been identified, the associated
            // `ParticleInteraction` instances can be retrieved.
            List<ParticleInteraction> interactions = new List<ParticleInteraction>();
            foreach (var key in keys) interactions.Add(simulation.InteractionCollection.GetValue(key));

            // The index pool can now be used to activate a `SineConnectorRenderer` instance for
            // each interaction.
            wavePool.MapConfig(interactions, MapConfigToInstance);
            return;

            // This local function, which is mapped over remote interaction instances, is responsible
            // for setting the start and end positions of the `SineConnectorRenderer` entities.
            void MapConfigToInstance(ParticleInteraction interaction,
                SineConnectorRenderer renderer)
            {

                // Calculate the centroid of the particles involved in the interaction
                Vector3 particlePositionCentroidInSimulationSpace = interaction.Particles.Aggregate(
                    Vector3.zero, (v, id) => v + frame.ParticlePositions[id]
                    ) / interaction.Particles.Count;

                // Transform from simulation space to world space
                var particlePositionCentroidInWorldSpace = innerSpaceTransform.TransformPoint(
                    particlePositionCentroidInSimulationSpace);


                // Set the start point as the location of the particle(s)
                renderer.StartPosition = particlePositionCentroidInWorldSpace;
                // and the end point as the location of the entity driving the interaction (this is
                // commonly the position of a controller).
                renderer.EndPosition = innerSpaceTransform.TransformPoint(interaction.Position);
            }
        }
    }
}