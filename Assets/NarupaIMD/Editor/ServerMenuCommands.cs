using NarupaXR;
using UnityEditor;
using UnityEngine;

namespace NarupaIMD.Editor
{
    /// <summary>
    /// Menu commands for debugging server commands.
    /// </summary>
    public static class ServerMenuCommands
    {
        /// <summary>
        /// Play the current server.
        /// </summary>
        [MenuItem("Narupa/Play")]
        public static void PlayServer()
        {
            Object.FindObjectOfType<NarupaXRPrototype>().Sessions.Trajectory?.Play();
        }

        /// <summary>
        /// Pause the current server.
        /// </summary>
        [MenuItem("Narupa/Pause")]
        public static void PauseServer()
        {
            Object.FindObjectOfType<NarupaXRPrototype>().Sessions.Trajectory?.Pause();
        }

        /// <summary>
        /// Reset the current server.
        /// </summary>
        [MenuItem("Narupa/Reset")]
        public static void ResetServer()
        {
            Object.FindObjectOfType<NarupaXRPrototype>().Sessions.Trajectory?.Reset();
        }

        /// <summary>
        /// Step the current server.
        /// </summary>
        [MenuItem("Narupa/Step")]
        public static void StepServer()
        {
            Object.FindObjectOfType<NarupaXRPrototype>().Sessions.Trajectory?.Step();
        }
    }
}