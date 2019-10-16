using Narupa.Frontend.XR;
using Narupa.Session;
using System.Collections;
using System.Linq;
using Narupa.Core.Async;
using Narupa.Core.Math;
using UnityEngine;
using UnityEngine.XR;
using Narupa.Frontend.Utility;

namespace NarupaXR
{
    public class NarupaXRAvatarManager : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private NarupaXRPrototype narupa;
#pragma warning restore 0649

        private IndexedPool<Transform> avatarObjects;

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
            Transform CreateAvatarCube()
            {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.localScale = Vector3.one * 0.1f;
                return cube.transform;
            }

            avatarObjects = new IndexedPool<Transform>(
                CreateAvatarCube, 
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
                    narupa.Sessions.Multiplayer.SetVRAvatar(TransformPoseWorldToCalibrated(headset.Pose),
                                                            TransformPoseWorldToCalibrated(leftHand.Pose),
                                                            TransformPoseWorldToCalibrated(rightHand.Pose));

                yield return null;
            }
        }

        private void UpdateRendering()
        {
            var localPlayerId = narupa.Sessions.Multiplayer.PlayerId;
            var components = narupa.Sessions.Multiplayer
                .Avatars.Values
                .SelectMany(avatar => avatar.Components.Select(pair => new { avatar.PlayerId, Name = pair.Key, Pose = pair.Value }))
                .Where(component => component.PlayerId != localPlayerId
                                 || component.Name != MultiplayerSession.HeadsetName)
                .Select(component => component.Pose)
                .OfType<Transformation>();

            avatarObjects.MapConfig(components, UpdateAvatarComponent);

            void UpdateAvatarComponent(Transformation pose, Transform transform)
            {
                var transformed = TransformPoseCalibratedToWorld(pose).Value;
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
