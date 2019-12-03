using System.Linq;
using Narupa.Core.Math;
using Narupa.Frontend.Utility;
using Narupa.Session;
using NarupaIMD.State;
using NarupaIMD.UI;
using UnityEngine;

namespace NarupaIMD.Widget
{
    public class OtherPlayerAvatarsWidget : MonoBehaviour
    {
        [SerializeField]
        private CalibratedSpaceWidget calibratedSpace;

        [SerializeField]
        private ConnectedApplicationState narupa;

        [SerializeField]
        private GameObject headsetPrefab;

        [SerializeField]
        private GameObject controllerPrefab;

        private IndexedPool<GameObject> headAvatarPool;

        private IndexedPool<GameObject> controllerAvatarPool;

        private void OnEnable()
        {
            Setup();
        }

        private void Setup()
        {
            if (headAvatarPool == null)
                headAvatarPool = new IndexedPool<GameObject>(CreateHeadAvatar, ActivateObject, DeactivateObject);
            if (controllerAvatarPool == null)
                controllerAvatarPool = new IndexedPool<GameObject>(CreateControllerAvatar, ActivateObject, DeactivateObject);
        }

        private static void DeactivateObject(GameObject obj)
        {
            obj.SetActive(false);
        }

        private static void ActivateObject(GameObject obj)
        {
            obj.SetActive(true);
        }

        private void OnDisable()
        {
            headAvatarPool.SetActiveInstanceCount(0);
            controllerAvatarPool.SetActiveInstanceCount(0);
        }

        private GameObject CreateControllerAvatar()
        {
            return Instantiate(controllerPrefab);
        }

        private GameObject CreateHeadAvatar()
        {
            return Instantiate(headsetPrefab);
        }

        private void Update()
        {
            UpdateRendering();
        }

        private void UpdateRendering()
        {
            var localPlayerId = narupa.Sessions.Multiplayer.PlayerId;
            var components = narupa.Sessions.Multiplayer
                                   .Avatars.Values
                                   .SelectMany(avatar => avatar.Components.Select(pair => new
                                   {
                                       avatar.PlayerId,
                                       Name = pair.Key,
                                       Pose = pair.Value
                                   }))
                                   .Where(component => component.PlayerId != localPlayerId)
                                   .ToList();

            var headsets = components
                           .Where(component => component.Name == MultiplayerSession.HeadsetName)
                           .Select(component => component.Pose.Value);

            var controllers = components
                              .Where(component => component.Name != MultiplayerSession.HeadsetName)
                              .Select(component => component.Pose.Value);

            headAvatarPool.MapConfig(headsets, UpdateAvatarComponent);
            controllerAvatarPool.MapConfig(controllers, UpdateAvatarComponent);

            void UpdateAvatarComponent(Transformation pose, GameObject go)
            {
                var transformed = TransformPoseCalibratedToWorld(pose).Value;
                go.transform.SetPositionAndRotation(transformed.Position, transformed.Rotation);
            }
        }

        public Transformation? TransformPoseCalibratedToWorld(Transformation? pose)
        {
            if (pose is Transformation calibratedPose)
                return calibratedSpace.CalibratedSpace.TransformPoseCalibratedToWorld(
                    calibratedPose);

            return null;
        }
    }
}