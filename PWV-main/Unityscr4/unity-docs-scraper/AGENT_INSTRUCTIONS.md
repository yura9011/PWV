# Unity Documentation - Instrucciones para Agentes

Este repositorio contiene documentación de Unity indexada para búsqueda semántica via MCP.

## Configuración

Agregar a tu `mcp.json`:

```json
{
  "mcpServers": {
    "unity-docs": {
      "command": "node",
      "args": ["C:/ruta/a/smart-coding-mcp/dist/inde.js"],
      "env": {
        "WORKSPACE_PATH": "C:/ruta/a/unity-docs-scraper/unity_docs_json"
      }
    }
  }
}
```

Reemplazar rutas con las absolutas de tu máquina.

## Uso

### Herramientas disponibles

| Tool | Descripción |
|------|-------------|
| `a_semantic_search` | Buscar en la documentación por significado |
| `b_index_codebase` | Re-indexar si hay cambios |
| `c_clear_cache` | Limpiar cache de embeddings |

### Búsquedas recomendadas

```
# API - Buscar clases y métodos
"GameObject instantiate destroy"
"Rigidbody velocity force"
"Physics raycast collision"
"AudioSource play clip"
"SceneManager load scene async"
"Transform position rotation"
"Input GetKey GetAxis"
"Animator SetTrigger parameters"

# Manual - Buscar conceptos y guías
"2D physics colliders"
"animation blend trees"
"UI Canvas setup"
"optimization performance tips"
"mobile platform build"
```

### Estructura del contenido

Cada archivo JSON contiene:

```json
{
  "class": "GameObject",
  "category": "core",
  "type": "api",
  "url": "https://docs.unity3d.com/ScriptReference/GameObject.html",
  "content": "... documentación completa ..."
}
```

## Categorías API (178 clases)

- `core`: GameObject, Transform, MonoBehaviour, Component, ScriptableObject
- `physics`: Physics, Rigidbody, Collider, RaycastHit, Joint
- `physics2d`: Physics2D, Rigidbody2D, Collider2D
- `rendering`: Camera, Light, Material, Mesh, Shader, Renderer
- `animation`: Animator, Animation, AnimationClip
- `audio`: AudioSource, AudioClip, AudioMixer
- `ui`: Canvas, Button, Image, InputField, TMP_Text
- `input`: Input, Touch, KeyCode
- `math`: Vector2, Vector3, Quaternion, Mathf, Color
- `scene`: SceneManager, Scene
- `2d`: Sprite, Tilemap, SpriteRenderer
- `navigation`: NavMesh, NavMeshAgent
- `networking`: UnityWebRequest, WWW
- `utility`: Time, Debug, PlayerPrefs, Resources

## Categorías Manual (118 páginas)

- `getting_started`: Introducción, glosario
- `2d`: Desarrollo 2D, sprites, tilemaps
- `physics`: Física 3D y 2D
- `animation`: Sistema de animación, Mecanim
- `audio`: Sistema de audio
- `ui`: UI Toolkit, IMGUI, Canvas
- `scripting`: Programación en Unity
- `graphics`: Rendering, shaders, lighting
- `platforms`: Android, iOS, WebGL
- `optimization`: Performance, profiling, memoria

## Tips para agentes

1. Usar búsqueda semántica para encontrar documentación relevante antes de escribir código Unity
2. Combinar términos de clase + método para resultados más precisos
3. Para conceptos generales, buscar en el manual
4. Para API específica, buscar nombre de clase
5. Los resultados incluyen URL oficial para referencia adicional
