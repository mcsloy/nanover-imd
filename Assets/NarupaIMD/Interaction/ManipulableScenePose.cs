// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using Narupa.Frontend.Manipulation;
using Narupa.Session;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Narupa.Core.Async;
using Narupa.Core.Math;
using Narupa.Grpc.Multiplayer;
using UnityEngine;

namespace NarupaXR.Interaction
{
    /// <summary>
    /// Provides the ability to move the simulation scene, but preventing this
    /// if multiplayer is active and the user does not have a lock on the 
    /// scene.
    /// </summary>
    public class ManipulableScenePose
    {
        private readonly Transform sceneTransform;
        private readonly ManipulableTransform manipulable;
        private readonly MultiplayerSession multiplayer;
        private readonly NarupaXRPrototype prototype;

        private readonly HashSet<IActiveManipulation> manipulations 
            = new HashSet<IActiveManipulation>();

        public ManipulableScenePose(Transform sceneTransform,
                                    MultiplayerSession multiplayer,
                                    NarupaXRPrototype prototype)
        {
            this.sceneTransform = sceneTransform;
            this.multiplayer = multiplayer;
            this.prototype = prototype;
            manipulable = new ManipulableTransform(sceneTransform);
            this.multiplayer.SimulationPose.StateChanged += SimulationPoseOnStateChanged;
            this.multiplayer.SimulationPose.MultiplayerKeyChanged += MultiplayerKeyChanged;

            Update().AwaitInBackground();
        }

        private void MultiplayerKeyChanged()
        {
            if (multiplayer.SimulationPose.State != ResourceLockState.Accepted)
            {
                SetSceneToMirrorMultiplayer();
            }
        }

        private void SimulationPoseOnStateChanged()
        {
            if (multiplayer.SimulationPose.State == ResourceLockState.Rejected)
            {
                EndAllManipulations();

                SetSceneToMirrorMultiplayer();
            }
        }

        private void SetSceneToMirrorMultiplayer()
        {
            var worldPose = prototype.CalibratedSpace.TransformPoseCalibratedToWorld(multiplayer.SimulationPose.Value);
            worldPose.CopyToTransform(sceneTransform);
        }

        /// <summary>
        /// Attempt to start a grab manipulation on this box, with a 
        /// manipulator at the current pose.
        /// </summary>
        public IActiveManipulation StartGrabManipulation(Transformation manipulatorPose)
        {
            if (manipulable.StartGrabManipulation(manipulatorPose) is IActiveManipulation manipulation)
            {
                manipulations.Add(manipulation);
                manipulation.ManipulationEnded += () => OnManipulationEnded(manipulation);
                return manipulation;
            }

            return null;
        }

        private void OnManipulationEnded(IActiveManipulation manipulation)
        {
            manipulations.Remove(manipulation);
            if(manipulations.Count == 0)
                multiplayer.SimulationPose.TryRelease();
        }

        private async Task Update()
        {
            while (true)
            {
                if (manipulations.Count > 0)
                {
                    var worldPose = new Transformation(sceneTransform.localPosition,
                                                       sceneTransform.localRotation,
                                                       sceneTransform.localScale);
                    var calibPose = prototype.CalibratedSpace.TransformPoseWorldToCalibrated(worldPose);
                    multiplayer.SimulationPose.SetLocalAndTryLocking(calibPose);
                }

                await Task.Delay(10);
            }
        }

        private void EndAllManipulations()
        {
            foreach (var manipulation in manipulations.ToList())
                manipulation.EndManipulation();
        }
    }
}
