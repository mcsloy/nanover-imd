using System;
using System.Collections;
using System.Linq;
using Narupa.Core.Async;
using Narupa.Core.Math;
using Narupa.Frontend.Utility;
using Narupa.Frontend.XR;
using Narupa.Grpc.Multiplayer;
using Narupa.Session;
using NarupaIMD;
using NarupaIMD.UI;
using UnityEngine;
using UnityEngine.XR;
using Avatar = Narupa.Grpc.Multiplayer.Avatar;

namespace NarupaXR
{
    public class NarupaXRAvatarManager : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private NarupaMultiplayer multiplayer;
        
        [SerializeField]
        private NarupaXRPrototype application;
        
        [SerializeField]
        private NarupaImdSimulation simulation;

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
            
            multiplayer.Session.MultiplayerJoined += SessionOnMultiplayerJoined;
        }

        private void SessionOnMultiplayerJoined()
        {
            multiplayer.Session.Avatars.LocalAvatar.Color = PlayerColor.GetPlayerColor();
            multiplayer.Session.Avatars.LocalAvatar.Name = PlayerName.GetPlayerName();
            sendAvatarsCoroutine = StartCoroutine(SendAvatars());
        }

        private void OnDisable()
        {
            StopCoroutine(sendAvatarsCoroutine);
        }

        private IEnumerator SendAvatars()
        {
            var leftHand = XRNode.LeftHand.WrapAsPosedObject();
            var rightHand = XRNode.RightHand.WrapAsPosedObject();
            var headset = XRNode.Head.WrapAsPosedObject();

            while (true)
            {
                if (simulation.Multiplayer.HasPlayer)
                    simulation.Multiplayer.Avatars.LocalAvatar.SetTransformations(
                        TransformPoseWorldToCalibrated(headset.Pose),
                        TransformPoseWorldToCalibrated(leftHand.Pose),
                        TransformPoseWorldToCalibrated(rightHand.Pose));

                simulation.Multiplayer.Avatars.FlushLocalAvatar();

                yield return null;
            }
        }

        private void UpdateRendering()
        {
            var headsets = simulation.Multiplayer
                                     .Avatars.OtherPlayerAvatars
                                     .SelectMany(avatar => avatar.Components, (avatar, component) =>
                                                     (Avatar: avatar, Component: component))
                                     .Where(res => res.Component.Name == Avatar.HeadsetName);

            var controllers = simulation.Multiplayer
                                        .Avatars.OtherPlayerAvatars
                                        .SelectMany(avatar => avatar.Components, (avatar, component) =>
                                                        (Avatar: avatar, Component: component))
                                        .Where(res => res.Component.Name == Avatar.LeftHandName
                                                   || res.Component.Name == Avatar.RightHandName);

            headsetObjects.MapConfig(headsets, UpdateAvatarComponent);
            controllerObjects.MapConfig(controllers, UpdateAvatarComponent);

            void UpdateAvatarComponent((Avatar Avatar, AvatarComponent Component) value, AvatarModel model)
            {
                var transformed = TransformPoseCalibratedToWorld(value.Component.Transformation).Value;
                model.transform.localPosition = transformed.Position;
                model.transform.localRotation = transformed.Rotation;
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