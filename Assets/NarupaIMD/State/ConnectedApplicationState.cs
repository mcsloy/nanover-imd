using System;
using Narupa.Core.Async;
using NarupaIMD.State;
using NarupaXR;
using UnityEngine;
using Valve.VR;

namespace NarupaIMD.UI
{
    /// <summary>
    /// Represents the state of being connected to a server.
    /// </summary>
    public class ConnectedApplicationState : ApplicationState
    {
        [SerializeField]
        private SteamVR_ActionSet actionSet;

        [SerializeField]
        private NarupaXRPrototype prototype;

        [SerializeField]
        private NarupaXRSessionManager sessions;

        public NarupaXRSessionManager Sessions => sessions;
        
        public bool IsConnected => isActiveAndEnabled;

        public event Action ConnectToServer;

        public event Action DisconnectFromServer;
        
        protected virtual void OnEnable()
        {
            actionSet.Initialize();
            actionSet.Activate();
            sessions = new NarupaXRSessionManager();
            Application.GotoUserInteractionState();
        }

        private class ConnectionInfo
        {
            public string address;
            public int? trajectoryPort;
            public int? imdPort;
            public int? multiplayerPort;
        }

        private ConnectionInfo connectionInfo;

        public void Connect(string address, int? trajectoryPort, int? imdPort, int? multiplayerPort)
        {
            connectionInfo = new ConnectionInfo
            {
                address = address,
                trajectoryPort = trajectoryPort,
                imdPort = imdPort,
                multiplayerPort = multiplayerPort
            };
            if(!isActiveAndEnabled)
                Application.GotoConnectedState();
            sessions.Connect(address, trajectoryPort, imdPort, multiplayerPort);
            ConnectToServer?.Invoke();
        }

        protected virtual void OnDisable()
        {
            actionSet.Deactivate();
            sessions.CloseAsync().AwaitInBackground();
            sessions = null;
            DisconnectFromServer?.Invoke();
        }

        public void Play()
        {
            if(isActiveAndEnabled)
                Sessions.Trajectory.Play();
        }
        
        public void Pause()
        {
            if(isActiveAndEnabled)
                Sessions.Trajectory.Pause();
        }
        
        public void Reset()
        {
            if(isActiveAndEnabled)
                Sessions.Trajectory.Reset();
        }
        
        public void Disconnect()
        {
            Application.GotoDisconnectedState();
        }

        public void Reconnect()
        {
            Application.RefreshConnectedState();
            Connect(connectionInfo.address,
                    connectionInfo.trajectoryPort,
                    connectionInfo.imdPort,
                    connectionInfo.multiplayerPort);
        }

    }
}