# Cómo Usar AOE en Escena Existente

## Método Rápido: Usar Escena Existente

En lugar de crear una nueva escena, puedes convertir cualquier escena existente en una escena de prueba AOE.

### Pasos Simples:

#### 1. Abrir Escena Existente
Abre cualquiera de estas escenas en Unity:
- `RegionInicio.unity` (RECOMENDADO - más simple)
- `Region1.unity`
- `Mazmorra1_1.unity`

#### 2. Agregar el Convertidor AOE
1. En la escena abierta, crea un GameObject vacío
2. Nómbralo "AOE_Converter"
3. Agrega el componente `AOE_SceneConverter`
4. En el inspector, asegúrate que esté marcado "Setup On Start"

#### 3. Ejecutar la Escena
1. Presiona Play en Unity
2. El script automáticamente:
   - Encuentra el jugador existente
   - Agrega los componentes AOE
   - Crea enemigos de prueba
   - Configura la cámara

#### 4. Probar AOE
Una vez que la escena esté corriendo:
- **G** = Ground Targeting AOE (círculo rojo sigue el mouse)
- **R** = Player-Centered AOE (círculo azul alrededor del jugador)
- **T** = Cone Attack AOE (cono verde al frente del jugador)

### Método Manual (Si el automático no funciona):

#### 1. Encontrar el Jugador
Busca en la jerarquía objetos como:
- TestPlayer
- TestPlayerWithAbilities
- Player
- NetworkPlayer

#### 2. Agregar Componentes AOE
Al objeto del jugador, agrega:
- `AOE_MasterTestController`

#### 3. Crear Enemigos Manualmente
1. Crea Capsule primitives
2. Ponles tag "Enemy"
3. Aplica material rojo
4. Posiciona alrededor del jugador:
   - Cerca: (3,0,0), (-3,0,0), (0,0,3), (0,0,-3)
   - Medio: (6,0,6), (-6,0,6), (6,0,-6)
   - Lejos: (0,0,8), (2,0,10), (-2,0,10)

### Escenas Recomendadas:

**RegionInicio.unity** - La más simple, ideal para pruebas
**Region1.unity** - Más compleja, pero funcional
**Mazmorra1_1.unity** - Ambiente de dungeon

### Controles de Prueba:

| Tecla | Función |
|-------|---------|
| G | Ground Targeting - Mueve mouse, click para confirmar |
| R | Player AOE - Área alrededor del jugador |
| T | Cone Attack - Cono frontal del jugador |
| WASD | Mover jugador |
| Mouse | Rotar cámara/jugador |
| ESC/Click Der | Cancelar ground targeting |

### Verificación:

✅ **Funciona si ves:**
- Círculos/conos de colores cuando presionas G, R, T
- Mensajes en la consola mostrando enemigos detectados
- Enemigos cambian de color brevemente cuando son golpeados

❌ **Problemas comunes:**
- **No aparecen indicadores**: Verifica que el jugador tenga AOE_MasterTestController
- **No detecta enemigos**: Asegúrate que tengan tag "Enemy"
- **Ground targeting no funciona**: Verifica que haya un suelo con collider

### Consola de Debug:

Abre la consola de Unity (Window > General > Console) para ver:
```
[AreaDetector] Checking 8 enemies for circular AOE...
[AreaDetector] Enemy 'TestEnemy_1' detected in radius - Distance: 2.50
[PlayerCenteredTest] Enemies hit: 3
```

### Siguiente Paso:

Una vez que confirmes que funciona, puedes integrar el sistema AOE con el sistema de combate existente del juego.

---

**Creado**: 2026-01-20  
**Para**: Pruebas rápidas de AOE  
**Tiempo estimado**: 2-3 minutos de setup