# NightShift City 3D — v0.3

A Unity 3D city-maintenance robot simulation. CleanerBots clean trash; RepairBots fix potholes — all running autonomously while a live HUD tracks city health. Built entirely with Unity primitives and generated materials.

---

## What's New in v0.3 — Visual Polish Update

| Area | Changes |
|---|---|
| **Buildings** | Lit + unlit window mix, entrance doors, 4 types of rooftop props (water tower, HVAC, antenna, penthouse) |
| **Roads** | Crosswalks at 5 major intersections, curb edges alongside roads |
| **Lighting** | 21 total street lights (intersections + mid-road fill), subtle night fog |
| **Robots** | CleanerBot brush disc at front, RepairBot toolbox on body, pulsing indicator lights |
| **Trash** | 3-piece pile (two cubes + a can cylinder), brighter yellow |
| **Potholes** | Dark disc + emissive warning ring + 3 crack lines |
| **HUD** | Version badge (v0.3), STABLE / WARNING / CRITICAL status, colour-coded bot sections |
| **Camera** | WASD / scroll / Q-E navigation during Play mode |

---

## Features

- **Autonomous CleanerBots (blue)** — find nearest unclaimed trash, claim it, clean it
- **Autonomous RepairBots (orange)** — find nearest unclaimed pothole, claim it, repair it
- **Exclusive claiming** — no two bots of the same type share a target
- **Procedural city builder** — one-click Editor tool generates the entire scene from code
- **Night atmosphere** — dark ambient, warm point-lit streets, emissive windows, distance fog
- **Live city HUD** — status label, colour-coded health, active counts, bot counts
- **WASD camera controller** — navigate freely in Play mode
- **Pulsing robot sensors** — indicator lights breathe on each bot
- **Primitive-only visuals** — zero external asset dependencies

---

## Tech Stack

| Tool | Version |
|---|---|
| Unity | 6.4 (URP) |
| C# | 9.0 |
| Render Pipeline | Universal Render Pipeline |
| UI | TextMeshPro (Unity built-in) |
| Assets | Primitives only |

---

## How to Run

1. Clone this repository
2. Open in **Unity 6** (Universal Render Pipeline)
3. Open `Assets/Scenes/SampleScene`
4. Click **Tools → NightShift City → Build Basic Scene**
5. Press **Play**

No Inspector wiring required.

---

## Camera Controls (Play Mode)

| Key | Action |
|---|---|
| W / S / ↑ / ↓ | Pan forward / back |
| A / D / ← / → | Pan left / right |
| Scroll wheel | Zoom in / out |
| Q / E | Keyboard zoom out / in |

---

## Project Structure

```
Assets/
├── Editor/
│   └── NightShiftCityBuilder.cs   # One-click scene generator (v0.3)
├── Prefabs/
│   ├── Trash.prefab               # 3-piece yellow pile
│   └── Pothole.prefab             # Disc + warning ring + cracks
├── Scenes/
│   └── SampleScene.unity
├── BotPulse.cs                    # Pulsing indicator light animation
├── CameraController.cs            # WASD/scroll camera navigation
├── CityManager.cs                 # Singleton — all city stats
├── CityUI.cs                      # Self-building runtime HUD (v0.3)
├── RobotController.cs             # CleanerBot movement + trash logic
├── RobotLabel.cs                  # Billboard name labels above bots
├── RepairBotController.cs         # RepairBot movement + pothole logic
├── TrashItem.cs                   # Trash claim / release
├── TrashSpawner.cs                # Timed trash spawning
├── PotholeItem.cs                 # Pothole claim / release
└── PotholeSpawner.cs              # Timed pothole spawning on road grid
```

---

## City Health Formula

```
health = 100 − (activeTrash × 3) − (activePotholes × 8)
clamped to [0, 100]
```

| Status | Range |
|---|---|
| 🟢 STABLE | 76 – 100 |
| 🟡 WARNING | 41 – 75 |
| 🔴 CRITICAL | 0 – 40 |

---

## Roadmap

- [ ] NavMesh pathfinding around buildings
- [ ] Day/night lighting cycle
- [ ] Bot patrol zones (no random wandering)
- [ ] Sound effects on clean / repair events
- [ ] Different robot speeds and priorities
- [ ] Score and high-score system
- [ ] Scene save / load

---

## License

MIT — free to use for learning and portfolio purposes.
