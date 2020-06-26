using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Narupa.Frame;
using NarupaXR;
using UnityEngine;
using UnityEngine.Windows;
using Debug = UnityEngine.Debug;
using File = System.IO.File;

namespace NarupaIMD
{
    public class NarupaXRDebugger : MonoBehaviour
    {
        [SerializeField]
        private NarupaXRPrototype prototype;

        public EventTimer FrameReceiving { get; } = new EventTimer();
        
        public EventTimer MultiplayerSend { get; } = new EventTimer();

        public EventTimer MultiplayerReceive { get; } = new EventTimer();
        public EventTimer InteractionsSend { get; } = new EventTimer();
        public EventTimer InteractionsReceive { get; } = new EventTimer();

        public EventTimer MultiplayerPingPong { get; } = new EventTimer();

        public static EventLogger FrameUpdateReceivedLog { get; } = new EventLogger("frame-received");
        public static EventLogger FrameUpdatedLog { get; } = new EventLogger("frame-updated");
        public static EventLogger SharedStateSentLog { get; } = new EventLogger("sharedstate-sent");
        public static EventLogger SharedStateReceivedLog { get; } = new EventLogger("sharedstate-received");
        public static EventLogger SharedStateUpdatedLog { get; } = new EventLogger("sharedstate-updated");
        public static EventLogger SharedStatePingPong { get; } = new EventLogger("sharedstate-pingpong");
        public static EventLogger UnityFrameLog { get; } = new EventLogger("unity-update");
        public static EventLogger AvatarCount { get; } = new EventLogger("avatar-count");
        public static EventLogger InteractionCount { get; } = new EventLogger("interaction-count");
        public static EventLogger InteractionsSentLog { get; } = new EventLogger("interactions-sent");
        public static EventLogger InteractionsReceivedLog { get; } = new EventLogger("interactions-received");
        public static EventLogger InteractionsUpdatedLog { get; } = new EventLogger("interactions-updated");
        
        public EventLogger[] Loggers = new EventLogger[]
        {
            FrameUpdateReceivedLog,
            FrameUpdatedLog,
            SharedStateSentLog,
            SharedStateReceivedLog,
            SharedStateUpdatedLog,
            SharedStatePingPong,
            UnityFrameLog,
            AvatarCount,
            InteractionCount,
            InteractionsSentLog,
            InteractionsReceivedLog,
            InteractionsUpdatedLog
        };

        private string guid;

        private bool isLogging = false;

        public bool IsLogging => isLogging;

        private void Update()
        {
            UnityFrameLog.Log(Time);
            InteractionCount.Log(Time, prototype.Sessions.Imd.Interactions.Count);
            AvatarCount.Log(Time, prototype.Sessions.Multiplayer.Avatars.AvatarCount);
        }

        public void StartLogging()
        {
            StopLogging();
            var guid = Guid.NewGuid().ToString();
            foreach(var logger in Loggers)
                logger.StartLogging(guid);
            isLogging = true;
        }

        public void StopLogging()
        {
            foreach(var logger in Loggers)
                logger.StopLogging();
            isLogging = false;
        }

        private void OnDisable()
        {
            StopLogging();
        }

        private void OnDestroy()
        {
            StopLogging();
        }

        private static Stopwatch watch = new Stopwatch();

        public static float Time => watch.ElapsedMilliseconds / 1000f;

        private void Start()
        {
            watch.Start();
            
            guid = Guid.NewGuid().ToString();
            
            prototype.Sessions.Trajectory.FrameChanged += (frame, changes) => FrameReceiving.AddEvent();
            prototype.Sessions.Trajectory.FrameChanged += (frame, changes) =>
            {
                FrameUpdatedLog.Log(Time, prototype.Sessions.Trajectory.CurrentFrameIndex);
            };

            prototype.Sessions.Trajectory.FrameUpdateReceived += () =>
            {
                FrameUpdateReceivedLog.Log(Time);
            };
            
            prototype.Sessions.Multiplayer.BeforeStateChangesSent += AddTimestampToSharedState;
            prototype.Sessions.Multiplayer.BeforeStateChangesSent += () => MultiplayerSend.AddEvent();
            prototype.Sessions.Multiplayer.BeforeStateChangesSent += () =>
            {
                SharedStateSentLog.Log(Time);
            };
            prototype.Sessions.Multiplayer.SharedStateDictionaryKeyUpdated += ReceiveMultiplayerKey;
            prototype.Sessions.Multiplayer.BeforeSharedStateUpdate += () =>
            {
                SharedStateUpdatedLog.Log(Time);
            };
            
            prototype.Sessions.Multiplayer.StateUpdateRecieved += () => MultiplayerReceive.AddEvent();
            prototype.Sessions.Multiplayer.StateUpdateRecieved += () =>
            {
                SharedStateReceivedLog.Log(Time);
            };

            prototype.Sessions.Imd.InteractionsReceived += () => InteractionsReceive.AddEvent();
            prototype.Sessions.Imd.InteractionsReceived += () =>
            {
                InteractionsReceivedLog.Log(Time);
            };
            
            prototype.Sessions.Imd.InteractionsSent += () => InteractionsSend.AddEvent();
            prototype.Sessions.Imd.InteractionsSent += () =>
            {
                InteractionsSentLog.Log(Time);
            };
            
            prototype.Sessions.Imd.InteractionsUpdated += () =>
            {
                InteractionsUpdatedLog.Log(Time);
            };
        }

        private void ReceiveMultiplayerKey(string key, object value)
        {
            if (key == $"debug.{guid}" && value is double t)
            {
                var timeDifference = Time - t;
                MultiplayerPingPong.AddTimeDifference((float) timeDifference);
                SharedStatePingPong.Log(t, Time);
            }
        }

        private void AddTimestampToSharedState()
        {
            prototype.Sessions.Multiplayer.SetSharedState($"debug.{guid}", Time);
        }
    }

    public class EventTimer
    {
        private int runningAverageCount = 30;
        private float? previousTime;
        private float average;

        public void AddEvent()
        {
            AppendTime(NarupaXRDebugger.Time);
        }

        private void AppendTime(float time)
        {
            if (!previousTime.HasValue)
            {
                previousTime = time;
                return;
            }

            var timeDifference = time - previousTime.Value;
            previousTime = time;
            AddTimeDifference(timeDifference);
        }

        public void AddTimeDifference(float td)
        {
            average += (td - average) / runningAverageCount;
        }

        public float AverageTimeDifference()
        {
            return average;
        }

        public float AverageNumberPerSecond()
        {
            return 1f / AverageTimeDifference();
        }
    }

    public class EventLogger
    {
        private string suffix;
        private StreamWriter file;

        public EventLogger(string suffix)
        {
            this.suffix = suffix;
        }

        public void StartLogging(string guid)
        {
            var directory = Path.Combine(Application.dataPath, "../Debug");
            System.IO.Directory.CreateDirectory(directory);
            var filename = $"{guid}-{suffix}.csv";
            file = new StreamWriter(Path.Combine(directory, filename));
        }

        public void Log(params object[] arguments)
        {
            if (file == null)
                return;
            file.Write(string.Join(", ", arguments) + "\n");
        }

        public void StopLogging()
        {
            file?.Close();
            file = null;
        }
    }
}