---
inclusion: always
---

# MCP Servers Disponibles

Este proyecto tiene 2 MCP servers configurados para asistir el desarrollo.

## 1. Unity MCP (`unityMCP`)

Interacción directa con el Unity Editor.

### Cuándo Usar
- Manipular GameObjects y componentes
- Leer consola de Unity
- Ejecutar menús del Editor
- Obtener jerarquía de escena
- Play/Pause/Stop del juego

### Herramientas Principales
| Tool | Uso |
|------|-----|
| `manage_scene` | Cargar, guardar escenas, obtener jerarquía |
| `manage_gameobject` | CRUD de GameObjects |
| `find_gameobjects` | Buscar por nombre, tag, componente |
| `manage_components` | Agregar, modificar componentes |
| `read_console` | Leer logs de Unity |
| `manage_editor` | Play/Pause/Stop |

### Requisitos
- Unity debe estar abierto
- Servidor corre en `localhost:8080`

---

## 2. Smart Coding / Unity Docs (`unity-docs`)

Búsqueda semántica en documentación de Unity y código.

### Cuándo Usar
- Buscar en documentación de Unity
- Encontrar ejemplos de uso de APIs
- Buscar patrones en el código del proyecto
- Entender cómo funciona una clase/método de Unity

### Herramientas
| Tool | Uso |
|------|-----|
| `a_semantic_search` | Búsqueda semántica por significado |
| `b_index_codebase` | Indexar código para búsquedas |
| `c_clear_cache` | Limpiar caché de índices |

### Ejemplos de Búsqueda
```
# Buscar cómo usar NetworkTransform
a_semantic_search query="NetworkTransform synchronization position"

# Buscar sobre sistema de input
a_semantic_search query="Input System action map binding"

# Buscar sobre Cinemachine
a_semantic_search query="Cinemachine FreeLook camera collision"
```

---

## Prioridad de Uso

1. **Para operaciones en Unity Editor** → `unityMCP`
2. **Para buscar documentación/ejemplos** → `unity-docs`
3. **Para leer/escribir código** → Herramientas de archivo normales

## Troubleshooting

### Unity MCP no conecta
1. Verificar Unity abierto
2. `Window > MCP for Unity` en Unity
3. Reiniciar servidor si es necesario

### Búsqueda semántica no encuentra resultados
1. Verificar que el índice esté actualizado: `b_index_codebase`
2. Probar con términos más generales
