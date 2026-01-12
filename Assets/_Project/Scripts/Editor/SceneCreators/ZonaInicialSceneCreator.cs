using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace EtherDomes.Editor
{
    /// <summary>
    /// Crea la escena de Zona Inicial - Punto de spawn para nuevos jugadores
    /// </summary>
    public static class ZonaInicialSceneCreator
    {
        private const string SCENE_PATH = "Assets/_Project/Scenes/Zones/ZonaInicial.unity";
        private const string MENU_PATH = "Tools/EtherDomes/Crear Escena Zona Inicial";

        [MenuItem(MENU_PATH)]
        public static void CreateScene()
        {
            if (!EditorUtility.DisplayDialog("Crear Zona Inicial",
                "¿Crear la escena de Zona Inicial?\n\nEsta será el punto de spawn para nuevos jugadores.",
                "Crear", "Cancelar"))
            {
                return;
            }

            // Crear nueva escena
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Crear estructura básica
            CreateLighting();
            CreateTerrain();
            CreateSpawnPoints();
            CreateZoneBoundaries();
            CreateNPCs();
            CreatePortals();

            // Guardar escena
            EnsureDirectoryExists();
            EditorSceneManager.SaveScene(newScene, SCENE_PATH);

            Debug.Log($"[ZonaInicialSceneCreator] Escena creada en: {SCENE_PATH}");
            EditorUtility.DisplayDialog("Éxito", "Zona Inicial creada correctamente.\n\nRecuerda agregar la escena al Build Settings.", "OK");
        }

        private static void EnsureDirectoryExists()
        {
            string directory = "Assets/_Project/Scenes/Zones";
            if (!AssetDatabase.IsValidFolder(directory))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Scenes", "Zones");
            }
        }

        private static void CreateLighting()
        {
            // Directional Light (Sol)
            GameObject sun = new GameObject("Directional Light");
            Light sunLight = sun.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.color = new Color(1f, 0.95f, 0.85f);
            sunLight.intensity = 1.2f;
            sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void CreateTerrain()
        {
            // Suelo principal
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(10f, 1f, 10f); // 100x100 unidades
            ground.isStatic = true;

            // Material verde para el suelo
            var renderer = ground.GetComponent<MeshRenderer>();
            Material groundMat = new Material(Shader.Find("Standard"));
            groundMat.color = new Color(0.3f, 0.5f, 0.2f); // Verde hierba
            renderer.material = groundMat;

            // Marcador visual del centro
            GameObject centerMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            centerMarker.name = "CenterMarker";
            centerMarker.transform.position = new Vector3(0f, 0.1f, 0f);
            centerMarker.transform.localScale = new Vector3(2f, 0.1f, 2f);
            var markerRenderer = centerMarker.GetComponent<MeshRenderer>();
            Material markerMat = new Material(Shader.Find("Standard"));
            markerMat.color = new Color(0.8f, 0.8f, 0.6f); // Piedra clara
            markerRenderer.material = markerMat;
        }

        private static void CreateSpawnPoints()
        {
            GameObject spawnContainer = new GameObject("--- SPAWN POINTS ---");

            // Spawn principal (centro)
            GameObject mainSpawn = new GameObject("SpawnPoint_Main");
            mainSpawn.transform.parent = spawnContainer.transform;
            mainSpawn.transform.position = new Vector3(0f, 0.5f, 0f);
            mainSpawn.tag = "Respawn";

            // Spawns adicionales en círculo
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 3f, 0.5f, Mathf.Sin(angle) * 3f);
                
                GameObject spawn = new GameObject($"SpawnPoint_{i + 1}");
                spawn.transform.parent = spawnContainer.transform;
                spawn.transform.position = pos;
                spawn.tag = "Respawn";
            }
        }

        private static void CreateZoneBoundaries()
        {
            GameObject boundaryContainer = new GameObject("--- ZONE BOUNDARIES ---");

            // Crear paredes invisibles en los bordes
            float size = 50f;
            float height = 10f;

            CreateInvisibleWall("Wall_North", new Vector3(0, height/2, size), new Vector3(size*2, height, 1f), boundaryContainer.transform);
            CreateInvisibleWall("Wall_South", new Vector3(0, height/2, -size), new Vector3(size*2, height, 1f), boundaryContainer.transform);
            CreateInvisibleWall("Wall_East", new Vector3(size, height/2, 0), new Vector3(1f, height, size*2), boundaryContainer.transform);
            CreateInvisibleWall("Wall_West", new Vector3(-size, height/2, 0), new Vector3(1f, height, size*2), boundaryContainer.transform);
        }

        private static void CreateInvisibleWall(string name, Vector3 position, Vector3 scale, Transform parent)
        {
            GameObject wall = new GameObject(name);
            wall.transform.parent = parent;
            wall.transform.position = position;
            
            BoxCollider collider = wall.AddComponent<BoxCollider>();
            collider.size = scale;
            wall.layer = LayerMask.NameToLayer("Default");
        }

        private static void CreateNPCs()
        {
            GameObject npcContainer = new GameObject("--- NPCs ---");

            // Placeholder para NPC de tutorial
            GameObject tutorialNPC = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            tutorialNPC.name = "NPC_Tutorial (Placeholder)";
            tutorialNPC.transform.parent = npcContainer.transform;
            tutorialNPC.transform.position = new Vector3(5f, 1f, 5f);
            
            var renderer = tutorialNPC.GetComponent<MeshRenderer>();
            Material npcMat = new Material(Shader.Find("Standard"));
            npcMat.color = Color.yellow;
            renderer.material = npcMat;

            // Placeholder para NPC vendedor
            GameObject vendorNPC = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            vendorNPC.name = "NPC_Vendor (Placeholder)";
            vendorNPC.transform.parent = npcContainer.transform;
            vendorNPC.transform.position = new Vector3(-5f, 1f, 5f);
            
            var vendorRenderer = vendorNPC.GetComponent<MeshRenderer>();
            Material vendorMat = new Material(Shader.Find("Standard"));
            vendorMat.color = Color.cyan;
            vendorRenderer.material = vendorMat;
        }

        private static void CreatePortals()
        {
            GameObject portalContainer = new GameObject("--- PORTALS ---");

            // Portal a Zona 1
            CreatePortalPlaceholder("Portal_ToZona1", new Vector3(0f, 1f, 20f), Color.blue, "Zona 1", portalContainer.transform);

            // Portal a Mazmorra 1.1
            CreatePortalPlaceholder("Portal_ToMazmorra1_1", new Vector3(20f, 1f, 0f), Color.red, "Mazmorra 1.1", portalContainer.transform);
        }

        private static void CreatePortalPlaceholder(string name, Vector3 position, Color color, string destination, Transform parent)
        {
            GameObject portal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            portal.name = name;
            portal.transform.parent = parent;
            portal.transform.position = position;
            portal.transform.localScale = new Vector3(3f, 4f, 0.5f);

            var renderer = portal.GetComponent<MeshRenderer>();
            Material portalMat = new Material(Shader.Find("Standard"));
            portalMat.color = color;
            portalMat.SetFloat("_Mode", 3); // Transparent
            Color transparentColor = color;
            transparentColor.a = 0.5f;
            portalMat.color = transparentColor;
            renderer.material = portalMat;

            // Texto indicador (usando un cubo pequeño como placeholder)
            GameObject label = new GameObject($"Label_{destination}");
            label.transform.parent = portal.transform;
            label.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        }
    }
}
