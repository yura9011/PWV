# Unity Documentation Scraper

Documentación de Unity indexable con [smart-coding-mcp](https://github.com/omar-haris/smart-coding-mcp).

## Configuración para tu equipo

Cada miembro del equipo debe agregar esto a su `mcp.json`:

```json
{
  "mcpServers": {
    "unity-docs": {
      "command": "node",
      "args": ["C:/ruta/a/smart-coding-mcp/dist/index.js"],
      "env": {
        "WORKSPACE_PATH": "C:/ruta/a/unity-docs-scraper/unity_docs_json"
      }
    }
  }
}
```

Reemplazar las rutas con las rutas absolutas de cada máquina.

Al iniciar, el MCP indexará automáticamente los 280 archivos JSON.

## Contenido

```
unity-docs-scraper/
├── unity_docs/           # 📚 Documentación organizada (296 archivos)
│   ├── api/              # Scripting API (178 clases)
│   │   ├── core/         # GameObject, Transform, Component...
│   │   ├── physics/      # Physics, Rigidbody, Colliders...
│   │   ├── physics2d/    # Physics2D, Rigidbody2D...
│   │   ├── rendering/    # Camera, Light, Material, Shader...
│   │   ├── animation/    # Animator, Animation...
│   │   ├── audio/        # AudioSource, AudioClip...
│   │   ├── ui/           # Canvas, RectTransform, Button...
│   │   ├── input/        # Input, Touch, KeyCode...
│   │   ├── math/         # Vector2, Vector3, Quaternion...
│   │   ├── scene/        # SceneManager, Scene...
│   │   ├── 2d/           # Sprite, Tilemap, Grid...
│   │   ├── navigation/   # NavMesh, NavMeshAgent...
│   │   ├── networking/   # UnityWebRequest, WWW...
│   │   ├── terrain/      # Terrain, TerrainData
│   │   ├── video/        # VideoPlayer, VideoClip
│   │   ├── editor/       # Editor, EditorWindow...
│   │   └── utility/      # Time, Debug, PlayerPrefs...
│   │
│   └── manual/           # Unity Manual (118 páginas)
│       ├── getting_started/
│       ├── 2d/
│       ├── physics/
│       ├── animation/
│       ├── audio/
│       ├── ui/
│       ├── scripting/
│       ├── graphics/
│       ├── platforms/
│       └── optimization/
│
├── scripts/              # Scripts de scraping
│   ├── parse_toc.py      # Extrae lista de clases del TOC
│   ├── scraper.py        # Scraper principal de API
│   └── process_chunks.py # Procesa datos para MCP
│
├── data/                 # Datos JSON scrapeados
│
└── mcp-config.json       # Configuración MCP
```

## Cobertura

| Métrica | Valor |
|---------|-------|
| Clases API | 178 |
| Páginas Manual | 118 |
| Total archivos | 296 |
| **API Coverage** | 98.9% |
| **Manual Coverage** | 87.5% |
| **Overall Score** | 93.2% |

## Uso con MCP

```bash
# Navegar al directorio de documentación
cd unity_docs

# Inicializar MCP
mcp init

# Indexar documentación
mcp index

# Buscar
mcp search "how to detect collisions"
mcp search "load scene async"
mcp search "play audio clip"
```

## Re-scrapear

Si necesitas actualizar la documentación:

```bash
# 1. Extraer lista de clases
python scripts/parse_toc.py

# 2. Scrapear API
python scripts/scraper.py

# 3. Procesar para MCP
python scripts/process_chunks.py
```

## Notas

- Rate limiting: 2-4 segundos entre requests
- La documentación es propiedad de Unity Technologies
- Solo para uso personal/educativo
