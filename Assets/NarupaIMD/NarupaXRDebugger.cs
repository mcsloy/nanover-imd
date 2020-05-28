using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Frame;
using NarupaXR;
using UnityEngine;

namespace NarupaIMD
{
    public class NarupaXRDebugger : MonoBehaviour
    {
        [SerializeField]
        private NarupaXRPrototype prototype;

        public EventLogger FrameReceiving { get; } = new EventLogger();
        
        public EventLogger MultiplayerSend { get; } = new EventLogger();

        public EventLogger MultiplayerReceive { get; } = new EventLogger();

        public EventLogger MultiplayerPingPong { get; } = new EventLogger();

        private string guid;
        
        private void Start()
        {
            guid = new Guid().ToString();
            prototype.Sessions.Trajectory.FrameChanged += (frame, changes) => FrameReceiving.AddEvent();
            prototype.Sessions.Multiplayer.BeforeFlushChanges += AddTimestampToSharedState;
            prototype.Sessions.Multiplayer.BeforeFlushChanges += () => MultiplayerSend.AddEvent();
            prototype.Sessions.Multiplayer.SharedStateDictionaryKeyUpdated += ReceiveMultiplayerKey;
            prototype.Sessions.Multiplayer.ReceiveUpdate += () => MultiplayerReceive.AddEvent();
        }

        private void ReceiveMultiplayerKey(string key, object value)
        {
            if (key == $"debug.{guid}" && value is double t)
            {
                var timeDifference = Time.time - t;
                MultiplayerPingPong.AddTimeDifference((float) timeDifference);
            }
        }

        private void AddTimestampToSharedState()
        {
            prototype.Sessions.Multiplayer.SetSharedState($"debug.{guid}", Time.time);
        }
    }

    public class EventLogger
    {
        private int runningAverageCount = 30;
        private Queue<float> timeSteps = new Queue<float>();
        private float? previousTime;

        public void AddEvent()
        {
            AppendTime(Time.time);
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
}