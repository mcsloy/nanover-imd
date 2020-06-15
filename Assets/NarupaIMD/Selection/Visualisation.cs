using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace NarupaIMD.Selection
{
    [Serializable]
    [DataContract]
    public class Visualisation
    {
        [DataMember(Name = "display_name")]
        public string DisplayName { get; set; }
        
        [DataMember(Name = "selection")]
        public string SelectionKey { get; set; }
        
        [DataMember(Name = "frame")]
        public Dictionary<string, object> Frame { get; set; }
        
        [DataMember(Name = "visualiser")]
        public object Visualiser { get; set; }
        
        [DataMember(Name = "hide")]
        public bool? Hide { get; set; }
        
        [DataMember(Name = "layer")]
        public int? Layer { get; set; }
        
        [DataMember(Name = "priority")]
        public float? Priority { get; set; }
        
        [JsonExtensionData]
        public Dictionary<string, object> OtherData { get; set; }
    }
}