# Prompt para Nueva Sesión - The Ether Domes

## Estado Actual del Proyecto (16 Enero 2026)

**The Ether Domes** es un Micro-MMORPG cooperativo desarrollado en Unity 6.3 LTS con Netcode for GameObjects.

### ✅ Sistemas Completados

**Sistema de Testing Offline Completo:**
- TestPlayer con controles WoW funcionando en todas las escenas
- SimpleThirdPersonCamera siguiendo correctamente al player
- Sistema de portales ScenePortal conectando todas las escenas
- 6 escenas configuradas: RegionInicio, Region1, Mazmorra1_1, 1_2, 1_3, 1_4

**Navegación Entre Escenas:**
- RegionInicio → Region1 (portal funcional)
- Region1 → Mazmorra1_1, 1_2, 1_3, 1_4 (4 portales configurados)
- Todas las mazmorras → Region1 (portales de regreso)

**Controles Estilo WoW:**
- WASD: Movimiento y rotación
- Tab: Targeting de enemigos
- 1-2: Ataques básico y pesado
- Click derecho: Control de cámara
- Portales: Navegación automática entre escenas

### 🔄 En Progreso - Fase 3: Sistema de Combate

**Pendiente de Refinamiento:**
- Mejorar feedback visual de ataques
- Agregar efectos de sonido
- Floating Combat Text en escenas de test
- Target Frame UI mejorado
- Integrar sistema de combate con NetworkPlayer para modo online

### 📁 Archivos Clave

```
Assets/_Project/Scripts/
├── Testing/
│   ├── TestPlayer.cs          # Jugador offline completo
│   └── TestEnemy.cs           # Enemigos con IA básica
├── Camera/
│   └── SimpleThirdPersonCamera.cs  # Cámara funcionando
├── World/
│   └── ScenePortal.cs         # Sistema de portales
└── Combat/
    ├── CombatManager.cs       # Sistema de combate
    └── TargetingSystem.cs     # Targeting Tab/Click

Assets/_Project/Scenes/
├── Regions/
│   ├── RegionInicio.unity     # Región inicial
│   └── Region1.unity          # Región principal
└── Dungeons/
    ├── Mazmorra1_1.unity      # Dungeons configurados
    ├── Mazmorra1_2.unity
    ├── Mazmorra1_3.unity
    └── Mazmorra1_4.unity
```

### 🎮 Cómo Probar

1. Abrir cualquier escena (RegionInicio recomendado)
2. Play en Unity
3. WASD para moverse, Tab para targeting, 1-2 para atacar
4. Caminar hacia portales para cambiar de escena

### 📋 Próximas Prioridades

1. **Efectos Visuales**: Mejorar feedback de ataques
2. **Audio**: Sonidos de combate y ambiente
3. **UI**: Target Frame y Combat Text mejorados
4. **Integración Online**: Conectar TestPlayer con NetworkPlayer

### 🛠️ Herramientas Disponibles

- **MCP Unity**: Para manipular GameObjects y escenas
- **MCP Unity Docs**: Para búsqueda semántica en documentación
- **Logs**: Crear en `logs/[sistema]/` al completar trabajo

---

## Instrucciones para Kiro

Al iniciar nueva sesión:

1. **Leer contexto**: `docs/PROYECTO_ESTADO.md`, `docs/TODO.md`
2. **Verificar Unity abierto**: MCP requiere Editor activo
3. **Probar sistema actual**: Cargar una escena y verificar funcionamiento
4. **Identificar siguiente tarea**: Basado en prioridades en TODO.md
5. **Documentar trabajo**: Crear logs al completar tareas significativas

**Estado**: Sistema de testing offline completamente funcional. Listo para refinamientos y mejoras visuales/audio.