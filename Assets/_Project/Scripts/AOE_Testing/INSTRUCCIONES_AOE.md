# Sistema AOE - Instrucciones de Uso

## ✅ ESTADO: FUNCIONANDO CON MOUSE TARGETING
Los scripts core del sistema AOE están implementados y funcionando correctamente.
**NUEVO**: El cono AOE ahora sigue la dirección del mouse para targeting dinámico!

## 🎮 CÓMO USAR EL SISTEMA AOE

### Opción 1: Usar Escena Existente (RECOMENDADO)

1. **Abre cualquier escena** (RegionInicio, Region1, etc.)
2. **Busca un GameObject con tag "Player"** o crea uno
3. **Agrega el componente** `AOE_MasterTestController` al player
4. **Presiona Play**
5. **Usa las teclas**:
   - **G** = Ground Targeting AOE (click en el suelo)
   - **R** = Player-Centered AOE (área alrededor del jugador)
   - **T** = Cone Attack AOE (NUEVO: sigue el mouse!)

### Opción 2: Crear Escena de Prueba Manual

1. **Crea nueva escena**
2. **Agrega un Plane** (suelo)
3. **Crea un Capsule** y ponle tag "Player"
4. **Agrega componente** `AOE_MasterTestController`
5. **Crea varios Capsules** con tag "Enemy" (rojos)
6. **Posiciona la cámara** arriba para ver bien
7. **Presiona Play y prueba G, R, T**

## 🔧 COMPONENTES PRINCIPALES

### `AOE_MasterTestController`
- Controlador principal que maneja los 3 tipos de AOE
- Se agrega al GameObject del jugador
- Maneja input (G, R, T) y coordina los sistemas

### Scripts Core (NO tocar)
- `AreaDetector.cs` - Detección de enemigos en áreas
- `AOEVisualIndicator.cs` - Indicadores visuales
- `GroundTargetingTest.cs` - Sistema de ground targeting
- `PlayerCenteredTest.cs` - AOE centrado en jugador  
- `ConeAttackTest.cs` - Ataques cónicos **CON MOUSE TARGETING**

## 🎯 CONTROLES ACTUALIZADOS

| Tecla | Función | Descripción |
|-------|---------|-------------|
| **G** | Ground AOE | Click en el suelo para seleccionar área |
| **R** | Player AOE | Área circular alrededor del jugador |
| **T** | Cone AOE | **NUEVO**: Mantén presionado T, mueve mouse para apuntar, suelta T para atacar |
| **ESC** | Cancelar | Cancela targeting activo |
| **Click Derecho** | Cancelar Cono | Cancela el aiming del cono |

## 🆕 NUEVO: CONE ATTACK CON MOUSE

### Cómo usar el Cone Attack mejorado:
1. **Mantén presionado T** - Entra en modo aiming
2. **Mueve el mouse** - El cono amarillo sigue tu cursor
3. **Suelta T** - Confirma el ataque en esa dirección
4. **Click derecho o ESC** - Cancela el aiming

### Características del nuevo sistema:
- ✅ **Preview en tiempo real** - Cono amarillo muestra dónde atacarás
- ✅ **Targeting preciso** - Apunta exactamente donde quieres
- ✅ **Cancelación fácil** - Click derecho para cancelar
- ✅ **Feedback visual** - Indicadores claros de aiming vs ataque

## 📊 RESULTADOS

- **Consola de Unity**: Muestra enemigos detectados
- **Indicadores visuales**: 
  - Verde/rojo para ataques confirmados
  - Amarillo para preview/aiming
- **Debug logs**: Información detallada de detección

## ⚠️ NOTAS IMPORTANTES

1. **Los enemigos deben tener tag "Enemy"** para ser detectados
2. **El suelo debe tener collider** para ground targeting y cone aiming
3. **Mira la consola** para ver resultados de detección
4. **Los indicadores visuales** son básicos (LineRenderer)
5. **El cone targeting requiere cámara** para convertir posición del mouse

## 🚫 SCRIPTS DESACTIVADOS

Los siguientes scripts están desactivados porque causaban crash:
- `AOE_AutoSceneSetup.cs` (auto-setup)
- `AOE_EditorSetup.cs` (editor auto-setup)

**Usa setup manual** como se describe arriba.

## ✅ VERIFICACIÓN

Para verificar que todo funciona:

1. Abre cualquier escena
2. Agrega `AOE_MasterTestController` a un player
3. Crea algunos enemigos con tag "Enemy"
4. Presiona Play
5. Prueba G, R, T (especialmente el nuevo T con mouse)
6. Verifica en consola que detecta enemigos

**Si ves logs de detección = ¡FUNCIONA!**

## 🎯 MEJORAS IMPLEMENTADAS

- ✅ **Cone Attack dinámico** - Sigue la dirección del mouse
- ✅ **Preview en tiempo real** - Muestra dónde atacarás antes de confirmar
- ✅ **Controles intuitivos** - Hold-to-aim, release-to-attack
- ✅ **Cancelación fácil** - Click derecho o ESC
- ✅ **Feedback visual mejorado** - Colores diferentes para preview vs ataque