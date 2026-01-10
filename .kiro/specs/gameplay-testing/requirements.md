# Requirements Document

## Introduction

Este documento define los requisitos para la fase de testing y polish del gameplay de "The Ether Domes". Incluye verificación del movimiento del jugador, creación de enemigos para probar el sistema Tab-Target, migración al nuevo Input System, y preparación de property tests para validar la correctness de los sistemas core.

## Glossary

- **PlayerController**: Componente que procesa input de movimiento y controla el personaje del jugador.
- **Enemy_Prefab**: Prefab de enemigo básico con componentes de red y sistema de targeting.
- **Input_System_Package**: Unity Input System - sistema moderno de input que reemplaza al Input Manager legacy.
- **Property_Test**: Test automatizado que verifica propiedades universales usando generación aleatoria de inputs.
- **Tab_Target**: Sistema de selección de enemigos mediante la tecla Tab.
- **ITargetable**: Interfaz que permite a una entidad ser seleccionada como objetivo.

## Requirements

### Requirement 1: Verificación de Movimiento del Jugador

**User Story:** As a player, I want to move my character using WASD keys, so that I can navigate the game world.

#### Acceptance Criteria

1. WHEN a player presses W, THE PlayerController SHALL move the character forward relative to camera
2. WHEN a player presses S, THE PlayerController SHALL move the character backward relative to camera
3. WHEN a player presses A, THE PlayerController SHALL move the character left relative to camera
4. WHEN a player presses D, THE PlayerController SHALL move the character right relative to camera
5. WHEN a player presses diagonal keys (WA, WD, SA, SD), THE PlayerController SHALL move in the combined direction
6. WHEN IsOwner is false, THE PlayerController SHALL ignore all movement input
7. THE PlayerController SHALL synchronize position to all clients via NetworkTransform

### Requirement 2: Creación de Enemigo Básico para Testing

**User Story:** As a developer, I want a basic enemy prefab, so that I can test the Tab-Target combat system.

#### Acceptance Criteria

1. THE Enemy_Prefab SHALL implement ITargetable interface
2. THE Enemy_Prefab SHALL have a visible mesh (red capsule) distinguishable from players
3. THE Enemy_Prefab SHALL have a NetworkObject component for network synchronization
4. THE Enemy_Prefab SHALL have configurable health and display name
5. WHEN an enemy is within 40 meters, THE TargetSystem SHALL include it in Tab cycling
6. WHEN an enemy dies, THE TargetSystem SHALL remove it from valid targets
7. THE Enemy_Prefab SHALL have a visual indicator when targeted (highlight/circle)

### Requirement 3: Migración a Unity Input System

**User Story:** As a developer, I want to use the modern Input System, so that the project uses supported APIs and enables input rebinding.

#### Acceptance Criteria

1. THE Project SHALL use Unity Input System package instead of legacy Input Manager
2. THE Input_System SHALL define an InputActionAsset with Player action map
3. THE Player action map SHALL include Move (Vector2), Tab (Button), Escape (Button), Ability1-9 (Buttons)
4. THE PlayerController SHALL read input from InputAction references instead of Input.GetAxis
5. WHEN input bindings are changed, THE Input_System SHALL persist changes locally
6. THE Input_System SHALL support keyboard and gamepad input

### Requirement 4: Property Tests para Sistemas Core

**User Story:** As a developer, I want automated property tests, so that I can verify system correctness across many inputs.

#### Acceptance Criteria

1. THE Test_Suite SHALL include property tests for NetworkSessionManager state consistency
2. THE Test_Suite SHALL include property tests for TargetSystem Tab cycling
3. THE Test_Suite SHALL include property tests for AggroSystem threat calculations
4. THE Test_Suite SHALL include property tests for ProgressionSystem XP calculations
5. THE Test_Suite SHALL include property tests for CharacterPersistence round-trip
6. WHEN a property test runs, THE Test_Suite SHALL execute minimum 100 iterations
7. THE Test_Suite SHALL use NUnit with custom generators for domain types

