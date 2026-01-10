namespace EtherDomes.Combat
{
    /// <summary>
    /// Interface for entities that can take damage.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Apply damage to this entity.
        /// </summary>
        void TakeDamage(float damage);
        
        /// <summary>
        /// Apply damage from a specific source.
        /// </summary>
        void TakeDamage(float damage, ulong sourceId);
    }
}
