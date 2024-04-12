using Essd;
using Nanover.Grpc;
using Nanover.Grpc.Trajectory;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;
using Nanover.Grpc.Multiplayer;
using Nanover.Visualisation;
using Nanover.Frontend.InputControlSystem.Utilities;
using UnityEngine.InputSystem;
using Nanover.Core.Math;
using Nanover.Frontend.XR;
using NanoverImd.InputHandlers;
using NanoverImd.Interaction;
using static UnityEngine.InputSystem.HID.HID;


/* Developer's Notes
 *  1) As it currently stands the <c>SynchronisedFrameSource<c> element is attached to an object.
 *     This this a little worrying as it indicates that we do not know exactly where and when the
 *     frame source will be at any one point in time. If this is only a <c>MonoBehaviour</c> based
 *     class for the same of the functionality offered by <c>Update</c> then this can just be called
 *     from the parent object. That is to say, it would be better to have a single consistent object
 *     from which this data can be sourced, whose location is always known.
 *
 *  2) System translation, rotation, & scale are now performed by <c>TransformHybridInputHandler</c>.
 *     Therefor, there is no longer a need for <c>ManipulableScenePose</c>, the back link to the
 *     <c>NanoverImdApplication</c> component can also be removed as it is was only ever used by
 *     the <c>ManipulableScenePose</c> entity and the <c>ResetBox</c> method. The latter of which
 *     has also been removed.
 *
 *  3) The <c>ManipulableParticles</c> entity is no longer needed as its functionality is now covered
 *     by the <c>ImpulseMonoInputInputHandler</c>. For the same reason the <c>InteractableScene</c>
 *     and <c>rightHandedSimulationSpace</c> entities have also been removed.
 *
 *  4) Media related methods have been removed as their functionality has now been relocated to the
 *     <c>ImdMediaMonoInputHandler</c> input handler.
 */


namespace NanoverImd
{
    /// <summary>
    /// Provides basic functionality necessary to connect the client to a interactive molecular dynamics
    /// server with multiplayer capabilities. [PLACEHOLDER DOC-STRING]
    /// </summary>
    /// <remarks>
    /// Note that this class and its documentation still require much work before it is suitable for
    /// production level use.
    /// </remarks>
    public class NanoverImdSimulation : MonoBehaviour, IMultiplayerSessionSource, ITrajectorySessionSource, IPhysicallyCalibratedSpaceSource, ISimulationSpaceTransformSource
    {

        /// <summary> Backlink to the Nanover IMD Application component.</summary>
        [SerializeField]
        private NanoverImdApplication application;

        /// <summary>
        /// The <see cref="UnityEngine.Transform">transform</see> representing the simulation's
        /// "<i>outer</i>" space within the virtual environment.
        /// </summary>
        /// <remarks>
        /// This transform defines the overall location, orientation, and scale of the simulation
        /// within the virtual space. This may be freely manipulated as needed in the manner that
        /// one would expect of an objects transform. However, care must be taken to ensure that
        /// local changes made to this transform are synced with the server using the appropriate
        /// multiplayer resource.
        /// </remarks>
        [SerializeField]
        private Transform outerSimulationSpaceTransform;

        /// <summary>
        /// The <see cref="UnityEngine.Transform">transform</see> representing the simulation's
        /// "<i>inner</i>" space. 
        /// </summary>
        /// <remarks>
        /// Molecular simulation packages often adopt the right-hand coordinate system, in contrast
        /// to Unity's left-handed system. This discrepancy results in the rendering of molecular
        /// systems as mirror images, with R-enantiomers appearing as S-enantiomers. To address this,
        /// an inner simulation space with a scale value of [-1, 1, 1] is employed to accurately
        /// reflect the simulation. Therefore, to obtain the correct controller position within the
        /// simulation space, this transform must be used. It is crucial to note that, unlike the
        /// <see cref="outerSimulationSpaceTransform">outer transform</see>, the inner transform
        /// should <u>never</u> be manipulated directly. Within the legacy Narupa code this vaw
        /// formally known as <c>rightHandedSimulationSpace</c>.
        /// </remarks>
        [SerializeField]
        private Transform innerSimulationSpaceTransform;
        
        public (Transform, Transform) SimulationSpaceTransforms => (outerSimulationSpaceTransform, innerSimulationSpaceTransform);

        /// <summary>
        /// Trajectory session entity from which geometry "frame data" can be sourced.
        /// </summary>
        public TrajectorySession Trajectory { get; } = new TrajectorySession();

        /// <summary>
        /// Multiplayer session which is responsible for managing the state of a single local user
        /// engaging in "multiplayer activities".
        /// </summary>
        public MultiplayerSession Multiplayer { get; } = new MultiplayerSession();

        /// <summary>
        /// The <see cref="PhysicallyCalibratedSpace">physically calibrated space</see> entity that
        /// represents the shared coordinate space which the users inhabit.
        /// </summary>
        /// <remarks>
        /// This is needed to allow for positions be converted from client-side coordinate space
        /// to an abstract virtual shared server space. Without this translation layer each client
        /// would see objects at different positions in physical space. This is only important when
        /// users occupy the same real world physical space (i.e. are in the same room).
        /// </remarks>
        public PhysicallyCalibratedSpace PhysicallyCalibratedSpace => application.CalibratedSpace;

        /// <summary>
        /// A collection of gRPC channels, each uniquely identified by a socket address.
        /// </summary>
        /// <remarks>
        /// This dictionary serves as a repository mapping string keys to <see cref="GrpcConnection"/> instances. 
        /// Each key comprises a socket address, formatted as "address:port", which uniquely identifies 
        /// the corresponding <c>GrpcConnection</c>. The presence of a specific key indicates an established
        /// connection to its associated socket address, thereby facilitating efficient reuse of gRPC channels.
        /// </remarks>
        private Dictionary<string, GrpcConnection> channels
            = new Dictionary<string, GrpcConnection>();

        /// <summary>
        /// Name of the trajectory service.
        /// </summary>
        private const string TrajectoryServiceName = "trajectory";

        /// <summary>
        /// Name of the multiplayer service.
        /// </summary>
        private const string MultiplayerServiceName = "multiplayer";

        /// <summary>
        /// The <c>ConnectionEstablished</c> event is triggered upon successfully connecting to a service.
        /// </summary>
        public event Action ConnectionEstablished;

        /// <summary>
        /// Entity through which frame data can be sourced synchronously.
        /// </summary>
        public SynchronisedFrameSource FrameSynchroniser { get; private set; }

        public ParticleInteractionCollection InteractionCollection
        {
            get => ImpulseMonoInputInputHandler.interactionCollection;
            set => ImpulseMonoInputInputHandler.interactionCollection = value;
        }

        /// <summary>
        /// Positions of the atoms.
        /// </summary>
        /// <remarks>
        /// This is just a helper function which makes getting the positions of the atoms easier
        /// while helping to reduce code duplication.
        /// </remarks>
        private Vector3[] Positions
        {
            get
            {
                var frame = FrameSynchroniser.CurrentFrame;
                if (frame != null && frame.ParticlePositions != null)
                    return frame.ParticlePositions;
                else
                    return null;
            }
        }

        /// <summary>
        /// Connect to the host address and attempt to open clients for the
        /// trajectory and IMD services.
        /// </summary>
        public async Task Connect(string address,
            int? trajectoryPort,
            int? multiplayerPort = null)
        {
            await CloseAsync();

            if (trajectoryPort.HasValue)
                Trajectory.OpenClient(GetChannel(address, trajectoryPort.Value));


            if (multiplayerPort.HasValue)
                Multiplayer.OpenClient(GetChannel(address, multiplayerPort.Value));


            gameObject.SetActive(true);

            ConnectionEstablished?.Invoke();
        }

        /// <summary>
        /// Establishes connections to services as advertised by the specified ESSD service hub.
        /// </summary>
        /// <param name="hub">The ESSD service hub, encapsulating the service information.</param>
        /// <remarks>
        /// This method is responsible for initiating connections to various services based on the 
        /// information provided by the ESSD service hub. It attempts to retrieve the 'services' 
        /// property from the hub's properties, expecting a JSON object. If successful, connections 
        /// to the specified services are established using their respective port numbers. Should the 
        /// 'services' property be absent or not formatted as a JSON object, appropriate error handling 
        /// is to be implemented.
        /// </remarks>
        public async Task Connect(ServiceHub hub)
        {
            // Attempting to extract the 'services' JSON object from the hub's properties.
            if (hub.Properties.TryGetValue("services", out var servicesObj) && servicesObj is JObject services)
            {
                // Establishing connections to each service using their respective port numbers.
                await Connect(hub.Address,
                    GetServicePort(services, TrajectoryServiceName),
                    GetServicePort(services, MultiplayerServiceName));
            }
            else
            {
                // TODO: Implement error handling for cases where 'services' is missing or improperly formatted.
            }

            //CopyRemoteTransformToLocal();
        }

        /// <summary>
        /// Retrieves the port number associated with a given service name from the provided services JSON object.
        /// </summary>
        /// <param name="services">The JSON object containing service-port mappings.</param>
        /// <param name="serviceName">The name of the service whose port number is sought.</param>
        /// <returns>The port number as an integer, or null if the service name is not found.</returns>
        /// <remarks>
        /// This helper method is integral to the process of determining the correct port number for 
        /// each service. It searches the provided JSON object for the specified service name. If the 
        /// service name exists within the object, its associated port number is returned. In the absence 
        /// of the service name, null is returned, indicating that the service is not available or not 
        /// configured.
        /// </remarks>
        private int? GetServicePort(JObject services, string serviceName) =>
            services.ContainsKey(serviceName) ? services[serviceName].ToObject<int?>() : null;


        /// <summary>
        /// Performs an ESSD search and establishes a connection to the first service discovered,
        /// or none if the specified timeout elapses without detecting any service.
        /// </summary>
        /// <param name="timeout">The timeout duration in milliseconds for the ESSD search.</param>
        /// <returns>A Task representing the asynchronous operation of this method.</returns>
        public async Task AutoConnect(int timeout = 1000)
        {
            // Creating a new Client instance for conducting the search.
            var client = new Client();

            // Asynchronously searching for services, adhering to the specified timeout.
            var services = await Task.Run(() => client.SearchForServices(timeout));

            // Checking if any service is found within the timeout period.
            if (services.Count > 0)
            {
                // If services are found, connecting to the first one in the list.
                await Connect(services.First());
            }
            // If no services are found, the function completes without establishing a connection.
        }


        /// <summary>
        /// Retrieves the gRPC connection associated with the specified socket address.
        /// </summary>
        /// <param name="address">The address of the target, specifically an IP address or a fully
        /// qualified domain name.</param>
        /// <param name="port">The port of the target.</param>
        /// <returns>A <c>GrpcConnection</c> instance representing the connection to the specified
        /// target.</returns>
        /// <remarks>
        /// The <c>channels</c> dictionary is consulted for an existing connection corresponding to the
        /// provided socket address. In the absence of an existing connection, a new connection is
        /// established and subsequently added to the dictionary for future reference. This method
        /// guarantees the provision of a valid <c>GrpcConnection</c> instance for each unique socket
        /// address.
        /// </remarks>
        private GrpcConnection GetChannel(string address, int port)
        {
            // String keys mapping to gRPC connections are composed using the specified address & port.
            string key = $"{address}:{port}";

            // Attempts to retrieve an existing connection for the socket address. If unsuccessful, a
            // new connection is instantiated and added to the `channels` dictionary.
            if (!channels.TryGetValue(key, out var channel))
            {
                channel = new GrpcConnection(address, port);
                channels[key] = channel;
            }

            return channel;
        }

        
        private void Awake()
        {
            FrameSynchroniser = gameObject.GetComponent<SynchronisedFrameSource>();
            if (FrameSynchroniser == null)
            {
                // Create the frame synchroniser component.  
                FrameSynchroniser = gameObject.AddComponent<SynchronisedFrameSource>();
            }

            // and set the frame source of the frame synchroniser.
            FrameSynchroniser.FrameSource = Trajectory;

            // If the system's transform is changed by the server or another agent then a call to
            // `CopyRemoteTransformToLocal` is made to apply that change to the local game object.
            Multiplayer.SimulationPose.RemoteValueChanged += CopyRemoteTransformToLocalWithGuard;
        }


        /// <summary>
        /// Terminates all active sessions and deactivates the game object.
        /// </summary>
        /// <remarks>
        /// This method is responsible for closing all open gRPC connections and deactivating 
        /// the associated Unity game object. Initially, it invokes the closure of the 
        /// Trajectory and Multiplayer clients. Subsequently, each gRPC channel in the 
        /// 'channels' dictionary is asynchronously closed. Upon completion of these closures, 
        /// the dictionary is cleared. Finally, if the current instance and the associated 
        /// game object are not null, the game object is deactivated, signifying the termination 
        /// of active sessions.
        /// </remarks>
        public async Task CloseAsync()
        {
            // Closing the Trajectory and Multiplayer sessions.
            Trajectory.CloseClient();
            Multiplayer.CloseClient();

            // Iterating through and closing each gRPC channel in the 'channels' dictionary.
            foreach (var channel in channels.Values)
            {
                await channel.CloseAsync();
            }

            // Clearing the dictionary to remove all references to the closed channels.
            channels.Clear();

            // Deactivating the game object if the current instance and the game object exist.
            if (this != null && gameObject != null)
                gameObject.SetActive(false);
        }


        private void OnDestroy()
        {
            // Previous versions awaited the `CloseAsync` method, however the `OnDestroy` method
            // does not support the execution of asynchronous operations.
#pragma warning disable CS4014
            CloseAsync();
#pragma warning restore CS4014
        }

        public void Disconnect() => _ = CloseAsync();

        /// <summary>
        /// Reset the box to the unit position.
        /// </summary>
        public void ResetBox()
        {
            var calibPose = PhysicallyCalibratedSpace.TransformPoseWorldToCalibrated(Transformation.Identity);
            Multiplayer.SimulationPose.UpdateValueWithLock(calibPose);
            CopyRemoteTransformToLocal();
            // TODO: Is the lock actually released here?

        }




        /// <summary>
        /// Run the radial orientation command on the server. This generates
        /// shared state values that suggest relative origin positions for all
        /// connected users.
        /// </summary>
        public void RunRadialOrientation()
        {
            Trajectory.RunCommand(
                "multiuser/radially-orient-origins",
                new Dictionary<string, object> { ["radius"] = .01 }
            );
        }


        private void CopyRemoteTransformToLocalWithGuard()
        {
            // Do not respond to local changes as these will be applied by the entity responsible.
            // Note that the `Locked` state only indicates if a local lock has been acquired thus
            // not additional checks are required.
            if (Multiplayer.SimulationPose.LockState != MultiplayerResourceLockState.Locked)
                CopyRemoteTransformToLocal();

        }

        private void CopyRemoteTransformToLocal()
        {

            // Fetch the new transform from the shared multiplayer resource entity.
            var calibratedTransformation = Multiplayer.SimulationPose.Value;

            /* Developer's Notes;
             * This check is a holdover from the original code and the rationed given is as follows:
             * "This is necessary because the default value of multiplayer.SimulationPose is
             *  degenerate (0 scale) and there seems to be no way to tell if the remote value has
             *  been set yet or is default."
             */
            if (calibratedTransformation.Scale.x <= 0.001f)
                calibratedTransformation = new Transformation(Vector3.zero, Quaternion.identity, Vector3.one);

            // Convert from common user-agnostic "calibrated space" to client-side world space.
            // Note that this "world space" is not the same "world space" that Unity recognises.
            var worldTransformation = PhysicallyCalibratedSpace.TransformPoseCalibratedToWorld(calibratedTransformation);

            // Set the local position, orientation, and scale of the target `Transform` equal to
            // those within `worldTransformation`.
            worldTransformation.CopyToTransformRelativeToParent(outerSimulationSpaceTransform);
            
        }


    }
}