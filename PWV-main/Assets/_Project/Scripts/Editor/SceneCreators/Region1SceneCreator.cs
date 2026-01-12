using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace EtherDomes.Editor
{
    public static class Region1SceneCreator
    {
        private const string SCENE_PATH = "Assets/_Project/Scenes/Regions/Region1.unity";
        private const string MENU_PATH = "Tools/EtherDomes/Crear Escena Region 1";

        [MenuItem(MENU_PATH)]
        public static void CreateScene()
        {
            if (!EditorUtility.DisplayDialog("Crear Region 1",
                "¿Crear la escena de Region 1?\n\nPrimera zona de aventura:\n- Enemigos nivel 1-10\n- Quests iniciales\n- Acceso a mazmorras 1.1-1.4",
                "Crear", "Cancelar")) return;

            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateLighting();
            CreateTerrain();
            CreateSpawnPoints();
            CreateEnemyAreas();
            CreateNPCs();
            CreatePortals();
            CreateDecorations();
            CreatePlayerWithController();
            EnsureDirectoryExists();
            EditorSceneManager.SaveScene(newScene, SCENE_PATH);
            Debug.Log($"[Region1SceneCreator] Escena creada en: {SCENE_PATH}");
            EditorUtility.DisplayDialog("Éxito", "Region 1 creada correctamente.", "OK");
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
            RenderSettings.ambientSkyColor = new Color(0.45f, 0.55f, 0.7f);
            RenderSettings.ambientEquatorColor = new Color(0.35f, 0.45f, 0.35f);
            RenderSettings.ambientGroundColor = new Color(0.2f, 0.22f, 0.18f);
            GameObject sun = new GameObject("Directional Light");
            Light sunLight = sun.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.color = new Color(1f, 0.92f, 0.8f);
            sunLight.intensity = 1.1f;
            sun.transform.rotation = Quaternion.Euler(40f, -45f, 0f);
        }

        private static void CreateTerrain()
        {
            GameObject container = new GameObject("--- TERRAIN ---");
            // Suelo principal grande
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground_Main"; ground.transform.parent = container.transform;
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(30f, 1f, 30f);
            ground.isStatic = true;
            ApplyMaterial(ground, new Color(0.4f, 0.5f, 0.3f));
            // Colinas
            CreateHill("Hill_1", new Vector3(80f, 5f, 80f), new Vector3(40f, 10f, 40f), container.transform);
            CreateHill("Hill_2", new Vector3(-80f, 4f, 60f), new Vector3(35f, 8f, 35f), container.transform);
            CreateHill("Hill_3", new Vector3(60f, 3f, -70f), new Vector3(30f, 6f, 30f), container.transform);
            CreateHill("Hill_4", new Vector3(-70f, 4f, -80f), new Vector3(35f, 8f, 35f), container.transform);
            // Lago
            GameObject lake = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lake.name = "Lake"; lake.transform.parent = container.transform;
            lake.transform.position = new Vector3(-40f, -0.5f, 40f);
            lake.transform.localScale = new Vector3(30f, 1f, 30f);
            lake.isStatic = true;
            ApplyMaterial(lake, new Color(0.2f, 0.4f, 0.6f));
            // Caminos principales
            CreatePath("Path_ToRegionInicio", new Vector3(0f, 0.02f, -80f), new Vector3(10f, 0.05f, 80f), container.transform);
            CreatePath("Path_ToDungeons", new Vector3(80f, 0.02f, 0f), new Vector3(80f, 0.05f, 10f), container.transform);
            CreatePath("Path_ToRegion2", new Vector3(0f, 0.02f, 120f), new Vector3(10f, 0.05f, 60f), container.transform);
        }

        private static void CreateHill(string name, Vector3 pos, Vector3 scale, Transform parent)
        {
            GameObject hill = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hill.name = name; hill.transform.parent = parent;
            hill.transform.position = pos; hill.transform.localScale = scale;
            hill.isStatic = true;
            ApplyMaterial(hill, new Color(0.35f, 0.45f, 0.25f));
        }

        private static void CreatePath(string name, Vector3 pos, Vector3 scale, Transform parent)
        {
            GameObject path = GameObject.CreatePrimitive(PrimitiveType.Cube);
            path.name = name; path.transform.parent = parent;
            path.transform.position = pos; path.transform.localScale = scale;
            path.isStatic = true;
            ApplyMaterial(path, new Color(0.5f, 0.42f, 0.32f));
        }

        private static void CreateSpawnPoints()
        {
            GameObject container = new GameObject("--- SPAWN POINTS ---");
            GameObject mainSpawn = new GameObject("SpawnPoint_FromRegionInicio");
            mainSpawn.transform.parent = container.transform;
            mainSpawn.transform.position = new Vector3(0f, 0.5f, -120f);
            mainSpawn.tag = "Respawn";
            GameObject dungeonSpawn = new GameObject("SpawnPoint_FromDungeons");
            dungeonSpawn.transform.parent = container.transform;
            dungeonSpawn.transform.position = new Vector3(120f, 0.5f, 0f);
            dungeonSpawn.tag = "Respawn";
        }


        private static void CreateEnemyAreas()
        {
            GameObject container = new GameObject("--- ENEMY AREAS ---");
            // Área nivel 1-3
            CreateEnemyArea("Area_Lvl1-3", new Vector3(30f, 0f, -30f), 25f, 5, container.transform, Color.green);
            // Área nivel 3-5
            CreateEnemyArea("Area_Lvl3-5", new Vector3(-50f, 0f, -20f), 30f, 6, container.transform, Color.yellow);
            // Área nivel 5-7
            CreateEnemyArea("Area_Lvl5-7", new Vector3(50f, 0f, 50f), 30f, 7, container.transform, new Color(1f, 0.5f, 0f));
            // Área nivel 7-10
            CreateEnemyArea("Area_Lvl7-10", new Vector3(-30f, 0f, 80f), 35f, 8, container.transform, Color.red);
        }

        private static void CreateEnemyArea(string name, Vector3 center, float radius, int count, Transform parent, Color color)
        {
            GameObject area = new GameObject(name);
            area.transform.parent = parent;
            area.transform.position = center;
            for (int i = 0; i < count; i++)
            {
                float angle = i * (360f / count) * Mathf.Deg2Rad;
                float dist = Random.Range(radius * 0.3f, radius);
                Vector3 pos = center + new Vector3(Mathf.Cos(angle) * dist, 0.5f, Mathf.Sin(angle) * dist);
                GameObject spawner = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                spawner.name = $"EnemySpawner_{i}";
                spawner.transform.parent = area.transform;
                spawner.transform.position = pos;
                spawner.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                ApplyMaterial(spawner, color);
                Object.DestroyImmediate(spawner.GetComponent<Collider>());
            }
        }

        private static void CreateNPCs()
        {
            GameObject container = new GameObject("--- NPCs ---");
            CreateNPC("NPC_QuestGiver_1", new Vector3(10f, 1f, -50f), new Color(0.8f, 0.5f, 0.2f), container.transform);
            CreateNPC("NPC_QuestGiver_2", new Vector3(-20f, 1f, 30f), new Color(0.8f, 0.5f, 0.2f), container.transform);
            CreateNPC("NPC_QuestGiver_3", new Vector3(40f, 1f, 70f), new Color(0.8f, 0.5f, 0.2f), container.transform);
            CreateNPC("NPC_Vendor_Potions", new Vector3(0f, 1f, -40f), Color.cyan, container.transform);
            CreateNPC("NPC_Vendor_Weapons", new Vector3(20f, 1f, 20f), new Color(0.6f, 0.6f, 0.7f), container.transform);
            CreateNPC("NPC_FlightMaster", new Vector3(-10f, 1f, -60f), new Color(0.4f, 0.8f, 0.4f), container.transform);
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
            CreatePortal("Portal_ToRegionInicio", new Vector3(0f, 2f, -140f), new Color(0.3f, 0.8f, 0.3f), "Region Inicio", container.transform);
            CreatePortal("Portal_ToMazmorra1_1", new Vector3(130f, 2f, -30f), new Color(0.8f, 0.2f, 0.2f), "Mazmorra 1.1", container.transform);
            CreatePortal("Portal_ToMazmorra1_2", new Vector3(130f, 2f, 0f), new Color(0.8f, 0.3f, 0.2f), "Mazmorra 1.2", container.transform);
            CreatePortal("Portal_ToMazmorra1_3", new Vector3(130f, 2f, 30f), new Color(0.8f, 0.4f, 0.2f), "Mazmorra 1.3", container.transform);
            CreatePortal("Portal_ToMazmorra1_4", new Vector3(130f, 2f, 60f), new Color(0.9f, 0.1f, 0.1f), "Mazmorra 1.4", container.transform);
            CreatePortal("Portal_ToRegion2", new Vector3(0f, 2f, 150f), new Color(0.2f, 0.5f, 1f), "Region 2 (Bloqueado)", container.transform);
        }

        private static void CreatePortal(string name, Vector3 pos, Color color, string dest, Transform parent)
        {
            GameObject portal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            portal.name = name; portal.transform.parent = parent;
            portal.transform.position = pos;
            portal.transform.localScale = new Vector3(4f, 5f, 0.5f);
            ApplyMaterial(portal, color);
        }

        private static void CreateDecorations()
        {
            GameObject container = new GameObject("--- DECORATIONS ---");
            // Árboles dispersos
            Vector3[] treePositions = {
                new Vector3(20f, 4f, 10f), new Vector3(-25f, 4f, 15f), new Vector3(35f, 4f, -15f),
                new Vector3(-40f, 4f, -35f), new Vector3(60f, 4f, 25f), new Vector3(-60f, 4f, 50f),
                new Vector3(45f, 4f, -50f), new Vector3(-35f, 4f, 65f), new Vector3(70f, 4f, 70f),
                new Vector3(-70f, 4f, -60f), new Vector3(15f, 4f, 90f), new Vector3(-55f, 4f, 100f)
            };
            for (int i = 0; i < treePositions.Length; i++)
            {
                GameObject tree = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                tree.name = $"Tree_{i}"; tree.transform.parent = container.transform;
                tree.transform.position = treePositions[i];
                tree.transform.localScale = new Vector3(4f, 8f, 4f);
                tree.isStatic = true;
                ApplyMaterial(tree, new Color(0.15f + Random.Range(0f, 0.1f), 0.4f + Random.Range(0f, 0.15f), 0.1f));
            }
            // Rocas
            for (int i = 0; i < 8; i++)
            {
                Vector3 rockPos = new Vector3(Random.Range(-100f, 100f), 1f, Random.Range(-100f, 100f));
                GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.name = $"Rock_{i}"; rock.transform.parent = container.transform;
                rock.transform.position = rockPos;
                rock.transform.localScale = new Vector3(Random.Range(2f, 5f), Random.Range(1.5f, 3f), Random.Range(2f, 5f));
                rock.isStatic = true;
                ApplyMaterial(rock, new Color(0.4f, 0.38f, 0.35f));
            }
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
            Vector3 spawnPos = new Vector3(0f, 0.5f, -115f);
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
            c.backgroundColor = new Color(0.35f, 0.5f, 0.75f);
            c.fieldOfView = 60f;
            cam.AddComponent<AudioListener>();
            cam.transform.position = playerPos + new Vector3(0f, 6f, -15f);
            cam.transform.LookAt(playerPos + Vector3.up * 1.5f);
        }
    }
}
