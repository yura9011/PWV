# Unity Documentation Index

Organized documentation for Unity Engine, ready for MCP semantic search.

## Structure

```
unity_docs/
├── api/                    # Scripting API Reference (178 classes)
│   ├── core/              # GameObject, Transform, Component, etc.
│   ├── physics/           # Physics, Rigidbody, Colliders, Joints
│   ├── physics2d/         # 2D Physics components
│   ├── rendering/         # Camera, Light, Materials, Shaders
│   ├── animation/         # Animator, Animation, Avatar
│   ├── audio/             # AudioSource, AudioClip, Mixer
│   ├── ui/                # Canvas, RectTransform, UI components
│   ├── input/             # Input, Touch, KeyCode
│   ├── math/              # Vector, Quaternion, Mathf
│   ├── scene/             # SceneManager, Scene
│   ├── 2d/                # Sprite, Tilemap, Grid
│   ├── navigation/        # NavMesh, NavMeshAgent
│   ├── networking/        # UnityWebRequest, WWW
│   ├── terrain/           # Terrain, TerrainData
│   ├── video/             # VideoPlayer
│   ├── editor/            # Editor, EditorWindow
│   └── utility/           # Time, Debug, PlayerPrefs
│
└── manual/                 # Unity Manual (118 pages)
    ├── getting_started/   # Introduction, Glossary
    ├── 2d/                # 2D Game Development
    ├── physics/           # Physics guides
    ├── animation/         # Animation system
    ├── audio/             # Audio system
    ├── ui/                # UI Toolkit, UGUI
    ├── scripting/         # C# scripting guides
    ├── graphics/          # Rendering, Lighting, Shaders
    ├── platforms/         # Android, iOS, WebGL
    └── optimization/      # Performance, Profiling
```

## Usage with MCP

```bash
cd unity_docs
mcp init
mcp index
mcp search "how to detect collisions"
```

## Coverage

- **API Coverage**: 98.9% of important classes
- **Manual Coverage**: 87.5% of important topics
- **Overall Score**: 93.2%
