using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace EtherDomes.Editor
{
    public static class RegionInicioSceneCreator
    {
        private const string SCENE_PATH = "Assets/_Project/Scenes/Regions/RegionInicio.unity";
        private const string MENU_PATH = "Tools/EtherDomes/Crear Escena Region Inicio";

        [MenuItem(MENU_PATH)]
        public static void CreateScene()
        {
            if (!EditorUtility.DisplayDialog("Crear Region Inicio",
                "¿Crear la escena de Region Inicio?\n\nZona inicial para nuevos jugadores:\n- Tutorial básico\n- NPCs de ayuda\n- Portales a otras zonas",
                "Crear", "Cancelar")) return;

            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateLighting();
            CreateTerrain();
            CreateSpawnPoints();
            CreateNPCs();
            CreatePortals();
            CreateDecorations();
            CreatePlayerWithController();
            EnsureDirectoryExists();
            EditorSceneManager.SaveScene(newScene, SCENE_PATH);
            Debug.Log($"[RegionInicioSceneCreator] Escena creada en: {SCENE_PATH}");
            EditorUtility.DisplayDialog("Éxito", "Region Inicio creada correctamente.", "OK");
        }

        private static void EnsureDirectoryExists()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Scenes/Regions"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Project/Scenes"))
                    AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
                AssetDatabase.CreateFolder("Assets/_Project/Scenes", "Regions");
            }
        }

        private static void CreateLighting()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.5f, 0.6f, 0.8f);
            RenderSettings.ambientEquatorColor = new Color(0.4f, 0.5f, 0.4f);
            RenderSettings.ambientGroundColor = new Color(0.2f, 0.25f, 0.2f);
            GameObject sun = new GameObject("Directional Light");
            Light sunLight = sun.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.color = new Color(1f, 0.95f, 0.85f);
            sunLight.intensity = 1.3f;
            sun.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
        }

        private static void CreateTerrain()
        {
            GameObject container = new GameObject("--- TERRAIN ---");
            // Suelo principal
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground_Main"; ground.transform.parent = container.transform;
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(15f, 1f, 15f);
            ground.isStatic = true;
            ApplyMaterial(ground, new Color(0.35f, 0.5f, 0.25f));
            // Plaza central
            GameObject plaza = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plaza.name = "Plaza_Central"; plaza.transform.parent = container.transform;
            plaza.transform.position = new Vector3(0f, 0.05f, 0f);
            plaza.transform.localScale = new Vector3(20f, 0.1f, 20f);
            plaza.isStatic = true;
            ApplyMaterial(plaza, new Color(0.6f, 0.55f, 0.45f));
            // Caminos
            CreatePath("Path_North", new Vector3(0f, 0.02f, 30f), new Vector3(8f, 0.05f, 50f), container.transform);
            CreatePath("Path_East", new Vector3(30f, 0.02f, 0f), new Vector3(50f, 0.05f, 8f), container.transform);
            CreatePath("Path_West", new Vector3(-30f, 0.02f, 0f), new Vector3(50f, 0.05f, 8f), container.transform);
        }

        private static void CreatePath(string name, Vector3 pos, Vector3 scale, Transform parent)
        {
            GameObject path = GameObject.CreatePrimitive(PrimitiveType.Cube);
            path.name = name; path.transform.parent = parent;
            path.transform.position = pos; path.transform.localScale = scale;
            path.isStatic = true;
            ApplyMaterial(path, new Color(0.5f, 0.45f, 0.35f));
        }

        private static void CreateSpawnPoints()
        {
            GameObject container = new GameObject("--- SPAWN POINTS ---");
            GameObject mainSpawn = new GameObject("SpawnPoint_Main");
            mainSpawn.transform.parent = container.transform;
            mainSpawn.transform.position = new Vector3(0f, 0.5f, 0f);
            mainSpawn.tag = "Respawn";
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 5f, 0.5f, Mathf.Sin(angle) * 5f);
                GameObject spawn = new GameObject($"SpawnPoint_{i + 1}");
                spawn.transform.parent = container.transform;
                spawn.transform.position = pos;
                spawn.tag = "Respawn";
            }
        }


        private static void CreateNPCs()
        {
            GameObject container = new GameObject("--- NPCs ---");
            CreateNPC("NPC_Tutorial", new Vector3(8f, 1f, 5f), Color.yellow, container.transform);
            CreateNPC("NPC_Vendor", new Vector3(-8f, 1f, 5f), Color.cyan, container.transform);
            CreateNPC("NPC_QuestGiver", new Vector3(0f, 1f, 12f), new Color(0.8f, 0.5f, 0.2f), container.transform);
            CreateNPC("NPC_ClassTrainer", new Vector3(12f, 1f, -5f), new Color(0.6f, 0.3f, 0.8f), container.transform);
            CreateNPC("NPC_Banker", new Vector3(-12f, 1f, -5f), new Color(0.9f, 0.8f, 0.2f), container.transform);
        }

        private static void CreateNPC(string name, Vector3 pos, Color color, Transform parent)
        {
            GameObject npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            npc.name = name + " (Placeholder)";
            npc.transform.parent = parent;
            npc.transform.position = pos;
            ApplyMaterial(npc, color);
        }

        private static void CreatePortals()
        {
            GameObject container = new GameObject("--- PORTALS ---");
            CreatePortal("Portal_ToRegion1", new Vector3(0f, 2f, 55f), new Color(0.2f, 0.5f, 1f), "Region 1", container.transform);
            CreatePortal("Portal_ToMazmorra1_1", new Vector3(55f, 2f, 0f), new Color(0.8f, 0.2f, 0.2f), "Mazmorra 1.1", container.transform);
            CreatePortal("Portal_ToArena", new Vector3(-55f, 2f, 0f), new Color(0.8f, 0.6f, 0.2f), "Arena PvP", container.transform);
        }

        private static void CreatePortal(string name, Vector3 pos, Color color, string dest, Transform parent)
        {
            GameObject portal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            portal.name = name; portal.transform.parent = parent;
            portal.transform.position = pos;
            portal.transform.localScale = new Vector3(4f, 5f, 0.5f);
            ApplyMaterial(portal, color);
            GameObject label = new GameObject($"Label_{dest}");
            label.transform.parent = portal.transform;
            label.transform.localPosition = new Vector3(0f, 3f, 0f);
        }

        private static void CreateDecorations()
        {
            GameObject container = new GameObject("--- DECORATIONS ---");
            // Fuente central
            GameObject fountain = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fountain.name = "Fountain"; fountain.transform.parent = container.transform;
            fountain.transform.position = new Vector3(0f, 0.5f, 0f);
            fountain.transform.localScale = new Vector3(4f, 1f, 4f);
            ApplyMaterial(fountain, new Color(0.5f, 0.5f, 0.55f));
            // Árboles
            Vector3[] treePositions = {
                new Vector3(25f, 3f, 25f), new Vector3(-25f, 3f, 25f),
                new Vector3(25f, 3f, -25f), new Vector3(-25f, 3f, -25f),
                new Vector3(40f, 3f, 10f), new Vector3(-40f, 3f, 10f),
                new Vector3(40f, 3f, -10f), new Vector3(-40f, 3f, -10f)
            };
            for (int i = 0; i < treePositions.Length; i++)
            {
                GameObject tree = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                tree.name = $"Tree_{i}"; tree.transform.parent = container.transform;
                tree.transform.position = treePositions[i];
                tree.transform.localScale = new Vector3(3f, 6f, 3f);
                tree.isStatic = true;
                ApplyMaterial(tree, new Color(0.2f, 0.45f, 0.15f));
            }
            // Bancos
            CreateBench("Bench_1", new Vector3(6f, 0.4f, -8f), container.transform);
            CreateBench("Bench_2", new Vector3(-6f, 0.4f, -8f), container.transform);
        }

        private static void CreateBench(string name, Vector3 pos, Transform parent)
        {
            GameObject bench = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bench.name = name; bench.transform.parent = parent;
            bench.transform.position = pos;
            bench.transform.localScale = new Vector3(3f, 0.8f, 1f);
            bench.isStatic = true;
            ApplyMaterial(bench, new Color(0.4f, 0.25f, 0.15f));
        }

        private static void ApplyMaterial(GameObject obj, Color color)
        {
            var renderer = obj.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color; renderer.material = mat;
        }

        private static void CreatePlayerWithController()
        {
            GameObject container = new GameObject("--- PLAYER ---");
            Vector3 spawnPos = new Vector3(0f, 0.5f, -3f);
            string[] guids = AssetDatabase.FindAssets("NetworkPlayer t:Prefab");
            GameObject playerPrefab = null;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("_Project") && path.EndsWith("NetworkPlayer.prefab"))
                { playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path); break; }
            }
            GameObject player;
            if (playerPrefab != null)
            {
                player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                player.name = "Player (Local Test)"; player.transform.position = spawnPos;
                player.transform.parent = container.transform;
                var netObj = player.GetComponent<Unity.Netcode.NetworkObject>();
                if (netObj != null) Object.DestroyImmediate(netObj);
                var netTrans = player.GetComponent<Unity.Netcode.Components.NetworkTransform>();
                if (netTrans != null) Object.DestroyImmediate(netTrans);
            }
            else
            {
                player = new GameObject("Player (Local Test)");
                player.transform.position = spawnPos; player.transform.parent = container.transform;
                player.tag = "Player";
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = "Body"; body.transform.parent = player.transform;
                body.transform.localPosition = new Vector3(0f, 1f, 0f);
                Object.DestroyImmediate(body.GetComponent<Collider>());
                ApplyMaterial(body, new Color(0.2f, 0.6f, 0.8f));
                CharacterController cc = player.AddComponent<CharacterController>();
                cc.height = 2f; cc.radius = 0.5f; cc.center = new Vector3(0f, 1f, 0f);
                player.AddComponent<EtherDomes.Player.PlayerController>();
            }
            CreateMainCamera(spawnPos);
            SceneCreatorUtils.CreateEventSystem();
        }

        private static void CreateMainCamera(Vector3 playerPos)
        {
            UnityEngine.Camera existing = Object.FindFirstObjectByType<UnityEngine.Camera>();
            if (existing != null) Object.DestroyImmediate(existing.gameObject);
            GameObject cam = new GameObject("Main Camera");
            cam.tag = "MainCamera";
            UnityEngine.Camera c = cam.AddComponent<UnityEngine.Camera>();
            c.clearFlags = CameraClearFlags.SolidColor;
            c.backgroundColor = new Color(0.4f, 0.6f, 0.9f);
            c.fieldOfView = 60f;
            cam.AddComponent<AudioListener>();
            cam.transform.position = playerPos + new Vector3(0f, 5f, -12f);
            cam.transform.LookAt(playerPos + Vector3.up * 1.5f);
        }
    }
}
