using UnityEngine;
using UnityEditor;

namespace EtherDomes.Editor
{
    /// <summary>
    /// Utilidades compartidas para los creadores de escenas
    /// </summary>
    public static class SceneCreatorUtils
    {
        /// <summary>
        /// Crea el setup completo de jugador para testing local
        /// </summary>
        public static void CreatePlayerSetup(Vector3 spawnPosition)
        {
            // Buscar el prefab de NetworkPlayer
            string[] guids = AssetDatabase.FindAssets("NetworkPlayer t:Prefab");
            GameObject playerPrefab = null;
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("_Project") && path.EndsWith("NetworkPlayer.prefab"))
                {
                    playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    break;
                }
            }

            if (playerPrefab != null)
            {
                // Instanciar el prefab
                GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                player.name = "Player (Local Test)";
                player.transform.position = spawnPosition;
                
                // Desactivar componentes de red para testing local
                var networkObject = player.GetComponent<Unity.Netcode.NetworkObject>();
                if (networkObject != null)
                {
                    Object.DestroyImmediate(networkObject);
                }
                
                var networkTransform = player.GetComponent<Unity.Netcode.Components.NetworkTransform>();
                if (networkTransform != null)
                {
                    Object.DestroyImmediate(networkTransform);
                }
                
                // Buscar y eliminar ClientNetworkTransform si existe
                var clientNetTransform = player.GetComponent("ClientNetworkTransform");
                if (clientNetTransform != null)
                {
                    Object.DestroyImmediate(clientNetTransform);
                }

                Debug.Log("[SceneCreatorUtils] Player prefab instanciado (componentes de red removidos para testing local)");
            }
            else
            {
                // Crear player básico si no existe el prefab
                CreateBasicPlayer(spawnPosition);
            }
        }

        /// <summary>
        /// Crea un jugador básico sin dependencias de red
        /// </summary>
        public static void CreateBasicPlayer(Vector3 spawnPosition)
        {
            GameObject player = new GameObject("Player (Basic)");
            player.transform.position = spawnPosition;
            player.tag = "Player";
            player.layer = LayerMask.NameToLayer("Default");

            // Cuerpo visual (cápsula)
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.parent = player.transform;
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            Object.DestroyImmediate(body.GetComponent<Collider>());
            
            var renderer = body.GetComponent<MeshRenderer>();
            Material playerMat = new Material(Shader.Find("Standard"));
            playerMat.color = new Color(0.2f, 0.6f, 0.8f);
            renderer.material = playerMat;

            // CharacterController
            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.5f;
            cc.center = new Vector3(0f, 1f, 0f);

            Debug.Log("[SceneCreatorUtils] Player básico creado con CharacterController");
        }

        /// <summary>
        /// Crea la cámara principal con seguimiento al jugador
        /// </summary>
        public static void CreateMainCamera(Vector3 playerPosition)
        {
            // Buscar si ya existe una cámara
            UnityEngine.Camera existingCam = Object.FindFirstObjectByType<UnityEngine.Camera>();
            if (existingCam != null)
            {
                Object.DestroyImmediate(existingCam.gameObject);
            }

            GameObject cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            
            UnityEngine.Camera cam = cameraObj.AddComponent<UnityEngine.Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 1000f;

            cameraObj.AddComponent<AudioListener>();

            // Posicionar detrás del jugador
            cameraObj.transform.position = playerPosition + new Vector3(0f, 3f, -8f);
            cameraObj.transform.LookAt(playerPosition + Vector3.up * 1.5f);

            Debug.Log("[SceneCreatorUtils] Cámara principal creada");
        }

        /// <summary>
        /// Crea el EventSystem necesario para UI
        /// </summary>
        public static void CreateEventSystem()
        {
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null)
                return;

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }
}
