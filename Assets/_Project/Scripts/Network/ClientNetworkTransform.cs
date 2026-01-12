using Unity.Netcode.Components;
using UnityEngine;

namespace EtherDomes.Network
{
    /// <summary>
    /// Custom NetworkTransform that gives authority to the owning client.
    /// This allows clients to move their own characters without server authority.
    /// </summary>
    [DisallowMultipleComponent]
    public class ClientNetworkTransform : NetworkTransform
    {
        /// <summary>
        /// Override to allow owner to have authority over their transform.
        /// </summary>
        protected override bool OnIsServerAuthoritative()
        {
            return false; // Client has authority
        }
    }
}
