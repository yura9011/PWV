using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace EtherDomes.Editor
{
    /// <summary>
    /// Crea la escena de Mazmorra 1.2 - Segunda dungeon del juego
    /// </summary>
    public static class Mazmorra1_2SceneCreator
    {
        private const string SCENE_PATH = "Assets/_Project/Scenes/Dungeons/Mazmorra1_2.unity";
        private const string MENU_PATH = "Tools/EtherDomes/Crear Escena Mazmorra 1.2";

        [MenuItem(MENU_PATH)]
        public static void CreateScene()
        {
            if (!EditorUtility.DisplayDialog("Crear Mazmorra 1.2",
                "¿Crear la escena de Mazmorra 1.2?\n\nEsta será la segunda dungeon con:\n- 4 salas de trash mobs\n- 1 sala de mini-boss\n- 1 sala de boss final",
                "Crear", "Cancelar"))
            {
                return;
            }

            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateLighting();
            CreateDungeonLayout();
            CreateSpawnPoints();
            CreateEnemySpawners();
            CreateMiniBossRoom();
            CreateBossRoom();
            CreateExitPortal();
            CreatePlayerWithController();

            EnsureDirectoryExists();
            EditorSceneManager.SaveScene(newScene, SCENE_PATH);

            Debug.Log($"[Mazmorra1_2SceneCreator] Escena creada en: {SCENE_PATH}");
            EditorUtility.DisplayDialog("Éxito", "Mazmorra 1.2 creada correctamente.\n\nRecuerda agregar la escena al Build Settings.", "OK");
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
            RenderSettings.ambientLight = new Color(0.1f, 0.12f, 0.15f);

            GameObject mainLight = new GameObject("Dungeon Light");
            Light light = mainLight.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.4f, 0.5f, 0.6f);
            light.intensity = 0.25f;
            mainLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void CreateDungeonLayout()
        {
            GameObject dungeonContainer = new GameObject("--- DUNGEON LAYOUT ---");

            CreateRoom("Room_Entrance", Vector3.zero, new Vector3(15f, 5f, 15f), dungeonContainer.transform);
            CreateCorridor("Corridor_1", new Vector3(0f, 0f, 15f), new Vector3(5f, 4f, 12f), dungeonContainer.transform);
            CreateRoom("Room_Trash_1", new Vector3(0f, 0f, 32f), new Vector3(22f, 5f, 22f), dungeonContainer.transform);
            CreateCorridor("Corridor_2", new Vector3(0f, 0f, 54f), new Vector3(5f, 4f, 10f), dungeonContainer.transform);
            CreateRoom("Room_Trash_2", new Vector3(0f, 0f, 69f), new Vector3(22f, 5f, 22f), dungeonContainer.transform);
            CreateCorridor("Corridor_3", new Vector3(0f, 0f, 91f), new Vector3(5f, 4f, 10f), dungeonContainer.transform);
            CreateRoom("Room_Trash_3", new Vector3(0f, 0f, 106f), new Vector3(22f, 5f, 22f), dungeonContainer.transform);
            CreateCorridor("Corridor_4", new Vector3(0f, 0f, 128f), new Vector3(5f, 4f, 10f), dungeonContainer.transform);
            CreateRoom("Room_Trash_4", new Vector3(0f, 0f, 143f), new Vector3(22f, 5f, 22f), dungeonContainer.transform);
            CreateCorridor("Corridor_MiniBoss", new Vector3(0f, 0f, 165f), new Vector3(6f, 5f, 12f), dungeonContainer.transform);
        }


        private static void CreateRoom(string name, Vector3 position, Vector3 size, Transform parent)
        {
            GameObject room = new GameObject(name);
            room.transform.parent = parent;
            room.transform.position = position;

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.parent = room.transform;
            floor.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(size.x, 1f, size.z);
            floor.isStatic = true;
            ApplyDungeonMaterial(floor, new Color(0.25f, 0.28f, 0.3f));

            CreateWall("Wall_North", room.transform, new Vector3(0f, size.y/2, size.z/2), new Vector3(size.x, size.y, 1f));
            CreateWall("Wall_South", room.transform, new Vector3(0f, size.y/2, -size.z/2), new Vector3(size.x, size.y, 1f));
            CreateWall("Wall_East", room.transform, new Vector3(size.x/2, size.y/2, 0f), new Vector3(1f, size.y, size.z));
            CreateWall("Wall_West", room.transform, new Vector3(-size.x/2, size.y/2, 0f), new Vector3(1f, size.y, size.z));

            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling";
            ceiling.transform.parent = room.transform;
            ceiling.transform.localPosition = new Vector3(0f, size.y, 0f);
            ceiling.transform.localScale = new Vector3(size.x, 0.5f, size.z);
            ceiling.isStatic = true;
            ApplyDungeonMaterial(ceiling, new Color(0.18f, 0.2f, 0.22f));

            CreateTorch(room.transform, new Vector3(size.x/2 - 1f, 2f, size.z/2 - 1f));
            CreateTorch(room.transform, new Vector3(-size.x/2 + 1f, 2f, size.z/2 - 1f));
            CreateTorch(room.transform, new Vector3(size.x/2 - 1f, 2f, -size.z/2 + 1f));
            CreateTorch(room.transform, new Vector3(-size.x/2 + 1f, 2f, -size.z/2 + 1f));
        }

        private static void CreateCorridor(string name, Vector3 position, Vector3 size, Transform parent)
        {
            GameObject corridor = new GameObject(name);
            corridor.transform.parent = parent;
            corridor.transform.position = position;

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.parent = corridor.transform;
            floor.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(size.x, 1f, size.z);
            floor.isStatic = true;
            ApplyDungeonMaterial(floor, new Color(0.22f, 0.24f, 0.26f));

            CreateWall("Wall_East", corridor.transform, new Vector3(size.x/2, size.y/2, 0f), new Vector3(0.5f, size.y, size.z));
            CreateWall("Wall_West", corridor.transform, new Vector3(-size.x/2, size.y/2, 0f), new Vector3(0.5f, size.y, size.z));

            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling";
            ceiling.transform.parent = corridor.transform;
            ceiling.transform.localPosition = new Vector3(0f, size.y, 0f);
            ceiling.transform.localScale = new Vector3(size.x, 0.5f, size.z);
            ceiling.isStatic = true;
            ApplyDungeonMaterial(ceiling, new Color(0.18f, 0.2f, 0.22f));
        }

        private static void CreateWall(string name, Transform parent, Vector3 localPos, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.parent = parent;
            wall.transform.localPosition = localPos;
            wall.transform.localScale = scale;
            wall.isStatic = true;
            ApplyDungeonMaterial(wall, new Color(0.3f, 0.32f, 0.35f));
        }

        private static void CreateTorch(Transform parent, Vector3 localPos)
        {
            GameObject torch = new GameObject("Torch");
            torch.transform.parent = parent;
            torch.transform.localPosition = localPos;

            Light torchLight = torch.AddComponent<Light>();
            torchLight.type = LightType.Point;
            torchLight.color = new Color(0.6f, 0.8f, 1f);
            torchLight.intensity = 1.2f;
            torchLight.range = 10f;

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.name = "TorchVisual";
            visual.transform.parent = torch.transform;
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(0.1f, 0.3f, 0.1f);
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            
            var renderer = visual.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.4f, 0.6f, 1f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.4f, 0.6f, 1f) * 2f);
            renderer.material = mat;
        }

        private static void ApplyDungeonMaterial(GameObject obj, Color color)
        {
            var renderer = obj.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            renderer.material = mat;
        }


        private static void CreateSpawnPoints()
        {
            GameObject spawnContainer = new GameObject("--- SPAWN POINTS ---");

            GameObject entranceSpawn = new GameObject("SpawnPoint_Entrance");
            entranceSpawn.transform.parent = spawnContainer.transform;
            entranceSpawn.transform.position = new Vector3(0f, 0.5f, -5f);
            entranceSpawn.tag = "Respawn";
        }

        private static void CreateEnemySpawners()
        {
            GameObject spawnerContainer = new GameObject("--- ENEMY SPAWNERS ---");

            // Sala 1
            CreateEnemySpawnerPlaceholder("Spawner_Room1_1", new Vector3(-6f, 0.5f, 32f), spawnerContainer.transform);
            CreateEnemySpawnerPlaceholder("Spawner_Room1_2", new Vector3(6f, 0.5f, 32f), spawnerContainer.transform);
            CreateEnemySpawnerPlaceholder("Spawner_Room1_3", new Vector3(0f, 0.5f, 38f), spawnerContainer.transform);

            // Sala 2
            CreateEnemySpawnerPlaceholder("Spawner_Room2_1", new Vector3(-6f, 0.5f, 69f), spawnerContainer.transform);
            CreateEnemySpawnerPlaceholder("Spawner_Room2_2", new Vector3(6f, 0.5f, 69f), spawnerContainer.transform);
            CreateEnemySpawnerPlaceholder("Spawner_Room2_3", new Vector3(0f, 0.5f, 75f), spawnerContainer.transform);
            CreateEnemySpawnerPlaceholder("Spawner_Room2_4", new Vector3(0f, 0.5f, 63f), spawnerContainer.transform);

            // Sala 3
            CreateEnemySpawnerPlaceholder("Spawner_Room3_1", new Vector3(-7f, 0.5f, 106f), spawnerContainer.transform);
            CreateEnemySpawnerPlaceholder("Spawner_Room3_2", new Vector3(7f, 0.5f, 106f), spawnerContainer.transform);
            CreateEnemySpawnerPlaceholder("Spawner_Room3_3", new Vector3(-3f, 0.5f, 112f), spawnerContainer.transform);
            CreateEnemySpawnerPlaceholder("Spawner_Room3_4", new Vector3(3f, 0.5f, 112f), spawnerContainer.transform);

            // Sala 4
            CreateEnemySpawnerPlaceholder("Spawner_Room4_1", new Vector3(-8f, 0.5f, 143f), spawnerContainer.transform);
            CreateEnemySpawnerPlaceholder("Spawner_Room4_2", new Vector3(8f, 0.5f, 143f), spawnerContainer.transform);
            CreateEnemySpawnerPlaceholder("Spawner_Room4_3", new Vector3(-4f, 0.5f, 149f), spawnerContainer.transform);
            CreateEnemySpawnerPlaceholder("Spawner_Room4_4", new Vector3(4f, 0.5f, 149f), spawnerContainer.transform);
            CreateEnemySpawnerPlaceholder("Spawner_Room4_5", new Vector3(0f, 0.5f, 137f), spawnerContainer.transform);
        }

        private static void CreateEnemySpawnerPlaceholder(string name, Vector3 position, Transform parent)
        {
            GameObject spawner = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spawner.name = name;
            spawner.transform.parent = parent;
            spawner.transform.position = position;
            spawner.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            var renderer = spawner.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.red;
            renderer.material = mat;

            Object.DestroyImmediate(spawner.GetComponent<Collider>());
        }

        private static void CreateMiniBossRoom()
        {
            GameObject miniBossContainer = new GameObject("--- MINI BOSS ROOM ---");

            Vector3 miniBossRoomPos = new Vector3(0f, 0f, 185f);
            Vector3 miniBossRoomSize = new Vector3(25f, 6f, 25f);

            GameObject miniBossRoom = new GameObject("MiniBossRoom");
            miniBossRoom.transform.parent = miniBossContainer.transform;
            miniBossRoom.transform.position = miniBossRoomPos;

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor_MiniBoss";
            floor.transform.parent = miniBossRoom.transform;
            floor.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(miniBossRoomSize.x, 1f, miniBossRoomSize.z);
            floor.isStatic = true;
            ApplyDungeonMaterial(floor, new Color(0.3f, 0.25f, 0.35f));

            CreateWall("Wall_North", miniBossRoom.transform, new Vector3(0f, miniBossRoomSize.y/2, miniBossRoomSize.z/2), new Vector3(miniBossRoomSize.x, miniBossRoomSize.y, 1f));
            CreateWall("Wall_South", miniBossRoom.transform, new Vector3(0f, miniBossRoomSize.y/2, -miniBossRoomSize.z/2), new Vector3(miniBossRoomSize.x, miniBossRoomSize.y, 1f));
            CreateWall("Wall_East", miniBossRoom.transform, new Vector3(miniBossRoomSize.x/2, miniBossRoomSize.y/2, 0f), new Vector3(1f, miniBossRoomSize.y, miniBossRoomSize.z));
            CreateWall("Wall_West", miniBossRoom.transform, new Vector3(-miniBossRoomSize.x/2, miniBossRoomSize.y/2, 0f), new Vector3(1f, miniBossRoomSize.y, miniBossRoomSize.z));

            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling_MiniBoss";
            ceiling.transform.parent = miniBossRoom.transform;
            ceiling.transform.localPosition = new Vector3(0f, miniBossRoomSize.y, 0f);
            ceiling.transform.localScale = new Vector3(miniBossRoomSize.x, 0.5f, miniBossRoomSize.z);
            ceiling.isStatic = true;
            ApplyDungeonMaterial(ceiling, new Color(0.25f, 0.2f, 0.3f));

            GameObject miniBossSpawner = GameObject.CreatePrimitive(PrimitiveType.Cube);
            miniBossSpawner.name = "MiniBossSpawner (Placeholder)";
            miniBossSpawner.transform.parent = miniBossContainer.transform;
            miniBossSpawner.transform.position = miniBossRoomPos + new Vector3(0f, 1.2f, 3f);
            miniBossSpawner.transform.localScale = new Vector3(1.5f, 2.5f, 1.5f);
            
            var miniBossRenderer = miniBossSpawner.GetComponent<MeshRenderer>();
            Material miniBossMat = new Material(Shader.Find("Standard"));
            miniBossMat.color = new Color(0.6f, 0.2f, 0.6f);
            miniBossRenderer.material = miniBossMat;

            CreateCorridor("Corridor_ToBoss", new Vector3(0f, 0f, 210f), new Vector3(8f, 6f, 15f), miniBossContainer.transform);
        }


        private static void CreateBossRoom()
        {
            GameObject bossContainer = new GameObject("--- BOSS ROOM ---");

            Vector3 bossRoomPos = new Vector3(0f, 0f, 235f);
            Vector3 bossRoomSize = new Vector3(35f, 10f, 35f);

            GameObject bossRoom = new GameObject("BossRoom");
            bossRoom.transform.parent = bossContainer.transform;
            bossRoom.transform.position = bossRoomPos;

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor_Boss";
            floor.transform.parent = bossRoom.transform;
            floor.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(bossRoomSize.x, 1f, bossRoomSize.z);
            floor.isStatic = true;
            ApplyDungeonMaterial(floor, new Color(0.35f, 0.15f, 0.15f));

            CreateWall("Wall_North", bossRoom.transform, new Vector3(0f, bossRoomSize.y/2, bossRoomSize.z/2), new Vector3(bossRoomSize.x, bossRoomSize.y, 1f));
            CreateWall("Wall_East", bossRoom.transform, new Vector3(bossRoomSize.x/2, bossRoomSize.y/2, 0f), new Vector3(1f, bossRoomSize.y, bossRoomSize.z));
            CreateWall("Wall_West", bossRoom.transform, new Vector3(-bossRoomSize.x/2, bossRoomSize.y/2, 0f), new Vector3(1f, bossRoomSize.y, bossRoomSize.z));

            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling_Boss";
            ceiling.transform.parent = bossRoom.transform;
            ceiling.transform.localPosition = new Vector3(0f, bossRoomSize.y, 0f);
            ceiling.transform.localScale = new Vector3(bossRoomSize.x, 0.5f, bossRoomSize.z);
            ceiling.isStatic = true;
            ApplyDungeonMaterial(ceiling, new Color(0.28f, 0.12f, 0.12f));

            GameObject bossSpawner = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bossSpawner.name = "BossSpawner (Placeholder)";
            bossSpawner.transform.parent = bossContainer.transform;
            bossSpawner.transform.position = bossRoomPos + new Vector3(0f, 2f, 8f);
            bossSpawner.transform.localScale = new Vector3(3f, 4f, 3f);
            
            var bossRenderer = bossSpawner.GetComponent<MeshRenderer>();
            Material bossMat = new Material(Shader.Find("Standard"));
            bossMat.color = new Color(0.9f, 0.1f, 0.1f);
            bossRenderer.material = bossMat;
        }

        private static void CreateExitPortal()
        {
            GameObject portalContainer = new GameObject("--- EXIT PORTAL ---");

            GameObject exitPortal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            exitPortal.name = "Portal_Exit (Inactive)";
            exitPortal.transform.parent = portalContainer.transform;
            exitPortal.transform.position = new Vector3(0f, 2.5f, 250f);
            exitPortal.transform.localScale = new Vector3(5f, 6f, 0.5f);
            exitPortal.SetActive(false);

            var renderer = exitPortal.GetComponent<MeshRenderer>();
            Material portalMat = new Material(Shader.Find("Standard"));
            portalMat.color = new Color(0f, 1f, 0.5f, 0.5f);
            renderer.material = portalMat;
        }

        private static void CreatePlayerWithController()
        {
            GameObject playerContainer = new GameObject("--- PLAYER ---");
            Vector3 spawnPos = new Vector3(0f, 0.5f, -3f);

            // Buscar prefab de NetworkPlayer
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

            GameObject player;
            if (playerPrefab != null)
            {
                player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                player.name = "Player (Local Test)";
                player.transform.position = spawnPos;
                player.transform.parent = playerContainer.transform;
                
                // Remover componentes de red para testing local
                var networkObject = player.GetComponent<Unity.Netcode.NetworkObject>();
                if (networkObject != null) Object.DestroyImmediate(networkObject);
                
                var networkTransform = player.GetComponent<Unity.Netcode.Components.NetworkTransform>();
                if (networkTransform != null) Object.DestroyImmediate(networkTransform);

                Debug.Log("[Mazmorra1_2SceneCreator] Player prefab instanciado con PlayerController");
            }
            else
            {
                // Crear player básico con PlayerController
                player = new GameObject("Player (Local Test)");
                player.transform.position = spawnPos;
                player.transform.parent = playerContainer.transform;
                player.tag = "Player";

                // Cuerpo visual
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = "Body";
                body.transform.parent = player.transform;
                body.transform.localPosition = new Vector3(0f, 1f, 0f);
                Object.DestroyImmediate(body.GetComponent<Collider>());
                
                var renderer = body.GetComponent<MeshRenderer>();
                Material playerMat = new Material(Shader.Find("Standard"));
                playerMat.color = new Color(0.2f, 0.6f, 0.8f);
                renderer.material = playerMat;

                // CharacterController (requerido por PlayerController)
                CharacterController cc = player.AddComponent<CharacterController>();
                cc.height = 2f;
                cc.radius = 0.5f;
                cc.center = new Vector3(0f, 1f, 0f);

                // Agregar PlayerController
                player.AddComponent<EtherDomes.Player.PlayerController>();

                Debug.Log("[Mazmorra1_2SceneCreator] Player básico creado con PlayerController");
            }

            // Crear cámara principal
            CreateMainCamera(spawnPos);
            
            // EventSystem para UI
            SceneCreatorUtils.CreateEventSystem();
        }

        private static void CreateMainCamera(Vector3 playerPosition)
        {
            UnityEngine.Camera existingCam = Object.FindFirstObjectByType<UnityEngine.Camera>();
            if (existingCam != null) Object.DestroyImmediate(existingCam.gameObject);

            GameObject cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            
            UnityEngine.Camera cam = cameraObj.AddComponent<UnityEngine.Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.1f);
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 500f;

            cameraObj.AddComponent<AudioListener>();

            cameraObj.transform.position = playerPosition + new Vector3(0f, 4f, -10f);
            cameraObj.transform.LookAt(playerPosition + Vector3.up * 1.5f);
        }
    }
}
