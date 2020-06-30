namespace NarupaIMD.Selection
{
    /// <summary>
    /// The method in which the particles with which to interact are chosen given a single particle
    /// selection.
    /// </summary>
    public enum InteractionGroupMethod
    {
        /// <summary>
        /// Interact solely with the selected particle.
        /// </summary>
        Single,
        
        /// <summary>
        /// Interact with all particles in the same interaction group as the particle.
        /// </summary>
        Group,
        
        /// <summary>
        /// Forbid interactions with this particle.
        /// </summary>
        None
    }
}