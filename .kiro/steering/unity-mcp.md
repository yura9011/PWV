---
inclusion: always
---

# Unity MCP Integration

Este proyecto usa **MCP for Unity** para permitir que Kiro interactúe directamente con el Unity Editor.

## Configuración

### Paquete Unity
El paquete está instalado via git URL en `Packages/manifest.json`:
```json
"com.coplay.mcp": "https://github.com/CoplayDev/unity-mcp.git"
```

### Configuración MCP (Kiro)
Ubicación: `~/.kiro/settings/mcp.json`
```json
"unityMCP": {
  "url": "http://localhost:8080/mcp",
  "disabled": false,
  "autoApprove": [
    "manage_editor",
    "manage_scene", 
    "execute_menu_item",
    "read_console",
    "find_gameobjects",
    "manage_components",
    "manage_gameobject"
  ]
}
```

## Herramientas Disponibles

### Escenas y GameObjects
- `manage_scene` - Cargar, guardar, crear escenas, obtener jerarquía, screenshots
- `manage_gameobject` - CRUD de GameObjects, duplicar, mover
- `find_gameobjects` - Buscar por nombre, tag, layer, componente, path o ID
- `manage_components` - Agregar, remover, modificar componentes

### Assets y Scripts
- `manage_asset` - Importar, crear, modificar, eliminar, buscar assets
- `manage_script` - Crear, leer, eliminar scripts C#
- `script_apply_edits` - Ediciones estructuradas de métodos/clases
- `apply_text_edits` - Ediciones precisas por línea/columna

### Editor
- `manage_editor` - Play/Pause/Stop, herramientas, tags, layers
- `execute_menu_item` - Ejecutar items del menú de Unity
- `read_console` - Leer/limpiar consola de Unity
- `refresh_unity` - Refrescar AssetDatabase y compilar

### Materiales y VFX
- `manage_material` - Crear materiales, asignar shaders, colores
- `manage_vfx` - Particle systems, line/trail renderers

### Testing
- `run_tests` - Ejecutar tests EditMode/PlayMode
- `get_test_job` - Obtener resultados de tests

## Uso Típico

### Ver estado del juego
```
mcp_unityMCP_manage_scene action="get_active"
mcp_unityMCP_read_console count=10
```

### Buscar GameObjects
```
mcp_unityMCP_find_gameobjects search_term="Player" search_method="by_name"
mcp_unityMCP_find_gameobjects search_term="Enemy" search_method="by_tag"
```

### Modificar componentes
```
mcp_unityMCP_manage_components action="set_property" target="Player" component_type="Transform" property="position" value=[0,1,0]
```

### Ejecutar menú
```
mcp_unityMCP_execute_menu_item menu_path="EtherDomes/Fix Enemy Components"
```

## Troubleshooting

### MCP no conecta
1. Verificar que Unity esté abierto
2. En Unity: `Window > MCP for Unity` para ver estado
3. El servidor corre en `localhost:8080`

### Refrescar conexión
- En Unity: `Ctrl+R` para domain reload
- O reiniciar el servidor desde `Window > MCP for Unity`

## Notas
- El MCP requiere que Unity esté en modo Editor (no en Play Mode para algunas operaciones)
- Las operaciones de assets requieren que el proyecto no esté compilando
- Screenshots se guardan en la carpeta del proyecto
