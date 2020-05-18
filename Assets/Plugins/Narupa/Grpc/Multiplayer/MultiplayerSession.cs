// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Narupa.Core.Math;
using Narupa.Grpc;
using Narupa.Grpc.Multiplayer;
using Narupa.Network;

namespace Narupa.Session
{
    /// <summary>
    /// Manages the state of a single user engaging in multiplayer. Tracks
    /// local and remote avatars and maintains a copy of the latest values in
    /// the shared key/value store.
    /// </summary>
    public sealed class MultiplayerSession : GrpcSession<MultiplayerClient>
    {
        public const string SimulationPoseKey = "scene";

        public MultiplayerAvatars Avatars { get; private set; }

        /// <summary>
        /// The transformation of the simulation box.
        /// </summary>
        public SharedStateResource<Transformation> SimulationPose { get; private set; }

        /// <summary>
        /// Has this session created a user?
        /// </summary>
        public bool HasPlayer => PlayerId != null;

        /// <summary>
        /// Username of the current player, if any.
        /// </summary>
        public string PlayerName { get; private set; }

        /// <summary>
        /// ID of the current player, if any.
        /// </summary>
        public string PlayerId { get; private set; }

        public event Action MultiplayerJoined;

        protected override MultiplayerClient CreateClient(GrpcConnection connection)
        {
            return new MultiplayerClient(connection);
        }

        public override void OpenClient(GrpcConnection connection)
        {
            base.OpenClient(connection);

            Avatars = new MultiplayerAvatars(this);
            SimulationPose = SharedState.GetResource(SimulationPoseKey,
                                                     PoseFromObject,
                                                     PoseToObject,
                                                     Transformation.Identity);
        }

        /// <summary>
        /// Close the current Multiplayer client and dispose all streams.
        /// </summary>
        public override void CloseClient()
        {
            Avatars?.CloseClient();

            base.CloseClient();

            SimulationPose = null;

            PlayerName = null;
            PlayerId = null;
        }

        /// <summary>
        /// Create a new multiplayer with the given username, subscribe avatar 
        /// and value updates, and begin publishing our avatar.
        /// </summary>
        public async Task JoinMultiplayer(string playerName)
        {
            if (HasPlayer)
                throw new InvalidOperationException($"Multiplayer already joined as {PlayerName}");

            var response = await Client.CreatePlayer(playerName);

            PlayerName = playerName;
            PlayerId = response.PlayerId;

            MultiplayerJoined?.Invoke();
        }

        private static object PoseToObject(Transformation pose)
        {
            var data = new object[]
            {
                pose.Position.x, pose.Position.y, pose.Position.z, pose.Rotation.x, pose.Rotation.y,
                pose.Rotation.z, pose.Rotation.w, pose.Scale.x, pose.Scale.y, pose.Scale.z,
            };

            return data;
        }

        private static Transformation PoseFromObject(object @object)
        {
            if (@object is List<object> list)
            {
                var values = list.Select(value => Convert.ToSingle(value)).ToList();
                var position = list.GetVector3(0);
                var rotation = list.GetQuaternion(3);
                var scale = list.GetVector3(7);

                return new Transformation(position, rotation, scale);
            }

            throw new ArgumentOutOfRangeException();
        }
    }
}