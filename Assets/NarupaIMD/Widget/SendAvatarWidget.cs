using Narupa.Frontend.XR;
using Narupa.Session;
using System.Collections;
using System.Linq;
using Narupa.Core.Async;
using Narupa.Core.Math;
using UnityEngine;
using UnityEngine.XR;
using Narupa.Frontend.Utility;
using NarupaIMD.State;
using NarupaIMD.UI;

namespace NarupaXR
{
    public class SendAvatarWidget : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private ConnectedApplicationState narupa;
#pragma warning restore 0649

        private IndexedPool<Transform> avatarObjects;

        [SerializeField]
        private CalibratedSpaceWidget calibratedSpace;

        private void OnEnable()
        {
            Setup();
        }

        private void Setup()
        {
            StartCoroutine(SendAvatars());
        }

        private IEnumerator SendAvatars()
        {
            var leftHand = XRNode.LeftHand.WrapAsPosedObject();
            var rightHand = XRNode.RightHand.WrapAsPosedObject();
            var headset = XRNode.Head.WrapAsPosedObject();

            while (true)
            {
                if (narupa.Sessions.Multiplayer.HasPlayer)
                    narupa.Sessions.Multiplayer.SetVRAvatar(TransformPoseWorldToCalibrated(headset.Pose),
                                                            TransformPoseWorldToCalibrated(leftHand.Pose),
                                                            TransformPoseWorldToCalibrated(rightHand.Pose));

                yield return null;
            }
        }

        public Transformation? TransformPoseWorldToCalibrated(Transformation? pose)
        {
            if (pose is Transformation worldPose)
                return calibratedSpace.CalibratedSpace.TransformPoseWorldToCalibrated(worldPose);

            return null;
        }
    }
}
