using System;
using System.Collections;
using System.Linq;
using Narupa.Core.Math;
using Narupa.Frontend.Utility;
using Narupa.Frontend.XR;
using Narupa.Grpc.Multiplayer;
using NarupaIMD;
using UnityEngine;
using UnityEngine.XR;

namespace NarupaXR
{
    public class NarupaXRAvatarManager : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private NarupaXRPrototype application;
        
        [SerializeField]
        private NarupaImdSimulation narupa;

        [SerializeField]
        private Transform headsetPrefab;

        [SerializeField]
        private Transform controllerPrefab;
#pragma warning restore 0649
        
        private IndexedPool<Transform> headsetObjects;
        private IndexedPool<Transform> controllerObjects;
        
        private Coroutine sendAvatarsCoroutine;

        private void Update()
        {
            UpdateRendering();
        }

        private void OnEnable()
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
            sendAvatarsCoroutine = StartCoroutine(UpdateLocalAvatar());
        }

        private void OnDisable()
        {
            StopCoroutine(sendAvatarsCoroutine);
        }

        private IEnumerator UpdateLocalAvatar()
        {
            var leftHand = XRNode.LeftHand.WrapAsPosedObject();
            var rightHand = XRNode.RightHand.WrapAsPosedObject();
            var headset = XRNode.Head.WrapAsPosedObject();

            while (true)
            {
                if (narupa.Multiplayer.IsOpen)
                {
                    narupa.Multiplayer.Avatars.LocalAvatar.SetTransformations(
                        TransformPoseWorldToCalibrated(headset.Pose),
                        TransformPoseWorldToCalibrated(leftHand.Pose),
                        TransformPoseWorldToCalibrated(rightHand.Pose));

                    narupa.Multiplayer.Avatars.FlushLocalAvatar();
                }

                yield return null;
            }
        }

        private void UpdateRendering()
        {
            var headsets = narupa.Multiplayer
                                 .Avatars.OtherPlayerAvatars
                                 .SelectMany(avatar => avatar.Components, (avatar, component) =>
                                                 (Avatar: avatar, Component: component))
                                 .Where(res => res.Component.Name == MultiplayerAvatar.HeadsetName);


            var controllers = narupa.Multiplayer
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
                return application.CalibratedSpace.TransformPoseCalibratedToWorld(calibratedPose);

            return null;
        }

        public Transformation? TransformPoseWorldToCalibrated(Transformation? pose)
        {
            if (pose is Transformation worldPose)
                return application.CalibratedSpace.TransformPoseWorldToCalibrated(worldPose);

            return null;
        }
    }
}