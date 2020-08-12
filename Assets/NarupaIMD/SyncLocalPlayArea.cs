using System;
using UnityEngine;
using Valve.VR;

namespace NarupaIMD
{
    public class SyncLocalPlayArea : MonoBehaviour
    {
        [SerializeField] private NarupaImdSimulation prototype;

        private void OnEnable()
        {
            prototype.Multiplayer.MultiplayerJoined += MultiplayerOnMultiplayerJoined;
        }

        private void MultiplayerOnMultiplayerJoined()
        {
            var chaperone = OpenVR.Chaperone;
            if (chaperone == null)
                throw new InvalidOperationException("Chaperone missing!");

            while (chaperone.GetCalibrationState() != ChaperoneCalibrationState.OK)
                throw new InvalidOperationException("Chaperone not calibrated");

            var rect = new HmdQuad_t();
            if(!chaperone.GetPlayAreaRect(ref rect))
                throw new InvalidOperationException("Can't get play area");

            var id = $"playarea.{prototype.Multiplayer.AccessToken}";

            var coordinates = new float[]
            {
                rect.vCorners0.v0,
                rect.vCorners0.v1,
                rect.vCorners0.v2,
                rect.vCorners1.v0,
                rect.vCorners1.v1,
                rect.vCorners1.v2,
                rect.vCorners2.v0,
                rect.vCorners2.v1,
                rect.vCorners2.v2,
                rect.vCorners3.v0,
                rect.vCorners3.v1,
                rect.vCorners3.v2
            };
        
            prototype.Multiplayer.SetSharedState(id, coordinates);
        }
    }
}
