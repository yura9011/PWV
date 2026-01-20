# Phase 1 - Plan 1

**Phase**: Phase 1: AOE Foundations
**Plan Number**: 1
**Created**: 2026-01-20
**Wave**: 1

---

## Goal
Create the foundational scripts for AOE detection (circular and cone) and basic visual indicators using Unity's built-in APIs, establishing the core architecture for all three AOE types.

---

## Prerequisites
- [x] Research completed (Phase 1 Research)
- [x] Unity project accessible (PWV-main)
- [x] TestPlayer system available for testing
- [x] Enemy objects with "Enemy" tag available in scenes

---

## Tasks

### Task 1: Create AOE Detection Utility Class

**Description**: Create a static utility class that handles all area detection logic using manual distance and angle calculations.

**Files to Create**:
- `Assets/_Project/Scripts/AOE_Testing/AreaDetector.cs`

**Content**:
```csharp
// Static utility class with methods:
// - GetEnemiesInRadius(Vector3 center, float radius)
// - GetEnemiesInCone(Vector3 origin, Vector3 forward, float angle, float range)
// - GetEnemiesInArea(Vector3 center, float radius, LayerMask layers) // Generic version
// Uses GameObject.FindGameObjectsWithTag("Enemy") and distance/angle calculations
```

**Implementation Notes**:
- Use Vector3.Distance() for circular detection
- Use Vector3.Angle() for cone detection
- Include LayerMask support for future flexibility
- Add debug logging for testing purposes

**Verification**:
- [ ] Script compiles without errors
- [ ] Methods return correct enemy lists when tested with known positions
- [ ] Console logs show detection results

---

### Task 2: Create Visual Indicator System

**Description**: Create a system for displaying AOE areas using LineRenderer components with simple circle and cone shapes.

**Files to Create**:
- `Assets/_Project/Scripts/AOE_Testing/AOEVisualIndicator.cs`

**Content**:
```csharp
// MonoBehaviour class with methods:
// - ShowCircle(Vector3 center, float radius, Color color)
// - ShowCone(Vector3 origin, Vector3 forward, float angle, float range, Color color)
// - Hide() // Clear all indicators
// Uses LineRenderer component for drawing
```

**Implementation Notes**:
- Use LineRenderer with 32 segments for circles
- Use LineRenderer with 4 points for cones (origin + 2 sides + back to origin)
- Set useWorldSpace = true
- Use semi-transparent materials for better visibility
- Raise indicators 0.1f above ground to avoid z-fighting

**Verification**:
- [ ] Circle indicators appear correctly at specified positions
- [ ] Cone indicators show proper angle and range
- [ ] Indicators are visible and properly colored
- [ ] Hide() method clears all visuals

---

### Task 3: Create Ground Targeting Controller

**Description**: Implement mouse-to-ground targeting using camera raycast for ground-targeted AOE abilities.

**Files to Create**:
- `Assets/_Project/Scripts/AOE_Testing/GroundTargetingTest.cs`

**Content**:
```csharp
// MonoBehaviour class for testing ground targeting:
// - Update() method that tracks mouse position
// - GetGroundPosition(Camera camera) using ScreenPointToRay
// - Visual indicator that follows mouse on ground
// - Key press (G) to confirm target and detect enemies in area
```

**Implementation Notes**:
- Use Camera.main or find camera by tag
- Raycast against LayerMask for ground objects
- Show circle indicator at mouse position
- On key press, detect enemies and log results
- Handle cases where raycast misses ground

**Verification**:
- [ ] Circle indicator follows mouse cursor on ground
- [ ] Pressing G key detects enemies in targeted area
- [ ] Console shows list of detected enemies
- [ ] Works on different terrain heights

---

### Task 4: Create Player-Centered AOE Test

**Description**: Implement AOE detection centered on the player position for instant area effects.

**Files to Create**:
- `Assets/_Project/Scripts/AOE_Testing/PlayerCenteredTest.cs`

**Content**:
```csharp
// MonoBehaviour class attached to TestPlayer:
// - Key press (R) to trigger player-centered AOE
// - Fixed radius (5 units) around player
// - Visual indicator appears around player
// - Detects and logs enemies in range
```

**Implementation Notes**:
- Get player position from transform
- Use AreaDetector.GetEnemiesInRadius()
- Show circle indicator centered on player
- Clear indicator after 2 seconds
- Add cooldown to prevent spam

**Verification**:
- [ ] Pressing R shows circle around player
- [ ] Detects enemies within 5 unit radius
- [ ] Visual indicator appears and disappears correctly
- [ ] Console logs detected enemies

---

### Task 5: Create Cone Attack Test

**Description**: Implement frontal cone attack detection based on player's forward direction.

**Files to Create**:
- `Assets/_Project/Scripts/AOE_Testing/ConeAttackTest.cs`

**Content**:
```csharp
// MonoBehaviour class attached to TestPlayer:
// - Key press (T) to trigger cone attack
// - 60-degree cone, 8 unit range in forward direction
// - Visual cone indicator in front of player
// - Detects and logs enemies in cone area
```

**Implementation Notes**:
- Use player's transform.forward for cone direction
- 60-degree angle, 8-unit range (configurable)
- Use AreaDetector.GetEnemiesInCone()
- Show cone indicator for 2 seconds
- Consider player rotation when calculating cone

**Verification**:
- [ ] Pressing T shows cone in front of player
- [ ] Cone rotates with player direction
- [ ] Detects enemies within cone area only
- [ ] Visual cone matches detection area

---

### Task 6: Create Test Scene Setup

**Description**: Set up a simple test scene with enemies positioned to validate all AOE types.

**Files to Create**:
- `Assets/_Project/Scenes/AOE_Testing/AOE_TestScene.unity`

**Content**:
```
// Scene setup:
// - TestPlayer at center (0,0,0)
// - 8-10 enemy objects positioned in various patterns
// - Ground plane for raycast targeting
// - Camera positioned for good visibility
// - Instructions UI text for key bindings
```

**Implementation Notes**:
- Use existing TestPlayer prefab
- Place enemies at known distances and angles for testing
- Ensure ground has proper collider for raycast
- Add simple UI text showing controls (G, R, T keys)
- Set appropriate layers for ground and enemies

**Verification**:
- [ ] Scene loads without errors
- [ ] TestPlayer can move normally
- [ ] All test scripts work in the scene
- [ ] Enemies are properly positioned for testing

---

## Success Criteria

- [ ] All three AOE detection types work correctly (circular, cone, ground-targeted)
- [ ] Visual indicators accurately represent detection areas
- [ ] Ground targeting follows mouse cursor smoothly
- [ ] Player-centered and cone attacks work from player position
- [ ] Console logging shows correct enemy detection results
- [ ] Test scene allows validation of all features
- [ ] No compilation errors or runtime exceptions

---

## Rollback Plan

If this plan fails:
1. Remove all created scripts from AOE_Testing folder
2. Delete test scene if created
3. Revert any modifications to existing scripts
4. Document specific failure points for plan revision

---

## Estimated Effort
Medium - 4 hours total
- Task 1: 1 hour (core detection logic)
- Task 2: 1 hour (visual indicators)
- Task 3: 1 hour (ground targeting)
- Task 4: 30 minutes (player-centered)
- Task 5: 30 minutes (cone attack)
- Task 6: 30 minutes (scene setup)

---

## XML Structure

```xml
<plan id="phase-1-plan-1" wave="1">
  <goal>Create foundational AOE detection and visual indicator scripts</goal>
  
  <prerequisites>
    <item>Research completed</item>
    <item>Unity project accessible</item>
    <item>TestPlayer system available</item>
    <item>Enemy objects available</item>
  </prerequisites>
  
  <tasks>
    <task id="1">
      <name>Create AOE Detection Utility Class</name>
      <description>Static utility class for area detection logic</description>
      <files>
        <create>Assets/_Project/Scripts/AOE_Testing/AreaDetector.cs</create>
      </files>
      <changes>
        <change>GetEnemiesInRadius method with distance calculations</change>
        <change>GetEnemiesInCone method with angle calculations</change>
        <change>Debug logging for testing</change>
      </changes>
      <verification>
        <check>Script compiles without errors</check>
        <check>Methods return correct enemy lists</check>
        <check>Console logs show detection results</check>
      </verification>
    </task>
    
    <task id="2">
      <name>Create Visual Indicator System</name>
      <description>LineRenderer-based visual indicators for AOE areas</description>
      <files>
        <create>Assets/_Project/Scripts/AOE_Testing/AOEVisualIndicator.cs</create>
      </files>
      <changes>
        <change>ShowCircle method with LineRenderer</change>
        <change>ShowCone method with LineRenderer</change>
        <change>Hide method to clear indicators</change>
      </changes>
      <verification>
        <check>Circle indicators appear correctly</check>
        <check>Cone indicators show proper angle and range</check>
        <check>Hide method clears all visuals</check>
      </verification>
    </task>
    
    <task id="3">
      <name>Create Ground Targeting Controller</name>
      <description>Mouse-to-ground targeting with camera raycast</description>
      <files>
        <create>Assets/_Project/Scripts/AOE_Testing/GroundTargetingTest.cs</create>
      </files>
      <changes>
        <change>GetGroundPosition using ScreenPointToRay</change>
        <change>Visual indicator following mouse</change>
        <change>Key press confirmation and enemy detection</change>
      </changes>
      <verification>
        <check>Circle indicator follows mouse cursor</check>
        <check>Key press detects enemies in area</check>
        <check>Works on different terrain heights</check>
      </verification>
    </task>
    
    <task id="4">
      <name>Create Player-Centered AOE Test</name>
      <description>AOE detection centered on player position</description>
      <files>
        <create>Assets/_Project/Scripts/AOE_Testing/PlayerCenteredTest.cs</create>
      </files>
      <changes>
        <change>Key press trigger for player-centered AOE</change>
        <change>Circle indicator around player</change>
        <change>Enemy detection in radius</change>
      </changes>
      <verification>
        <check>Circle appears around player on key press</check>
        <check>Detects enemies within radius</check>
        <check>Visual indicator timing works correctly</check>
      </verification>
    </task>
    
    <task id="5">
      <name>Create Cone Attack Test</name>
      <description>Frontal cone attack detection</description>
      <files>
        <create>Assets/_Project/Scripts/AOE_Testing/ConeAttackTest.cs</create>
      </files>
      <changes>
        <change>Key press trigger for cone attack</change>
        <change>Cone indicator in forward direction</change>
        <change>Enemy detection in cone area</change>
      </changes>
      <verification>
        <check>Cone shows in front of player</check>
        <check>Cone rotates with player direction</check>
        <check>Detects enemies within cone only</check>
      </verification>
    </task>
    
    <task id="6">
      <name>Create Test Scene Setup</name>
      <description>Test scene with positioned enemies for validation</description>
      <files>
        <create>Assets/_Project/Scenes/AOE_Testing/AOE_TestScene.unity</create>
      </files>
      <changes>
        <change>TestPlayer at center position</change>
        <change>Multiple enemy objects positioned for testing</change>
        <change>Ground plane with proper collider</change>
        <change>UI instructions for controls</change>
      </changes>
      <verification>
        <check>Scene loads without errors</check>
        <check>All test scripts work in scene</check>
        <check>Enemies properly positioned</check>
      </verification>
    </task>
  </tasks>
  
  <success_criteria>
    <criterion>All three AOE detection types work correctly</criterion>
    <criterion>Visual indicators accurately represent areas</criterion>
    <criterion>Ground targeting follows mouse smoothly</criterion>
    <criterion>Console logging shows correct results</criterion>
    <criterion>No compilation or runtime errors</criterion>
  </success_criteria>
</plan>
```