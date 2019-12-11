// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using Narupa.Frontend.Manipulation;
using Narupa.Session;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Narupa.Core.Async;
using Narupa.Core.Math;
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
        public bool HaveSceneLock => !multiplayer.IsOpen || multiplayer.HasSimulationPoseLock;

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

            Update().AwaitInBackground();
        }

        /// <summary>
        /// Attempt to start a grab manipulation on this box, with a 
        /// manipulator at the current pose.
        /// </summary>
        public IActiveManipulation StartGrabManipulation(UnitScaleTransformation manipulatorPose)
        {
            if (!HaveSceneLock)
                return null;

            if (manipulable.StartGrabManipulation(manipulatorPose) is IActiveManipulation manipulation)
            {
                manipulations.Add(manipulation);
                manipulation.ManipulationEnded += () => manipulations.Remove(manipulation);
                return manipulation;
            }

            return null;
        }

        private async Task Update()
        {
            while (true)
            {
                if (!HaveSceneLock)
                {
                    EndAllManipulations();

                    var worldPose = prototype.CalibratedSpace.TransformPoseCalibratedToWorld(multiplayer.SimulationPose);
                    //worldPose.CopyToTransformRelativeToParent(sceneTransform);
                }
                else if (manipulations.Count > 0)
                {
                    var worldPose = Transformation.FromTransformRelativeToParent(sceneTransform);
                    var calibPose = prototype.CalibratedSpace.TransformPoseWorldToCalibrated(worldPose);
                    multiplayer.SetSimulationPose(calibPose);
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
