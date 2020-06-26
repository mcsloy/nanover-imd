using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

namespace NarupaIMD
{
    public class NarupaXRDebugger : MonoBehaviour
    {
        [SerializeField]
        private NarupaIMDPrototype prototype;

        [SerializeField]
        private NarupaImdSimulation simulation;

        public EventTimer FrameReceiving { get; } = new EventTimer();
        
        public EventTimer MultiplayerSend { get; } = new EventTimer();

        public EventTimer MultiplayerReceive { get; } = new EventTimer();

        public EventTimer MultiplayerPingPong { get; } = new EventTimer();

        public static EventLogger FrameReceivedLog { get; } = new EventLogger("frame-received");
        public static EventLogger MultiplayerSentLog { get; } = new EventLogger("mp-sent");
        public static EventLogger MultiplayerReceivedLog { get; } = new EventLogger("mp-received");
        public static EventLogger MultiplayerPingPongLog { get; } = new EventLogger("mp-pingpong");
        public static EventLogger UnityFrameLog { get; } = new EventLogger("unity-update");
        
        public EventLogger[] Loggers = new EventLogger[]
        {
            FrameReceivedLog,
            MultiplayerSentLog,
            MultiplayerReceivedLog,
            MultiplayerPingPongLog,
            UnityFrameLog
        };

        private string guid;

        private bool isLogging = false;

        public bool IsLogging => isLogging;

        private void Update()
        {
            UnityFrameLog.Log(Time);
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
            simulation.Trajectory.FrameChanged += (frame, changes) => FrameReceiving.AddEvent();
            simulation.Trajectory.FrameChanged += (frame, changes) =>
            {
                FrameReceivedLog.Log(Time, simulation.Trajectory.CurrentFrameIndex);
            };
            simulation.Multiplayer.BeforeFlushChanges += AddTimestampToSharedState;
            simulation.Multiplayer.BeforeFlushChanges += () => MultiplayerSend.AddEvent();
            simulation.Multiplayer.BeforeFlushChanges += () =>
            {
                MultiplayerSentLog.Log(Time);
            };
            simulation.Multiplayer.SharedStateDictionaryKeyUpdated += ReceiveMultiplayerKey;
            simulation.Multiplayer.ReceiveUpdate += () =>
            {
                MultiplayerReceivedLog.Log(Time);
            };
            simulation.Multiplayer.ReceiveUpdate += () => MultiplayerReceive.AddEvent();
        }

        private void ReceiveMultiplayerKey(string key, object value)
        {
            if (key == $"debug.{guid}" && value is double t)
            {
                var timeDifference = Time - t;
                MultiplayerPingPong.AddTimeDifference((float) timeDifference);
                MultiplayerPingPongLog.Log(t, Time);
            }
        }

        private void AddTimestampToSharedState()
        {
            simulation.Multiplayer.SetSharedState($"debug.{guid}", Time);
        }
    }

    public class EventTimer
    {
        private int runningAverageCount = 30;
        private Queue<float> timeSteps = new Queue<float>();
        private float? previousTime;

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
            timeSteps.Enqueue(td);
            if (timeSteps.Count > runningAverageCount)
                timeSteps.Dequeue();
        }

        public float AverageTimeDifference()
        {
            if (timeSteps.Count < runningAverageCount)
                return float.NaN;
            return timeSteps.Average();
        }

        public float AverageNumberPerSecond()
        {
            return 1f / AverageTimeDifference();
        }

        public float LatestTimeDifference => timeSteps.Last();
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