# Implementation Plan: Network Player Foundation

## Overview

Plan de implementación incremental para el sistema fundacional de red y movimiento de jugadores. Cada tarea construye sobre la anterior, comenzando con la estructura del proyecto y terminando con la integración completa. El objetivo final es tener dos cápsulas moviéndose en red.

## Tasks

- [ ] 1. Configurar estructura del proyecto y dependencias
  - Crear estructura de carpetas: `_Project/Scripts/Network`, `_Project/Scripts/Player`, `_Project/Scripts/Persistence`, `_Project/Scripts/Input`, `_Project/ScriptableObjects`, `_Project/Scenes`, `_Project/Prefabs/Characters`
  - Crear archivo `.asmdef` para cada módulo (Network, Player, Persistence, Input, Tests)
  - Configurar paquete FsCheck para property-based testing via NuGet
  - _Requirements: Setup técnico base_

- [ ] 2. Implementar NetworkSessionManager
  - [ ] 2.1 Crear interfaz INetworkSessionManager y clase NetworkSessionManager
    - Implementar wrapper sobre Unity NetworkManager
    - Implementar StartAsHost(), StartAsClient(), StartAsDedicatedServer(), Disconnect()
    - Implementar eventos OnPlayerConnected, OnPlayerDisconnected, OnConnectionFailed
    - Implementar propiedades IsHost, IsClient, IsServer, ConnectedPlayerCount, MaxPlayers
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.6_
  - [ ] 2.2 Write property test for Session State Consistency
    - **Property 1: Session State Consistency**
    - **Validates: Requirements 1.1, 1.3**
  - [ ] 2.3 Write property test for Player Count Enforcement
    - **Property 2: Player Count Enforcement**
    - **Validates: Requirements 1.6**

- [ ] 3. Implementar CharacterPersistenceService
  - [ ] 3.1 Crear interfaz ICharacterPersistenceService y modelos de datos
    - Definir CharacterData, EquipmentData, ItemData como clases serializables
    - Implementar ICharacterPersistenceService con métodos async
    - _Requirements: 7.1, 7.5_
  - [ ] 3.2 Implementar encriptación y persistencia local
    - Implementar SaveCharacterAsync con encriptación AES
    - Implementar LoadCharacterAsync con decriptación y validación de integridad
    - Implementar hash de integridad (SHA256)
    - _Requirements: 7.2, 7.3, 7.4_
  - [ ] 3.3 Implementar export/import para Cross-World
    - Implementar ExportCharacterForNetwork (serialización + encriptación)
    - Implementar ImportCharacterFromNetwork (decriptación + deserialización)
    - _Requirements: 7.5_
  - [ ] 3.4 Write property test for Character Persistence Round-Trip
    - **Property 10: Character Persistence Round-Trip**
    - **Validates: Requirements 7.1, 7.3**
  - [ ] 3.5 Write property test for Network Export/Import Round-Trip
    - **Property 11: Network Export/Import Round-Trip**
    - **Validates: Requirements 7.5**
  - [ ] 3.6 Write property test for Encrypted Storage Non-Plaintext
    - **Property 12: Encrypted Storage Non-Plaintext**
    - **Validates: Requirements 7.2**
  - [ ] 3.7 Write property test for Corrupted Data Rejection
    - **Property 13: Corrupted Data Rejection**
    - **Validates: Requirements 7.4**

- [ ] 4. Checkpoint - Verificar persistencia funciona
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 5. Implementar ConnectionApprovalHandler
  - [ ] 5.1 Crear interfaz IConnectionApprovalHandler y implementación
    - Implementar ValidateConnectionRequest con parsing de payload
    - Implementar validación de rangos de stats de equipo
    - Implementar timeout handling
    - Definir ConnectionApprovalResult y ApprovalErrorCode
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5_
  - [ ] 5.2 Integrar con NetworkSessionManager
    - Conectar ConnectionApprovalHandler al evento de aprobación de NGO
    - Configurar CharacterPersistenceService para importar datos del payload
    - _Requirements: 6.1_
  - [ ] 5.3 Write property test for Character Data Validation
    - **Property 9: Character Data Validation Correctness**
    - **Validates: Requirements 6.2, 6.3, 6.4**

- [ ] 6. Implementar PlayerController con ownership
  - [ ] 6.1 Crear interfaz IPlayerController y clase PlayerController
    - Implementar verificación IsOwner al inicio de ProcessMovementInput
    - Implementar movimiento en 8 direcciones con normalización
    - Implementar transformación relativa a cámara
    - _Requirements: 3.1, 3.2, 3.4, 3.5_
  - [ ] 6.2 Crear Player Prefab con componentes de red
    - Crear prefab con Capsule/Cube mesh
    - Añadir NetworkObject component
    - Añadir NetworkTransform component
    - Añadir PlayerController script
    - Añadir Rigidbody y Collider
    - _Requirements: 2.1, 2.2, 3.3_
  - [ ] 6.3 Write property test for Ownership-Based Input Processing
    - **Property 3: Ownership-Based Input Processing**
    - **Validates: Requirements 3.1, 3.2**
  - [ ] 6.4 Write property test for Eight-Direction Movement
    - **Property 4: Eight-Direction Movement Validity**
    - **Validates: Requirements 3.4**
  - [ ] 6.5 Write property test for Camera-Relative Movement
    - **Property 5: Camera-Relative Movement Transformation**
    - **Validates: Requirements 3.5**

- [ ] 7. Checkpoint - Verificar movimiento local funciona
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 8. Configurar Physics Layer para no-colisión entre jugadores
  - [ ] 8.1 Configurar Physics Layer Matrix
    - Crear layer "Player" en Project Settings > Tags and Layers
    - Configurar Physics Layer Collision Matrix para ignorar Player-Player
    - Asignar layer "Player" al Player Prefab
    - _Requirements: 5.1, 5.2, 5.3_
  - [ ] 8.2 Write property test for Player-Player Non-Collision
    - **Property 8: Player-Player Non-Collision**
    - **Validates: Requirements 5.1, 5.2**

- [ ] 9. Implementar InputBindingService
  - [ ] 9.1 Crear Input Action Asset y Action Maps
    - Crear PlayerInputActions.inputactions asset
    - Definir Action Map "Player" con acciones: Move, Jump, Interact
    - Configurar bindings default (WASD para Move)
    - _Requirements: 4.1, 4.4_
  - [ ] 9.2 Crear interfaz IInputBindingService e implementación
    - Implementar RebindAction usando InputActionRebindingExtensions
    - Implementar GetCurrentBinding
    - Implementar SaveBindings/LoadBindings con PlayerPrefs o JSON
    - Implementar HasConflict y GetConflictingActions
    - _Requirements: 4.2, 4.3, 4.5_
  - [ ] 9.3 Write property test for Input Binding Round-Trip
    - **Property 6: Input Binding Round-Trip Persistence**
    - **Validates: Requirements 4.3**
  - [ ] 9.4 Write property test for Binding Conflict Detection
    - **Property 7: Binding Conflict Detection**
    - **Validates: Requirements 4.5**

- [ ] 10. Integrar PlayerController con InputBindingService
  - [ ] 10.1 Conectar Input System al PlayerController
    - Añadir PlayerInput component al Player Prefab
    - Conectar eventos de input a PlayerController.ProcessMovementInput
    - Verificar que solo el owner procesa input
    - _Requirements: 3.1, 4.1_

- [ ] 11. Crear escena de prueba con UI de conexión
  - [ ] 11.1 Crear NetworkTestScene
    - Crear escena en `_Project/Scenes/NetworkTestScene`
    - Añadir NetworkManager GameObject con NetworkSessionManager
    - Añadir suelo y spawn points
    - _Requirements: 2.3_
  - [ ] 11.2 Crear UI temporal de conexión
    - Crear Canvas con botones [HOST] [CLIENT] [SERVER]
    - Crear InputField para IP address
    - Conectar botones a NetworkSessionManager
    - Mostrar mensajes de error en UI
    - _Requirements: 1.1, 1.2, 1.3, 1.4_

- [ ] 12. Checkpoint final - Verificar dos cápsulas en red
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
