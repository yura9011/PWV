namespace EtherDomes.Network
{
    /// <summary>
    /// Clase estática para almacenar datos del servidor.
    /// Permite que ConnectionApprovalManager acceda sin depender del assembly UI.
    /// </summary>
    public static class ServerPasswordHolder
    {
        /// <summary>
        /// Hash de la contraseña del servidor actual.
        /// Vacío si el servidor no tiene contraseña.
        /// </summary>
        public static string CurrentPasswordHash { get; set; } = "";
        
        /// <summary>
        /// Indica si el servidor es dedicado (sin jugador local).
        /// </summary>
        public static bool IsDedicatedServer { get; set; } = false;
        
        /// <summary>
        /// Limpia todos los datos almacenados.
        /// </summary>
        public static void Clear()
        {
            CurrentPasswordHash = "";
            IsDedicatedServer = false;
        }
    }
}
