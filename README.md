# 2D Action Platformer

A 2D action platformer game built with Unity and C#. Features a responsive combat system with combos, parry mechanics, wall climbing, dashing, and diverse enemy types across multiple biome-themed levels.

---

## Features

- **Combat System** — 3-hit combo attacks, directional attacks (up/down/forward)
- **Movement** — run, dash, wall climb, ceiling climb, coyote time, air jumps
- **Parry & Shield** — parry window with counter-attack bonus, timed shield
- **Enemy Variety** — melee, ranged, and flying enemy types with unique behaviors
- **Level Progression** — multiple biome locations (desert, forest, lava, crystal dungeon, etc.)
- **Save System** — player progress persistence
- **Highscore Table** — score tracking with leaderboard
- **UI System** — main menu, pause menu, death screen with run statistics

---

## Tech Stack

| Technology | Version |
|---|---|
| Unity | 2022+ (URP) |
| C# | .NET Standard 2.1 |
| Render Pipeline | Universal Render Pipeline (URP) |
| Input | Unity Input System (New) + Legacy Input |
| UI | TextMesh Pro |
| Animation | Animator + LeanTween |
| Audio | Unity Audio (AudioSource) |

---

## Controls

| Action | Key |
|---|---|
| Move | A / D or Arrow Keys |
| Run | Left Shift |
| Jump | Space |
| Attack | Attack button (configurable) |
| Dash | Dash button (configurable) |
| Parry | Parry button (configurable) |
| Shield | Shield button (configurable) |
| Pause | Escape |

---

## Project Structure

```
Assets/
├── Game/
│   ├── Script/
│   │   ├── Player/          # All player mechanics
│   │   ├── Enemy/
│   │   │   ├── Melee/       # AgileEnemy, ArmoredEnemy, BerserkEnemy
│   │   │   ├── Flying/      # CrystalEnemy, FireSpiritEnemy
│   │   │   ├── Ranged/      # JumpingShootingEnemy, SerpentEnemy
│   │   │   ├── Enemy.cs     # Abstract base class
│   │   │   ├── Projectile.cs
│   │   │   └── ObjectPool.cs
│   │   ├── Manager/         # Game managers (doors, respawn, transitions)
│   │   ├── UI2.0/           # Main UI system (UIManager, views)
│   │   ├── UI/              # Legacy UI components
│   │   ├── Map/             # Background, map logic, end sequences
│   │   ├── Sound/           # AudioManager, UIAudio
│   │   ├── Camera/          # Camera follow, destroy helpers
│   │   ├── Save/            # Save data system
│   │   └── Vfx/             # Particles, wind effects
│   ├── Scene/               # Game scenes
│   └── VFX/                 # Visual effect assets
├── 2D Fantasy sprite bundle/ # Environment art packs
└── GameFile/                 # Custom game art assets
```

---

## Getting Started

### Requirements
- Unity 2022.3 LTS or newer
- Universal Render Pipeline package
- TextMesh Pro package (installed via Package Manager)

### Running the Project
1. Clone or download the repository
2. Open Unity Hub and click **Add project from disk**
3. Select the project folder
4. Open the `MainMenu` scene: `Assets/Game/Scene/MainMenu.unity`
5. Press **Play**

---

## Screenshots

> *Coming soon*

---

## Architecture Overview

The project uses a **component-based architecture** for the player:

- `PlayerController` — central coordinator, reads input and delegates to components
- `PlayerConfig` (ScriptableObject) — all tunable parameters in one place
- `GlobalController` (singleton) — manages player spawning and scene transitions
- `UIManager` (singleton) — dynamically instantiates UI panels per scene
- `Enemy` (abstract class) — shared base for all enemy types

---

## License

This project is for educational and portfolio purposes.
