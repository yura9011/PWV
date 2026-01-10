# Requirements Document - MVP 10 Features

## Introduction

Especificación detallada para las 10 features del MVP de "The Ether Domes", un Micro-MMORPG cooperativo para 1-10 jugadores. Este documento captura las decisiones de diseño específicas basadas en el análisis de requisitos realizado.

## Glossary

### Términos de Persistencia
- **Character_Lock**: Sistema que previene el uso simultáneo del mismo personaje en múltiples sesiones.
- **Data_Migration**: Proceso de actualizar datos de personajes guardados cuando cambia la estructura del juego.
- **Version_Tag**: Identificador de versión almacenado con los datos del personaje para migración.

### Términos de Combate
- **Proc**: Efecto aleatorio que se activa con cierta probabilidad al usar una habilidad.
- **Interrupt**: Habilidad que cancela el cast de un enemigo.
- **Friendly_Fire**: Daño o curación que afecta a aliados además de enemigos.
- **Floating_Combat_Text**: Números de daño/curación que aparecen sobre los personajes.
- **Tell**: Indicador visual que anticipa un ataque fuerte del enemigo.

### Términos de Loot
- **Need_Greed**: Sistema de votación donde jugadores eligen "Need" (lo necesito) o "Greed" (lo quiero).
- **Bad_Luck_Protection**: Sistema que aumenta probabilidad de drops raros después de intentos fallidos.
- **Salvage**: Convertir items no deseados en materiales.
- **Durability**: Puntos de resistencia de un item que se degradan con el uso.

### Términos de Dungeon
- **Wipe**: Cuando todos los jugadores del grupo mueren.
- **Wipe_Limit**: Número máximo de wipes permitidos antes de expulsión.
- **Weekly_Lockout**: Restricción que limita el loot de un boss a una vez por semana.
- **Difficulty_Tier**: Nivel de dificultad de dungeon (Normal, Heroic, Mythic).

### Términos de Recursos
- **Rage**: Recurso del Warrior que se genera al dar/recibir daño.
- **Holy_Power**: Recurso del Paladin que se acumula con ciertas habilidades.
- **Secondary_Resource**: Recurso adicional a Mana específico de ciertas clases.

---

## Requirements

### Requirement 1: Sistema de Persistencia con Bloqueo de Sesión

**User Story:** As a player, I want my character data protected from duplication exploits, so that the game economy remains fair.

#### Acceptance Criteria

1. WHEN a player loads a character, THE Persistence_System SHALL create a session lock file
2. IF a session lock already exists for that character, THEN THE Persistence_System SHALL reject the load with error "Character in use"
3. WHEN a player disconnects normally, THE Persistence_System SHALL remove the session lock
4. IF the game crashes, THEN THE Persistence_System SHALL detect stale locks (>5 minutes) and allow override
5. THE Persistence_System SHALL store a Version_Tag with each character save
6. WHEN loading a character with older Version_Tag, THE Persistence_System SHALL run Data_Migration automatically
7. THE Data_Migration system SHALL preserve all player progress while updating data structure
8. IF Data_Migration fails, THEN THE Persistence_System SHALL create a backup and notify the player

---

### Requirement 2: Sistema de Clases con Habilidades Diferenciadas

**User Story:** As a player, I want each class specialization to feel unique, so that my choice of spec matters.

#### Acceptance Criteria

1. WHEN a Hybrid_Class changes specialization, THE Class_System SHALL replace the ability set completely (not scale existing abilities)
2. THE Class_System SHALL provide 4-5 unique abilities per specialization for MVP
3. WHEN a Paladin switches from Retribution to Holy, THE Class_System SHALL give healing abilities with their own balanced damage values
4. THE Class_System SHALL NOT use percentage modifiers for Off_Spec effectiveness
5. WHEN a group enters a dungeon without a healer, THE Dungeon_System SHALL NOT modify enemy damage or player healing
6. THE Class_System SHALL ensure each spec is self-sufficient for solo content

---

### Requirement 3: IA de Enemigos con Aggro Dinámico

**User Story:** As a tank, I want enemies to behave predictably based on threat, so that I can protect my group effectively.

#### Acceptance Criteria

1. WHEN an enemy loses its current target (death), THE Enemy_AI SHALL attack the player who dealt the most total damage
2. WHEN a player pulls an enemy, THE Enemy_AI SHALL alert all enemies within 15 meters to join combat
3. WHEN an enemy is pulled beyond 40 meters from spawn, THE Enemy_AI SHALL return to spawn position
4. WHEN an enemy returns to spawn, THE Enemy_AI SHALL maintain current HP (no regeneration)
5. THE Enemy_AI SHALL reset threat table only when returning to spawn
6. WHEN an enemy prepares a strong attack, THE Enemy_AI SHALL display a visual "tell" indicator 2 seconds before execution
7. THE Enemy_AI SHALL telegraph AoE attacks with ground markers

---

### Requirement 4: Sistema de Loot con Need/Greed

**User Story:** As a group member, I want fair loot distribution, so that everyone has a chance at upgrades.

#### Acceptance Criteria

1. WHEN loot drops from an enemy, THE Loot_System SHALL display a Need/Greed window to all eligible players
2. WHEN a player selects "Need", THE Loot_System SHALL prioritize them over "Greed" selections
3. WHEN multiple players select "Need", THE Loot_System SHALL randomly select one winner
4. THE Loot_System SHALL implement Bad_Luck_Protection after 10 failed attempts at rare items
5. WHEN Bad_Luck_Protection activates, THE Loot_System SHALL increase drop chance by 5% per additional attempt
6. THE Loot_System SHALL allow item trading between players without restrictions
7. THE Loot_System SHALL provide a Salvage option to convert unwanted items into crafting materials
8. WHEN an item is salvaged, THE Loot_System SHALL return materials based on item rarity

---

### Requirement 5: Sistema de Dungeons con Dificultades

**User Story:** As a player, I want challenging content with meaningful rewards, so that I have goals to work towards.

#### Acceptance Criteria

1. THE Dungeon_System SHALL support three difficulty tiers: Normal, Heroic, Mythic
2. WHEN difficulty increases, THE Dungeon_System SHALL increase enemy stats AND add new mechanics
3. WHEN difficulty is Mythic, THE Dungeon_System SHALL drop exclusive loot not available in lower difficulties
4. WHEN a group wipes, THE Dungeon_System SHALL respawn players at the dungeon entrance
5. WHEN a group reaches 3 wipes, THE Dungeon_System SHALL reset the dungeon completely and expel the group
6. THE Dungeon_System SHALL NOT implement time limits or enrage timers
7. THE Dungeon_System SHALL implement Weekly_Lockout for boss loot (global across all difficulties)
8. WHEN a player has already looted a boss this week, THE Dungeon_System SHALL still allow participation but no loot

---

### Requirement 6: UI de Combate con Feedback Visual

**User Story:** As a player, I want clear visual feedback during combat, so that I can make informed decisions.

#### Acceptance Criteria

1. THE Target_Frame SHALL display: HP bar, name, cast bar, buffs/debuffs, level, and enemy type
2. THE Aggro_System SHALL indicate current aggro holder with: icon over player, colored name in party frame, AND visual line to enemy
3. THE Combat_System SHALL display Floating_Combat_Text for all damage and healing
4. THE Floating_Combat_Text SHALL use different colors: white (normal), yellow (crit), green (healing), red (damage taken)
5. THE Combat_System SHALL display combat log as floating text near the action
6. WHEN an enemy casts an ability, THE Target_Frame SHALL show cast bar with ability name and duration

---

### Requirement 7: Networking Server-Authoritative

**User Story:** As a player, I want consistent combat regardless of my connection, so that the game feels fair.

#### Acceptance Criteria

1. THE Network_System SHALL use server-authoritative model for all combat actions
2. WHEN a player uses an ability, THE Network_System SHALL wait for server confirmation before showing effects
3. WHEN a player experiences latency >500ms, THE Network_System SHALL pause their actions until connection stabilizes
4. THE Network_System SHALL display a warning icon when latency exceeds 200ms
5. THE Network_System SHALL NOT add artificial delay to the Host player (PvE focus makes this acceptable)
6. WHEN connection is lost, THE Network_System SHALL attempt reconnection for 30 seconds before disconnecting

---

### Requirement 8: Progresión con Soft Caps

**User Story:** As a max-level player, I want meaningful gear progression, so that I have reasons to keep playing.

#### Acceptance Criteria

1. WHEN a player reaches level 60, THE Progression_System SHALL focus progression on gear acquisition
2. THE Progression_System SHALL implement soft caps with diminishing returns for all secondary stats
3. WHEN a stat exceeds 30% effectiveness, THE Progression_System SHALL apply 50% diminishing returns
4. WHEN a stat exceeds 50% effectiveness, THE Progression_System SHALL apply 75% diminishing returns
5. THE Equipment_System SHALL implement Durability for all equipped items
6. WHEN Durability reaches 0, THE Equipment_System SHALL reduce item stats by 50% (item still functions)
7. THE Equipment_System SHALL provide repair option at vendors or with repair kits
8. THE Weekly_Lockout system SHALL reset every Monday at 00:00 UTC

---

### Requirement 9: Sistema de Habilidades con Procs

**User Story:** As a player, I want exciting combat moments, so that combat feels dynamic and rewarding.

#### Acceptance Criteria

1. THE Ability_System SHALL support proc effects (random chance to trigger bonus effects)
2. WHEN a proc triggers, THE Ability_System SHALL display visual and audio feedback
3. THE Ability_System SHALL support class-specific secondary resources:
   - Warrior: Rage (generated by dealing/receiving damage)
   - Paladin: Holy Power (generated by specific abilities)
   - Mage: Mana only
   - Priest: Mana only (for MVP, expandable later)
4. THE Ability_System SHALL implement AoE abilities with Friendly_Fire (can damage allies)
5. THE Ability_System SHALL implement AoE heals with Friendly_Fire (can heal enemies)
6. THE Ability_System SHALL provide at least one Interrupt ability to each class
7. WHEN an Interrupt hits during enemy cast, THE Ability_System SHALL cancel the cast and apply a 4-second lockout

---

### Requirement 10: Primera Dungeon - Proof of Concept

**User Story:** As a player, I want to experience a complete dungeon run, so that I can see the core gameplay loop.

#### Acceptance Criteria

1. THE First_Dungeon SHALL contain 1 boss (proof of concept for MVP)
2. THE First_Dungeon SHALL contain 5-8 normal enemies before the boss
3. THE Boss SHALL have at least 2 unique mechanics with visual tells
4. THE Boss SHALL have 3 difficulty versions (Normal, Heroic, Mythic)
5. THE Boss SHALL drop loot appropriate to difficulty tier
6. THE First_Dungeon SHALL support 2-10 players with scaling
7. WHEN group size changes, THE Dungeon_System SHALL scale boss HP by 20% per additional player
8. THE First_Dungeon SHALL serve as template for future dungeon development

---

## Non-Functional Requirements

### Performance
1. THE Combat_System SHALL process ability execution within 100ms on server
2. THE Network_System SHALL maintain <50ms additional latency for combat actions
3. THE UI_System SHALL render at minimum 30 FPS during combat with 10 players

### Security
1. THE Persistence_System SHALL validate all character data on connection
2. THE Network_System SHALL reject invalid ability requests (cooldown violations, range violations)
3. THE Loot_System SHALL generate loot server-side only

### Scalability
1. THE Dungeon_System architecture SHALL support adding new dungeons without code changes
2. THE Class_System architecture SHALL support adding new classes without modifying existing code
3. THE Ability_System architecture SHALL support adding new abilities via ScriptableObjects

---

## MVP Scope Summary

### Included in MVP
- 4 classes with 4-5 abilities each per spec
- 1 dungeon with 1 boss (3 difficulties)
- Need/Greed loot system with Bad Luck Protection
- Floating combat text and visual feedback
- Server-authoritative networking
- Character persistence with session locking
- Soft caps and durability system
- Proc system and class resources (Rage, Holy Power)
- Friendly fire for AoE abilities
- Interrupt system for all classes

### Excluded from MVP (Post-MVP)
- Guild Base system
- Additional dungeons
- Complex combo systems
- Paragon/Prestige systems
- Chat system
- Matchmaking
- Dedicated server builds

---

## Decision Log

| Question | Decision | Rationale |
|----------|----------|-----------|
| Simultaneous sessions | Block | Prevent duplication exploits |
| Data migration | Auto-migrate with versioning | Preserve player progress |
| Off-spec effectiveness | Separate ability sets | Better class fantasy |
| Group without healer | No compensation | Player responsibility |
| Enemy tells | Yes, 2s warning | Skill-based gameplay |
| Enemy leash | 40m, keep damage | Prevent exploit kiting |
| Enemy reinforcements | All nearby join | Dynamic combat |
| Target on death | Highest damage dealer | Reward contribution |
| Loot distribution | Need/Greed | Fair group play |
| Bad luck protection | After 10 attempts | Reduce frustration |
| Item binding | Tradeable always | Social economy |
| Salvage system | Yes | Item sink |
| Dungeon difficulties | 3 tiers with exclusive loot | Progression depth |
| Wipe handling | 3 wipes = full reset | Meaningful challenge |
| Timer/Enrage | No | Casual-friendly |
| Target frame info | Full (HP, cast, buffs, level) | Informed decisions |
| Aggro indication | Multiple indicators | Clear communication |
| Combat log | Floating text | Immersive feedback |
| Network model | Server authoritative | Consistency over responsiveness |
| High latency | Pause actions | Fair play |
| Host advantage | Accept (PvE) | Simplicity |
| Endgame | Gear farming | Clear goal |
| Stat caps | Soft caps | Prevent extremes |
| Weekly lockout | Global | Pacing |
| Durability | Reduced stats at 0 | Not punishing |
| Procs | Simple procs | Dynamic combat |
| Class resources | Hybrid (some unique) | Class identity |
| Friendly fire | Yes (damage and heals) | Tactical depth |
| Interrupts | All classes | Group utility |
| MVP abilities | 4-5 per spec | Focused scope |
| MVP dungeon | 1 boss | Proof of concept |
| MVP priority | Combat polish | Core experience |
| Guild Base | Post-MVP | Scope control |


---

## Requisitos Adicionales - Puntos Ciegos Identificados

### Requirement 11: Extensión de CharacterData

**User Story:** As a developer, I want CharacterData to support all new systems, so that player progress persists correctly.

#### Acceptance Criteria

1. THE CharacterData SHALL include a DataVersion field for migration support
2. THE CharacterData SHALL include SecondaryResource and ResourceType fields for class resources
3. THE CharacterData SHALL include a LootAttempts dictionary for Bad Luck Protection tracking
4. THE CharacterData SHALL include LockedBossIds list for Weekly Lockout tracking
5. THE CharacterData SHALL include an Inventory list separate from Equipment
6. THE CharacterData SHALL include CurrentMana and MaxMana fields
7. WHEN migrating from old versions, THE Migration_System SHALL initialize new fields with sensible defaults

---

### Requirement 12: Sistema de Mana

**User Story:** As a player, I want abilities to cost mana, so that resource management is part of combat strategy.

#### Acceptance Criteria

1. THE Mana_System SHALL track CurrentMana and MaxMana per player
2. WHEN an ability is used, THE Mana_System SHALL deduct ManaCost from CurrentMana
3. IF CurrentMana < ManaCost, THEN THE Ability_System SHALL reject the ability with "Not enough mana"
4. THE Mana_System SHALL regenerate mana at 2% of MaxMana per second out of combat
5. THE Mana_System SHALL regenerate mana at 0.5% of MaxMana per second in combat
6. WHEN a player levels up, THE Mana_System SHALL increase MaxMana based on class and Intellect

---

### Requirement 13: Tracking de Daño por Jugador en Enemigos

**User Story:** As a tank, I want enemies to target the highest damage dealer when I die, so that threat mechanics work correctly.

#### Acceptance Criteria

1. THE Enemy SHALL track total damage received from each player in a dictionary
2. WHEN damage is applied, THE Enemy SHALL update the damage tracking for that player
3. WHEN the current target dies, THE Enemy_AI SHALL select the player with highest total damage
4. WHEN the enemy resets (returns to spawn), THE Enemy SHALL clear the damage tracking
5. THE damage tracking SHALL be server-authoritative

---

### Requirement 14: Sistema de Party/Grupo

**User Story:** As a player, I want to form groups with other players, so that we can coordinate for dungeons and loot.

#### Acceptance Criteria

1. THE Party_System SHALL support groups of 2-10 players
2. WHEN a player invites another, THE Party_System SHALL send an invitation that can be accepted or declined
3. WHEN a player joins a party, THE Party_System SHALL sync party member list to all members
4. THE Party_System SHALL designate one player as Party Leader
5. WHEN the Party Leader disconnects, THE Party_System SHALL promote the next member to leader
6. THE Loot_System SHALL use party membership to determine Need/Greed eligibility
7. THE Dungeon_System SHALL use party membership for instance creation and scaling

---

### Requirement 15: UI de Loot Window

**User Story:** As a player, I want a visual interface for loot distribution, so that I can make informed decisions.

#### Acceptance Criteria

1. WHEN loot drops, THE Loot_Window_UI SHALL display all items with name, icon, and rarity color
2. THE Loot_Window_UI SHALL show Need, Greed, and Pass buttons for each item
3. THE Loot_Window_UI SHALL display a 30-second countdown timer
4. WHEN time expires, THE Loot_Window_UI SHALL auto-pass for non-responders
5. THE Loot_Window_UI SHALL show roll results after all players have voted
6. THE Loot_Window_UI SHALL highlight the winner for each item

---

### Requirement 16: UI de Inventario

**User Story:** As a player, I want to manage my inventory visually, so that I can organize and use items.

#### Acceptance Criteria

1. THE Inventory_UI SHALL display a grid of 30 inventory slots
2. THE Inventory_UI SHALL show item icon, name, and rarity color per slot
3. WHEN hovering over an item, THE Inventory_UI SHALL display a tooltip with stats
4. WHEN right-clicking an equippable item, THE Inventory_UI SHALL show "Equip" option
5. WHEN right-clicking any item, THE Inventory_UI SHALL show "Salvage" option
6. THE Inventory_UI SHALL toggle visibility with the "I" key
7. THE Inventory_UI SHALL display current gold/currency

---

### Requirement 17: UI de Selección de Personaje

**User Story:** As a player, I want to create and select characters visually, so that I can manage multiple characters.

#### Acceptance Criteria

1. THE Character_Select_UI SHALL display a list of saved characters with name, class, and level
2. THE Character_Select_UI SHALL provide "New Character", "Play", and "Delete" buttons
3. WHEN creating a new character, THE Character_Creation_UI SHALL show class selection with descriptions
4. THE Character_Creation_UI SHALL show a name input field with validation (3-16 characters)
5. THE Character_Creation_UI SHALL preview base stats for the selected class
6. WHEN "Play" is clicked, THE Character_Select_UI SHALL load the character and connect to game

---

### Requirement 18: UI de Reparación

**User Story:** As a player, I want to repair my equipment, so that I can restore full stats.

#### Acceptance Criteria

1. THE Repair_UI SHALL display all equipped items with current durability percentage
2. THE Repair_UI SHALL show repair cost per item based on item level and damage
3. THE Repair_UI SHALL provide "Repair" button per item and "Repair All" button
4. WHEN repair is clicked, THE Repair_System SHALL restore durability to 100% and deduct gold
5. IF player lacks gold, THEN THE Repair_UI SHALL disable repair buttons and show "Not enough gold"

---

### Requirement 19: NavMesh para Enemy AI

**User Story:** As a developer, I want enemies to navigate the environment correctly, so that AI movement is smooth.

#### Acceptance Criteria

1. THE Enemy_AI SHALL use Unity NavMeshAgent for pathfinding
2. ALL dungeon and region scenes SHALL have NavMesh baked
3. WHEN an enemy cannot reach its target, THE Enemy_AI SHALL find the closest reachable point
4. THE NavMesh SHALL be configured with appropriate agent radius and height for enemies
5. THE Editor SHALL provide a tool to rebake NavMesh for all scenes

---

### Requirement 20: Integración de Session Lock con Network

**User Story:** As a player, I want my character protected during network operations, so that data isn't corrupted.

#### Acceptance Criteria

1. WHEN connecting to a server, THE Network_System SHALL acquire session lock before sending character data
2. IF session lock fails, THEN THE Network_System SHALL abort connection with "Character in use" error
3. WHEN disconnecting, THE Network_System SHALL release session lock
4. IF connection is lost unexpectedly, THE Network_System SHALL release lock after timeout (5 minutes)

---

### Requirement 21: Manejo de Latencia Alta

**User Story:** As a player with unstable connection, I want the game to handle lag gracefully, so that I don't lose progress.

#### Acceptance Criteria

1. THE Network_System SHALL monitor round-trip latency continuously
2. WHEN latency exceeds 200ms, THE Network_System SHALL display a warning icon
3. WHEN latency exceeds 500ms, THE Network_System SHALL pause player actions locally
4. WHEN latency returns below 400ms, THE Network_System SHALL resume player actions
5. THE paused state SHALL prevent ability usage, movement commands, and loot interactions
6. THE Network_System SHALL display "High Latency - Actions Paused" message during pause

---

### Requirement 22: Testing Framework Setup

**User Story:** As a developer, I want property-based testing available, so that I can verify system correctness.

#### Acceptance Criteria

1. THE project SHALL include FsCheck library for property-based testing
2. THE test assembly SHALL reference FsCheck and FsCheck.NUnit
3. THE project SHALL include custom generators for game data types (CharacterData, ItemData, etc.)
4. ALL property tests SHALL run minimum 100 iterations
5. THE CI/CD pipeline SHALL run property tests on every commit

---

## Technical Debt - Código Existente a Modificar

### Modificaciones Requeridas

| Archivo | Cambio Requerido |
|---------|------------------|
| `CharacterData.cs` | Agregar DataVersion, SecondaryResource, ResourceType, LootAttempts, LockedBossIds, Inventory, CurrentMana, MaxMana |
| `Enemy.cs` | Agregar Dictionary<ulong, float> _damageByPlayer para tracking |
| `AbilitySystem.cs` | Integrar ManaSystem, ProcSystem, SecondaryResourceSystem |
| `NetworkSessionManager.cs` | Integrar SessionLockManager, LatencyMonitor |
| `Packages/manifest.json` | Agregar FsCheck como dependencia |

### Escenas que Requieren NavMesh

- `Scenes/Dungeons/Dungeon_Crypt_Small.unity` (nueva)
- `Scenes/Regions/Roca.unity` (si existe)
- `Scenes/TestScene.unity` (existente)

### Conflicto de Specs

La spec existente en `.kiro/specs/requirements.md` contiene 20 requirements generales. Esta nueva spec `mvp-10-features` es un subconjunto enfocado en las 10 features del MVP. Ambas specs son complementarias:
- `.kiro/specs/` = Spec general del proyecto completo
- `.kiro/specs/mvp-10-features/` = Spec específica para el MVP

No hay conflicto real, pero las tareas deben referenciar ambas specs cuando aplique.
