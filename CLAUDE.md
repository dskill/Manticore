# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

"Dead Balls" is a Unity3D bowling game where players roll balls at animated pins. This is a legacy Unity project (circa 2012) using MonoDevelop, Visual Studio project files, and older Unity APIs.

## Build System

This is a Unity project - open it in Unity Editor to build and run. The project uses:
- Solution files: `Manticore.sln` (standard) and `Manticore-csharp.sln` (C# only)
- Unity Package Manager: dependencies defined in `Packages/manifest.json`
- Key packages: TextMeshPro, Timeline, uGUI, Test Framework

**Important:** This project uses Unity's legacy scripting (note the `.pidb` files and older Unity APIs). Code must be compatible with the Unity version this project was created for.

## Architecture

### Core Game Loop
The game follows a frame-based bowling structure managed by the `GameLoop` class:
- **GameLoop** (`Assets/Scripts/General/GameLoop.cs`): Central manager for game state, frame progression, scoring, and UI. Tracks active pins, manages frame completion, and coordinates between game systems.
- **GameState**: `StartMenu` → `FrameActive` → `FrameCompleted` → `EndCredits`
- Frame progression: Player bowls → ball returns → score calculated → frame completes → next frame starts

### Event System
**EventManager** (`Assets/Scripts/General/EventManager.cs`): Static event-driven communication system using C# delegates. Core events:
- `BallReturned`: Triggered when ball returns, advances throw count
- `FrameStarted(int)`: New frame begins, spawns pins
- `FrameCompleted(int)`: Frame ends, cleanup triggered
- `GoToWaypoint(int, float)`: Camera/object movement along splines
- `PinHit(Pin)`: Pin collision detected, score tracked

Components subscribe/unsubscribe in `OnEnable`/`OnDisable` to avoid memory leaks.

### Object Hierarchy
```
BaseObject (base class)
└── SpawnedObject
    ├── Ball: Physics-driven, handles rolling sounds and collision audio
    ├── Destructible: Objects that can be destroyed/pooled
    └── Pawn (animated entities)
        ├── Pin: Detects hits, triggers particles, manages destruction
        └── Player: Player controller
```

**BaseObject** (`Assets/Scripts/General/BaseObject.cs`): Provides cached references (`myTransform`, `myGameObject`) and static access to `GameLoop` and `Persistent` managers.

### Object Pooling
**ObjectPoolManager** (`Assets/Scripts/Plugins/Object Pool Manager/ObjectPoolManager.cs`): Singleton that recycles frequently created/destroyed objects (pins, particles, text) to avoid GC overhead.
- Replace `Instantiate()` → `ObjectPoolManager.CreatePooled()`
- Replace `Destroy()` → `ObjectPoolManager.DestroyPooled()`
- Pooled objects' `Start()` method is called on revival for re-initialization

### Bowling Scoring System
**Score** (`Assets/Scripts/General/Score.cs`): Implements standard bowling rules with strike/spare calculation. Tracks 10 frames with up to 2 extra frames for strikes/spares in frame 10.

### Key Components
- **Pin** (`Assets/Scripts/Spawned Objects/Pawns/Pin.cs`): Manages pin collision, particle effects, destruction, and scoring events. Supports destructible mode where pins break into pieces.
- **Ball** (`Assets/Scripts/Spawned Objects/Ball.cs`): Rigidbody-based ball with velocity-dependent audio (slow/fast rolling sounds).
- **MoveAlongSpline**: Component for moving objects along spline paths (camera movements, pin animations).
- **SyncedPinPositions**: Manages waypoints for synchronized pin movement during frames.

## Code Patterns

### Accessing Game State
```csharp
// Access global game loop from any BaseObject
if (gameLoop) {
    gameLoop.activePins.Count;
    gameLoop.currentFrame;
    gameLoop.score;
}
```

### Subscribing to Events
```csharp
protected void OnEnable() {
    EventManager.PinHit += OnPinHit;
    EventManager.FrameCompleted += OnFrameCompleted;
}

protected void OnDisable() {
    EventManager.PinHit -= OnPinHit;
    EventManager.FrameCompleted -= OnFrameCompleted;
}
```

### Using Object Pooling
```csharp
// Creating pooled objects
GameObject obj = ObjectPoolManager.CreatePooled(prefab.gameObject, position, rotation);

// Destroying pooled objects
ObjectPoolManager.DestroyPooled(gameObject);
ObjectPoolManager.DestroyPooled(gameObject, delaySeconds); // delayed
```

## Third-Party Assets

- **TK2D** (`Assets/TK2DROOT/`): 2D sprite and text rendering toolkit
- **iTween** (`Assets/Scripts/Plugins/iTween.cs`): Animation/tweening library

## Unity 2019+ Migration

The codebase has been migrated to Unity 2019. All critical compatibility issues have been resolved.

### Completed Updates
- **SetActiveRecursively → SetActive**: All calls to deprecated `SetActiveRecursively()` replaced with `SetActive()` across 5 files (15 instances)
- **Ball.cs optimized**: Cached `AudioSource` and `Rigidbody` components to eliminate repeated `GetComponent()` calls
- **iTween.cs updated**: Replaced legacy iTween with Unity 2019-compatible version from https://github.com/PixelWizards/iTween (removes all GUITexture/GUIText references). Added `CallEasingFunction()` method for backward compatibility with old iTween API
- **TK2D camera fixed**: Removed obsolete `RuntimePlatform.WindowsWebPlayer` reference from tk2dCamera.cs
- **Unity Collaborate removed**: Removed `com.unity.collab-proxy` package from manifest.json (was causing team license errors)
- **Editor scripts updated**: Commented out deprecated APIs in RPMenu.cs and ComponentCopier.cs:
  - `BuildPipeline.BuildAssetBundle()` replaced with warning (use AssetBundleBuild workflow)
  - AudioImporter properties (threeD, format, loadType) commented out (use AudioImporterSampleSettings)
  - Fixed escape sequences in file paths
- **Backup created**: Old iTween saved as `iTween.cs.backup` in case of compatibility issues

### Remaining Deprecated APIs (Non-Critical)
These APIs still exist in the codebase but don't cause compilation errors:
- `Application.loadedLevel` → Use SceneManager (Persistent.cs line 36)
- `Application.LoadLevel()` → Use `SceneManager.LoadScene()` (GameLoop.cs line 259)
- `OnLevelWasLoaded()` → Use `SceneManager.sceneLoaded` event (Persistent.cs line 44)
- `Application.isLoadingLevel` → Check with SceneManager (Persistent.cs line 36)

## Important Notes

- This is a **legacy Unity project** - be mindful of deprecated APIs when modernizing
- Object pooling is critical for performance - use it for frequently spawned objects
- All game logic relies on the EventManager - always subscribe/unsubscribe properly
- The game uses physics-based bowling mechanics with Rigidbody components
- Audio system dynamically adjusts based on collision velocities and object states
