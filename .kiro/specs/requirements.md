# Requirements Document

## Introduction

Sistema fundacional de red y movimiento de jugadores para "The Ether Domes", un Micro-MMORPG cooperativo para 1-10 jugadores. Este módulo establece la arquitectura híbrida Host-Play/Servidor Dedicado usando Unity Netcode for GameObjects (NGO), permitiendo que múltiples jugadores se conecten y muevan sus personajes en un mundo compartido.

## Glossary

- **NetworkManager**: Componente central de NGO que gestiona conexiones, spawning de jugadores y sincronización de red.
- **Host**: Jugador que actúa simultáneamente como servidor y cliente (modelo Valheim).
- **Client**: Jugador conectado a un Host o Servidor Dedicado.
- **Dedicated_Server**: Instancia headless del juego que solo actúa como servidor sin renderizado.
- **NetworkObject**: Componente que marca un GameObject para sincronización en red.
- **IsOwner**: Propiedad que indica si el cliente local tiene autoridad sobre un NetworkObject.
- **Player_Prefab**: Prefab del personaje jugador con componentes de red configurados.
- **UnityTransport**: Capa de transporte de red basada en UDP para NGO.
- **Connection_Approval**: Sistema de validación que verifica la integridad de datos del personaje al conectar.

## Requirements

### Requirement 1: Gestión de Sesiones de Red

**User Story:** As a player, I want to host a game session or join an existing one, so that I can play cooperatively with other players.

#### Acceptance Criteria

1. WHEN a player selects "Host", THE NetworkManager SHALL start a session as Host (servidor + cliente simultáneo)
2. WHEN a player selects "Client" and provides a valid IP address, THE NetworkManager SHALL attempt connection to that Host
3. WHEN a player selects "Dedicated Server", THE NetworkManager SHALL start in modo headless sin renderizado
4. WHEN a connection attempt fails, THE NetworkManager SHALL display an error message to the player
5. WHEN a Host disconnects, THE NetworkManager SHALL notify all connected Clients and terminate their sessions
6. THE NetworkManager SHALL support between 1 and 10 simultaneous players per session

### Requirement 2: Spawning de Jugadores

**User Story:** As a connected player, I want my character to appear in the game world, so that I can interact with the environment and other players.

#### Acceptance Criteria

1. WHEN a player successfully connects, THE NetworkManager SHALL instantiate a Player_Prefab for that player
2. WHEN a Player_Prefab is instantiated, THE NetworkObject component SHALL be assigned ownership to the connecting client
3. WHEN multiple players connect, THE NetworkManager SHALL spawn each player at a designated spawn point
4. WHEN a player disconnects, THE NetworkManager SHALL destroy their Player_Prefab from all clients

### Requirement 3: Movimiento de Personaje con Autoridad de Cliente

**User Story:** As a player, I want to control my character's movement smoothly, so that I can navigate the game world responsively.

#### Acceptance Criteria

1. WHEN a player provides movement input, THE Player_Controller SHALL only process input if IsOwner is true
2. WHEN IsOwner is false, THE Player_Controller SHALL ignore local input for that NetworkObject
3. WHEN a player moves their character, THE NetworkTransform SHALL synchronize position to all other clients
4. THE Player_Controller SHALL support movement in 8 directions (WASD + diagonals)
5. WHEN movement input is received, THE Player_Controller SHALL apply movement relative to camera orientation (tercera persona)

### Requirement 4: Configuración de Input Remapeable

**User Story:** As a player, I want to customize my control bindings, so that I can play with my preferred key configuration.

#### Acceptance Criteria

1. THE Input_System SHALL use Unity Input System Action Maps for all player inputs
2. WHEN a player accesses input settings, THE Input_System SHALL display current key bindings
3. WHEN a player modifies a key binding, THE Input_System SHALL persist the change locally
4. THE Input_System SHALL support rebinding of movement keys (default: WASD)
5. WHEN conflicting bindings are detected, THE Input_System SHALL warn the player before applying

### Requirement 5: No Colisión Entre Jugadores

**User Story:** As a player, I want to move through other players without collision, so that movement is not blocked in crowded areas.

#### Acceptance Criteria

1. THE Physics_System SHALL configure player colliders to ignore collisions with other player colliders
2. WHEN two players occupy the same space, THE Physics_System SHALL allow both to move freely
3. THE Physics_System SHALL maintain collision between players and environment/NPCs

### Requirement 6: Validación de Conexión (Anti-Cheat Básico)

**User Story:** As a Host, I want to validate connecting players' data integrity, so that cheaters cannot join with modified stats.

#### Acceptance Criteria

1. WHEN a Client requests connection, THE Connection_Approval system SHALL validate character data integrity
2. WHEN character data fails validation, THE Connection_Approval system SHALL reject the connection with an error code
3. WHEN character data passes validation, THE Connection_Approval system SHALL allow the connection to proceed
4. THE Connection_Approval system SHALL verify equipment stats against known valid ranges
5. IF a validation timeout occurs, THEN THE Connection_Approval system SHALL reject the connection

### Requirement 7: Persistencia Local de Personaje

**User Story:** As a player, I want my character data saved locally, so that I can use the same character across different game sessions and servers (Cross-World).

#### Acceptance Criteria

1. WHEN a player creates or modifies their character, THE Persistence_System SHALL save data to local storage
2. WHEN saving character data, THE Persistence_System SHALL encrypt the data to prevent tampering
3. WHEN loading character data, THE Persistence_System SHALL decrypt and validate integrity
4. IF character data is corrupted, THEN THE Persistence_System SHALL notify the player and prevent loading
5. THE Persistence_System SHALL store character data in a portable format for Cross-World functionality
