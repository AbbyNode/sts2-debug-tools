# STS2 Debug Tools

A **Slay the Spire 2** mod that adds two debugging utilities to any run:

| Feature | When available | What it does |
|---|---|---|
| **Win Battle** | During combat | Instantly kills all primary enemies and triggers the normal end-of-combat flow (rewards, map unlock, etc.). |
| **Teleport to Room** | On the map (between rooms) | Opens a dialog listing every room on the current act's map; clicking one teleports you there immediately. |

Both tools appear as a small floating button panel in the top-left area of the screen.

---

## Installation

Drop the `Sts2DebugTools/` folder (containing `Sts2DebugTools.dll` and `Sts2DebugTools.json`) into your Slay the Spire 2 `mods/` directory.

**Dependency:** [BaseLib](https://github.com/Alchyr/sts2-base-lib) must be installed.

---

## Building from source

Prerequisites: **.NET 9 SDK**, **Godot 4.5.1 mono**, and a local install of **Slay the Spire 2**.

1. Copy `Directory.Build.props.example` to `Directory.Build.props` and adjust the paths.
2. `dotnet build -c Release`

The compiled DLL and manifest are copied to your mods folder automatically.

---

## Implementation notes

* **Win Battle** — uses `AccessTools.Field` (Harmony) to reflectively set each primary enemy's `_currentHp` field to 0, fires `InvokeDiedEvent()`, then calls `CombatManager.Instance.CheckWinCondition()` to let the game run its normal victory sequence.
* **Teleport to Room** — calls `RunManager.Instance.EnterMapCoordDebug(coord, roomType, pointType)` which bypasses travel-restriction checks and transitions straight to the selected room.
