// AutoMCPConnect.cs
// Auto-connects Unity to the MCP server on domain reload using the MCP for Unity SDK
// Place in Assets/_Project/Scripts/Editor (requires MCPForUnity.Editor asmdef reference)
// 
// DISABLED: MCPServiceLocator API changed in newer versions of MCP for Unity.
// Re-enable when API is updated or use Window > MCP for Unity to connect manually.

using UnityEngine;
using UnityEditor;

namespace EtherDomes.Editor
{
    // [InitializeOnLoad] - Disabled due to API changes
    public static class AutoMCPConnect
    {
        // MCP auto-connect disabled - use Window > MCP for Unity to connect manually
        
        [MenuItem("EtherDomes/MCP Info")]
        public static void ShowMCPInfo()
        {
            Debug.Log("[AutoMCPConnect] Auto-connect disabled. Use Window > MCP for Unity to connect manually.");
        }
    }
}
