# Phase 1: AOE Foundations - Context

## Phase Overview
Create the foundational architecture for the AOE spell system, including core classes, enums, and detection algorithms.

## Current System Analysis

### Existing AbilitySystem
Located in `Assets/_Project/Scripts/Combat/`:
- `AbilitySystem.cs` - Main ability execution system
- `AbilityDataSO.cs` - ScriptableObject for ability data
- `AbilityData.cs` - Data model for abilities

### Current Combat Flow
```
Player Input → AbilitySystem.ExecuteAbility() → Single Target → Apply Damage
```

### Target System
- `TargetingSystem.cs` - Single target selection
- Tab targeting and click targeting implemented
- Works with `ITargetable` interface

## Implementation Strategy

### 1. Extend Existing Architecture
Rather than replace, we'll extend the current system:
- Create `AOEAbilityData` inheriting from `AbilityDataSO`
- Add AOE detection to existing flow
- Maintain compatibility with single-target abilities

### 2. Core Components to Create

**AOETargetingSystem.cs**
- Singleton pattern for global access
- Manages all AOE targeting modes
- Coordinates with existing TargetingSystem

**AOEParameters.cs**
- Data structure for AOE configuration
- Serializable for ScriptableObject integration
- Includes radius, angle, range, max targets

**AreaDetector.cs**
- Static utility class for area detection
- Methods for circular, cone, and custom shapes
- Optimized algorithms using Unity Physics

### 3. Detection Algorithms

**Circular Detection**
```csharp
public static List<ITargetable> DetectInCircle(Vector3 center, float radius, LayerMask layers)
{
    Collider[] colliders = Physics.OverlapSphere(center, radius, layers);
    return FilterTargetables(colliders);
}
```

**Cone Detection**
```csharp
public static List<ITargetable> DetectInCone(Vector3 origin, Vector3 forward, float angle, float range, LayerMask layers)
{
    // First get all in sphere, then filter by angle
    Collider[] colliders = Physics.OverlapSphere(origin, range, layers);
    return FilterByCone(colliders, origin, forward, angle);
}
```

## Technical Considerations

### Performance
- Use `Physics.OverlapSphereNonAlloc` for frequent calls
- Cache results when possible
- Limit detection frequency (e.g., 10Hz for channeling)

### Integration Points
- Hook into existing `AbilitySystem.ExecuteAbility()`
- Extend `CombatSystem.ApplyDamage()` for multiple targets
- Use existing `ITargetable` interface

### Visual Feedback
- Create indicator prefabs for each AOE type
- Use simple materials with transparency
- Object pooling for performance

## Unity API References

Based on the agent instructions, search for:
```powershell
.\Unityscr4\unity-docs-scraper\search-unity-docs.ps1 -Query "Physics OverlapSphere"
.\Unityscr4\unity-docs-scraper\search-unity-docs.ps1 -Query "Collider bounds"
.\Unityscr4\unity-docs-scraper\search-unity-docs.ps1 -Query "Vector3 angle"
```

## File Structure
```
Assets/_Project/Scripts/Combat/AOE/
├── AOETargetingSystem.cs
├── AOEAbilityData.cs
├── AOEParameters.cs
├── AreaDetector.cs
└── Enums/
    └── AOEEnums.cs
```

## Testing Strategy
- Create test scene with multiple dummy enemies
- Test each detection algorithm independently
- Verify performance with 20+ targets
- Validate integration with existing systems

## Success Criteria for Phase 1
- [ ] AOE architecture classes created and compiling
- [ ] Basic circular detection working
- [ ] Basic cone detection working
- [ ] Integration hooks with AbilitySystem identified
- [ ] Test scene setup with validation scripts

## Next Phase Preparation
Phase 2 will focus on Ground Targeting, which requires:
- Camera raycast system
- Ground layer detection
- Visual indicator movement
- Input handling for confirmation/cancellation

The foundation created in Phase 1 will support all three AOE types.