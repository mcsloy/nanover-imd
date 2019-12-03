using System;
using Narupa.Visualisation;
using NarupaIMD.UI;
using UnityEngine;

namespace NarupaIMD.State
{
    public class TrajectoryWidget : MonoBehaviour
    {
        [SerializeField]
        private ConnectedApplicationState application;
        
        public SynchronisedFrameSource FrameSynchronizer { get; private set; }
        
        private void OnEnable()
        {
            FrameSynchronizer = gameObject.GetComponent<SynchronisedFrameSource>();
            if (FrameSynchronizer == null)
                FrameSynchronizer = gameObject.AddComponent<SynchronisedFrameSource>();
            FrameSynchronizer.FrameSource = application.Sessions.Trajectory;
        }

        private void OnDisable()
        {
            FrameSynchronizer.FrameSource = null;
        }
    }
}