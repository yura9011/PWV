# AOE Test Scene Setup Guide

## Overview
This guide explains how to set up the AOE test scene for testing the three AOE features:
1. Ground-Targeted AOE (G key)
2. Player-Centered AOE (R key) 
3. Cone Attack AOE (T key)

## Manual Scene Creation Steps

Since Unity scene files are binary and cannot be created via text, follow these steps to create the test scene:

### 1. Create New Scene
1. In Unity, go to `File > New Scene`
2. Choose "3D" template
3. Save as `AOE_TestScene.unity` in `Assets/_Project/Scenes/AOE_Testing/`

### 2. Setup Player
1. Find the TestPlayer prefab or use existing TestPlayerWithAbilities
2. Place at position (0, 0, 0)
3. Add the `AOE_MasterTestController` component
4. Ensure the player has movement controls (WASD)

### 3. Setup Ground
1. Create a Plane primitive (`GameObject > 3D Object > Plane`)
2. Name it "Ground"
3. Position at (0, 0, 0)
4. Scale to (5, 1, 5) for a 50x50 unit area
5. Set layer to "Default" (layer 0) for ground raycast
6. Apply a gray material for visibility

### 4. Setup Enemies
**Option A: Use AOE_TestSceneSetup Script**
1. Create empty GameObject named "SceneSetup"
2. Add `AOE_TestSceneSetup` component
3. Click "Setup Scene" button in inspector or at runtime

**Option B: Manual Enemy Placement**
1. Create Capsule primitives for enemies
2. Set tag to "Enemy" (CRITICAL for detection)
3. Apply red material for visibility
4. Position enemies at these locations for optimal testing:

```
Close Range (Player-Centered AOE):
- (2, 0, 0), (-2, 0, 0), (0, 0, 2), (0, 0, -2)

Medium Range (Ground Targeting):
- (5, 0, 5), (-5, 0, 5), (5, 0, -5)

Cone Testing (Front of Player):
- (0, 0, 6), (2, 0, 8), (-2, 0, 8)

Far Range (Outside AOE):
- (12, 0, 0), (0, 0, 12), (-12, 0, -12)
```

### 5. Setup Camera
1. Position Main Camera at (0, 15, -10)
2. Rotate to (45, 0, 0) for good overview
3. Ensure it's tagged "MainCamera"

### 6. Add Lighting
1. Ensure Directional Light exists
2. Position for good visibility of indicators

## Testing Instructions

### Controls
- **G Key**: Start/confirm ground targeting AOE
- **R Key**: Trigger player-centered AOE
- **T Key**: Trigger cone attack AOE
- **Right-click/ESC**: Cancel ground targeting
- **WASD**: Move player
- **Mouse**: Rotate camera/player

### Expected Behavior

**Ground Targeting (G Key):**
1. Press G to enter targeting mode
2. Red circle follows mouse cursor on ground
3. Press G again to confirm and detect enemies
4. Console shows detected enemies
5. Circle remains visible for 2 seconds

**Player-Centered AOE (R Key):**
1. Press R to trigger AOE around player
2. Blue circle appears around player
3. Enemies within 5 units are detected
4. Enemies briefly turn yellow
5. 1-second cooldown before next use

**Cone Attack (T Key):**
1. Press T to trigger cone attack
2. Green cone appears in front of player
3. Enemies within 60° cone and 8 units are detected
4. Enemies briefly turn red
5. Cone rotates with player direction

### Verification Checklist
- [ ] All three AOE types show visual indicators
- [ ] Console logs show correct enemy detection
- [ ] Ground targeting follows mouse smoothly
- [ ] Player-centered AOE detects nearby enemies
- [ ] Cone attack detects enemies in front only
- [ ] Visual indicators disappear after duration
- [ ] No compilation errors or exceptions

## Troubleshooting

### Common Issues

**"No enemies detected"**
- Ensure enemies have "Enemy" tag
- Check enemy positions are within AOE range
- Verify AreaDetector.cs is working

**"Ground targeting not working"**
- Ensure ground has collider
- Check ground layer mask settings
- Verify camera raycast is hitting ground

**"Visual indicators not showing"**
- Check LineRenderer materials
- Ensure indicators are above ground (y + 0.1)
- Verify AOEVisualIndicator component setup

**"Console spam or errors"**
- Check for null references in scripts
- Ensure all required components are attached
- Verify scene setup is complete

### Debug Tips
- Enable Gizmos in Scene view to see enemy identifiers
- Check Console for detailed AOE detection logs
- Use Scene view to verify indicator positions
- Test each AOE type individually

## File Structure
```
Assets/_Project/
├── Scenes/AOE_Testing/
│   ├── AOE_TestScene.unity          # Main test scene
│   └── README_AOE_TestScene.md      # This guide
└── Scripts/AOE_Testing/
    ├── AreaDetector.cs              # Core detection logic
    ├── AOEVisualIndicator.cs        # Visual indicators
    ├── GroundTargetingTest.cs       # Ground targeting
    ├── PlayerCenteredTest.cs        # Player-centered AOE
    ├── ConeAttackTest.cs            # Cone attacks
    ├── AOE_TestSceneSetup.cs        # Automated setup
    └── AOE_MasterTestController.cs  # Master controller
```

## Next Steps
After successful testing:
1. Verify all three AOE types work correctly
2. Test with different enemy positions
3. Validate performance with 10+ enemies
4. Document any issues or improvements needed
5. Consider integration with existing combat system

---
**Created**: 2026-01-20  
**Phase**: Phase 1 - AOE Foundations  
**Status**: Ready for Testing