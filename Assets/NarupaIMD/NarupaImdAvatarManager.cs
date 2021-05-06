using System;
using System.Collections;
using System.Linq;
using Narupa.Core.Math;
using Narupa.Frontend.Utility;
using Narupa.Frontend.XR;
using Narupa.Grpc.Multiplayer;
using UnityEngine;
using UnityEngine.XR;

namespace NarupaImd
{
    public class NarupaImdAvatarManager : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private NarupaImdApplication application;
        
        [SerializeField]
        private NarupaImdSimulation narupa;

        [SerializeField]
        private AvatarModel headsetPrefab;

        [SerializeField]
        private AvatarModel controllerPrefab;
#pragma warning restore 0649
        
        private IndexedPool<AvatarModel> headsetObjects;
        private IndexedPool<AvatarModel> controllerObjects;
        
        private Coroutine sendAvatarsCoroutine;

        private void Update()
        {
            UpdateRendering();
        }

        private void OnEnable()
        {
            headsetObjects = new IndexedPool<AvatarModel>(
                () => Instantiate(headsetPrefab),
                transform => transform.gameObject.SetActive(true),
                transform => transform.gameObject.SetActive(false)
            );

            controllerObjects = new IndexedPool<AvatarModel>(
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

            // TODO: remove
            var name = Environment.MachineName;
            var color = Color.HSVToRGB(UnityEngine.Random.value, .8f, 1f);

            while (true)
            {
                if (narupa.Multiplayer.IsOpen)
                {
                    narupa.Multiplayer.Avatars.LocalAvatar.SetTransformations(
                        TransformPoseWorldToCalibrated(headset.Pose),
                        TransformPoseWorldToCalibrated(leftHand.Pose),
                        TransformPoseWorldToCalibrated(rightHand.Pose));
                    narupa.Multiplayer.Avatars.LocalAvatar.Name = name;
                    narupa.Multiplayer.Avatars.LocalAvatar.Color = color;
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

            void UpdateAvatarComponent((MultiplayerAvatar Avatar, MultiplayerAvatar.Component Component) value, AvatarModel model)
            {
                var transformed = TransformPoseCalibratedToWorld(value.Component.Transformation).Value;
                model.transform.SetPositionAndRotation(transformed.Position, transformed.Rotation);
                model.SetPlayerColor(value.Avatar.Color);
                model.SetPlayerName(value.Avatar.Name);
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