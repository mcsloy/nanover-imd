using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace NarupaIMD
{
    public class SyncTrackedObjects : MonoBehaviour
    {
        [SerializeField] private NarupaImdSimulation prototype;

        private void MultiplayerOnMultiplayerJoined()
        {
            var id = $"trackedobjects.{prototype.Multiplayer.AccessToken}";
            prototype.Multiplayer.SetSharedState(id, data);
        }

        private object[] data = new object[0];
        
        private void OnNewPoses(TrackedDevicePose_t[] poses)
        {
            if (poses.Length == 0)
                return;

            var list = new List<object>();

            var i = 0u;

            foreach (var pose in poses)
            {
                if (!pose.bDeviceIsConnected)
                    continue;

                if (!pose.bPoseIsValid)
                    continue;
                
                var xyz = new SteamVR_Utils.RigidTransform(pose.mDeviceToAbsoluteTracking);
                
                uint index = 0;
                var error = ETrackedPropertyError.TrackedProp_Success;
                
                var result = new System.Text.StringBuilder((int)64);
                var name = OpenVR.System.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_RenderModelName_String, result, 64, ref error);

                var data = new Dictionary<string, object>()
                {
                    ["position"] = new object[] {xyz.pos.x, xyz.pos.y, xyz.pos.z},
                    ["rotation"] = new object[] {xyz.rot.x, xyz.rot.y, xyz.rot.z, xyz.rot.w},
                    ["name"] = name.ToString()
                };
                
                list.Add(data);

                i++;
            }

            data = list.ToArray();
        }

        SteamVR_Events.Action newPosesAction;

        private void OnEnable()
        {
            prototype.Multiplayer.MultiplayerJoined += MultiplayerOnMultiplayerJoined;
            
            newPosesAction = SteamVR_Events.NewPosesAction(OnNewPoses);
            
            newPosesAction.enabled = true;
        }

        void OnDisable()
        {
            newPosesAction.enabled = false;
        }
    }
}
