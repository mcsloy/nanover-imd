using System;
using System.Collections;
using System.Linq;
using Narupa.Core.Async;
using Narupa.Core.Math;
using Narupa.Frontend.Utility;
using Narupa.Frontend.XR;
using Narupa.Session;
using NarupaIMD;
using UnityEngine;
using UnityEngine.XR;

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
                    simulation.Multiplayer.SetVRAvatar(
                        TransformPoseWorldToCalibrated(headset.Pose),
                        TransformPoseWorldToCalibrated(leftHand.Pose),
                        TransformPoseWorldToCalibrated(rightHand.Pose));

                yield return null;
            }
        }

        private void UpdateRendering()
        {
            var localPlayerId = simulation.Multiplayer.PlayerId;
            var headsets = simulation.Multiplayer
                                     .Avatars.Values
                                     .SelectMany(avatar => avatar.Components.Select(pair => new
                                     {
                                         avatar.PlayerId,
                                         Name = pair.Key,
                                         Pose = pair.Value
                                     }))
                                     .Where(component => component.Name == MultiplayerSession.HeadsetName)
                                     .Where(component => component.PlayerId != localPlayerId)
                                     .Select(component => component.Pose)
                                     .OfType<Transformation>();

            var controllers = simulation.Multiplayer
                                        .Avatars.Values
                                        .SelectMany(avatar => avatar.Components.Select(pair => new
                                        {
                                            avatar.PlayerId,
                                            Name = pair.Key,
                                            Pose = pair.Value
                                        }))
                                        .Where(component => component.Name == MultiplayerSession.LeftHandName 
                                                         || component.Name == MultiplayerSession.RightHandName)
                                        .Where(component => component.PlayerId != localPlayerId)
                                        .Select(component => component.Pose)
                                        .OfType<Transformation>();

            headsetObjects.MapConfig(headsets, UpdateAvatarComponent);
            controllerObjects.MapConfig(controllers, UpdateAvatarComponent);

            void UpdateAvatarComponent(Transformation pose, Transform transform)
            {
                var transformed = TransformPoseCalibratedToWorld(pose).Value;
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