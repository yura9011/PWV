# Guía de Desarrollo - The Ether Domes

## Para Nuevos Desarrolladores

### Configuración Inicial

1. **Clonar repositorio**
2. **Abrir con Unity 6.3 LTS**
3. **Crear layers** en `Edit > Project Settings > Tags and Layers`:
   - Layer 8: `Player`
   - Layer 9: `Enemy`
4. **Crear escena de prueba**: `Tools > EtherDomes > Setup Complete Test Scene`

---

## Convenciones de Código

### Namespaces
```csharp
namespace EtherDomes.Combat { }
namespace EtherDomes.Player { }
namespace EtherDomes.Enemy { }
// etc.
```

### Naming
- Clases: `PascalCase`
- Métodos: `PascalCase`
- Variables privadas: `_camelCase`
- Variables públicas: `PascalCase`
- Interfaces: `IPascalCase`

### Ejemplo
```csharp
namespace EtherDomes.Combat
{
    public class MiSistema : MonoBehaviour, IMiSistema
    {
        [SerializeField] private float _miVariable;
        
        public float MiPropiedad => _miVariable;
        
        public void MiMetodo() { }
    }
}
```

---

## Arquitectura de Sistemas

### GameManager
Contiene todos los sistemas como hijos:
```
GameManager
├── TargetSystem
├── AbilitySystem
├── AggroSystem
├── CombatSystem
├── ClassSystem
├── ProgressionSystem
├── LootSystem
├── EquipmentSystem
├── WorldManager
├── DungeonSystem
├── BossSystem
└── GuildBaseSystem
```

### Flujo de Datos de Combate
```
Input (1-9) → AbilitySystem → Validación → ExecuteAbility
                                              ↓
                                        OnAbilityExecuted
                                              ↓
                                    CombatTestUI.OnAbilityExecuted
                                              ↓
                                    Enemy.TakeDamage (ServerRpc)
                                              ↓
                                    NetworkVariable<Health> sync
```

---

## Cómo Agregar Nuevas Habilidades

### 1. Definir en AbilityData
```csharp
new AbilityData
{
    AbilityName = "Nueva Habilidad",
    Description = "Descripción",
    CastTime = 0f,        // 0 = instant
    Cooldown = 10f,
    Range = 5f,
    RequiresTarget = true,
    AffectedByGCD = true,
    BaseDamage = 50f,
    Type = AbilityType.Damage
}
```

### 2. Agregar al array de habilidades
En `CombatTestUI.CreateTestAbilities()` o en el sistema de clases.

---

## Cómo Agregar Nuevos Enemigos

### 1. Crear prefab
Duplicar `Assets/_Project/Prefabs/Enemies/BasicEnemy.prefab`

### 2. Configurar componente Enemy
- `_displayName`: Nombre visible
- `_level`: Nivel
- `_maxHealth`: Vida máxima

### 3. Agregar a escena
El enemigo se registra automáticamente con `TargetSystem` al spawnear.

---

## Cómo Agregar Nueva UI

### Para UI de Debug (OnGUI)
```csharp
namespace EtherDomes.UI.Debug
{
    public class MiDebugUI : MonoBehaviour
    {
        private void OnGUI()
        {
            // Usar UnityEngine.Debug.Log, no Debug.Log
        }
    }
}
```

### Para UI de Producción (Canvas)
Crear en `Assets/_Project/Scripts/UI/` con namespace `EtherDomes.UI`

---

## Testing

### Escena de Prueba
`Tools > EtherDomes > Setup Complete Test Scene`

### Probar Multiplayer Local
1. Build del proyecto
2. Ejecutar build como Host
3. En Editor, click "Join as Client"

### Logs Importantes
- `[TargetSystem]` - Sistema de targeting
- `[AbilitySystem]` - Sistema de habilidades
- `[CombatTestUI]` - UI de combate
- `[Enemy]` - Entidades enemigas
- `[NetworkSessionManager]` - Networking

---

## Áreas de Trabajo Disponibles

### Sin Asignar
- [ ] Sistema de clases completo
- [ ] IA de enemigos
- [ ] Sistema de dungeons
- [ ] UI de menú principal
- [ ] Sistema de inventario
- [ ] Efectos visuales

### En Progreso
- [x] Combat básico (Tab-target + habilidades)
- [x] Networking básico (Host/Client)

---

## Resolución de Problemas

### "Input no funciona"
1. Verificar que `AbilitySystem` está en la escena
2. Verificar que `CombatTestUI` está en la escena
3. Revisar consola por errores de inicialización

### "Daño no se aplica"
1. Verificar que estás en modo Host (no solo Client)
2. El daño se procesa en servidor via ServerRpc

### "Enemigos no aparecen en Tab"
1. Verificar que tienen componente `Enemy`
2. Verificar que tienen `NetworkObject`
3. Revisar logs de `[TargetSystem] Registered target:`

### "Namespace Debug no existe"
En archivos bajo `EtherDomes.UI.Debug`, usar `UnityEngine.Debug.Log`
