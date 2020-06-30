using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace NarupaIMD.Selection
{
    /// <summary>
    /// A set of properties that apply to the given selection of particles which affect how the
    /// user can interact with them.
    /// </summary>
    [DataContract]
    public class InteractionGroupData
    {
        /// <summary>
        /// The selection these properties apply to.
        /// </summary>
        [DataMember(Name = "selection")]
        public string SelectionKey { get; set; }
        
        /// <summary>
        /// The method in which this group should be interacted with by default.
        /// </summary>
        [DataMember(Name="method")]
        public InteractionGroupMethod Method { get; set; }
        
        /// <summary>
        /// Should an interaction with these atoms have reset velocities applied to them after an
        /// interaction has finished?
        /// </summary>
        [DataMember(Name="reset_velocities")]
        public bool ResetVelocities { get; set; }
        
        /// <summary>
        /// User-defined properties for this interaction group
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> OtherData { get; set; }
    }
}