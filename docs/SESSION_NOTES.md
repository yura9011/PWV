# Session Notes - The Ether Domes

## 2026-01-10 (Sesión 2) - Corrección de Bugs del Menú

### ✅ Completado

1. **Menu5 con 3 Pestañas**
   - Servidores Recientes (con botón X para eliminar + confirmación)
   - Servidores de Amigos (placeholder)
   - Servidores Públicos (placeholder con botón Actualizar)

2. **Sistema de Contraseña de Servidor**
   - `ServerPasswordHolder.cs` - Clase estática para compartir contraseña entre assemblies
   - `ConnectionApprovalManager` valida contraseña del cliente
   - Host guarda contraseña al iniciar servidor

3. **Servidores Recientes**
   - Se agregan automáticamente al conectarse como cliente
   - Botón X para eliminar con popup de confirmación
   - `LocalDataManager.DeleteRecentServer()` implementado

4. **Desconexión de Cliente**
   - Popup "Conexión perdida con el servidor" cuando el host cierra
   - Botón Aceptar lleva al Menu 5
   - Evento `OnClientStopped` para detectar desconexión

5. **Personaje del Host**
   - Host aplica personaje inmediatamente al iniciar servidor
   - Servidor dedicado NO aplica personaje (correcto)

6. **Música del Menú**
   - Delay de 2 segundos al entrar al menú principal
   - Otros menús inician música inmediatamente
   - Cambiada a `Mainmenutrackmusic.mp3`

7. **Video de Fondo**
   - `MenuBackground.cs` reproduce `fondobosquewallpaperliver.mp4` en loop
   - Fallback a imagen estática si video no carga
   - Requiere módulo Video habilitado en Built-in packages

### Archivos Creados/Modificados

| Archivo | Cambio |
|---------|--------|
| `Menu5_UnirsePartida.cs` | 3 pestañas, botón eliminar, popup confirmación |
| `GameStarter.cs` | Agregar a recientes, ServerPasswordHolder, personaje host |
| `ServerPasswordHolder.cs` | NUEVO - Compartir contraseña entre assemblies |
| `ConnectionApprovalManager.cs` | Usa ServerPasswordHolder |
| `LocalDataManager.cs` | Agregado DeleteRecentServer() |
| `MenuMusic.cs` | Delay 2s en menú principal, nueva música |
| `MenuBackground.cs` | Video de fondo con VideoPlayer |

### Bugs Pendientes de Verificar
- [ ] Contraseña de servidor funciona correctamente
- [ ] Cliente se agrega a servidores recientes
- [ ] Popup de desconexión aparece cuando host cierra
- [ ] Video de fondo se reproduce

---

## 2026-01-10 (Sesión 1) - Sistema de Menús Completo + Git

### ✅ Completado

1. **Sistema de Menús OnGUI Completo**
   - 5 menús implementados con navegación fluida
   - Menu 1: Principal (Empezar, Ajustes, Salir)
   - Menu 2: Forma de Juego (Crear, Unirse, Atrás)
   - Menu 3: Crear Partida (mundos locales, contraseña opcional)
   - Menu 4: Selección Personaje (12 max, preview capsule, sin clase inicial)
   - Menu 5: Unirse a Partida (recientes, amigos, públicos, código Relay)

2. **Persistencia Local**
   - `LocalDataManager.cs` - Gestión centralizada de datos
   - `WorldSaveData.cs` - Mundos del host (archivos locales)
   - `CharacterSaveData.cs` - Personajes globales (máx 12)
   - `RecentServerData.cs` - Últimos 10 servidores visitados

3. **Git Repository**
   - Inicializado repositorio local
   - Configurado remote: https://github.com/yura9011/PWV.git

---

## 2026-01-08 - Camera & Movement Polish

### ✅ Completado
- Cliente puede mover su personaje (ClientNetworkTransform)
- Zoom de cámara con rueda del ratón (0-16m)
- Colisión de cámara con suelo y obstáculos
- Menú de pausa (ESC)
- Relay Code en UI
- Controles estilo WoW completos

---

## Estado del Proyecto

### Sistemas Completados
- ✅ Sistema de Menús (5 menús OnGUI)
- ✅ Networking (Relay, Host/Client)
- ✅ Player Movement (estilo WoW)
- ✅ Camera System (tercera persona)
- ✅ Persistencia Local (mundos, personajes, recientes)
- ✅ Música y Video de fondo

### Pendiente de Testing
- [ ] Sistema de contraseña de servidor
- [ ] Desconexión de cliente (popup)
- [ ] Servidores recientes se guardan
- [ ] Video de fondo funciona

### Próximos Pasos
- Sistema de combate básico
- Tab targeting funcional
- Enemy AI movement
- Sistema de amigos (placeholder actual)
- Servidores públicos (placeholder actual)

---

## Archivos Clave Actualizados

```
Assets/_Project/Scripts/
├── Network/
│   ├── ServerPasswordHolder.cs     # NUEVO - Contraseña compartida
│   └── ConnectionApprovalManager.cs # Validación de contraseña
├── Data/
│   └── LocalDataManager.cs         # DeleteRecentServer()
└── UI/
    └── MainMenu/
        ├── GameStarter.cs          # Recientes, password, personaje
        ├── MenuMusic.cs            # Delay 2s, nueva música
        ├── MenuBackground.cs       # Video de fondo
        └── Menus/
            └── Menu5_UnirsePartida.cs # 3 pestañas
```
