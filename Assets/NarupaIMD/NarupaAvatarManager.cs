using System.Collections;
using System.Linq;
using Narupa.Core.Math;
using Narupa.Frontend.Utility;
using Narupa.Frontend.XR;
using Narupa.Grpc.Multiplayer;
using NarupaIMD;
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

            multiplayer.Session.MultiplayerJoined += SessionOnMultiplayerJoined;
            PlayerColor.PlayerColorChanged += PlayerColorOnPlayerColorChanged;
            PlayerName.PlayerNameChanged += PlayerNameOnPlayerNameChanged;
        }

        private void PlayerNameOnPlayerNameChanged()
        {
            multiplayer.Session.Avatars.LocalAvatar.Name = PlayerName.GetPlayerName();
        }

        private void PlayerColorOnPlayerColorChanged()
        {
            multiplayer.Session.Avatars.LocalAvatar.Color = PlayerColor.GetPlayerColor();
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
            multiplayer.Session.MultiplayerJoined -= SessionOnMultiplayerJoined;
            PlayerColor.PlayerColorChanged -= PlayerColorOnPlayerColorChanged;
            PlayerName.PlayerNameChanged -= PlayerNameOnPlayerNameChanged;
        }

        private IEnumerator SendAvatars()
        {
            while (true)
            {
                // multiplayer to world transformation
                var worldToMultiplayer = UnitScaleTransformation
                                         .FromTransformRelativeToWorld(transform).inverse;
                if (simulation.Multiplayer.HasPlayer)
                {
                    var headTransform = XRNode.Head.GetTransformation();
                    var leftHandTransform = XRNode.LeftHand.GetTransformation();
                    var rightHandTransform = XRNode.RightHand.GetTransformation();
                    simulation.Multiplayer.Avatars.LocalAvatar.SetTransformations(
                        worldToMultiplayer * headTransform,
                        worldToMultiplayer * leftHandTransform,
                        worldToMultiplayer * rightHandTransform);

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
                                        .SelectMany(avatar => avatar.Components,
                                                    (avatar, component) =>
                                                        (Avatar: avatar, Component: component))
                                        .Where(res => res.Component.Name == MultiplayerAvatar.LeftHandName
                                                   || res.Component.Name == MultiplayerAvatar.RightHandName);

            headsetObjects.MapConfig(headsets, UpdateAvatarComponent);
            controllerObjects.MapConfig(controllers, UpdateAvatarComponent);
            
            void UpdateAvatarComponent((MultiplayerAvatar Avatar, MultiplayerAvatar.Component Component) value, AvatarModel model)
            {
                model.transform.parent = this.transform;
                model.transform.localPosition = value.Component.Position;
                model.transform.localRotation = value.Component.Rotation;
                model.SetPlayerColor(value.Avatar.Color);
                model.SetPlayerName(value.Avatar.Name);
            }
        }
    }
}