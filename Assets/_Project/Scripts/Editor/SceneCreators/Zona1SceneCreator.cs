using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace EtherDomes.Editor
{
    /// <summary>
    /// Crea la escena de Zona 1 - Primera zona de exploración/questing
    /// </summary>
    public static class Zona1SceneCreator
    {
        private const string SCENE_PATH = "Assets/_Project/Scenes/Zones/Zona1.unity";
        private const string MENU_PATH = "Tools/EtherDomes/Crear Escena Zona 1";

        [MenuItem(MENU_PATH)]
        public static void CreateScene()
        {
            if (!EditorUtility.DisplayDialog("Crear Zona 1",
                "¿Crear la escena de Zona 1?\n\nEsta será la primera zona de exploración con:\n- Área abierta para questing\n- Enemigos de nivel 1-5\n- NPCs de misiones\n- Entrada a Mazmorra 1.1",
                "Crear", "Cancelar"))
            {
                return;
            }

            // Crear nueva escena
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Crear estructura de zona
            CreateLighting();
            CreateTerrain();
            CreateSpawnPoints();
            CreateEnemyAreas();
            CreateQuestNPCs();
            CreateLandmarks();
            CreatePortals();

            // Guardar escena
            EnsureDirectoryExists();
            EditorSceneManager.SaveScene(newScene, SCENE_PATH);

            Debug.Log($"[Zona1SceneCreator] Escena creada en: {SCENE_PATH}");
            EditorUtility.DisplayDialog("Éxito", "Zona 1 creada correctamente.\n\nRecuerda agregar la escena al Build Settings.", "OK");
        }

        private static void EnsureDirectoryExists()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Scenes/Zones"))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Scenes", "Zones");
            }
        }

        private static void CreateLighting()
        {
            // Sol de atardecer para ambiente de aventura
            GameObject sun = new GameObject("Directional Light");
            Light sunLight = sun.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.color = new Color(1f, 0.9f, 0.7f);
            sunLight.intensity = 1.0f;
            sun.transform.rotation = Quaternion.Euler(35f, -45f, 0f);

            // Configurar ambiente
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.5f, 0.6f, 0.8f);
            RenderSettings.ambientEquatorColor = new Color(0.4f, 0.5f, 0.4f);
            RenderSettings.ambientGroundColor = new Color(0.2f, 0.25f, 0.2f);
        }

        private static void CreateTerrain()
        {
            GameObject terrainContainer = new GameObject("--- TERRAIN ---");

            // Suelo principal grande (200x200)
            GameObject mainGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
            mainGround.name = "Ground_Main";
            mainGround.transform.parent = terrainContainer.transform;
            mainGround.transform.position = Vector3.zero;
            mainGround.transform.localScale = new Vector3(20f, 1f, 20f);
            mainGround.isStatic = true;
            ApplyTerrainMaterial(mainGround, new Color(0.35f, 0.45f, 0.25f)); // Verde bosque

            // Colinas decorativas
            CreateHill("Hill_1", new Vector3(40f, 0f, 40f), 15f, 5f, terrainContainer.transform);
            CreateHill("Hill_2", new Vector3(-50f, 0f, 30f), 20f, 7f, terrainContainer.transform);
            CreateHill("Hill_3", new Vector3(30f, 0f, -60f), 12f, 4f, terrainContainer.transform);
            CreateHill("Hill_4", new Vector3(-40f, 0f, -40f), 18f, 6f, terrainContainer.transform);

            // Rocas decorativas
            CreateRock("Rock_1", new Vector3(20f, 0f, 15f), 3f, terrainContainer.transform);
            CreateRock("Rock_2", new Vector3(-25f, 0f, 50f), 4f, terrainContainer.transform);
            CreateRock("Rock_3", new Vector3(60f, 0f, -20f), 5f, terrainContainer.transform);
            CreateRock("Rock_4", new Vector3(-70f, 0f, -50f), 3.5f, terrainContainer.transform);

            // Árboles placeholder
            CreateTreeCluster("TreeCluster_1", new Vector3(50f, 0f, 50f), 5, terrainContainer.transform);
            CreateTreeCluster("TreeCluster_2", new Vector3(-60f, 0f, 40f), 7, terrainContainer.transform);
            CreateTreeCluster("TreeCluster_3", new Vector3(70f, 0f, -40f), 4, terrainContainer.transform);
            CreateTreeCluster("TreeCluster_4", new Vector3(-50f, 0f, -60f), 6, terrainContainer.transform);
        }

        private static void CreateHill(string name, Vector3 position, float radius, float height, Transform parent)
        {
            GameObject hill = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hill.name = name;
            hill.transform.parent = parent;
            hill.transform.position = position;
            hill.transform.localScale = new Vector3(radius * 2f, height, radius * 2f);
            hill.isStatic = true;
            ApplyTerrainMaterial(hill, new Color(0.3f, 0.4f, 0.2f));
        }

        private static void CreateRock(string name, Vector3 position, float size, Transform parent)
        {
            GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rock.name = name;
            rock.transform.parent = parent;
            rock.transform.position = position + new Vector3(0f, size * 0.3f, 0f);
            rock.transform.localScale = new Vector3(size, size * 0.6f, size * 0.8f);
            rock.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), Random.Range(-10f, 10f));
            rock.isStatic = true;
            ApplyTerrainMaterial(rock, new Color(0.5f, 0.5f, 0.45f));
        }

        private static void CreateTreeCluster(string name, Vector3 center, int count, Transform parent)
        {
            GameObject cluster = new GameObject(name);
            cluster.transform.parent = parent;
            cluster.transform.position = center;

            for (int i = 0; i < count; i++)
            {
                float angle = (i / (float)count) * Mathf.PI * 2f;
                float distance = Random.Range(3f, 10f);
                Vector3 offset = new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);

                CreateTree($"Tree_{i}", offset, cluster.transform);
            }
        }

        private static void CreateTree(string name, Vector3 localPos, Transform parent)
        {
            GameObject tree = new GameObject(name);
            tree.transform.parent = parent;
            tree.transform.localPosition = localPos;

            // Tronco
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.parent = tree.transform;
            trunk.transform.localPosition = new Vector3(0f, 2f, 0f);
            trunk.transform.localScale = new Vector3(0.5f, 2f, 0.5f);
            trunk.isStatic = true;
            ApplyTerrainMaterial(trunk, new Color(0.4f, 0.25f, 0.1f));

            // Copa
            GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            foliage.name = "Foliage";
            foliage.transform.parent = tree.transform;
            foliage.transform.localPosition = new Vector3(0f, 5f, 0f);
            foliage.transform.localScale = new Vector3(3f, 4f, 3f);
            foliage.isStatic = true;
            Object.DestroyImmediate(foliage.GetComponent<Collider>());
            ApplyTerrainMaterial(foliage, new Color(0.2f, 0.5f, 0.15f));
        }

        private static void ApplyTerrainMaterial(GameObject obj, Color color)
        {
            var renderer = obj.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            renderer.material = mat;
        }

        private static void CreateSpawnPoints()
        {
            GameObject spawnContainer = new GameObject("--- SPAWN POINTS ---");

            // Spawn principal (entrada desde Zona Inicial)
            GameObject mainSpawn = new GameObject("SpawnPoint_FromZonaInicial");
            mainSpawn.transform.parent = spawnContainer.transform;
            mainSpawn.transform.position = new Vector3(0f, 0.5f, -90f);
            mainSpawn.tag = "Respawn";

            // Spawn desde Mazmorra
            GameObject dungeonSpawn = new GameObject("SpawnPoint_FromMazmorra");
            dungeonSpawn.transform.parent = spawnContainer.transform;
            dungeonSpawn.transform.position = new Vector3(80f, 0.5f, 0f);
        }

        private static void CreateEnemyAreas()
        {
            GameObject enemyContainer = new GameObject("--- ENEMY AREAS ---");

            // Área de enemigos nivel 1-2 (cerca del spawn)
            CreateEnemyArea("EnemyArea_Lvl1-2", new Vector3(0f, 0f, -50f), 30f, "Nivel 1-2", Color.green, enemyContainer.transform);

            // Área de enemigos nivel 2-3
            CreateEnemyArea("EnemyArea_Lvl2-3", new Vector3(-40f, 0f, 0f), 35f, "Nivel 2-3", Color.yellow, enemyContainer.transform);

            // Área de enemigos nivel 3-4
            CreateEnemyArea("EnemyArea_Lvl3-4", new Vector3(40f, 0f, 30f), 35f, "Nivel 3-4", new Color(1f, 0.5f, 0f), enemyContainer.transform);

            // Área de enemigos nivel 4-5 (cerca de la mazmorra)
            CreateEnemyArea("EnemyArea_Lvl4-5", new Vector3(60f, 0f, -30f), 30f, "Nivel 4-5", Color.red, enemyContainer.transform);
        }

        private static void CreateEnemyArea(string name, Vector3 position, float radius, string levelRange, Color color, Transform parent)
        {
            GameObject area = new GameObject(name);
            area.transform.parent = parent;
            area.transform.position = position;

            // Marcador visual del área (círculo en el suelo)
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = $"AreaMarker_{levelRange}";
            marker.transform.parent = area.transform;
            marker.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            marker.transform.localScale = new Vector3(radius * 2f, 0.1f, radius * 2f);
            Object.DestroyImmediate(marker.GetComponent<Collider>());

            var renderer = marker.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));
            Color transparentColor = color;
            transparentColor.a = 0.2f;
            mat.color = transparentColor;
            mat.SetFloat("_Mode", 3); // Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            renderer.material = mat;

            // Spawners de enemigos dentro del área
            int spawnerCount = Mathf.RoundToInt(radius / 10f) + 2;
            for (int i = 0; i < spawnerCount; i++)
            {
                float angle = (i / (float)spawnerCount) * Mathf.PI * 2f;
                float distance = Random.Range(radius * 0.3f, radius * 0.8f);
                Vector3 spawnerPos = position + new Vector3(Mathf.Cos(angle) * distance, 0.5f, Mathf.Sin(angle) * distance);

                GameObject spawner = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                spawner.name = $"EnemySpawner_{i}";
                spawner.transform.parent = area.transform;
                spawner.transform.position = spawnerPos;
                spawner.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                Object.DestroyImmediate(spawner.GetComponent<Collider>());

                var spawnerRenderer = spawner.GetComponent<MeshRenderer>();
                Material spawnerMat = new Material(Shader.Find("Standard"));
                spawnerMat.color = color;
                spawnerRenderer.material = spawnerMat;
            }
        }

        private static void CreateQuestNPCs()
        {
            GameObject npcContainer = new GameObject("--- QUEST NPCs ---");

            // NPC de misiones principal
            CreateQuestNPC("NPC_QuestGiver_Main", new Vector3(0f, 1f, -70f), "Quest Giver Principal", Color.yellow, npcContainer.transform);

            // NPC de misiones secundario
            CreateQuestNPC("NPC_QuestGiver_Secondary", new Vector3(-30f, 1f, -20f), "Quest Giver Secundario", Color.cyan, npcContainer.transform);

            // NPC vendedor
            CreateQuestNPC("NPC_Vendor", new Vector3(20f, 1f, -60f), "Vendedor", Color.green, npcContainer.transform);

            // NPC de reparación
            CreateQuestNPC("NPC_Repair", new Vector3(-20f, 1f, -65f), "Herrero", new Color(0.6f, 0.4f, 0.2f), npcContainer.transform);
        }

        private static void CreateQuestNPC(string name, Vector3 position, string role, Color color, Transform parent)
        {
            GameObject npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            npc.name = $"{name} ({role})";
            npc.transform.parent = parent;
            npc.transform.position = position;

            var renderer = npc.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            renderer.material = mat;

            // Indicador de quest (!)
            GameObject questMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            questMarker.name = "QuestMarker";
            questMarker.transform.parent = npc.transform;
            questMarker.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            questMarker.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            Object.DestroyImmediate(questMarker.GetComponent<Collider>());

            var markerRenderer = questMarker.GetComponent<MeshRenderer>();
            Material markerMat = new Material(Shader.Find("Standard"));
            markerMat.color = Color.yellow;
            markerMat.EnableKeyword("_EMISSION");
            markerMat.SetColor("_EmissionColor", Color.yellow);
            markerRenderer.material = markerMat;
        }

        private static void CreateLandmarks()
        {
            GameObject landmarkContainer = new GameObject("--- LANDMARKS ---");

            // Torre antigua (punto de referencia)
            CreateTower("AncientTower", new Vector3(0f, 0f, 50f), landmarkContainer.transform);

            // Ruinas
            CreateRuins("OldRuins", new Vector3(-60f, 0f, 60f), landmarkContainer.transform);

            // Campamento
            CreateCamp("AdventurerCamp", new Vector3(30f, 0f, -40f), landmarkContainer.transform);
        }

        private static void CreateTower(string name, Vector3 position, Transform parent)
        {
            GameObject tower = new GameObject(name);
            tower.transform.parent = parent;
            tower.transform.position = position;

            // Base
            GameObject towerBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            towerBase.name = "TowerBase";
            towerBase.transform.parent = tower.transform;
            towerBase.transform.localPosition = new Vector3(0f, 5f, 0f);
            towerBase.transform.localScale = new Vector3(5f, 5f, 5f);
            towerBase.isStatic = true;
            ApplyTerrainMaterial(towerBase, new Color(0.5f, 0.45f, 0.4f));

            // Parte superior
            GameObject towerTop = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            towerTop.name = "TowerTop";
            towerTop.transform.parent = tower.transform;
            towerTop.transform.localPosition = new Vector3(0f, 12f, 0f);
            towerTop.transform.localScale = new Vector3(4f, 3f, 4f);
            towerTop.isStatic = true;
            ApplyTerrainMaterial(towerTop, new Color(0.45f, 0.4f, 0.35f));
        }

        private static void CreateRuins(string name, Vector3 position, Transform parent)
        {
            GameObject ruins = new GameObject(name);
            ruins.transform.parent = parent;
            ruins.transform.position = position;

            // Paredes rotas
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f * Mathf.Deg2Rad;
                Vector3 wallPos = new Vector3(Mathf.Cos(angle) * 8f, 2f, Mathf.Sin(angle) * 8f);

                GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = $"RuinWall_{i}";
                wall.transform.parent = ruins.transform;
                wall.transform.localPosition = wallPos;
                wall.transform.localScale = new Vector3(6f, Random.Range(2f, 5f), 1f);
                wall.transform.rotation = Quaternion.Euler(0f, i * 90f + Random.Range(-15f, 15f), Random.Range(-5f, 5f));
                wall.isStatic = true;
                ApplyTerrainMaterial(wall, new Color(0.55f, 0.5f, 0.45f));
            }
        }

        private static void CreateCamp(string name, Vector3 position, Transform parent)
        {
            GameObject camp = new GameObject(name);
            camp.transform.parent = parent;
            camp.transform.position = position;

            // Fogata
            GameObject campfire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            campfire.name = "Campfire";
            campfire.transform.parent = camp.transform;
            campfire.transform.localPosition = Vector3.zero;
            campfire.transform.localScale = new Vector3(1f, 0.3f, 1f);
            ApplyTerrainMaterial(campfire, new Color(0.3f, 0.2f, 0.1f));

            // Luz de fogata
            GameObject fireLight = new GameObject("FireLight");
            fireLight.transform.parent = campfire.transform;
            fireLight.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            Light light = fireLight.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.6f, 0.2f);
            light.intensity = 2f;
            light.range = 10f;

            // Tiendas
            for (int i = 0; i < 3; i++)
            {
                float angle = i * 120f * Mathf.Deg2Rad;
                Vector3 tentPos = new Vector3(Mathf.Cos(angle) * 5f, 1f, Mathf.Sin(angle) * 5f);

                GameObject tent = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tent.name = $"Tent_{i}";
                tent.transform.parent = camp.transform;
                tent.transform.localPosition = tentPos;
                tent.transform.localScale = new Vector3(2f, 2f, 3f);
                tent.transform.rotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg + 90f, 0f);
                ApplyTerrainMaterial(tent, new Color(0.6f, 0.5f, 0.3f));
            }
        }

        private static void CreatePortals()
        {
            GameObject portalContainer = new GameObject("--- PORTALS ---");

            // Portal de regreso a Zona Inicial
            CreatePortal("Portal_ToZonaInicial", new Vector3(0f, 2f, -95f), Color.blue, "Zona Inicial", portalContainer.transform);

            // Portal a Mazmorra 1.1
            CreatePortal("Portal_ToMazmorra1_1", new Vector3(90f, 2f, 0f), Color.red, "Mazmorra 1.1", portalContainer.transform);
        }

        private static void CreatePortal(string name, Vector3 position, Color color, string destination, Transform parent)
        {
            GameObject portal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            portal.name = $"{name} -> {destination}";
            portal.transform.parent = parent;
            portal.transform.position = position;
            portal.transform.localScale = new Vector3(4f, 5f, 0.5f);

            var renderer = portal.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));
            Color transparentColor = color;
            transparentColor.a = 0.6f;
            mat.color = transparentColor;
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;
            renderer.material = mat;
        }
    }
}
