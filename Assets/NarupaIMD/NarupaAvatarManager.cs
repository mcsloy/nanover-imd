using System.Collections;
using System.Linq;
using Narupa.Core.Math;
using Narupa.Frontend.Utility;
using Narupa.Frontend.XR;
using Narupa.Grpc.Multiplayer;
using NarupaIMD.UI;
using UnityEngine;
using UnityEngine.XR;

namespace NarupaIMD
{
    public class NarupaAvatarManager : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private NarupaMultiplayer multiplayer;
        
        [SerializeField]
        private NarupaIMDPrototype application;

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
            
            PlayerColor.PlayerColorChanged += PlayerColorOnPlayerColorChanged;
            PlayerName.PlayerNameChanged += PlayerNameOnPlayerNameChanged;
            
            simulation.Multiplayer.Avatars.LocalAvatar.Color = PlayerColor.GetPlayerColor();
            simulation.Multiplayer.Avatars.LocalAvatar.Name = PlayerName.GetPlayerName();
            
            sendAvatarsCoroutine = StartCoroutine(UpdateLocalAvatar());
        }

        private void PlayerNameOnPlayerNameChanged()
        {
            simulation.Multiplayer.Avatars.LocalAvatar.Name = PlayerName.GetPlayerName();
        }

        private void PlayerColorOnPlayerColorChanged()
        {
            simulation.Multiplayer.Avatars.LocalAvatar.Color = PlayerColor.GetPlayerColor();
        }
        
        private void OnDisable()
        {
            StopCoroutine(sendAvatarsCoroutine);
            PlayerColor.PlayerColorChanged -= PlayerColorOnPlayerColorChanged;
            PlayerName.PlayerNameChanged -= PlayerNameOnPlayerNameChanged;
        }

        private IEnumerator UpdateLocalAvatar()
        {
            var leftHand = XRNode.LeftHand.WrapAsPosedObject();
            var rightHand = XRNode.RightHand.WrapAsPosedObject();
            var headset = XRNode.Head.WrapAsPosedObject();

            while (true)
            {
                if (simulation.Multiplayer.IsOpen)
                {
                    simulation.Multiplayer.Avatars.LocalAvatar.SetTransformations(
                            TransformPoseWorldToCalibrated(headset.Pose),
                        TransformPoseWorldToCalibrated(leftHand.Pose),
                        TransformPoseWorldToCalibrated(rightHand.Pose));

                simulation.Multiplayer.Avatars.FlushLocalAvatar();
                }

                yield return null;
            }
        }

        private void UpdateRendering()
        {
            var headsets = simulation.Multiplayer
                                     .Avatars.OtherPlayerAvatars
                                     .SelectMany(avatar => avatar.Components, (avatar, component) =>
                                                     (Avatar: avatar, Component: component))
                                     .Where(res => res.Component.Name == MultiplayerAvatar.HeadsetName);

            var controllers = simulation.Multiplayer
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