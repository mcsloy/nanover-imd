using System.Collections;
using System.Linq;
using Narupa.Core.Async;
using Narupa.Core.Math;
using Narupa.Frontend.Utility;
using Narupa.Frontend.XR;
using Narupa.Grpc.Multiplayer;
using Narupa.Session;
using UnityEngine;
using UnityEngine.XR;

namespace NarupaXR
{
    public class NarupaXRAvatarManager : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private NarupaXRPrototype narupa;

        [SerializeField]
        private Transform headsetPrefab;

        [SerializeField]
        private Transform controllerPrefab;
#pragma warning restore 0649

        private IndexedPool<Transform> headsetObjects;
        private IndexedPool<Transform> controllerObjects;

        private void Awake()
        {
            Setup();
        }

        private void Update()
        {
            UpdateRendering();
        }

        private void Setup()
        {
            headsetObjects = new IndexedPool<Transform>(
                () => Instantiate(headsetPrefab),
                transform => transform.gameObject.SetActive(true),
                transform => transform.gameObject.SetActive(false)
            );

            controllerObjects = new IndexedPool<Transform>(
                () => Instantiate(controllerPrefab),
                transform => transform.gameObject.SetActive(true),
                transform => transform.gameObject.SetActive(false)
            );

            StartCoroutine(AutoJoin());
            StartCoroutine(SendAvatars());
        }

        private IEnumerator AutoJoin()
        {
            while (!narupa.Sessions.Multiplayer.IsOpen)
                yield return null;

            narupa.Sessions.Multiplayer.JoinMultiplayer("NarupaXR")
                  .AwaitInBackground();
        }

        private IEnumerator SendAvatars()
        {
            var leftHand = XRNode.LeftHand.WrapAsPosedObject();
            var rightHand = XRNode.RightHand.WrapAsPosedObject();
            var headset = XRNode.Head.WrapAsPosedObject();

            while (true)
            {
                if (narupa.Sessions.Multiplayer.HasPlayer)
                {
                    narupa.Sessions.Multiplayer.Avatars.LocalAvatar.SetTransformations(
                        TransformPoseWorldToCalibrated(headset.Pose),
                        TransformPoseWorldToCalibrated(leftHand.Pose),
                        TransformPoseWorldToCalibrated(rightHand.Pose));

                    narupa.Sessions.Multiplayer.Avatars.FlushLocalAvatar();
                }

                yield return null;
            }
        }

        private void UpdateRendering()
        {
            var headsets = narupa.Sessions.Multiplayer
                                 .Avatars.OtherPlayerAvatars
                                 .SelectMany(avatar => avatar.Components, (avatar, component) =>
                                                 (Avatar: avatar, Component: component))
                                 .Where(res => res.Component.Name == MultiplayerAvatar.HeadsetName);


            var controllers = narupa.Sessions.Multiplayer
                                    .Avatars.OtherPlayerAvatars
                                    .SelectMany(avatar => avatar.Components, (avatar, component) =>
                                                    (Avatar: avatar, Component: component))
                                    .Where(res => res.Component.Name == MultiplayerAvatar.LeftHandName
                                               || res.Component.Name == MultiplayerAvatar.RightHandName);


            headsetObjects.MapConfig(headsets, UpdateAvatarComponent);
            controllerObjects.MapConfig(controllers, UpdateAvatarComponent);

            void UpdateAvatarComponent((MultiplayerAvatar Avatar, MultiplayerAvatar.Component Component) value, Transform transform)
            {
                var transformed = TransformPoseCalibratedToWorld(value.Component.Transformation).Value;
                transform.SetPositionAndRotation(transformed.Position, transformed.Rotation);
            }
        }

        public Transformation? TransformPoseCalibratedToWorld(Transformation? pose)
        {
            if (pose is Transformation calibratedPose)
                return narupa.CalibratedSpace.TransformPoseCalibratedToWorld(calibratedPose);

            return null;
        }

        public Transformation? TransformPoseWorldToCalibrated(Transformation? pose)
        {
            if (pose is Transformation worldPose)
                return narupa.CalibratedSpace.TransformPoseWorldToCalibrated(worldPose);

            return null;
        }
    }
}