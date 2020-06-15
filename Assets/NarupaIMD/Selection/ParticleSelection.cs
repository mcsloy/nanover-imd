using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace NarupaIMD.Selection
{
    [Serializable]
    [DataContract]
    public class ParticleSelection
    {
        [DataMember(Name = "particle_ids")]
        public List<int> ParticleIds { get; set; }
        
        [JsonExtensionData]
        public Dictionary<string, object> OtherData { get; set; }
    }
}