using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace EtherDomes.Editor
{
    public static class Mazmorra1_4SceneCreator
    {
        private const string SCENE_PATH = "Assets/_Project/Scenes/Dungeons/Mazmorra1_4.unity";
        private const string MENU_PATH = "Tools/EtherDomes/Crear Escena Mazmorra 1.4";

        [MenuItem(MENU_PATH)]
        public static void CreateScene()
        {
            if (!EditorUtility.DisplayDialog("Crear Mazmorra 1.4",
                "¿Crear la escena de Mazmorra 1.4?\n\nCuarta dungeon - Boss final de la región:\n- 6 salas de trash\n- 3 mini-bosses\n- 1 boss final épico",
                "Crear", "Cancelar")) return;

            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateLighting();
            CreateDungeonLayout();
            CreateSpawnPoints();
            CreateEnemySpawners();
            CreateMiniBossRooms();
            CreateBossRoom();
            CreateExitPortal();
            CreatePlayerWithController();
            EnsureDirectoryExists();
            EditorSceneManager.SaveScene(newScene, SCENE_PATH);
            Debug.Log($"[Mazmorra1_4SceneCreator] Escena creada en: {SCENE_PATH}");
            EditorUtility.DisplayDialog("Éxito", "Mazmorra 1.4 creada correctamente.", "OK");
        }

        private static void EnsureDirectoryExists()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Scenes/Dungeons"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Project/Scenes"))
                    AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
                AssetDatabase.CreateFolder("Assets/_Project/Scenes", "Dungeons");
            }
        }

        private static void CreateLighting()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.08f, 0.05f, 0.05f);
            GameObject mainLight = new GameObject("Dungeon Light");
            Light light = mainLight.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.6f, 0.3f, 0.3f);
            light.intensity = 0.15f;
            mainLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void CreateDungeonLayout()
        {
            GameObject container = new GameObject("--- DUNGEON LAYOUT ---");
            CreateRoom("Room_Entrance", Vector3.zero, new Vector3(18f, 6f, 18f), container.transform);
            // Ala izquierda
            CreateCorridor("Corridor_L1", new Vector3(-18f, 0f, 0f), new Vector3(10f, 5f, 6f), container.transform);
            CreateRoom("Room_Trash_1", new Vector3(-35f, 0f, 0f), new Vector3(22f, 5f, 22f), container.transform);
            CreateCorridor("Corridor_L2", new Vector3(-35f, 0f, 18f), new Vector3(6f, 5f, 12f), container.transform);
            CreateRoom("Room_Trash_2", new Vector3(-35f, 0f, 38f), new Vector3(24f, 5f, 24f), container.transform);
            // Ala derecha
            CreateCorridor("Corridor_R1", new Vector3(18f, 0f, 0f), new Vector3(10f, 5f, 6f), container.transform);
            CreateRoom("Room_Trash_3", new Vector3(35f, 0f, 0f), new Vector3(22f, 5f, 22f), container.transform);
            CreateCorridor("Corridor_R2", new Vector3(35f, 0f, 18f), new Vector3(6f, 5f, 12f), container.transform);
            CreateRoom("Room_Trash_4", new Vector3(35f, 0f, 38f), new Vector3(24f, 5f, 24f), container.transform);
            // Centro hacia boss
            CreateCorridor("Corridor_C1", new Vector3(0f, 0f, 18f), new Vector3(6f, 5f, 12f), container.transform);
            CreateRoom("Room_Trash_5", new Vector3(0f, 0f, 38f), new Vector3(25f, 6f, 25f), container.transform);
            CreateCorridor("Corridor_C2", new Vector3(0f, 0f, 58f), new Vector3(6f, 5f, 12f), container.transform);
            CreateRoom("Room_Trash_6", new Vector3(0f, 0f, 78f), new Vector3(25f, 6f, 25f), container.transform);
        }


        private static void CreateRoom(string name, Vector3 position, Vector3 size, Transform parent)
        {
            GameObject room = new GameObject(name);
            room.transform.parent = parent; room.transform.position = position;
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor"; floor.transform.parent = room.transform;
            floor.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(size.x, 1f, size.z);
            floor.isStatic = true;
            ApplyMaterial(floor, new Color(0.25f, 0.15f, 0.15f));
            CreateWall("Wall_North", room.transform, new Vector3(0f, size.y/2, size.z/2), new Vector3(size.x, size.y, 1f));
            CreateWall("Wall_South", room.transform, new Vector3(0f, size.y/2, -size.z/2), new Vector3(size.x, size.y, 1f));
            CreateWall("Wall_East", room.transform, new Vector3(size.x/2, size.y/2, 0f), new Vector3(1f, size.y, size.z));
            CreateWall("Wall_West", room.transform, new Vector3(-size.x/2, size.y/2, 0f), new Vector3(1f, size.y, size.z));
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling"; ceiling.transform.parent = room.transform;
            ceiling.transform.localPosition = new Vector3(0f, size.y, 0f);
            ceiling.transform.localScale = new Vector3(size.x, 0.5f, size.z);
            ceiling.isStatic = true;
            ApplyMaterial(ceiling, new Color(0.18f, 0.1f, 0.1f));
            CreateTorch(room.transform, new Vector3(size.x/2 - 1f, 2.5f, size.z/2 - 1f));
            CreateTorch(room.transform, new Vector3(-size.x/2 + 1f, 2.5f, -size.z/2 + 1f));
        }

        private static void CreateCorridor(string name, Vector3 position, Vector3 size, Transform parent)
        {
            GameObject corridor = new GameObject(name);
            corridor.transform.parent = parent; corridor.transform.position = position;
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor"; floor.transform.parent = corridor.transform;
            floor.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(size.x, 1f, size.z);
            floor.isStatic = true;
            ApplyMaterial(floor, new Color(0.2f, 0.12f, 0.12f));
            CreateWall("Wall_Side1", corridor.transform, new Vector3(size.x/2, size.y/2, 0f), new Vector3(0.5f, size.y, size.z));
            CreateWall("Wall_Side2", corridor.transform, new Vector3(-size.x/2, size.y/2, 0f), new Vector3(0.5f, size.y, size.z));
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling"; ceiling.transform.parent = corridor.transform;
            ceiling.transform.localPosition = new Vector3(0f, size.y, 0f);
            ceiling.transform.localScale = new Vector3(size.x, 0.5f, size.z);
            ceiling.isStatic = true;
            ApplyMaterial(ceiling, new Color(0.18f, 0.1f, 0.1f));
        }

        private static void CreateWall(string name, Transform parent, Vector3 localPos, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name; wall.transform.parent = parent;
            wall.transform.localPosition = localPos; wall.transform.localScale = scale;
            wall.isStatic = true;
            ApplyMaterial(wall, new Color(0.3f, 0.18f, 0.18f));
        }

        private static void CreateTorch(Transform parent, Vector3 localPos)
        {
            GameObject torch = new GameObject("Torch");
            torch.transform.parent = parent; torch.transform.localPosition = localPos;
            Light torchLight = torch.AddComponent<Light>();
            torchLight.type = LightType.Point;
            torchLight.color = new Color(1f, 0.4f, 0.2f);
            torchLight.intensity = 1.5f; torchLight.range = 12f;
        }

        private static void ApplyMaterial(GameObject obj, Color color)
        {
            var renderer = obj.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color; renderer.material = mat;
        }

        private static void CreateSpawnPoints()
        {
            GameObject container = new GameObject("--- SPAWN POINTS ---");
            GameObject spawn = new GameObject("SpawnPoint_Entrance");
            spawn.transform.parent = container.transform;
            spawn.transform.position = new Vector3(0f, 0.5f, -7f);
            spawn.tag = "Respawn";
        }

        private static void CreateEnemySpawners()
        {
            GameObject container = new GameObject("--- ENEMY SPAWNERS ---");
            // Sala 1-2 (izquierda)
            CreateSpawner("Spawner_L1_1", new Vector3(-30f, 0.5f, -5f), container.transform);
            CreateSpawner("Spawner_L1_2", new Vector3(-40f, 0.5f, 5f), container.transform);
            CreateSpawner("Spawner_L2_1", new Vector3(-30f, 0.5f, 35f), container.transform);
            CreateSpawner("Spawner_L2_2", new Vector3(-40f, 0.5f, 42f), container.transform);
            // Sala 3-4 (derecha)
            CreateSpawner("Spawner_R1_1", new Vector3(30f, 0.5f, -5f), container.transform);
            CreateSpawner("Spawner_R1_2", new Vector3(40f, 0.5f, 5f), container.transform);
            CreateSpawner("Spawner_R2_1", new Vector3(30f, 0.5f, 35f), container.transform);
            CreateSpawner("Spawner_R2_2", new Vector3(40f, 0.5f, 42f), container.transform);
            // Sala 5-6 (centro)
            CreateSpawner("Spawner_C1_1", new Vector3(-8f, 0.5f, 35f), container.transform);
            CreateSpawner("Spawner_C1_2", new Vector3(8f, 0.5f, 42f), container.transform);
            CreateSpawner("Spawner_C2_1", new Vector3(-8f, 0.5f, 75f), container.transform);
            CreateSpawner("Spawner_C2_2", new Vector3(8f, 0.5f, 82f), container.transform);
        }

        private static void CreateSpawner(string name, Vector3 pos, Transform parent)
        {
            GameObject spawner = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spawner.name = name; spawner.transform.parent = parent;
            spawner.transform.position = pos;
            spawner.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            ApplyMaterial(spawner, Color.red);
            Object.DestroyImmediate(spawner.GetComponent<Collider>());
        }


        private static void CreateMiniBossRooms()
        {
            GameObject container = new GameObject("--- MINI BOSS ROOMS ---");
            // Mini Boss 1 (izquierda)
            CreateRoom("Room_MiniBoss_1", new Vector3(-35f, 0f, 65f), new Vector3(28f, 7f, 28f), container.transform);
            CreateMiniBossSpawner("MiniBoss1", new Vector3(-35f, 1.5f, 68f), container.transform);
            // Mini Boss 2 (derecha)
            CreateRoom("Room_MiniBoss_2", new Vector3(35f, 0f, 65f), new Vector3(28f, 7f, 28f), container.transform);
            CreateMiniBossSpawner("MiniBoss2", new Vector3(35f, 1.5f, 68f), container.transform);
            // Mini Boss 3 (centro antes del boss)
            CreateCorridor("Corridor_PreBoss", new Vector3(0f, 0f, 98f), new Vector3(8f, 6f, 12f), container.transform);
            CreateRoom("Room_MiniBoss_3", new Vector3(0f, 0f, 118f), new Vector3(30f, 8f, 30f), container.transform);
            CreateMiniBossSpawner("MiniBoss3", new Vector3(0f, 1.5f, 122f), container.transform);
            // Corredor al boss final
            CreateCorridor("Corridor_ToBoss", new Vector3(0f, 0f, 140f), new Vector3(10f, 7f, 15f), container.transform);
        }

        private static void CreateMiniBossSpawner(string name, Vector3 pos, Transform parent)
        {
            GameObject mb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mb.name = name + "_Spawner"; mb.transform.parent = parent;
            mb.transform.position = pos;
            mb.transform.localScale = new Vector3(2f, 3f, 2f);
            ApplyMaterial(mb, new Color(0.7f, 0.2f, 0.5f));
        }

        private static void CreateBossRoom()
        {
            GameObject container = new GameObject("--- BOSS ROOM ---");
            Vector3 pos = new Vector3(0f, 0f, 170f);
            Vector3 size = new Vector3(50f, 15f, 50f);
            GameObject room = new GameObject("BossRoom_Final");
            room.transform.parent = container.transform; room.transform.position = pos;
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor_Boss"; floor.transform.parent = room.transform;
            floor.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(size.x, 1f, size.z);
            floor.isStatic = true;
            ApplyMaterial(floor, new Color(0.5f, 0.1f, 0.1f));
            CreateWall("Wall_North", room.transform, new Vector3(0f, size.y/2, size.z/2), new Vector3(size.x, size.y, 1f));
            CreateWall("Wall_East", room.transform, new Vector3(size.x/2, size.y/2, 0f), new Vector3(1f, size.y, size.z));
            CreateWall("Wall_West", room.transform, new Vector3(-size.x/2, size.y/2, 0f), new Vector3(1f, size.y, size.z));
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling_Boss"; ceiling.transform.parent = room.transform;
            ceiling.transform.localPosition = new Vector3(0f, size.y, 0f);
            ceiling.transform.localScale = new Vector3(size.x, 0.5f, size.z);
            ceiling.isStatic = true;
            ApplyMaterial(ceiling, new Color(0.35f, 0.08f, 0.08f));
            // Boss épico
            GameObject boss = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boss.name = "FinalBoss_Spawner"; boss.transform.parent = container.transform;
            boss.transform.position = pos + new Vector3(0f, 3f, 12f);
            boss.transform.localScale = new Vector3(5f, 6f, 5f);
            ApplyMaterial(boss, new Color(1f, 0.1f, 0.1f));
            // Pilares decorativos
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f * Mathf.Deg2Rad + 45f * Mathf.Deg2Rad;
                Vector3 pillarPos = pos + new Vector3(Mathf.Cos(angle) * 18f, 4f, Mathf.Sin(angle) * 18f);
                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.name = $"Pillar_{i}"; pillar.transform.parent = room.transform;
                pillar.transform.position = pillarPos;
                pillar.transform.localScale = new Vector3(3f, 8f, 3f);
                pillar.isStatic = true;
                ApplyMaterial(pillar, new Color(0.4f, 0.15f, 0.15f));
            }
        }

        private static void CreateExitPortal()
        {
            GameObject container = new GameObject("--- EXIT PORTAL ---");
            GameObject portal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            portal.name = "Portal_Exit (Inactive)";
            portal.transform.parent = container.transform;
            portal.transform.position = new Vector3(0f, 4f, 193f);
            portal.transform.localScale = new Vector3(6f, 8f, 0.5f);
            portal.SetActive(false);
            ApplyMaterial(portal, new Color(0f, 1f, 0.5f));
        }

        private static void CreatePlayerWithController()
        {
            GameObject container = new GameObject("--- PLAYER ---");
            Vector3 spawnPos = new Vector3(0f, 0.5f, -5f);
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
            c.backgroundColor = new Color(0.04f, 0.02f, 0.02f);
            c.fieldOfView = 60f;
            cam.AddComponent<AudioListener>();
            cam.transform.position = playerPos + new Vector3(0f, 4f, -10f);
            cam.transform.LookAt(playerPos + Vector3.up * 1.5f);
        }
    }
}
