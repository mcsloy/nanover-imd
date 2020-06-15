using System.Runtime.Serialization;

namespace NarupaIMD.Selection
{
    [DataContract]
    public class InteractionGroup
    {
        [DataMember(Name = "selection")]
        public string SelectionKey { get; set; }
        
        [DataMember(Name="method")]
        public InteractionGroupMethod Method { get; set; }
        
        [DataMember(Name="reset_velocities")]
        public bool ResetVelocities { get; set; }
    }
}