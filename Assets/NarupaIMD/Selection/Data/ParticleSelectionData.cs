using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace NarupaIMD.Selection
{
    /// <summary>
    /// A set of particles indices which are used for defining behaviours such as interaction
    /// groups and visualisations.
    /// </summary>
    [Serializable]
    [DataContract]
    public class ParticleSelectionData
    {
        /// <summary>
        /// The ordered, 0-based indices of all particles in this selection.
        /// </summary>
        [DataMember(Name = "particle_ids")]
        public List<int> ParticleIds { get; set; }
        
        /// <summary>
        /// User-defined properties for this selection.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> OtherData { get; set; }
    }
}