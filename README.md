# NightShift City 3D — v0.2

A Unity 3D simulation of an autonomous city-maintenance robot fleet. CleanerBots tackle trash; RepairBots fix road potholes — all while a live HUD tracks city health in real time.

Built entirely with Unity primitive shapes and generated materials. No external assets.

---

## What's New in v0.2

- **RepairBots** (orange) autonomously find and repair road potholes
- **Potholes** spawn on the road grid with an emissive warning ring
- **Night city aesthetic** — dark ambient lighting, warm street lamp point lights, emissive windows
- **Road system upgrade** — dashed center lines, sidewalks on every street
- **Building windows** — emissive yellow window panes on all four faces of every building
- **Rooftop details** — contrasting darker cap on each building
- **Street lights** at all nine road intersections with warm point lights
- **Trees and grass patches** at city corners
- **Updated HUD** — shows trash, potholes, health, and both bot counts
- **Updated city health formula** — potholes hurt more (×8) than trash (×3)

---

## Features

- **Autonomous CleanerBots** — find nearest unclaimed trash, claim it exclusively, clean it
- **Autonomous RepairBots** — find nearest unclaimed pothole, claim it exclusively, repair it
- **Exclusive claiming** — no two bots of the same type chase the same target
- **Procedural city builder** — one-click Editor tool builds the full scene from code
- **Live city HUD** — colour-coded health, active counts, totals, bot counts
- **Night city visuals** — dark ambient, point-lit streets, emissive windows and sensors
- **Primitive-only visuals** — cubes, spheres, cylinders; no external asset packages

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
2. Open the project in **Unity 6** (Universal Render Pipeline)
3. Open `Assets/Scenes/SampleScene`
4. Click **Tools → NightShift City → Build Basic Scene**
5. Press **Play**

No manual Inspector setup is required — everything is wired by the builder.

---

## Project Structure

```
Assets/
├── Editor/
│   └── NightShiftCityBuilder.cs   # One-click scene generator (v0.2)
├── Prefabs/
│   ├── Trash.prefab               # Auto-generated yellow trash pile
│   └── Pothole.prefab             # Auto-generated road pothole
├── Scenes/
│   └── SampleScene.unity
├── CityManager.cs                 # Singleton — all city stats
├── CityUI.cs                      # Self-building runtime HUD
├── RobotController.cs             # CleanerBot movement + trash logic
├── RobotLabel.cs                  # Billboard name labels above bots
├── RepairBotController.cs         # RepairBot movement + pothole logic
├── TrashItem.cs                   # Trash claim / release
├── TrashSpawner.cs                # Timed trash spawning
├── PotholeItem.cs                 # Pothole claim / release
└── PotholeSpawner.cs              # Timed pothole spawning on road grid
```

---

## How the Simulation Works

```
TrashSpawner    → spawns Trash prefab (tag "Trash", TrashItem component)
PotholeSpawner  → spawns Pothole on road (tag "Pothole", PotholeItem component)

RobotController (CleanerBot):
  → FindGameObjectsWithTag("Trash")
  → nearest unclaimed → TrashItem.Claim()
  → MoveTowards target (X/Z plane)
  → on arrival → Destroy(trash) → CityManager.RegisterTrashCleaned()

RepairBotController (RepairBot):
  → FindGameObjectsWithTag("Pothole")
  → nearest unclaimed → PotholeItem.Claim()
  → MoveTowards target (X/Z plane)
  → on arrival → Destroy(pothole) → CityManager.RegisterPotholeRepaired()

CityManager.GetCityHealth():
  → 100 - activeTrash * 3 - activePotholes * 8   (clamped 0–100)

CityUI → reads CityManager every frame → updates HUD text
```

---

## City Health Formula

| Factor | Health impact |
|---|---|
| Each piece of active trash | −3 |
| Each unrepaired pothole | −8 |
| Maximum health | 100 |
| Minimum health | 0 |

Potholes damage the city more severely than loose trash, reflecting real infrastructure priorities.

---

## Roadmap

- [ ] NavMesh pathfinding around buildings
- [ ] Day/night lighting cycle
- [ ] Potholes: spray-paint warning effect when spotted
- [ ] Different robot speeds / priorities
- [ ] Score system and high scores
- [ ] Sound effects on clean/repair events
- [ ] Multiple robot types (sweeper, inspector, etc.)
- [ ] Scene save/load

---

## License

MIT — free to use for learning and portfolio purposes.
