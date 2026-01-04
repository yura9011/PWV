# Implementation Plan: Network Player Foundation

## Overview

Plan de implementación incremental para el sistema fundacional de red y movimiento de jugadores usando **Mirror Networking**. Cada tarea construye sobre la anterior, comenzando con la estructura del proyecto y terminando con la integración completa. El objetivo final es tener dos cápsulas moviéndose en red.

**Nota:** Este proyecto ya tiene Mirror 96.0.1 instalado y código de red existente (SERVERNetworkManager, Entity, Player). Las tareas se integrarán con la arquitectura existente.

## Tasks

- [x] 1. Configurar estructura del proyecto y dependencias
  - Crear estructura de carpetas: `Assets/_Project/Scripts/Network`, `Assets/_Project/Scripts/Player`, `Assets/_Project/Scripts/Persistence`, `Assets/_Project/Scripts/Input`, `Assets/_Project/ScriptableObjects`, `Assets/_Project/Scenes`, `Assets/_Project/Prefabs/Characters`
  - Crear archivo `.asmdef` para cada módulo (Network, Player, Persistence, Input, Tests)
  - Configurar paquete FsCheck para property-based testing via NuGet
  - _Requirements: Setup técnico base_

- [x] 2. Implementar NetworkSessionManager
  - [x] 2.1 Crear interfaz INetworkSessionManager y clase NetworkSessionManager
    - Implementar wrapper sobre Mirror NetworkManager (extender de existente SERVERNetworkManager)
    - Implementar StartHost(), StartClient(), StartServer(), StopHost(), StopClient(), StopServer()
    - Implementar eventos usando Mirror callbacks: OnServerConnect, OnServerDisconnect, OnClientConnect, OnClientDisconnect
    - Implementar propiedades IsHost, IsClient, IsServer usando NetworkServer.active y NetworkClient.active
    - Implementar ConnectedPlayerCount usando NetworkServer.connections.Count
    - Implementar MaxPlayers con límite de 10
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.6_
  - [x] 2.2 Write property test for Session State Consistency
    - **Property 1: Session State Consistency**
    - **Validates: Requirements 1.1, 1.3**
  - [x] 2.3 Write property test for Player Count Enforcement
    - **Property 2: Player Count Enforcement**
    - **Validates: Requirements 1.6**

- [x] 3. Implementar CharacterPersistenceService
  - [x] 3.1 Crear interfaz ICharacterPersistenceService y modelos de datos
    - Definir CharacterData, EquipmentData, ItemData como clases serializables (integrar con existente Database.cs)
    - Implementar ICharacterPersistenceService con métodos async
    - _Requirements: 7.1, 7.5_
  - [x] 3.2 Implementar encriptación y persistencia local
    - Implementar SaveCharacterAsync con encriptación AES
    - Implementar LoadCharacterAsync con decriptación y validación de integridad
    - Implementar hash de integridad (SHA256)
    - _Requirements: 7.2, 7.3, 7.4_
  - [x] 3.3 Implementar export/import para Cross-World
    - Implementar ExportCharacterForNetwork (serialización + encriptación)
    - Implementar ImportCharacterFromNetwork (decriptación + deserialización)
    - _Requirements: 7.5_
  - [x] 3.4 Write property test for Character Persistence Round-Trip
    - **Property 10: Character Persistence Round-Trip**
    - **Validates: Requirements 7.1, 7.3**
  - [x] 3.5 Write property test for Network Export/Import Round-Trip
    - **Property 11: Network Export/Import Round-Trip**
    - **Validates: Requirements 7.5**
  - [x] 3.6 Write property test for Encrypted Storage Non-Plaintext
    - **Property 12: Encrypted Storage Non-Plaintext**
    - **Validates: Requirements 7.2**
  - [x] 3.7 Write property test for Corrupted Data Rejection
    - **Property 13: Corrupted Data Rejection**
    - **Validates: Requirements 7.4**

- [x] 4. Checkpoint - Verificar persistencia funciona
  - Ensure all tests pass, ask the user if questions arise.

- [x] 5. Implementar ConnectionApprovalHandler (Mirror Authenticator)
  - [x] 5.1 Crear clase ConnectionApprovalAuthenticator extendiendo NetworkAuthenticator
    - Implementar OnAuthRequestMessage para validar payload
    - Implementar validación de rangos de stats de equipo
    - Implementar timeout handling con coroutines
    - Definir AuthRequestMessage y AuthResponseMessage como NetworkMessage
    - Usar ServerAccept(conn) y ServerReject(conn) para aprobar/rechazar
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5_
  - [x] 5.2 Integrar con NetworkSessionManager
    - Asignar Authenticator al NetworkManager
    - Configurar CharacterPersistenceService para importar datos del payload
    - Modificar CreateCharacterMessage existente para incluir payload encriptado
    - _Requirements: 6.1_
  - [x] 5.3 Write property test for Character Data Validation
    - **Property 9: Character Data Validation Correctness**
    - **Validates: Requirements 6.2, 6.3, 6.4**

- [x] 6. Implementar PlayerController con ownership
  - [x] 6.1 Crear interfaz IPlayerController y refactorizar clase Player existente
    - Implementar verificación isLocalPlayer al inicio de HandleInput (ya existe parcialmente)
    - Implementar movimiento en 8 direcciones con normalización (refactorizar de NavMesh a directo)
    - Implementar transformación relativa a cámara
    - _Requirements: 3.1, 3.2, 3.4, 3.5_
  - [x] 6.2 Crear Player Prefab simplificado para pruebas
    - Crear prefab con Capsule mesh
    - Añadir NetworkIdentity component
    - Añadir Player script (NetworkBehaviour)
    - Añadir Rigidbody y Collider
    - Usar [Command] para enviar movimiento al servidor
    - Usar [ClientRpc] para sincronizar posición a clientes
    - _Requirements: 2.1, 2.2, 3.3_
  - [x] 6.3 Write property test for Ownership-Based Input Processing
    - **Property 3: Ownership-Based Input Processing**
    - **Validates: Requirements 3.1, 3.2**
  - [x] 6.4 Write property test for Eight-Direction Movement
    - **Property 4: Eight-Direction Movement Validity**
    - **Validates: Requirements 3.4**
  - [x] 6.5 Write property test for Camera-Relative Movement
    - **Property 5: Camera-Relative Movement Transformation**
    - **Validates: Requirements 3.5**

- [x] 7. Checkpoint - Verificar movimiento local funciona
  - Ensure all tests pass, ask the user if questions arise.

- [x] 8. Configurar Physics Layer para no-colisión entre jugadores
  - [x] 8.1 Configurar Physics Layer Matrix
    - Crear layer "Player" en Project Settings > Tags and Layers
    - Configurar Physics Layer Collision Matrix para ignorar Player-Player
    - Asignar layer "Player" al Player Prefab
    - _Requirements: 5.1, 5.2, 5.3_
  - [x] 8.2 Write property test for Player-Player Non-Collision
    - **Property 8: Player-Player Non-Collision**
    - **Validates: Requirements 5.1, 5.2**

- [x] 9. Implementar InputBindingService
  - [x] 9.1 Crear Input Action Asset y Action Maps
    - Crear PlayerInputActions.inputactions asset
    - Definir Action Map "Player" con acciones: Move, Jump, Interact
    - Configurar bindings default (WASD para Move)
    - _Requirements: 4.1, 4.4_
  - [x] 9.2 Crear interfaz IInputBindingService e implementación
    - Implementar RebindAction usando InputActionRebindingExtensions
    - Implementar GetCurrentBinding
    - Implementar SaveBindings/LoadBindings con PlayerPrefs o JSON
    - Implementar HasConflict y GetConflictingActions
    - _Requirements: 4.2, 4.3, 4.5_
  - [x] 9.3 Write property test for Input Binding Round-Trip
    - **Property 6: Input Binding Round-Trip Persistence**
    - **Validates: Requirements 4.3**
  - [x] 9.4 Write property test for Binding Conflict Detection
    - **Property 7: Binding Conflict Detection**
    - **Validates: Requirements 4.5**

- [x] 10. Integrar PlayerController con InputBindingService
  - [x] 10.1 Conectar Input System al PlayerController
    - Añadir PlayerInput component al Player Prefab
    - Conectar eventos de input a Player.HandleInput (refactorizar)
    - Verificar que solo isLocalPlayer procesa input
    - _Requirements: 3.1, 4.1_

- [x] 11. Crear escena de prueba con UI de conexión
  - [x] 11.1 Crear NetworkTestScene
    - Crear escena en `Assets/_Project/Scenes/NetworkTestScene`
    - Añadir NetworkManager GameObject con NetworkSessionManager
    - Añadir suelo y spawn points (usar NetworkStartPosition de Mirror)
    - _Requirements: 2.3_
  - [x] 11.2 Crear UI temporal de conexión
    - Crear Canvas con botones [HOST] [CLIENT] [SERVER]
    - Crear InputField para IP address
    - Conectar botones a NetworkSessionManager (StartHost, StartClient, StartServer)
    - Mostrar mensajes de error usando OnClientError callback
    - _Requirements: 1.1, 1.2, 1.3, 1.4_

- [x] 12. Checkpoint final - Verificar dos cápsulas en red
  - Ensure all tests pass, ask the user if questions arise.
  - Verificar: Abrir dos instancias del juego, una como Host, otra como Client
  - Verificar: Ambas cápsulas visibles y moviéndose independientemente
  - Verificar: No hay colisión entre jugadores

## Notes

- Todas las tareas son requeridas para una implementación completa
- Cada task referencia requisitos específicos para trazabilidad
- Los checkpoints aseguran validación incremental
- Property tests validan propiedades universales de correctitud
- Unit tests validan ejemplos específicos y edge cases
- El objetivo final es el "primer hito real": dos cápsulas moviéndose en red
- **Mirror específico**: Usar NetworkServer.active, NetworkClient.active, isLocalPlayer, [Command], [ClientRpc], [SyncVar]
- **Integración**: El proyecto ya tiene SERVERNetworkManager, Entity, Player - las tareas extienden/refactorizan este código existente
