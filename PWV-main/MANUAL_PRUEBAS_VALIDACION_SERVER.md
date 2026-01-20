# Manual de Pruebas - Validación Server-Side

## Descripción
Este manual te guía para probar manualmente el sistema de validación server-side que detecta trampas como "99999 de fuerza" y otros valores imposibles.

## Configuración Inicial

### 1. Preparar la Escena
1. Abrir Unity y cargar cualquier escena del juego
2. Asegurarse de que existe un GameObject con `ConnectionApprovalManager`
3. Crear un GameObject vacío y agregar el componente `ServerValidationTester`

### 2. Configurar el Tester
En el componente `ServerValidationTester`:
- ✅ Marcar `Run Tests On Start` si quieres pruebas automáticas
- ✅ Configurar qué tests ejecutar (todos marcados por defecto)

## Pruebas Automáticas

### Ejecutar Todas las Pruebas
1. **Método 1 - Automático**: Marcar `Run Tests On Start` y hacer Play
2. **Método 2 - Manual**: Click derecho en el componente → `Run All Validation Tests`

### Resultados Esperados
```
=== SERVER VALIDATION TESTS STARTING ===
✅ Valid character approved correctly
✅ Cheat stats detected and rejected: Invalid Strength: 99999 (must be 0-10000)
✅ Invalid level detected and rejected: Invalid level: 999 (must be 1-60)
✅ Invalid items detected and rejected: Too many equipped items
✅ Corrupted data detected and rejected: Failed to decrypt character data
✅ Empty payload detected and rejected: Empty payload received
=== SERVER VALIDATION TESTS COMPLETED ===
```

## Pruebas Manuales Específicas

### 1. Probar Personaje con Stats Hackeados
```
Click derecho → "Test Cheat Stats (99999 Strength)"
```
**Resultado esperado**: ❌ REJECTED - "Invalid Strength: 99999"

### 2. Probar Personaje Válido
```
Click derecho → "Test Valid Character"
```
**Resultado esperado**: ✅ APPROVED

### 3. Crear Personaje Trampa Personalizado
```
Click derecho → "Create Test Character with Cheats"
```
**Verás en consola**:
```
Cheat Character Created:
  Name: ObviousCheat
  Level: 999
  Strength: 99999
  HP: 999999/999999
  Mana: 888888/888888
Validation Result: REJECTED
Rejection Reason: Invalid level: 999 (must be 1-60)
```

### 4. Simular Conexiones de Red
```
Click derecho → "Simulate Network Connection Test"
```
**Verás múltiples escenarios**:
- ✅ APPROVED - Legitimate Player
- ❌ REJECTED - Speed Hacker
- ❌ REJECTED - Stat Hacker
- ❌ REJECTED - Item Duper
- ❌ REJECTED - Level Hacker

## Pruebas de Integración con NetworkManager

### Configurar Servidor de Prueba
1. En `ConnectionApprovalManager`:
   - ✅ Desmarcar `Skip Authentication For Testing`
   - ✅ Desmarcar `Skip Stats Validation`
   - Asignar `Class Definitions` y `Item Database` si están disponibles

### Probar Conexión Real
1. **Host**: Iniciar como Host en Unity
2. **Cliente**: Abrir segunda instancia de Unity
3. **Modificar datos**: Usar herramientas externas para modificar el archivo de guardado
4. **Conectar**: El cliente modificado debería ser rechazado

## Modificar Archivo de Guardado para Pruebas

### Ubicación del Archivo
```
Windows: C:/Users/[Usuario]/AppData/LocalLow/DefaultCompany/TheEtherDomes/etherdomes_save.ted
```

### ⚠️ ADVERTENCIA
El archivo está encriptado con AES-256. Para modificarlo necesitarías:
1. Desencriptar el archivo
2. Modificar el JSON
3. Re-encriptar con la misma clave
4. Esto es **intencionalmente difícil** para prevenir trampas

### Alternativa: Usar SaveSystemTester
1. Usar `SaveSystemTester` → `Create Character`
2. Modificar stats directamente en código
3. Guardar con `Save()`
4. Intentar conectar

## Escenarios de Prueba Específicos

### Escenario 1: Guerrero con 99999 de Fuerza
```csharp
// En código de prueba
character.TotalStrength = 99999;
character.TotalAttackPower = 50000;
character.MaxHP = 999999;
```
**Resultado**: ❌ REJECTED - "Invalid Strength: 99999 (must be 0-10000)"

### Escenario 2: Mago Nivel 999
```csharp
character.Level = 999;
character.TotalIntellect = 75000;
character.MaxMana = 888888;
```
**Resultado**: ❌ REJECTED - "Invalid level: 999 (must be 1-60)"

### Escenario 3: Personaje con 50 Items Equipados
```csharp
for (int i = 0; i < 50; i++)
{
    character.EquippedItemIDs.Add($"legendary_item_{i}");
}
```
**Resultado**: ❌ REJECTED - "Too many equipped items"

### Escenario 4: HP/Mana Imposibles
```csharp
character.CurrentHP = 999999;
character.CurrentMana = 888888;
character.MaxHP = 100; // Menor que current
character.MaxMana = 50; // Menor que current
```
**Resultado**: ❌ REJECTED - "CurrentHP exceeds MaxHP"

## Verificar Logs de Seguridad

### En la Consola de Unity
Buscar estos mensajes:
```
[ConnectionApprovalManager] Validation found X discrepancies:
  - Strength: client=99999, expected=25
  - MaxHP: client=999999, expected=400
  - CurrentHP exceeds MaxHP: 999999 > 400

[ConnectionApprovalManager] Character data was sanitized for client 1
```

### Logs de Conexión
```
[ConnectionApprovalManager] Validating connection from ClientID: 1
[ConnectionApprovalManager] Connection rejected: Invalid Strength: 99999
```

## Límites de Validación Actuales

### Stats
- **Mínimo**: 0
- **Máximo**: 10,000
- **Nivel**: 1-60
- **Items Equipados**: Máximo 20

### Para Ver Límites Actuales
```
Click derecho → "Show Validation Limits"
```

## Personalizar Límites de Validación

### En ConnectionApprovalHandler
```csharp
_approvalHandler.SetStatRanges(0, 5000); // Cambiar límites
```

### En ConnectionApprovalManager
Modificar las constantes:
```csharp
public const int DEFAULT_MAX_STAT = 5000; // Cambiar aquí
public const int DEFAULT_MAX_LEVEL = 100; // Cambiar aquí
```

## Troubleshooting

### ❌ "SaveManager instance not found"
**Solución**: Agregar GameObject con componente `SaveManager` a la escena

### ❌ "ConnectionApprovalManager not found"
**Solución**: Agregar GameObject con componente `ConnectionApprovalManager` a la escena

### ❌ "Encryption service not initialized"
**Solución**: El servicio se inicializa automáticamente, verificar logs de error

### ❌ Tests no se ejecutan
**Solución**: Verificar que `Run Tests On Start` esté marcado o usar menú contextual

## Interpretación de Resultados

### ✅ APPROVED
- El personaje pasó todas las validaciones
- Stats están dentro de rangos válidos
- Nivel es válido (1-60)
- Items equipados son razonables

### ❌ REJECTED
- Se detectó al menos una anomalía
- El `RejectionReason` indica qué falló
- El `ErrorCode` categoriza el tipo de error

### Códigos de Error
- `InvalidDataFormat`: Datos corruptos o mal formateados
- `CorruptedData`: No se pudo desencriptar
- `StatsOutOfRange`: Stats fuera de límites válidos

## Conclusión
Este sistema detecta efectivamente:
- ✅ Stats imposibles (99999 de fuerza)
- ✅ Niveles inválidos
- ✅ HP/Mana imposibles
- ✅ Demasiados items equipados
- ✅ Datos corruptos o manipulados
- ✅ Payloads vacíos o inválidos

El sistema es robusto y previene la mayoría de trampas comunes en MMORPGs.