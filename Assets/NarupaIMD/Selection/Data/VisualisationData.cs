using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace NarupaIMD.Selection
{
    /// <summary>
    /// A visualisation of a set of particles, using a given renderer. Visualisations on the same
    /// layer are mutually exclusive, with those of higher priority taking precedence over those
    /// below.
    /// </summary>
    [Serializable]
    [DataContract]
    public class VisualisationData
    {
        /// <summary>
        /// A user-facing display name for the visualisation.
        /// </summary>
        [DataMember(Name = "display_name")]
        public string DisplayName { get; set; }
        
        /// <summary>
        /// The key of the particle selection that this visualisation is linked to.
        /// </summary>
        [DataMember(Name = "selection")]
        public string SelectionKey { get; set; }

        /// <summary>
        /// The definition of the visualiser this visualisation should use.
        /// </summary>
        [DataMember(Name = "visualiser")]
        public object Visualiser { get; set; }
        
        /// <summary>
        /// Should this visualisation be hidden from view?
        /// </summary>
        [DataMember(Name = "hide")]
        public bool? Hide { get; set; }
        
        /// <summary>
        /// The layer this visualisation is on. This dictates the order in which it is drawn to the
        /// screen, with layers being drawn from lowest to highest.
        /// </summary>
        [DataMember(Name = "layer")]
        public int? Layer { get; set; }
        
        /// <summary>
        /// The priority this visualisation has within its layer. Visualisations with higher
        /// priority are drawn after those with lower priority. If a particle would belong to two
        /// or more visualisers in the same layer, it is only drawn by the visualiser with the
        /// highest priority. 
        /// </summary>
        [DataMember(Name = "priority")]
        public float? Priority { get; set; }
        
        /// <summary>
        /// User-defined properties for this selection.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> OtherData { get; set; }
    }
}