// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using NarupaIMD;
using UnityEngine;

namespace NarupaXR
{
    /// <summary>
    /// Component that exposes the trajectory playback commands to Unity UI
    /// components.
    /// </summary>
    public sealed class TrajectoryCommands : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private NarupaImdSimulation simulation;
#pragma warning restore 0649

        public void SendPlayCommand() => simulation.Trajectory.Play();
        public void SendPauseCommand() => simulation.Trajectory.Pause();
        public void SendStepCommand() => simulation.Trajectory.Step();
        public void SendResetCommand() => simulation.Trajectory.Reset();
    }
}