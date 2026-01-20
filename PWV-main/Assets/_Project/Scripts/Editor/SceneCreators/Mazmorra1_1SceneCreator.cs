using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace EtherDomes.Editor
{
    /// <summary>
    /// Crea la escena de Mazmorra 1.1 - Primera dungeon del juego
    /// </summary>
    public static class Mazmorra1_1SceneCreator
    {
        private const string SCENE_PATH = "Assets/_Project/Scenes/Dungeons/Mazmorra1_1.unity";
        private const string MENU_PATH = "Tools/EtherDomes/Crear Escena Mazmorra 1.1";

        [MenuItem(MENU_PATH)]
        public static void CreateScene()
        {
            if (!EditorUtility.DisplayDialog("Crear Mazmorra 1.1",
                "¿Crear la escena de Mazmorra 1.1?\n\nEsta será la primera dungeon con:\n- 3 salas de trash mobs\n- 1 sala de boss",
                "Crear", "Cancelar"))
            {
                return;
            }

            // Crear nueva escena
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Crear estructura de dungeon
            CreateLighting();
            CreateDungeonLayout();
            CreateSpawnPoints();
            CreateEnemySpawners();
            CreateBossRoom();
            CreateExitPortal();

            // Guardar escena
            EnsureDirectoryExists();
            EditorSceneManager.SaveScene(newScene, SCENE_PATH);

            Debug.Log($"[Mazmorra1_1SceneCreator] Escena creada en: {SCENE_PATH}");
            EditorUtility.DisplayDialog("Éxito", "Mazmorra 1.1 creada correctamente.\n\nRecuerda agregar la escena al Build Settings.", "OK");
        }

        private static void EnsureDirectoryExists()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Scenes/Dungeons"))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Scenes", "Dungeons");
            }
        }

        private static void CreateLighting()
        {
            // Luz ambiental tenue para dungeon
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.15f, 0.1f, 0.1f);

            // Luz direccional tenue
            GameObject mainLight = new GameObject("Dungeon Light");
            Light light = mainLight.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.6f, 0.5f, 0.4f);
            light.intensity = 0.3f;
            mainLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void CreateDungeonLayout()
        {
            GameObject dungeonContainer = new GameObject("--- DUNGEON LAYOUT ---");

            // Sala de entrada
            CreateRoom("Room_Entrance", Vector3.zero, new Vector3(15f, 5f, 15f), dungeonContainer.transform);

            // Pasillo 1
            CreateCorridor("Corridor_1", new Vector3(0f, 0f, 15f), new Vector3(5f, 4f, 10f), dungeonContainer.transform);

            // Sala 1 (Trash)
            CreateRoom("Room_Trash_1", new Vector3(0f, 0f, 30f), new Vector3(20f, 5f, 20f), dungeonContainer.transform);

            // Pasillo 2
            CreateCorridor("Corridor_2", new Vector3(0f, 0f, 50f), new Vector3(5f, 4f, 10f), dungeonContainer.transform);

            // Sala 2 (Trash)
            CreateRoom("Room_Trash_2", new Vector3(0f, 0f, 65f), new Vector3(20f, 5f, 20f), dungeonContainer.transform);

            // Pasillo 3
            CreateCorridor("Corridor_3", new Vector3(0f, 0f, 85f), new Vector3(5f, 4f, 10f), dungeonContainer.transform);

            // Sala 3 (Trash)
            CreateRoom("Room_Trash_3", new Vector3(0f, 0f, 100f), new Vector3(20f, 5f, 20f), dungeonContainer.transform);

            // Pasillo al Boss
            CreateCorridor("Corridor_Boss", new Vector3(0f, 0f, 120f), new Vector3(8f, 5f, 15f), dungeonContainer.transform);
        }

        private static void CreateRoom(string name, Vector3 position, Vector3 size, Transform parent)
        {
            GameObject room = new GameObject(name);
            room.transform.parent = parent;
            room.transform.position = position;

            // Suelo
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.parent = room.transform;
            floor.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(size.x, 1f, size.z);
            floor.isStatic = true;
            ApplyDungeonMaterial(floor, new Color(0.3f, 0.25f, 0.2f));

            // Paredes
            CreateWall("Wall_North", room.transform, new Vector3(0f, size.y/2, size.z/2), new Vector3(size.x, size.y, 1f));
            CreateWall("Wall_South", room.transform, new Vector3(0f, size.y/2, -size.z/2), new Vector3(size.x, size.y, 1f));
            CreateWall("Wall_East", room.transform, new Vector3(size.x/2, size.y/2, 0f), new Vector3(1f, size.y, size.z));
            CreateWall("Wall_West", room.transform, new Vector3(-size.x/2, size.y/2, 0f), new Vector3(1f, size.y, size.z));

            // Techo
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling";
            ceiling.transform.parent = room.transform;
            ceiling.transform.localPosition = new Vector3(0f, size.y, 0f);
            ceiling.transform.localScale = new Vector3(size.x, 0.5f, size.z);
            ceiling.isStatic = true;
            ApplyDungeonMaterial(ceiling, new Color(0.2f, 0.15f, 0.1f));

            // Antorchas en las esquinas
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

            // Suelo
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.parent = corridor.transform;
            floor.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(size.x, 1f, size.z);
            floor.isStatic = true;
            ApplyDungeonMaterial(floor, new Color(0.25f, 0.2f, 0.15f));

            // Paredes laterales
            CreateWall("Wall_East", corridor.transform, new Vector3(size.x/2, size.y/2, 0f), new Vector3(0.5f, size.y, size.z));
            CreateWall("Wall_West", corridor.transform, new Vector3(-size.x/2, size.y/2, 0f), new Vector3(0.5f, size.y, size.z));

            // Techo
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling";
            ceiling.transform.parent = corridor.transform;
            ceiling.transform.localPosition = new Vector3(0f, size.y, 0f);
            ceiling.transform.localScale = new Vector3(size.x, 0.5f, size.z);
            ceiling.isStatic = true;
            ApplyDungeonMaterial(ceiling, new Color(0.2f, 0.15f, 0.1f));
        }

        private static void CreateWall(string name, Transform parent, Vector3 localPos, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.parent = parent;
            wall.transform.localPosition = localPos;
            wall.transform.localScale = scale;
            wall.isStatic = true;
            ApplyDungeonMaterial(wall, new Color(0.35f, 0.3f, 0.25f));
        }

        private static void CreateTorch(Transform parent, Vector3 localPos)
        {
            GameObject torch = new GameObject("Torch");
            torch.transform.parent = parent;
            torch.transform.localPosition = localPos;

            // Luz de antorcha
            Light torchLight = torch.AddComponent<Light>();
            torchLight.type = LightType.Point;
            torchLight.color = new Color(1f, 0.7f, 0.3f);
            torchLight.intensity = 1.5f;
            torchLight.range = 8f;

            // Visual de antorcha (placeholder)
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.name = "TorchVisual";
            visual.transform.parent = torch.transform;
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(0.1f, 0.3f, 0.1f);
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            
            var renderer = visual.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(1f, 0.5f, 0f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(1f, 0.5f, 0f) * 2f);
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

            // Spawn de entrada
            GameObject entranceSpawn = new GameObject("SpawnPoint_Entrance");
            entranceSpawn.transform.parent = spawnContainer.transform;
            entranceSpawn.transform.position = new Vector3(0f, 0.5f, -5f);
            entranceSpawn.tag = "Respawn";
        }

        private static void CreateEnemySpawners()
        {
            GameObject spawnerContainer = new GameObject("--- ENEMY SPAWNERS ---");

            // Spawners en Sala 1
            CreateEnemySpawnerPlaceholder("Spawner_Room1_1", new Vector3(-5f, 0.5f, 30f), spawnerContainer.transform);
            CreateEnemySpawnerPlaceholder("Spawner_Room1_2", new Vector3(5f, 0.5f, 30f), spawnerContainer.transform);
            CreateEnemySpawnerPlaceholder("Spawner_Room1_3", new Vector3(0f, 0.5f, 35f), spawnerContainer.transform);

            // Spawners en Sala 2
            CreateEnemySpawnerPlaceholder("Spawner_Room2_1", new Vector3(-5f, 0.5f, 65f), spawnerContainer.transform);
            CreateEnemySpawnerPlaceholder("Spawner_Room2_2", new Vector3(5f, 0.5f, 65f), spawnerContainer.transform);
            CreateEnemySpawnerPlaceholder("Spawner_Room2_3", new Vector3(0f, 0.5f, 70f), spawnerContainer.transform);
            CreateEnemySpawnerPlaceholder("Spawner_Room2_4", new Vector3(0f, 0.5f, 60f), spawnerContainer.transform);

            // Spawners en Sala 3
            CreateEnemySpawnerPlaceholder("Spawner_Room3_1", new Vector3(-7f, 0.5f, 100f), spawnerContainer.transform);
            CreateEnemySpawnerPlaceholder("Spawner_Room3_2", new Vector3(7f, 0.5f, 100f), spawnerContainer.transform);
            CreateEnemySpawnerPlaceholder("Spawner_Room3_3", new Vector3(-3f, 0.5f, 105f), spawnerContainer.transform);
            CreateEnemySpawnerPlaceholder("Spawner_Room3_4", new Vector3(3f, 0.5f, 105f), spawnerContainer.transform);
            CreateEnemySpawnerPlaceholder("Spawner_Room3_5", new Vector3(0f, 0.5f, 95f), spawnerContainer.transform);
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

            // Quitar collider del placeholder
            Object.DestroyImmediate(spawner.GetComponent<Collider>());
        }

        private static void CreateBossRoom()
        {
            GameObject bossContainer = new GameObject("--- BOSS ROOM ---");

            // Sala del Boss (más grande)
            Vector3 bossRoomPos = new Vector3(0f, 0f, 145f);
            Vector3 bossRoomSize = new Vector3(30f, 8f, 30f);

            GameObject bossRoom = new GameObject("BossRoom");
            bossRoom.transform.parent = bossContainer.transform;
            bossRoom.transform.position = bossRoomPos;

            // Suelo especial
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor_Boss";
            floor.transform.parent = bossRoom.transform;
            floor.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(bossRoomSize.x, 1f, bossRoomSize.z);
            floor.isStatic = true;
            ApplyDungeonMaterial(floor, new Color(0.4f, 0.2f, 0.2f)); // Rojo oscuro

            // Paredes
            CreateWall("Wall_North", bossRoom.transform, new Vector3(0f, bossRoomSize.y/2, bossRoomSize.z/2), new Vector3(bossRoomSize.x, bossRoomSize.y, 1f));
            CreateWall("Wall_East", bossRoom.transform, new Vector3(bossRoomSize.x/2, bossRoomSize.y/2, 0f), new Vector3(1f, bossRoomSize.y, bossRoomSize.z));
            CreateWall("Wall_West", bossRoom.transform, new Vector3(-bossRoomSize.x/2, bossRoomSize.y/2, 0f), new Vector3(1f, bossRoomSize.y, bossRoomSize.z));

            // Techo
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling_Boss";
            ceiling.transform.parent = bossRoom.transform;
            ceiling.transform.localPosition = new Vector3(0f, bossRoomSize.y, 0f);
            ceiling.transform.localScale = new Vector3(bossRoomSize.x, 0.5f, bossRoomSize.z);
            ceiling.isStatic = true;
            ApplyDungeonMaterial(ceiling, new Color(0.3f, 0.15f, 0.15f));

            // Boss Spawner
            GameObject bossSpawner = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bossSpawner.name = "BossSpawner (Placeholder)";
            bossSpawner.transform.parent = bossContainer.transform;
            bossSpawner.transform.position = bossRoomPos + new Vector3(0f, 1.5f, 5f);
            bossSpawner.transform.localScale = new Vector3(2f, 3f, 2f);
            
            var bossRenderer = bossSpawner.GetComponent<MeshRenderer>();
            Material bossMat = new Material(Shader.Find("Standard"));
            bossMat.color = new Color(0.8f, 0.1f, 0.1f);
            bossRenderer.material = bossMat;
        }

        private static void CreateExitPortal()
        {
            GameObject portalContainer = new GameObject("--- EXIT PORTAL ---");

            // Portal de salida (aparece después de matar al boss)
            GameObject exitPortal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            exitPortal.name = "Portal_Exit (Inactive)";
            exitPortal.transform.parent = portalContainer.transform;
            exitPortal.transform.position = new Vector3(0f, 2f, 158f);
            exitPortal.transform.localScale = new Vector3(4f, 5f, 0.5f);
            exitPortal.SetActive(false); // Inactivo hasta matar al boss

            var renderer = exitPortal.GetComponent<MeshRenderer>();
            Material portalMat = new Material(Shader.Find("Standard"));
            portalMat.color = new Color(0f, 1f, 0.5f, 0.5f);
            renderer.material = portalMat;
        }
    }
}
