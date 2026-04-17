# STS2 Debug Tools

A **Slay the Spire 2** mod that adds two debugging utilities to any run:

| Feature | When available | What it does |
|---|---|---|
| **Win Battle** | During combat | Instantly kills all primary enemies and triggers the normal end-of-combat flow (rewards, map unlock, etc.). |
| **Map Teleport** | On the map (between rooms) | Enables debug travel mode on the map screen — click any room to teleport there directly, bypassing normal travel restrictions. |

**Win Battle** appears as a floating button in the top-left area of the screen during combat. **Map Teleport** is always active whenever the map screen is open (no button required).

---

## Installation

Drop the `Sts2DebugTools/` folder (containing `Sts2DebugTools.dll` and `Sts2DebugTools.json`) into your Slay the Spire 2 `mods/` directory.

**Dependency:** [BaseLib](https://github.com/Alchyr/sts2-base-lib) must be installed.

---

## Building from source

Prerequisites: **.NET 9 SDK** and a local install of **Slay the Spire 2** (Godot is only needed if you want to export a `.pck` via `dotnet publish`).

1. Edit `Directory.Build.props` to verify the paths match your local setup.
2. `dotnet build -c Release`

The compiled DLL and manifest are copied to your mods folder automatically.

---

## Implementation notes

* **Win Battle** — uses `AccessTools.Field` (Harmony) to reflectively set each primary enemy's `_currentHp` field to 0, fires `InvokeDiedEvent()`, then calls `CombatManager.Instance.CheckWinCondition()` to let the game run its normal victory sequence.
* **Map Teleport** — a Harmony postfix on `NMapScreen.Open` calls `SetDebugTravelEnabled(true)` each time the map screen opens, enabling the game's built-in debug travel mode so the player can click any room directly.
