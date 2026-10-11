<div align="center">

# 🐍 SnakeGame

### The classic, rebuilt for every screen with .NET MAUI Graphics

[![.NET MAUI](https://img.shields.io/badge/.NET%20MAUI-.NET%2010-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/apps/maui)
[![MAUI Graphics](https://img.shields.io/badge/MAUI%20Graphics-GraphicsView-7B3FE4?style=for-the-badge)](https://learn.microsoft.com/dotnet/maui/user-interface/graphics/)
[![Android](https://img.shields.io/badge/Android-3DDC84?style=for-the-badge&logo=android&logoColor=white)](#-supported-platforms)
[![Windows](https://img.shields.io/badge/Windows-0078D4?style=for-the-badge)](#-supported-platforms)

</div>

---

## 🎮 Gameplay

| Mode | What happens |
|---|---|
| **Classic** | No walls — the snake wraps around the edges |
| **Walls** | Boundary walls — touch one and it's game over |
| **Complex** | Four inner walls with gaps to weave through |

- 🎚️ **Three difficulties** — Easy, Medium, Hard
- ⚡ **Adjustable speed level**, toggleable **grid** and **sound**
- 🍎 **Regular & bonus food** with level progression
- 🏆 **Persistent high scores** via `HighScoreManager`
- 🔊 **Audio service** for eating, collisions and game-over

## 🧱 Architecture

```
SnakeGame/
├── Models/        # Snake, Food, Position, Direction, Level, GameState, HighScore, enums
├── Services/      # GameEngine (rules, collisions, spawning) · AudioService
├── Drawables/     # GameDrawable — renders the board with MAUI Graphics
└── Views/         # HomePage · GamePage · SelectionPopup
```

- The **`GameEngine`** owns the rules and raises events (`FoodEaten`, `CollisionDetected`, `LevelCompleted`, `GameOverEvent`).
- **`GamePage`** drives the loop with an `IDispatcherTimer` and invalidates a `GraphicsView`.
- **`GameDrawable`** paints snake, food and walls each frame — no sprites, pure vector drawing.

## 📱 Supported Platforms

Android · iOS · macOS (Mac Catalyst) · Windows — all from a single .NET 10 MAUI project.

## ⚡ Getting Started

```bash
git clone https://github.com/smuhammadbilalh/SnakeGame.git
cd SnakeGame
dotnet restore
dotnet build -t:Run -f net10.0-windows10.0.19041.0   # or net10.0-android
```

Requires Visual Studio 2022 with the **.NET Multi-platform App UI development** workload (or the .NET 10 SDK + MAUI workload).

## 🗺️ Roadmap

- [ ] Swipe gestures on mobile
- [ ] Online leaderboard
- [ ] Themes & skins

---

## 👨‍💻 Author

<div align="center">

**Muhammad Bilal** — IT Specialist & App Developer · Gujrat, Pakistan

[![GitHub](https://img.shields.io/badge/GitHub-smuhammadbilalh-181717?style=for-the-badge&logo=github)](https://github.com/smuhammadbilalh)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-muhammadbilaltech-0A66C2?style=for-the-badge)](https://www.linkedin.com/in/muhammadbilaltech)
[![Email](https://img.shields.io/badge/Email-smuhammadbilalh%40gmail.com-EA4335?style=for-the-badge&logo=gmail&logoColor=white)](mailto:smuhammadbilalh@gmail.com)

⭐ If this project helped you, please consider giving it a star!

</div>
