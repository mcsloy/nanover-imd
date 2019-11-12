// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using UnityEngine;

namespace NarupaXR
{
    public sealed class TrajectoryCommands : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private NarupaXRPrototype prototype;
#pragma warning restore 0649

        public void SendPlayCommand() => prototype.Sessions.Trajectory.Play();
        public void SendPauseCommand() => prototype.Sessions.Trajectory.Pause();
        public void SendStepCommand() => prototype.Sessions.Trajectory.Step();
        public void SendResetCommand() => prototype.Sessions.Trajectory.Reset();
    }
}