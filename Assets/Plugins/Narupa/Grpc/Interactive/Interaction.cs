using System.Collections.Generic;
using System.Runtime.Serialization;
using Narupa.Grpc.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace Narupa.Grpc.Interactive
{
    [DataContract]
    public class Interaction
    {
        public string InteractionId;

        [DataMember(Name = "position")]
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 Position;

        [DataMember(Name = "particles")]
        public List<int> Particles;

        [DataMember(Name = "properties")]
        public InteractionProperties Properties;

        [DataContract]
        public class InteractionProperties
        {
            [DataMember(Name = "type")]
            public string InteractionType = "gaussian";

            [DataMember(Name = "scale")]
            public float Scale = 1f;

            [DataMember(Name = "mass_weighted")]
            public bool MassWeighted = true;

            [DataMember(Name = "reset_velocities")]
            public bool ResetVelocities = false;
            
            [DataMember(Name = "max_force")]
            public float MaxForce = float.PositiveInfinity;
        }
    }
}