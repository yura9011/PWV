# Phase 1 - AOE System Research

**Phase**: Phase 1: AOE Foundations
**Created**: 2026-01-20
**Researcher**: Kiro AI Agent

---

## Research Goals

What we need to understand:
- Unity Physics APIs for area detection (circular and cone-shaped)
- Camera raycast methods for ground targeting
- LineRenderer capabilities for visual indicators
- Vector3 math operations for cone calculations
- Best practices for simple AOE implementation

---

## Findings

### Topic 1: Area Detection Methods

**Options Evaluated**:

1. **Physics.OverlapSphere (3D)**
   - **Pros**: Built-in Unity method, efficient, returns Collider[]
   - **Cons**: Not found in available documentation (may be newer API)
   - **Use Case**: Circular AOE detection around a point
   - **Documentation**: Not available in current docs

2. **Physics2D.OverlapCircle (2D Alternative)**
   - **Pros**: Available in docs, similar functionality for 2D
   - **Cons**: Only works for 2D physics
   - **Use Case**: 2D games or as reference for 3D implementation
   - **Documentation**: Available in api_physics2d_Physics2D.json

3. **Manual Collider Detection**
   - **Pros**: Full control, works with any Unity version
   - **Cons**: More complex implementation, potentially less efficient
   - **Use Case**: When built-in methods not available
   - **Implementation**: Use Collider.bounds.Contains() or distance checks

**Recommendation**: Use manual detection with distance calculations since Physics.OverlapSphere not found in docs. Fallback to iterating through nearby colliders and checking distances.

---

### Topic 2: Ground Targeting Implementation

**Camera Methods Evaluated**:

1. **Camera.ScreenPointToRay()**
   - **Description**: Converts screen position to world ray
   - **Pros**: Perfect for mouse-to-world conversion, built-in Unity method
   - **Cons**: None significant
   - **Example**: 
   ```csharp
   Ray ray = camera.ScreenPointToRay(Input.mousePosition);
   if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
   {
       Vector3 groundPoint = hit.point;
   }
   ```

2. **Camera.ScreenToWorldPoint()**
   - **Description**: Direct screen to world conversion
   - **Pros**: Direct conversion
   - **Cons**: Requires specific Z distance, less flexible than raycast
   - **Use Case**: When exact distance known

**Recommendation**: Use Camera.ScreenPointToRay() with Physics.Raycast for ground targeting as it's more flexible and handles terrain variations.

---

### Topic 3: Visual Indicators

**LineRenderer Analysis**:

**Key Properties**:
- `positionCount`: Set number of vertices for the line
- `SetPosition(index, Vector3)`: Set individual vertex positions
- `useWorldSpace`: Enable for world-space coordinates
- `startWidth/endWidth`: Control line thickness
- `startColor/endColor`: Control line colors
- `loop`: Connect start/end for closed shapes

**Circle Creation**:
```csharp
void CreateCircleIndicator(Vector3 center, float radius, int segments = 32)
{
    LineRenderer lr = GetComponent<LineRenderer>();
    lr.positionCount = segments + 1;
    lr.useWorldSpace = true;
    lr.loop = true;
    
    for (int i = 0; i <= segments; i++)
    {
        float angle = i * 2f * Mathf.PI / segments;
        Vector3 pos = center + new Vector3(
            Mathf.Cos(angle) * radius,
            0,
            Mathf.Sin(angle) * radius
        );
        lr.SetPosition(i, pos);
    }
}
```

**Cone Creation**:
```csharp
void CreateConeIndicator(Vector3 origin, Vector3 forward, float angle, float range)
{
    LineRenderer lr = GetComponent<LineRenderer>();
    lr.positionCount = 4; // Origin + 2 sides + back to origin
    lr.useWorldSpace = true;
    
    Vector3 left = Quaternion.AngleAxis(-angle/2, Vector3.up) * forward * range;
    Vector3 right = Quaternion.AngleAxis(angle/2, Vector3.up) * forward * range;
    
    lr.SetPosition(0, origin);
    lr.SetPosition(1, origin + left);
    lr.SetPosition(2, origin + right);
    lr.SetPosition(3, origin);
}
```

---

### Topic 4: Cone Detection Mathematics

**Vector3 Operations Available**:
- `Vector3.Angle(from, to)`: Returns angle between two vectors
- `Vector3.Distance(a, b)`: Distance between two points
- `Vector3.normalized`: Unit vector in same direction
- `magnitude`: Length of vector

**Cone Detection Algorithm**:
```csharp
bool IsInCone(Vector3 origin, Vector3 forward, float coneAngle, float range, Vector3 target)
{
    Vector3 dirToTarget = (target - origin).normalized;
    float distance = Vector3.Distance(origin, target);
    float angleToTarget = Vector3.Angle(forward, dirToTarget);
    
    return distance <= range && angleToTarget <= coneAngle * 0.5f;
}
```

**Rationale**: Use Vector3.Angle() for precise angle calculation, check both distance and angle constraints.

---

## Best Practices

### Area Detection
- Cache collider references when possible to avoid repeated GetComponent calls
- Use LayerMask to filter relevant objects (enemies, players)
- Consider using sqrMagnitude instead of Distance for performance when possible
- Limit detection frequency (e.g., every 0.1s) for channeling abilities

### Visual Indicators
- Use object pooling for LineRenderer components to avoid instantiation overhead
- Set appropriate sorting order to ensure visibility
- Use semi-transparent materials for better visual feedback
- Clean up indicators when abilities are cancelled or completed

### Performance
- Avoid creating/destroying GameObjects frequently
- Cache Transform and component references
- Use simple collision detection for basic AOE (distance checks)
- Consider using Coroutines for timed effects instead of Update loops

---

## Pitfalls to Avoid

- **Pitfall 1: Z-fighting with ground**: Raise indicators slightly above ground level (y + 0.1f)
- **Pitfall 2: Memory leaks**: Always clean up LineRenderer components and materials
- **Pitfall 3: Performance issues**: Don't check all enemies every frame, use reasonable update intervals
- **Pitfall 4: Incorrect cone calculations**: Remember that Vector3.Angle returns degrees, not radians

---

## Implementation Approach

Based on research, here's the recommended approach:

1. **Start with Manual Detection**: Since Physics.OverlapSphere not in docs, implement distance-based detection
2. **Use LineRenderer for Visuals**: Simple and effective for basic circle/cone indicators
3. **Camera Raycast for Ground**: Use ScreenPointToRay for mouse-to-ground conversion
4. **Modular Design**: Separate detection, visualization, and input handling

---

## Code Examples

### Example 1: Basic Area Detection
```csharp
public static List<GameObject> GetEnemiesInRadius(Vector3 center, float radius)
{
    List<GameObject> enemies = new List<GameObject>();
    GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
    
    foreach (GameObject enemy in allEnemies)
    {
        float distance = Vector3.Distance(center, enemy.transform.position);
        if (distance <= radius)
        {
            enemies.Add(enemy);
        }
    }
    
    return enemies;
}
```

### Example 2: Ground Targeting
```csharp
public Vector3 GetGroundPosition(Camera camera, LayerMask groundLayer)
{
    Ray ray = camera.ScreenPointToRay(Input.mousePosition);
    if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
    {
        return hit.point;
    }
    return Vector3.zero;
}
```

### Example 3: Cone Detection
```csharp
public static List<GameObject> GetEnemiesInCone(Vector3 origin, Vector3 forward, float angle, float range)
{
    List<GameObject> enemies = new List<GameObject>();
    GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
    
    foreach (GameObject enemy in allEnemies)
    {
        Vector3 dirToEnemy = (enemy.transform.position - origin).normalized;
        float distance = Vector3.Distance(origin, enemy.transform.position);
        float angleToEnemy = Vector3.Angle(forward, dirToEnemy);
        
        if (distance <= range && angleToEnemy <= angle * 0.5f)
        {
            enemies.Add(enemy);
        }
    }
    
    return enemies;
}
```

---

## Dependencies

**Existing Unity APIs to Leverage**:
- `Camera.ScreenPointToRay()` - For ground targeting
- `Physics.Raycast()` - For ground detection
- `LineRenderer` - For visual indicators
- `Vector3.Angle()` - For cone calculations
- `Vector3.Distance()` - For range checks
- `GameObject.FindGameObjectsWithTag()` - For enemy detection

**New Dependencies to Add**:
- None required - using built-in Unity APIs only

---

## Performance Considerations

- **Expected load**: 5-10 enemies per AOE check, max 3 simultaneous AOE
- **Update frequency**: 10Hz for channeling, instant for cast-time abilities
- **Memory usage**: Minimal - reuse LineRenderer components
- **Optimization**: Use sqrMagnitude for distance comparisons when possible

---

## Security Considerations

- **Input validation**: Ensure ground raycast hits valid terrain
- **Range limiting**: Clamp AOE ranges to reasonable values
- **Target validation**: Verify targets are valid before applying effects

---

## Testing Strategy

**Unit Tests**:
- Test cone detection with known positions and angles
- Test circular detection with various radii
- Test ground raycast with different terrain heights

**Integration Tests**:
- Test visual indicators appear correctly
- Test mouse input converts to proper ground positions
- Test multiple AOE types don't interfere

**Manual Tests**:
- Visual verification of indicator accuracy
- Performance testing with 10+ enemies
- Edge case testing (targets at exact range/angle limits)

---

## References

- [Unity Camera API](PWV-main/Unityscr4/unity-docs-scraper/unity_docs_json/api_rendering_Camera.json)
- [Unity LineRenderer API](PWV-main/Unityscr4/unity-docs-scraper/unity_docs_json/api_rendering_LineRenderer.json)
- [Unity Vector3 API](PWV-main/Unityscr4/unity-docs-scraper/unity_docs_json/api_math_Vector3.json)
- [Unity RaycastHit API](PWV-main/Unityscr4/unity-docs-scraper/unity_docs_json/api_physics_RaycastHit.json)

---

## Open Questions

- [ ] Should we implement object pooling for LineRenderer components from the start?
- [ ] What's the optimal update frequency for channeling AOE detection?
- [ ] Should we use a single detection system or separate systems per AOE type?

---

## Summary

Research shows that Unity provides all necessary APIs for basic AOE implementation. Key findings: use Camera.ScreenPointToRay for ground targeting, LineRenderer for visual indicators, and manual distance/angle calculations for area detection. The approach will be simple and performant, avoiding complex integrations with existing systems.