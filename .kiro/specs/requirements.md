# Requirements Document

## Introduction

Sistema completo para "The Ether Domes", un Micro-MMORPG cooperativo para 1-10 jugadores con combate Tab-Target estilo WoW, sistema de clases con Trinidad flexible, mazmorras con jefes, y progresión por niveles. El juego utiliza una arquitectura híbrida Host-Play/Servidor Dedicado usando Unity Netcode for GameObjects (NGO), con persistencia local de personajes Cross-World (estilo Valheim).

## Glossary

### Términos de Red
- **NetworkManager**: Componente central de NGO que gestiona conexiones, spawning de jugadores y sincronización de red.
- **Host**: Jugador que actúa simultáneamente como servidor y cliente (modelo Valheim).
- **Client**: Jugador conectado a un Host o Servidor Dedicado.
- **Dedicated_Server**: Instancia headless del juego que solo actúa como servidor sin renderizado.
- **NetworkObject**: Componente que marca un GameObject para sincronización en red.
- **IsOwner**: Propiedad que indica si el cliente local tiene autoridad sobre un NetworkObject.
- **Player_Prefab**: Prefab del personaje jugador con componentes de red configurados.
- **UnityTransport**: Capa de transporte de red basada en UDP para NGO.
- **Connection_Approval**: Sistema de validación que verifica la integridad de datos del personaje al conectar.

### Términos de Combate
- **Tab_Target**: Sistema de combate donde el jugador selecciona un enemigo (Tab) y las habilidades se dirigen automáticamente a ese objetivo.
- **Target_System**: Sistema que gestiona la selección y cambio de objetivos enemigos.
- **Ability**: Habilidad activa que el jugador puede usar con una tecla (1-9, etc.).
- **GCD**: Global Cooldown - tiempo mínimo entre uso de habilidades (típicamente 1.5s).
- **Cast_Time**: Tiempo de canalización de una habilidad antes de ejecutarse.
- **Cooldown**: Tiempo de espera antes de poder usar una habilidad de nuevo.
- **Aggro**: Sistema de amenaza que determina qué jugador ataca el enemigo.
- **Threat**: Valor numérico de amenaza generado por daño y habilidades de tanque.

### Términos de Clases
- **Trinity**: Sistema de roles Tank/Healer/DPS.
- **Tank**: Rol que absorbe daño y mantiene aggro de enemigos.
- **Healer**: Rol que restaura vida a aliados.
- **DPS**: Damage Per Second - rol enfocado en hacer daño.
- **Hybrid_Class**: Clase que puede cumplir múltiples roles (ej. Paladín: Tank o Healer).
- **Off_Spec**: Especialización secundaria de una clase híbrida.

### Términos de Progresión
- **Level**: Nivel del personaje (1-60).
- **Experience**: Puntos de experiencia ganados al derrotar enemigos.
- **Item_Level**: Nivel de poder de un equipo.
- **Loot**: Botín obtenido de enemigos y jefes.
- **Drop_Rate**: Probabilidad de que un jefe suelte un item específico.

### Términos de Mundo
- **Region**: Una de las 5 zonas del mundo (Roca, Bosque, Nieve, Pantano, Ciudadela).
- **Dungeon**: Mazmorra instanciada con jefes.
- **Boss**: Enemigo poderoso con mecánicas especiales.
- **Guild_Base**: Base compartida personalizable para los jugadores.
- **Spawn_Point**: Punto de aparición de jugadores o enemigos.

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

---

## Combat System Requirements

### Requirement 8: Tab-Target Selection System

**User Story:** As a player, I want to select enemies by clicking or pressing Tab, so that my abilities automatically target the selected enemy.

#### Acceptance Criteria

1. WHEN a player presses Tab, THE Target_System SHALL cycle to the next visible enemy within 40 meters
2. WHEN a player clicks on an enemy, THE Target_System SHALL select that enemy as the current target
3. WHEN an enemy is selected, THE Target_System SHALL display a visual indicator (circle/highlight) around the target
4. WHEN the selected target dies, THE Target_System SHALL automatically clear the selection
5. WHEN the selected target moves beyond 40 meters, THE Target_System SHALL maintain selection but indicate "out of range"
6. WHEN a player presses Escape, THE Target_System SHALL clear the current target selection

### Requirement 9: Ability System with GCD

**User Story:** As a player, I want to use abilities with hotkeys, so that I can execute combat actions efficiently.

#### Acceptance Criteria

1. WHEN a player presses an ability hotkey (1-9), THE Ability_System SHALL attempt to execute that ability
2. WHEN an ability is executed, THE Ability_System SHALL trigger a Global Cooldown of 1.5 seconds
3. WHILE the GCD is active, THE Ability_System SHALL prevent execution of other GCD-affected abilities
4. WHEN an ability has a Cast_Time, THE Ability_System SHALL show a cast bar and execute after the cast completes
5. IF the player moves during a cast, THEN THE Ability_System SHALL interrupt the cast
6. WHEN an ability is on Cooldown, THE Ability_System SHALL display remaining cooldown time on the action bar
7. IF no target is selected for a targeted ability, THEN THE Ability_System SHALL display "No target" error

### Requirement 10: Threat and Aggro System

**User Story:** As a tank, I want enemies to attack me instead of my teammates, so that I can protect the group.

#### Acceptance Criteria

1. WHEN a player deals damage to an enemy, THE Aggro_System SHALL add Threat equal to damage dealt
2. WHEN a tank uses a taunt ability, THE Aggro_System SHALL set their Threat to highest + 10%
3. WHEN a healer heals a player in combat, THE Aggro_System SHALL add Threat equal to 50% of healing done (split among engaged enemies)
4. THE Aggro_System SHALL cause enemies to attack the player with highest Threat
5. WHEN Threat exceeds current target's Threat by 10% (melee) or 30% (ranged), THE Aggro_System SHALL switch enemy target
6. WHEN combat ends (no players in combat for 5 seconds), THE Aggro_System SHALL reset all Threat values

### Requirement 11: Combat Death and Resurrection

**User Story:** As a player, I want a fair death penalty, so that death is meaningful but not frustrating.

#### Acceptance Criteria

1. WHEN a player's health reaches 0, THE Combat_System SHALL mark them as "Dead" and disable all actions
2. WHEN a player dies, THE Combat_System SHALL allow resurrection by a Healer ability within 60 seconds
3. IF no resurrection occurs within 60 seconds, THEN THE Combat_System SHALL offer "Release Spirit" option
4. WHEN a player releases spirit, THE Combat_System SHALL respawn them at the nearest graveyard with 50% health
5. WHEN all players in a dungeon die (wipe), THE Combat_System SHALL reset the dungeon to the last checkpoint
6. THE Combat_System SHALL NOT drop equipment or lose experience on death

---

## Class System Requirements

### Requirement 12: Character Classes

**User Story:** As a player, I want to choose a class for my character, so that I have a defined role and unique abilities.

#### Acceptance Criteria

1. WHEN creating a character, THE Class_System SHALL offer at least 4 classes: Warrior (Tank/DPS), Mage (DPS), Priest (Healer), Paladin (Tank/Healer/DPS)
2. WHEN a class is selected, THE Class_System SHALL assign the class-specific ability set
3. THE Class_System SHALL allow Hybrid_Classes to switch specialization outside of combat
4. WHEN a player switches specialization, THE Class_System SHALL update their available abilities
5. THE Class_System SHALL ensure all classes are viable for solo play (self-sustain abilities)

### Requirement 13: Trinity Role Flexibility

**User Story:** As a group leader, I want flexible class roles, so that we can complete dungeons without requiring exactly 1 tank, 1 healer, 3 DPS.

#### Acceptance Criteria

1. THE Class_System SHALL allow dungeons to be completed with various group compositions
2. WHEN a Hybrid_Class is in Off_Spec, THE Class_System SHALL provide reduced effectiveness (70%) in that role
3. THE Class_System SHALL provide all DPS classes with minor self-healing abilities
4. THE Class_System SHALL scale dungeon difficulty based on group size (2-10 players)

---

## Progression System Requirements

### Requirement 14: Level Progression

**User Story:** As a player, I want to level up my character, so that I become stronger and unlock new abilities.

#### Acceptance Criteria

1. WHEN a player defeats an enemy, THE Progression_System SHALL award Experience based on enemy level
2. WHEN Experience reaches the threshold, THE Progression_System SHALL increase player Level by 1
3. THE Progression_System SHALL support levels 1 through 60
4. WHEN a player levels up, THE Progression_System SHALL increase base stats (Health, Mana, Damage)
5. WHEN a player reaches specific levels (10, 20, 30, 40, 50), THE Progression_System SHALL unlock new abilities
6. THE Progression_System SHALL reduce Experience gained from enemies 10+ levels below player level

### Requirement 15: Loot and Equipment System

**User Story:** As a player, I want to obtain better equipment from bosses, so that my character becomes more powerful.

#### Acceptance Criteria

1. WHEN a boss is defeated, THE Loot_System SHALL generate loot based on the boss's loot table
2. WHEN loot is generated, THE Loot_System SHALL use Drop_Rate probabilities (Common: 70%, Rare: 25%, Epic: 5%)
3. WHEN multiple players are present, THE Loot_System SHALL use round-robin or need/greed distribution
4. WHEN a player equips an item, THE Equipment_System SHALL update their stats accordingly
5. THE Equipment_System SHALL enforce Item_Level requirements based on player Level
6. THE Loot_System SHALL serialize equipped items for Cross-World persistence

---

## World and Dungeon Requirements

### Requirement 16: World Regions

**User Story:** As a player, I want to explore different regions, so that I experience variety in environments and challenges.

#### Acceptance Criteria

1. THE World_System SHALL contain 5 regions: Roca (starter), Bosque, Nieve, Pantano, Ciudadela (endgame)
2. WHEN a player enters a region, THE World_System SHALL load that region's assets and enemies
3. THE World_System SHALL implement vertical progression (Roca → Bosque → Nieve → Pantano → Ciudadela)
4. WHEN transitioning between regions, THE World_System SHALL use loading screens (instanced regions)
5. THE World_System SHALL scale enemy levels per region (Roca: 1-15, Bosque: 15-30, Nieve: 30-40, Pantano: 40-50, Ciudadela: 50-60)

### Requirement 17: Dungeon System

**User Story:** As a group, I want to enter dungeons with bosses, so that we can obtain powerful loot and face challenging content.

#### Acceptance Criteria

1. THE Dungeon_System SHALL support two dungeon sizes: Small (3 bosses) and Large (5 bosses)
2. WHEN a group enters a dungeon, THE Dungeon_System SHALL create an instanced copy for that group
3. WHEN all players leave a dungeon instance, THE Dungeon_System SHALL destroy that instance after 5 minutes
4. THE Dungeon_System SHALL scale boss health and damage based on group size (2-10 players)
5. WHEN a boss is defeated, THE Dungeon_System SHALL mark it as cleared for that instance
6. WHEN all bosses are defeated, THE Dungeon_System SHALL mark the dungeon as "Completed"

### Requirement 18: Boss Mechanics

**User Story:** As a player, I want bosses to have unique mechanics, so that combat is engaging and requires coordination.

#### Acceptance Criteria

1. WHEN a boss fight begins, THE Boss_System SHALL activate the boss's ability rotation
2. THE Boss_System SHALL implement at least 2 unique mechanics per boss (AoE attacks, adds, phases)
3. WHEN a boss reaches health thresholds (75%, 50%, 25%), THE Boss_System SHALL trigger phase transitions
4. THE Boss_System SHALL telegraph dangerous abilities with visual indicators (ground markers, cast bars)
5. WHEN a boss is defeated, THE Boss_System SHALL trigger loot generation and grant Experience

---

## Guild Base Requirements

### Requirement 19: Guild Base System

**User Story:** As a group of players, I want a shared base, so that we can store items, display trophies, and socialize.

#### Acceptance Criteria

1. THE Guild_Base_System SHALL provide a shared instanced space accessible to all connected players
2. WHEN a player enters the Guild Base, THE Guild_Base_System SHALL load the customized state
3. THE Guild_Base_System SHALL allow placement of furniture and decorations
4. WHEN a boss is defeated for the first time, THE Guild_Base_System SHALL unlock a trophy for display
5. THE Guild_Base_System SHALL persist base customization to the Host's save file
6. WHEN the Host is offline, THE Guild_Base_System SHALL be inaccessible to other players

### Requirement 20: World Persistence

**User Story:** As a Host, I want the world state to be saved, so that progress is maintained between sessions.

#### Acceptance Criteria

1. WHEN the Host disconnects, THE Persistence_System SHALL save world state (defeated bosses, guild base)
2. WHEN the Host reconnects, THE Persistence_System SHALL restore the saved world state
3. WHILE the Host is offline, THE World_System SHALL pause (no time progression, no respawns)
4. THE Persistence_System SHALL NOT persist individual player positions (players spawn at safe locations)
5. IF using Dedicated_Server, THEN THE Persistence_System SHALL auto-save every 5 minutes
