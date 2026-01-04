namespace EtherDomes.Combat
{
    /// <summary>
    /// Interface for entities that can show a target indicator.
    /// </summary>
    public interface ITargetIndicator
    {
        /// <summary>
        /// Set whether this entity is currently targeted.
        /// </summary>
        void SetTargeted(bool targeted);
    }
}
