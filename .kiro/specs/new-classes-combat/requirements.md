# Requirements Document

## Introduction

Este documento define los requisitos para agregar 4 nuevas clases al juego (Rogue, Hunter, Warlock, Death Knight) junto con los sistemas de soporte necesarios: buffs/debuffs básicos, habilidades canalizadas, mascotas pasivas y sigilo simplificado.

## Glossary

- **Buff**: Efecto positivo temporal que mejora las estadísticas o capacidades de un personaje
- **Debuff**: Efecto negativo temporal que reduce las estadísticas o capacidades de un personaje
- **DoT**: Damage over Time - daño que se aplica periódicamente durante una duración
- **HoT**: Healing over Time - curación que se aplica periódicamente durante una duración
- **Channeled_Ability**: Habilidad que aplica su efecto durante el tiempo de canalización, no al final
- **Pet**: Entidad controlada por el jugador que ataca automáticamente
- **Stealth**: Estado de invisibilidad que se rompe al atacar o recibir daño
- **Combo_Points**: Recurso secundario del Rogue que se acumula con ataques y se gasta en finishers
- **Energy**: Recurso primario del Rogue que regenera rápidamente
- **Focus**: Recurso primario del Hunter que regenera moderadamente
- **AbilitySystem**: Sistema existente que maneja ejecución de habilidades
- **CombatSystem**: Sistema existente que maneja daño, curación y muerte
- **ClassSystem**: Sistema existente que maneja clases y especializaciones

## Requirements

### Requirement 1: Sistema de Buffs/Debuffs Básico

**User Story:** As a player, I want to apply and receive temporary effects, so that combat has more depth and strategy.

#### Acceptance Criteria

1. THE Buff_System SHALL support effects with duration between 1 and 300 seconds
2. WHEN a buff/debuff is applied, THE Buff_System SHALL track the remaining duration
3. WHEN a buff/debuff expires, THE Buff_System SHALL remove the effect and notify the UI
4. THE Buff_System SHALL support DoT effects that apply damage every tick interval
5. THE Buff_System SHALL support HoT effects that apply healing every tick interval
6. WHEN a DoT/HoT ticks, THE Buff_System SHALL apply the damage/healing through CombatSystem
7. THE Buff_System SHALL limit each entity to a maximum of 20 active buffs and 20 active debuffs
8. WHEN a new buff/debuff would exceed the limit, THE Buff_System SHALL replace the oldest effect of the same type
9. THE Buff_UI SHALL display active buffs/debuffs as icons with remaining duration
10. WHEN displaying buffs/debuffs, THE Buff_UI SHALL show buffs above the health bar and debuffs below

### Requirement 2: Habilidades Canalizadas

**User Story:** As a player, I want to use channeled abilities, so that I can deal sustained damage or healing over time.

#### Acceptance Criteria

1. THE AbilitySystem SHALL support channeled abilities with configurable channel duration
2. WHEN a channeled ability starts, THE AbilitySystem SHALL apply effects at regular tick intervals
3. THE AbilitySystem SHALL configure tick interval per ability (default 1 second)
4. WHEN the player moves during a channel, THE AbilitySystem SHALL interrupt the channel
5. WHEN a channel is interrupted, THE AbilitySystem SHALL stop applying further ticks
6. THE CastBar SHALL display channel progress differently from cast progress (depleting instead of filling)
7. WHEN a channeled ability completes all ticks, THE AbilitySystem SHALL fire OnChannelCompleted event

### Requirement 3: Sistema de Mascotas Pasivas

**User Story:** As a Hunter or Warlock, I want a pet that follows me and attacks my target, so that I have additional damage output.

#### Acceptance Criteria

1. THE Pet_System SHALL spawn a pet entity when the player summons it
2. WHEN the player has a target, THE Pet SHALL automatically attack that target
3. WHEN the player has no target, THE Pet SHALL follow the player at 3 meters distance
4. THE Pet SHALL have its own health pool separate from the player
5. WHEN the pet dies, THE Pet_System SHALL allow resummoning after a 10 second cooldown
6. THE Pet SHALL use NavMeshAgent for pathfinding
7. WHEN the player enters a new zone, THE Pet SHALL teleport to the player if distance exceeds 40 meters
8. THE Pet_UI SHALL display pet health bar near the player frame

### Requirement 4: Sistema de Sigilo Simplificado

**User Story:** As a Rogue, I want to enter stealth, so that I can approach enemies undetected and use special abilities.

#### Acceptance Criteria

1. WHEN the Rogue activates Stealth, THE Stealth_System SHALL make the player invisible to enemies
2. WHEN the Rogue attacks from stealth, THE Stealth_System SHALL break stealth immediately
3. WHEN the Rogue receives damage, THE Stealth_System SHALL break stealth immediately
4. WHILE in stealth, THE Rogue SHALL move at 70% normal speed
5. THE Stealth_System SHALL enable special abilities that require stealth (Cheap Shot, Ambush)
6. WHEN stealth breaks, THE Stealth_System SHALL apply a 2 second cooldown before re-entering stealth
7. THE Stealth_Visual SHALL reduce player opacity to 30% for the local player and 0% for enemies

### Requirement 5: Clase Rogue

**User Story:** As a player, I want to play as a Rogue, so that I can deal burst damage from stealth and use combo points.

#### Acceptance Criteria

1. THE ClassSystem SHALL support Rogue class with Assassination and Combat specializations
2. THE Rogue SHALL use Energy as primary resource (100 max, regenerates 10 per second)
3. THE Rogue SHALL use Combo Points as secondary resource (5 max, generated by attacks)
4. WHEN a Rogue uses a combo point generator, THE ClassSystem SHALL add 1 combo point
5. WHEN a Rogue uses a finisher, THE ClassSystem SHALL consume all combo points and scale damage
6. THE Rogue_Assassination_Spec SHALL have abilities: Mutilate, Envenom, Rupture, Vendetta
7. THE Rogue_Combat_Spec SHALL have abilities: Sinister Strike, Eviscerate, Blade Flurry, Adrenaline Rush
8. THE Rogue SHALL have access to Stealth, Cheap Shot, Ambush, Kick (interrupt) regardless of spec

### Requirement 6: Clase Hunter

**User Story:** As a player, I want to play as a Hunter, so that I can deal ranged damage with a pet companion.

#### Acceptance Criteria

1. THE ClassSystem SHALL support Hunter class with Beast Mastery and Marksmanship specializations
2. THE Hunter SHALL use Focus as primary resource (100 max, regenerates 5 per second)
3. THE Hunter SHALL have a permanent pet that attacks automatically
4. THE Hunter_BeastMastery_Spec SHALL have abilities: Kill Command, Bestial Wrath, Cobra Shot, Multi-Shot
5. THE Hunter_Marksmanship_Spec SHALL have abilities: Aimed Shot, Rapid Fire, Arcane Shot, Volley
6. THE Hunter SHALL have access to Concussive Shot (slow), Freezing Trap, Disengage, Counter Shot (interrupt)
7. WHEN Hunter uses Kill Command, THE Pet SHALL perform a special attack on the target
8. THE Hunter SHALL have minimum range of 8 meters for most abilities (dead zone)

### Requirement 7: Clase Warlock

**User Story:** As a player, I want to play as a Warlock, so that I can deal damage over time and summon demons.

#### Acceptance Criteria

1. THE ClassSystem SHALL support Warlock class with Affliction and Destruction specializations
2. THE Warlock SHALL use Mana as primary resource
3. THE Warlock SHALL have a permanent demon pet that attacks automatically
4. THE Warlock_Affliction_Spec SHALL have abilities: Corruption (DoT), Agony (DoT), Drain Life (channel), Haunt
5. THE Warlock_Destruction_Spec SHALL have abilities: Chaos Bolt, Incinerate, Immolate (DoT), Rain of Fire (AoE)
6. THE Warlock SHALL have access to Fear (CC), Shadowfury (stun), Spell Lock (interrupt via pet)
7. WHEN Warlock uses Drain Life, THE AbilitySystem SHALL heal the Warlock for 50% of damage dealt
8. THE Warlock_Pet SHALL be a Voidwalker (tank) or Imp (damage) based on spec

### Requirement 8: Clase Death Knight

**User Story:** As a player, I want to play as a Death Knight, so that I can be a melee fighter with disease mechanics.

#### Acceptance Criteria

1. THE ClassSystem SHALL support Death Knight class with Blood and Frost specializations
2. THE Death_Knight SHALL use Mana as primary resource (simplified from runes)
3. THE Death_Knight_Blood_Spec SHALL be a tank spec with self-healing abilities
4. THE Death_Knight_Frost_Spec SHALL be a DPS spec with dual-wield or two-handed options
5. THE Death_Knight_Blood_Spec SHALL have abilities: Death Strike (heal), Heart Strike, Blood Boil, Vampiric Blood
6. THE Death_Knight_Frost_Spec SHALL have abilities: Obliterate, Frost Strike, Howling Blast, Pillar of Frost
7. THE Death_Knight SHALL have access to Death Grip (pull), Chains of Ice (slow), Mind Freeze (interrupt)
8. WHEN Death Knight uses Death Strike, THE CombatSystem SHALL heal for 25% of damage taken in last 5 seconds



### Requirement 9: Integración con UI de Target

**User Story:** As a player, I want to see buffs and debuffs on my target, so that I can track my DoTs and enemy abilities.

#### Acceptance Criteria

1. WHEN a target is selected, THE TargetFrame SHALL display the target's active buffs
2. WHEN a target is selected, THE TargetFrame SHALL display the target's active debuffs
3. THE TargetFrame SHALL show buff/debuff icons with remaining duration as countdown
4. WHEN a buff/debuff is applied to the target, THE TargetFrame SHALL update immediately
5. WHEN a buff/debuff expires on the target, THE TargetFrame SHALL remove the icon immediately
6. THE TargetFrame SHALL highlight debuffs applied by the local player with a colored border

### Requirement 10: Recursos Secundarios por Clase

**User Story:** As a player of different classes, I want class-specific resource mechanics, so that each class feels unique.

#### Acceptance Criteria

1. THE SecondaryResourceSystem SHALL support Energy for Rogue (regenerates constantly)
2. THE SecondaryResourceSystem SHALL support Focus for Hunter (regenerates constantly)
3. THE SecondaryResourceSystem SHALL support Combo Points for Rogue (generated by attacks)
4. WHEN Rogue uses a generator ability, THE SecondaryResourceSystem SHALL add combo points
5. WHEN Rogue uses a finisher ability, THE SecondaryResourceSystem SHALL consume all combo points
6. THE Resource_UI SHALL display the appropriate resource bar based on class
7. THE Resource_UI SHALL display combo points as discrete pips (1-5) for Rogue

### Requirement 11: Habilidades con Efectos Especiales

**User Story:** As a player, I want abilities with special effects, so that combat is more engaging.

#### Acceptance Criteria

1. WHEN a slow effect is applied, THE Movement_System SHALL reduce target speed by the specified percentage
2. WHEN a stun effect is applied, THE AbilitySystem SHALL prevent the target from acting for the duration
3. WHEN a fear effect is applied, THE AI_System SHALL make the target run in random directions
4. WHEN a pull effect is applied (Death Grip), THE Movement_System SHALL move the target to the caster
5. WHEN a knockback effect is applied (Disengage), THE Movement_System SHALL move the caster away from target
6. THE Effect_System SHALL support diminishing returns on CC effects (stun, fear, slow)
7. AFTER 3 applications of the same CC type, THE Effect_System SHALL make the target immune for 15 seconds

### Requirement 12: Balance de Stats por Clase

**User Story:** As a game designer, I want balanced base stats per class, so that each class has appropriate strengths.

#### Acceptance Criteria

1. THE ClassSystem SHALL define base stats for each new class at level 1
2. THE Rogue SHALL have high Agility, medium Stamina, low Strength and Intellect
3. THE Hunter SHALL have high Agility, medium Stamina, low Strength and Intellect
4. THE Warlock SHALL have high Intellect, medium Stamina, low Strength and Agility
5. THE Death_Knight SHALL have high Strength, high Stamina, medium Intellect, low Agility
6. WHEN a character levels up, THE ProgressionSystem SHALL increase stats based on class growth rates
7. THE ClassSystem SHALL provide stat growth rates per class (e.g., Rogue: +2 Agility, +1 Stamina per level)
