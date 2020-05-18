// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using Narupa.Core.Async;
using Narupa.Frame;
using Narupa.Grpc.Frame;
using Narupa.Grpc.Stream;
using Narupa.Protocol.Trajectory;

namespace Narupa.Grpc.Trajectory
{
    /// <summary>
    /// Adapts <see cref="TrajectoryClient" /> into an
    /// <see cref="ITrajectorySnapshot" /> where
    /// <see cref="ITrajectorySnapshot.CurrentFrame" /> is the latest received frame.
    /// </summary>
    public class TrajectorySession : GrpcSession<TrajectoryClient>, ITrajectorySnapshot
    {
        /// <inheritdoc cref="ITrajectorySnapshot.CurrentFrame" />
        public Narupa.Frame.Frame CurrentFrame => trajectorySnapshot.CurrentFrame;
        
        public int CurrentFrameIndex { get; private set; }

        /// <inheritdoc cref="ITrajectorySnapshot.FrameChanged" />
        public event FrameChanged FrameChanged;

        /// <summary>
        /// Underlying <see cref="TrajectorySnapshot" /> for tracking
        /// <see cref="CurrentFrame" />.
        /// </summary>
        private readonly TrajectorySnapshot trajectorySnapshot = new TrajectorySnapshot();

        private IncomingStream<GetFrameResponse> frameStream;

        public TrajectorySession()
        {
            trajectorySnapshot.FrameChanged += (sender, args) => FrameChanged?.Invoke(sender, args);
        }

        protected override TrajectoryClient CreateClient(GrpcConnection connection)
        {
            return new TrajectoryClient(connection);
        }

        public override void OpenClient(GrpcConnection connection)
        {
            base.OpenClient(connection);
            
            frameStream = Client.SubscribeLatestFrames(1f / 30f);
            frameStream.MessageReceived += Callback;
            frameStream.StartReceiving().AwaitInBackgroundIgnoreCancellation();

            void Callback(GetFrameResponse response)
            {
                var (frame, changes) = FrameConverter.ConvertFrame(response.Frame, CurrentFrame);
                trajectorySnapshot.SetCurrentFrame(frame, changes);
                CurrentFrameIndex = (int) response.FrameIndex;
            }

        }

        /// <summary>
        /// Close the current trajectory client.
        /// </summary>
        public override void CloseClient()
        {
            trajectorySnapshot.Clear();

            frameStream?.CloseAsync();
            frameStream?.Dispose();
            frameStream = null;
        }
        
        /// <inheritdoc cref="TrajectoryClient.CommandPlay"/>
        public void Play()
        {
            RunCommand(TrajectoryClient.CommandPlay);
        }
        
        /// <inheritdoc cref="TrajectoryClient.CommandPause"/>
        public void Pause()
        {
            RunCommand(TrajectoryClient.CommandPause);
        }
        
        /// <inheritdoc cref="TrajectoryClient.CommandReset"/>
        public void Reset()
        {
            RunCommand(TrajectoryClient.CommandReset);
        }
        
        /// <inheritdoc cref="TrajectoryClient.CommandStep"/>
        public void Step()
        {
            RunCommand(TrajectoryClient.CommandStep);
        }
    }
}