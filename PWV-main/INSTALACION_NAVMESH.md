# Instalación del Paquete AI Navigation

## Problema
Unity 6.3 LTS no incluye el sistema de Navigation por defecto. Necesitas instalarlo como paquete.

## Solución Automática (Recomendada)

1. **Usar el menú personalizado:**
   - En Unity, ir a `EtherDomes > Install Navigation Package`
   - Esperar a que aparezca el diálogo de confirmación
   - El paquete se instalará automáticamente

## Solución Manual (Si la automática falla)

1. **Abrir Package Manager:**
   - En Unity, ir a `Window > Package Manager`

2. **Agregar paquete por nombre:**
   - Hacer clic en el botón `+` (esquina superior izquierda)
   - Seleccionar `Add package by name...`
   - Escribir: `com.unity.ai.navigation`
   - Hacer clic en `Add`

3. **Esperar instalación:**
   - Unity descargará e instalará el paquete
   - Puede tomar unos minutos

## Verificar Instalación

Una vez instalado, deberías ver:

1. **Menú Navigation:**
   - `Window > AI > Navigation` disponible

2. **Navigation Static:**
   - Checkbox "Navigation Static" en el Inspector de GameObjects

3. **NavMesh Components:**
   - NavMeshSurface, NavMeshAgent, etc. disponibles

## Configurar NavMesh (Después de la instalación)

### Paso 1: Marcar Objetos como Navigation Static
1. Seleccionar todos los objetos de **piso/suelo** en la escena
2. En el Inspector, marcar **"Navigation Static"**
3. Hacer esto para todas las superficies caminables

### Paso 2: Bake NavMesh
1. Ir a `Window > AI > Navigation`
2. En la pestaña **"Bake"**
3. Ajustar configuraciones si es necesario:
   - Agent Radius: 0.5
   - Agent Height: 2.0
   - Max Slope: 45°
4. Hacer clic en **"Bake"**

### Paso 3: Verificar NavMesh
1. En la ventana Scene, deberías ver áreas **azules** que representan el NavMesh
2. Si no ves nada azul, revisa que los objetos estén marcados como Navigation Static

## Probar el Sistema

1. **Play** en Unity
2. Los enemigos ahora deberían:
   - Seguir al jugador inteligentemente
   - Rodear obstáculos automáticamente
   - No atravesar paredes
   - Mostrar paths azules en Scene view (si debug está activado)

## Troubleshooting

### "Failed to create agent because there is no valid NavMesh"
- **Causa:** NavMesh no está construido
- **Solución:** Seguir los pasos de "Configurar NavMesh" arriba

### No veo áreas azules después de Bake
- **Causa:** Objetos no marcados como Navigation Static
- **Solución:** Seleccionar pisos y marcar "Navigation Static"

### Enemigos no se mueven
- **Causa:** NavMeshAgent no configurado correctamente
- **Solución:** Verificar que cada enemigo tenga NavMeshAgent y SmartPathfinding3D

## Estado Actual del Sistema

✅ **SmartPathfinding3D** - Sistema de pathfinding implementado
✅ **NavMeshSetup** - Herramientas de configuración
✅ **TestEnemy** - Refactorizado para usar NavMesh
✅ **Componentes** - NavMeshAgent agregado a todos los enemigos
⚠️ **NavMesh Package** - Necesita instalación (este documento)
⚠️ **NavMesh Bake** - Necesita configuración manual

Una vez completados los pasos de este documento, el sistema de pathfinding inteligente estará completamente funcional.