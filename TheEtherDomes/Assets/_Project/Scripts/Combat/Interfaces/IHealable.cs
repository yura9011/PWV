namespace EtherDomes.Combat
{
    /// <summary>
    /// Interface for entities that can be healed.
    /// </summary>
    public interface IHealable
    {
        /// <summary>
        /// Apply healing to this entity.
        /// </summary>
        void Heal(float amount);
        
        /// <summary>
        /// Apply healing from a specific source.
        /// </summary>
        void Heal(float amount, ulong sourceId);
    }
}
