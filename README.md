# NightShift City 3D — v0.4

A Unity 3D city-maintenance robot simulation. CleanerBots clean trash; RepairBots fix potholes — all running autonomously while a live HUD tracks city health. **Now with Citizen Mode** — walk the streets in first person and watch the robots work. Built entirely with Unity primitives and generated materials.

---

## What's New in v0.4 — Citizen Mode

| Area | Changes |
|---|---|
| **Citizen Mode** | First-person player capsule with WASD walking, mouse look, sprint, and jump |
| **Camera modes** | Press **1** for overview, **2** for first-person citizen view |
| **Observation** | Walk near any robot to see its live status (Cleaning / Repairing / Searching) |
| **Materials** | Smooth wet-asphalt roads, matte concrete sidewalks |
| **HUD** | Version v0.4, current mode display, controls reference, robot observation line |

### Earlier — v0.3 Visual Polish

| Area | Changes |
|---|---|
| **Buildings** | Lit + unlit window mix, entrance doors, 4 types of rooftop props (water tower, HVAC, antenna, penthouse) |
| **Roads** | Crosswalks at 5 major intersections, curb edges alongside roads |
| **Lighting** | 21 total street lights, subtle night fog |
| **Robots** | CleanerBot brush disc, RepairBot toolbox, pulsing indicator lights |

---

## Features

- **Citizen Mode** — walk the city in first person and observe robots up close
- **Two camera modes** — switch between city overview and first-person citizen view
- **Autonomous CleanerBots (blue)** — find nearest unclaimed trash, claim it, clean it
- **Autonomous RepairBots (orange)** — find nearest unclaimed pothole, claim it, repair it
- **Exclusive claiming** — no two bots of the same type share a target
- **Procedural city builder** — one-click Editor tool generates the entire scene from code
- **Night atmosphere** — dark ambient, warm point-lit streets, emissive windows, distance fog
- **Live city HUD** — status label, colour-coded health, mode display, robot observation
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

## Controls (Play Mode)

**Mode switching**

| Key | Action |
|---|---|
| 1 | Overview camera |
| 2 | Citizen (first-person) |

**Overview mode**

| Key | Action |
|---|---|
| WASD / Arrows | Pan |
| Scroll / Q / E | Zoom |

**Citizen mode**

| Key | Action |
|---|---|
| WASD | Walk |
| Mouse | Look around |
| Left Shift | Sprint |
| Space | Jump |

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
├── CameraController.cs            # Overview WASD/scroll navigation
├── CameraModeManager.cs           # Switches overview <-> citizen cameras
├── PlayerController.cs            # First-person citizen movement
├── RobotObserver.cs               # Shows nearby robot status (citizen mode)
├── CityManager.cs                 # Singleton — all city stats
├── CityUI.cs                      # Self-building runtime HUD (v0.4)
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
