# The Ether Domes - Tareas Pendientes

## Prioridad Baja (No urgente)

### Migración Input System
- [ ] Migrar de Input Manager (legacy) al nuevo Input System package
- Razón: Unity marcó Input Manager como deprecado
- El código actual usa `Input.GetAxis`, `Input.GetKey`, etc.
- Archivos afectados: `PlayerController.cs` y otros que usen input
- Documentación: https://docs.unity3d.com/Packages/com.unity.inputsystem@latest

---

## Notas
- El proyecto compila correctamente en Unity 6.3 LTS
- Netcode for GameObjects versión 1.8.1
