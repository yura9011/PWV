# Implementation Plan: Gameplay Testing & Polish

## Overview

Plan de implementación para verificar y mejorar el gameplay de The Ether Domes. Las tareas están organizadas en orden de dependencia, comenzando con la verificación del movimiento existente, seguido por la creación de enemigos, migración del Input System, y finalmente los property tests.

## Tasks

### Phase 1: Verificar y Mejorar Movimiento

- [x] 1. Verificar PlayerController existente
  - [x] 1.1 Revisar implementación actual de PlayerController
    - Verificar que usa IsOwner check
    - Verificar movimiento relativo a cámara
    - Identificar uso de Input.GetAxis (legacy)
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6_
  - [x] 1.2 Corregir problemas encontrados en PlayerController
    - Asegurar que movimiento funciona con WASD
    - Asegurar que diagonal movement funciona
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5_

- [x] 2. Checkpoint - Verificar movimiento básico
  - Ensure all tests pass, ask the user if questions arise.
  - Probar en Unity: WASD mueve al jugador
  - Probar: Movimiento diagonal funciona

### Phase 2: Crear Enemigo Básico

- [x] 3. Implementar Enemy class
  - [x] 3.1 Crear Enemy.cs con ITargetable
    - Implementar ITargetable interface
    - Agregar NetworkVariable para health
    - Agregar NetworkVariable para isAlive
    - Implementar TakeDamage y Die
    - _Requirements: 2.1, 2.4_
  - [x] 3.2 Crear Enemy prefab
    - Crear prefab con capsule mesh (rojo)
    - Agregar NetworkObject component
    - Agregar Enemy component
    - Agregar collider para targeting
    - _Requirements: 2.2, 2.3_
  - [x] 3.3 Agregar indicador visual de target
    - Crear child object con círculo/highlight
    - Mostrar cuando enemy es targeted
    - Ocultar cuando no es targeted
    - _Requirements: 2.7_

- [x] 4. Integrar Enemy con TargetSystem
  - [x] 4.1 Verificar TargetSystem detecta enemigos
    - Asegurar que Tab cycling incluye enemigos en rango
    - Asegurar que enemigos fuera de 40m no se incluyen
    - _Requirements: 2.5_
  - [x] 4.2 Verificar limpieza de targets muertos
    - Asegurar que enemigos muertos se remueven de targets
    - _Requirements: 2.6_

- [x] 5. Agregar enemigos a la escena de test
  - Modificar EtherDomesSetup para crear enemigos de prueba
  - Posicionar 3-5 enemigos en la escena
  - _Requirements: 2.1, 2.2, 2.3_

- [x] 6. Checkpoint - Verificar Tab-Target
  - Ensure all tests pass, ask the user if questions arise.
  - Probar: Tab cicla entre enemigos
  - Probar: Click selecciona enemigo
  - Probar: Escape deselecciona

### Phase 3: Migrar a Unity Input System

- [x] 7. Configurar Unity Input System
  - [x] 7.1 Verificar paquete instalado
    - Confirmar com.unity.inputsystem en manifest.json ✓ (1.7.0)
    - Configurar Project Settings > Player > Active Input Handling = Both
    - _Requirements: 3.1_
  - [x] 7.2 Crear Input Action Asset
    - Crear EtherDomesInput.inputactions
    - Definir Player action map
    - Agregar Move (Vector2), Tab, Escape, Ability1-9
    - Configurar bindings para keyboard y gamepad
    - _Requirements: 3.2, 3.3, 3.6_

- [x] 8. Actualizar PlayerController para Input System
  - [x] 8.1 Refactorizar PlayerController
    - Reemplazar Input.GetAxis con InputAction
    - Agregar InputActionReference fields
    - Implementar callbacks para input
    - _Requirements: 3.4_
  - [x] 8.2 Actualizar Player prefab
    - Agregar PlayerInput component (opcional)
    - Asignar Input Action Asset
    - _Requirements: 3.4_

- [x] 9. Actualizar otros sistemas para Input System
  - [x] 9.1 Actualizar TargetSystem input
    - Usar InputAction para Tab y Escape
    - _Requirements: 3.3_
  - [x] 9.2 Actualizar AbilitySystem input
    - Usar InputAction para Ability1-9
    - _Requirements: 3.3_

- [x] 10. Checkpoint - Verificar Input System
  - Ensure all tests pass, ask the user if questions arise.
  - Probar: WASD funciona con nuevo sistema
  - Probar: Tab/Escape funcionan
  - Probar: Habilidades 1-9 funcionan

### Phase 4: Property Tests

- [x] 11. Configurar framework de testing
  - [x] 11.1 Crear estructura de tests
    - Crear carpeta Assets/Tests/EditMode ✓ (ya existía)
    - Crear assembly definition para tests ✓ (actualizado)
    - Configurar NUnit ✓
    - _Requirements: 4.7_

- [x] 12. Implementar Property Tests
  - [x] 12.1 Write property test for Movement Direction
    - **Property 1: Movement Direction Relative to Camera**
    - **Validates: Requirements 1.1, 1.2, 1.3, 1.4, 1.5**
  - [x] 12.2 Write property test for Ownership Blocking
    - **Property 2: Ownership-Based Input Blocking**
    - **Validates: Requirements 1.6**
  - [x] 12.3 Write property test for Target Range
    - **Property 3: Target Range Inclusion**
    - **Validates: Requirements 2.5**
  - [x] 12.4 Write property test for Dead Target Removal
    - **Property 4: Dead Target Removal**
    - **Validates: Requirements 2.6**
  - [x] 12.5 Write property test for Binding Persistence
    - **Property 5: Input Binding Persistence Round-Trip**
    - **Validates: Requirements 3.5**

- [x] 13. Implementar Property Tests de sistemas existentes
  - [x] 13.1 Write property test for Session State Consistency
    - **Property: Session State Consistency**
    - **Validates: Requirements from main spec 1.1, 1.3**
  - [x] 13.2 Write property test for Threat Calculation
    - **Property: Threat Calculation Correctness**
    - **Validates: Requirements from main spec 10.1**
  - [x] 13.3 Write property test for XP Calculation
    - **Property: Experience Calculation**
    - **Validates: Requirements from main spec 14.1, 14.6**
  - [x] 13.4 Write property test for Character Persistence
    - **Property: Character Persistence Round-Trip**
    - **Validates: Requirements from main spec 7.1, 7.3**

- [x] 14. Checkpoint final
  - Ensure all tests pass, ask the user if questions arise.
  - Verificar: Movimiento funciona correctamente
  - Verificar: Tab-Target funciona con enemigos
  - Verificar: Input System migrado
  - Verificar: Property tests pasan

### Phase 5: Combat UI y Habilidades

- [x] 15. Implementar Target Frame UI
  - [x] 15.1 Crear CombatTestUI.cs con OnGUI
    - Mostrar nombre del target y nivel
    - Mostrar barra de salud con colores (verde/amarillo/rojo)
    - Mostrar distancia y estado "In Range"/"Out of Range"
  - [x] 15.2 Actualizar ITargetable interface
    - Agregar CurrentHealth y MaxHealth properties

- [x] 16. Implementar Action Bar y Habilidades
  - [x] 16.1 Crear habilidades de prueba
    - Strike (instant, sin cooldown, 15 daño)
    - Heavy Strike (instant, 6s cooldown, 35 daño)
    - Fireball (2s cast, 30 rango, 50 daño)
    - Execute (instant, 15s cooldown, 100 daño)
  - [x] 16.2 Conectar AbilitySystem con Enemy.TakeDamage
    - OnAbilityExecuted aplica daño al target
  - [x] 16.3 Mostrar Action Bar en UI
    - Botones 1-4 con nombres de habilidades
    - Mostrar cooldowns restantes
    - Indicar GCD activo

- [x] 17. Integrar CombatTestUI en escena de test
  - [x] 17.1 Actualizar EtherDomesSetup
    - Agregar CombatTestUI al TestUI GameObject
  - [x] 17.2 Actualizar EtherDomes.UI.asmdef
    - Agregar referencia a EtherDomes.Enemy

## Notes

- Todas las tareas son requeridas para implementación completa
- Cada fase tiene un checkpoint para validación incremental
- La migración al Input System requiere cambiar Project Settings
- Los property tests usan NUnit con generadores custom
