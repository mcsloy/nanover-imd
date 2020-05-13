using System.Collections.Generic;
using System.Linq;
using Narupa.Core;
using Narupa.Core.Math;
using UnityEngine;

namespace Narupa.Grpc.Multiplayer
{
    public class Avatar
    {
        public const string HeadsetName = "headset";
        public const string LeftHandName = "hand.left";
        public const string RightHandName = "hand.right";

        private const string FieldID = "playerid";
        private const string FieldName = "name";
        private const string FieldColor = "color";
        private const string FieldComponents = "components";

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

        public Dictionary<string, object> ToData()
        {
            return new Dictionary<string, object>()
            {
                [FieldID] = ID,
                [FieldName] = Name,
                [FieldColor] = "#" + ColorUtility.ToHtmlStringRGB(Color),
                [FieldComponents] = Components.Select(c => c.ToData()).ToList()
            };
        }

        public void FromData(Dictionary<string, object> data)
        {
            if (data == null)
                return;
            if (data.TryGetValue<string>(FieldName, out var name))
                Name = name;
            if (data.TryGetValue<string>(FieldColor, out var colorStr) &&
                ColorUtility.TryParseHtmlString(colorStr, out var color))
                Color = color;
            if (data.TryGetValue<IReadOnlyList<object>>(FieldComponents, out var componentList))
            {
                Components.Clear();
                foreach (var componentData in componentList)
                {
                    var comp = new AvatarComponent();
                    comp.FromData(componentData as Dictionary<string, object>);
                    Components.Add(comp);
                }
            }
        }
    }

    public class AvatarComponent
    {
        public string Name;

        private Vector3 Position;

        private Quaternion Rotation;
        
        private const string FieldName = "name";
        private const string FieldPosition = "position";
        private const string FieldRotation = "rotation";

        public AvatarComponent()
        {
            
        }

        public AvatarComponent(string name, Vector3 position, Quaternion rotation)
        {
            Name = name;
            Position = position;
            Rotation = rotation;
        }

        public UnitScaleTransformation Transformation =>
            new UnitScaleTransformation(Position, Rotation);
        
        public Dictionary<string, object> ToData()
        {
            return new Dictionary<string, object>()
            {
                [FieldName] = Name,
                [FieldPosition] = new List<object> {Position.x, Position.y, Position.z},
                [FieldRotation] = new List<object> {Rotation.x, Rotation.y, Rotation.z, Rotation.w},
            };
        }

        public void FromData(Dictionary<string, object> data)
        {
            if (data == null)
                return;
            if (data.TryGetValue<string>(FieldName, out var name))
                Name = name;
            if (data.TryGetValue<IReadOnlyList<object>>(FieldPosition, out var pos))
                Position = pos.GetVector3();
            if (data.TryGetValue<IReadOnlyList<object>>(FieldRotation, out var rot))
                Rotation = rot.GetQuaternion();
        }
    }
}