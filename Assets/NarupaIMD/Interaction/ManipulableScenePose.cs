// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Narupa.Core.Async;
using Narupa.Core.Math;
using Narupa.Frontend.Manipulation;
using Narupa.Session;
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
            this.multiplayer.SimulationPose.LockRejected += SimulationPoseLockRejected;
            this.multiplayer.SimulationPose.SharedValueChanged +=
                MultiplayerSimulationPoseChanged;

            Update().AwaitInBackground();
        }

        /// <summary>
        /// Callback for when the simulation pose value is changed in the multiplayer dictionary.
        /// </summary>
        private void MultiplayerSimulationPoseChanged()
        {
            // If manipulations are active, then I'm controlling my box position.
            if (!manipulations.Any())
            {
                CopyMultiplayerPoseToLocal();
            }
        }

        /// <summary>
        /// Handler for if the simulation pose lock is rejected.
        /// </summary>
        private void SimulationPoseLockRejected()
        {
            EndAllManipulations();
            CopyMultiplayerPoseToLocal();
        }

        /// <summary>
        /// Copy the pose stored in the multiplayer to the current scene transform.
        /// </summary>
        private void CopyMultiplayerPoseToLocal()
        {
            var worldPose = prototype.CalibratedSpace
                                     .TransformPoseCalibratedToWorld(multiplayer.SimulationPose
                                                                                .Value);
            worldPose.CopyToTransform(sceneTransform);
        }

        /// <summary>
        /// Attempt to start a grab manipulation on this box, with a 
        /// manipulator at the current pose.
        /// </summary>
        public IActiveManipulation StartGrabManipulation(Transformation manipulatorPose)
        {
            if (manipulable.StartGrabManipulation(manipulatorPose) is IActiveManipulation
                    manipulation)
            {
                manipulations.Add(manipulation);
                manipulation.ManipulationEnded += () => OnManipulationEnded(manipulation);
                return manipulation;
            }

            return null;
        }

        /// <summary>
        /// Callback for when a manipulation is ended by the user.
        /// </summary>
        private void OnManipulationEnded(IActiveManipulation manipulation)
        {
            manipulations.Remove(manipulation);
            // If manipulations are over, then release the lock.
            if (!manipulations.Any())
            {
                multiplayer.SimulationPose.ReleaseLock();
                CopyMultiplayerPoseToLocal();
            }
        }

        private async Task Update()
        {
            while (true)
            {
                if (manipulations.Any())
                {
                    var worldPose = new Transformation(sceneTransform.localPosition,
                                                       sceneTransform.localRotation,
                                                       sceneTransform.localScale);
                    var calibPose = prototype.CalibratedSpace
                                             .TransformPoseWorldToCalibrated(worldPose);
                    multiplayer.SimulationPose.UpdateValueWithLock(calibPose);
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