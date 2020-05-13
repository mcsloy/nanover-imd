using System.Collections.Generic;
using Narupa.Core.Math;
using UnityEngine;

namespace Narupa.Grpc.Multiplayer
{
    public class Avatar
    {
        public const string HeadsetName = "headset";
        public const string LeftHandName = "hand.left";
        public const string RightHandName = "hand.right";

        public string ID;

        public string Name;

        public Color Color;

        public List<AvatarComponent> Components = new List<AvatarComponent>();

        public void SetTransformations(Transformation? headsetTransformation,
                                       Transformation? leftHandTransformation,
                                       Transformation? rightHandTransformation)
        {
            Components.Clear();
            if (headsetTransformation.HasValue)
            {
                Components.Add(new AvatarComponent(HeadsetName,
                                                   headsetTransformation.Value.Position,
                                                   headsetTransformation.Value.Rotation));
            }

            if (leftHandTransformation.HasValue)
            {
                Components.Add(new AvatarComponent(LeftHandName,
                                                   leftHandTransformation.Value.Position,
                                                   leftHandTransformation.Value.Rotation));
            }

            if (rightHandTransformation.HasValue)
            {
                Components.Add(new AvatarComponent(RightHandName,
                                                   rightHandTransformation.Value.Position,
                                                   rightHandTransformation.Value.Rotation));
            }
        }
    }

    public class AvatarComponent
    {
        public string Name;

        private Vector3 Position;

        private Quaternion Rotation;

        public AvatarComponent(string name, Vector3 position, Quaternion rotation)
        {
            Name = name;
            Position = position;
            Rotation = rotation;
        }

        public UnitScaleTransformation Transformation =>
            new UnitScaleTransformation(Position, Rotation);
    }
}