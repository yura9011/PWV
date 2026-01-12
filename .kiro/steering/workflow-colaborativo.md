---
inclusion: manual
---

# Flujo de Trabajo Colaborativo

## Comunicación entre Desarrolladores

### Archivos de Sincronización
- `SESSION_NOTES.md` - Actualizar al final de cada sesión con cambios realizados
- `TODO.md` - Marcar tareas completadas, agregar nuevas descubiertas
- `PROYECTO_ESTADO.md` - Actualizar cuando se complete un sistema o milestone

### Formato de Notas de Sesión
```markdown
## YYYY-MM-DD - [Nombre/Iniciales]

### Completado
- [x] Descripción de tarea completada
- [x] Otra tarea

### En Progreso
- [ ] Tarea iniciada pero no terminada

### Bloqueadores
- Descripción de problema que impide avanzar

### Notas para el Otro Dev
- Información importante que el otro desarrollador debe saber
```

## División de Trabajo Sugerida

### Por Módulo
Cada desarrollador puede "apropiarse" de módulos específicos:
- Dev A: Combat, Enemy, Progression
- Dev B: Network, UI, Persistence

### Por Feature
Alternativamente, dividir por features del MVP:
- Features 1-5: Dev A
- Features 6-10: Dev B

## Resolución de Conflictos

### Archivos Críticos (evitar editar simultáneamente)
- `GameManager.cs`
- `NetworkSessionManager.cs`
- Escenas principales

### Estrategia de Merge
1. Comunicar antes de editar archivos compartidos
2. Commits pequeños y frecuentes
3. Pull antes de empezar a trabajar
4. Resolver conflictos inmediatamente

## Uso de Kiro en Equipo

### Cada Desarrollador Debe
1. Tener MCP de Unity configurado localmente
2. Usar los mismos steering files (sincronizados via Git)
3. Actualizar documentación al hacer cambios significativos

### Skills Compartidos
Los skills en `.kiro/` están versionados y disponibles para ambos:
- `doc-coauthoring` - Para documentar features nuevas
- `docx` - Para documentación externa
- `skill-creator` - Para crear automatizaciones
