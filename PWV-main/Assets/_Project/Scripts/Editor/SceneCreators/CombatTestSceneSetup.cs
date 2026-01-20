using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace EtherDomes.Editor
{
    /// <summary>
    /// Configura la escena Mazmorra1_1 con el sistema de combate Phase 3 para testing.
    /// Incluye: Player con controles, cámara, enemigos de prueba, y UI de combate.
    /// </summary>
    public static class CombatTestSceneSetup
    {
        private const string SCENE_PATH = "Assets/_Project/Scenes/Dungeons/Mazmorra1_1.unity";
        
        [MenuItem("Tools/EtherDomes/Setup Combat Test Scene")]
        public static void SetupCombatTestScene()
        {
            if (!EditorUtility.DisplayDialog("Setup Combat Test Scene",
                "¿Configurar Mazmorra1_1 para testing del sistema de combate Phase 3?\n\n" +
                "Esto agregará:\n" +
                "- TestPlayer con controles WASD\n" +
                "- Cámara top-down\n" +
                "- Enemigos de prueba\n" +
                "- Sistema de targeting\n" +
                "- UI de combate",
                "Configurar", "Cancelar"))
            {
                return;
            }

            // Cargar o crear la escena
            var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
            
            // Limpiar objetos de test anteriores
            CleanupPreviousTestObjects();
            
            // Crear estructura de test
            CreateTestPlayer();
            CreateTestCamera();
            CreateTestEnemies();
            CreateCombatUI();
            CreateEventSystem();
            
            // Guardar escena
            EditorSceneManager.SaveScene(scene);
            
            Debug.Log("[CombatTestSceneSetup] Escena configurada para testing de combate Phase 3");
            EditorUtility.DisplayDialog("Éxito", 
                "Escena configurada para testing.\n\n" +
                "Controles:\n" +
                "- WASD: Movimiento\n" +
                "- Tab: Ciclar targets\n" +
                "- Click: Seleccionar target\n" +
                "- Space/1: Ataque básico\n" +
                "- 2: Ataque pesado\n" +
                "- Escape: Limpiar target", "OK");
        }

        private static void CleanupPreviousTestObjects()
        {
            // Eliminar objetos de test anteriores
            var toDestroy = new string[] { "TestPlayer", "TestCamera", "--- TEST ENEMIES ---", "CombatUI", "EventSystem" };
            foreach (var name in toDestroy)
            {
                var obj = GameObject.Find(name);
                if (obj != null)
                    Object.DestroyImmediate(obj);
            }
        }

        private static void CreateTestPlayer()
        {
            // Crear player
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "TestPlayer";
            player.transform.position = new Vector3(0f, 1f, -5f);
            player.layer = LayerMask.NameToLayer("Player");
            
            // Material verde
            var renderer = player.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.2f, 0.8f, 0.3f);
            renderer.material = mat;
            
            // CharacterController
            Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
            var cc = player.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.5f;
            cc.center = new Vector3(0f, 0f, 0f);
            
            // OfflinePlayerController script
            player.AddComponent<EtherDomes.Testing.OfflinePlayerController>();
            
            Debug.Log("[CombatTestSceneSetup] TestPlayer creado");
        }

        private static void CreateTestCamera()
        {
            // Buscar cámara existente o crear nueva
            UnityEngine.Camera existingCam = UnityEngine.Camera.main;
            GameObject camObj;
            
            if (existingCam != null)
            {
                camObj = existingCam.gameObject;
                camObj.name = "TestCamera";
            }
            else
            {
                camObj = new GameObject("TestCamera");
                camObj.AddComponent<UnityEngine.Camera>();
                camObj.AddComponent<AudioListener>();
                camObj.tag = "MainCamera";
            }
            
            // Posición inicial top-down
            camObj.transform.position = new Vector3(0f, 15f, -14f);
            camObj.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
            
            Debug.Log("[CombatTestSceneSetup] TestCamera configurada");
        }

        private static void CreateTestEnemies()
        {
            GameObject container = new GameObject("--- TEST ENEMIES ---");
            
            // Enemigos en diferentes posiciones de la dungeon
            // Sala de entrada - 2 enemigos fáciles
            CreateEnemy("Skeleton_1", new Vector3(-3f, 0.5f, 5f), 200f, 1, container.transform);
            CreateEnemy("Skeleton_2", new Vector3(3f, 0.5f, 5f), 200f, 1, container.transform);
            
            // Sala 1 - 3 enemigos
            CreateEnemy("Ghoul_1", new Vector3(-5f, 0.5f, 30f), 350f, 2, container.transform);
            CreateEnemy("Ghoul_2", new Vector3(5f, 0.5f, 30f), 350f, 2, container.transform);
            CreateEnemy("Ghoul_3", new Vector3(0f, 0.5f, 35f), 400f, 3, container.transform);
            
            // Sala 2 - 4 enemigos
            CreateEnemy("Wraith_1", new Vector3(-5f, 0.5f, 65f), 450f, 3, container.transform);
            CreateEnemy("Wraith_2", new Vector3(5f, 0.5f, 65f), 450f, 3, container.transform);
            CreateEnemy("Wraith_3", new Vector3(0f, 0.5f, 70f), 500f, 4, container.transform);
            CreateEnemy("Wraith_Elite", new Vector3(0f, 0.5f, 60f), 800f, 5, container.transform);
            
            // Sala 3 - 5 enemigos
            CreateEnemy("Revenant_1", new Vector3(-7f, 0.5f, 100f), 550f, 4, container.transform);
            CreateEnemy("Revenant_2", new Vector3(7f, 0.5f, 100f), 550f, 4, container.transform);
            CreateEnemy("Revenant_3", new Vector3(-3f, 0.5f, 105f), 600f, 5, container.transform);
            CreateEnemy("Revenant_4", new Vector3(3f, 0.5f, 105f), 600f, 5, container.transform);
            CreateEnemy("Revenant_Champion", new Vector3(0f, 0.5f, 95f), 1200f, 6, container.transform);
            
            // Boss Room - 1 boss
            CreateBoss("Crypt_Lord", new Vector3(0f, 1.5f, 150f), 5000f, 10, container.transform);
            
            Debug.Log("[CombatTestSceneSetup] 15 enemigos de prueba creados");
        }

        private static void CreateEnemy(string name, Vector3 position, float health, int level, Transform parent)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            enemy.name = name;
            enemy.transform.parent = parent;
            enemy.transform.position = position;
            enemy.transform.localScale = new Vector3(1f, 2f, 1f);
            enemy.tag = "Enemy";
            
            // Material rojo
            var renderer = enemy.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.red;
            renderer.material = mat;
            
            // TestEnemy script
            var testEnemy = enemy.AddComponent<EtherDomes.Testing.TestEnemy>();
            
            // Configurar via SerializedObject
            var so = new SerializedObject(testEnemy);
            so.FindProperty("_displayName").stringValue = name.Replace("_", " ");
            so.FindProperty("_level").intValue = level;
            so.FindProperty("_maxHealth").floatValue = health;
            so.FindProperty("_aggroRange").floatValue = 12f;
            so.FindProperty("_attackRange").floatValue = 2.5f;
            so.FindProperty("_moveSpeed").floatValue = 3f;
            so.FindProperty("_attackCooldown").floatValue = 2f;
            so.FindProperty("_damage").floatValue = 15f + (level * 5f);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateBoss(string name, Vector3 position, float health, int level, Transform parent)
        {
            GameObject boss = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boss.name = name;
            boss.transform.parent = parent;
            boss.transform.position = position;
            boss.transform.localScale = new Vector3(3f, 4f, 3f);
            boss.tag = "Enemy";
            
            // Material rojo oscuro
            var renderer = boss.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.6f, 0.1f, 0.1f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.3f, 0f, 0f));
            renderer.material = mat;
            
            // TestEnemy script con stats de boss
            var testEnemy = boss.AddComponent<EtherDomes.Testing.TestEnemy>();
            
            var so = new SerializedObject(testEnemy);
            so.FindProperty("_displayName").stringValue = name.Replace("_", " ");
            so.FindProperty("_level").intValue = level;
            so.FindProperty("_maxHealth").floatValue = health;
            so.FindProperty("_aggroRange").floatValue = 20f;
            so.FindProperty("_attackRange").floatValue = 4f;
            so.FindProperty("_moveSpeed").floatValue = 2f;
            so.FindProperty("_attackCooldown").floatValue = 3f;
            so.FindProperty("_damage").floatValue = 80f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateCombatUI()
        {
            // Canvas para UI de combate
            GameObject canvasObj = new GameObject("CombatUI");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Placeholder para Target Frame (esquina superior izquierda)
            CreateUIPlaceholder("TargetFrame_Placeholder", canvasObj.transform, 
                new Vector2(150, -80), new Vector2(280, 100), "Target Frame\n(Tab to target)");
            
            // Placeholder para Cast Bar (centro inferior)
            CreateUIPlaceholder("CastBar_Placeholder", canvasObj.transform,
                new Vector2(0, 100), new Vector2(300, 30), "Cast Bar");
            
            // Placeholder para Action Bar (centro inferior)
            CreateUIPlaceholder("ActionBar_Placeholder", canvasObj.transform,
                new Vector2(0, 50), new Vector2(400, 50), "1: Attack | 2: Heavy | Space: Attack");
            
            Debug.Log("[CombatTestSceneSetup] Combat UI placeholders creados");
        }

        private static void CreateUIPlaceholder(string name, Transform parent, Vector2 anchoredPos, Vector2 size, string text)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;
            
            // Ajustar anchor para TargetFrame
            if (name.Contains("Target"))
            {
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
            }
            
            // Background
            var image = obj.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0f, 0f, 0f, 0.5f);
            
            // Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var textComp = textObj.AddComponent<UnityEngine.UI.Text>();
            textComp.text = text;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComp.fontSize = 14;
            textComp.color = Color.white;
            textComp.alignment = TextAnchor.MiddleCenter;
        }

        private static void CreateEventSystem()
        {
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null)
                return;
                
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            
            Debug.Log("[CombatTestSceneSetup] EventSystem creado");
        }
    }
}
