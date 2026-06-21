# The Amazing Digital Circus — Mobile Fangame

A single-player 2D side-scrolling adventure/platformer fangame for **The Amazing
Digital Circus**, built in **Unity (URP)** for **Android & iOS**. Whimsical,
chaotic, with the show's psychological-horror undertones — find the exit before
you *abstract*.

> Fan project. Not affiliated with or endorsed by Glitch Productions.

## Concept

- **Genre:** 2D platformer with exploration, light puzzles, dialogue trees, and
  episode-themed mini-games.
- **Playable cast:** Pomni (default), Ragatha, Jax, Gangle, Kinger, Zooble — each
  with a unique signature ability.
- **Structure:** A Circus Hub + 9 chapters spanning Episodes 1–9 (Pilot → The Last
  Act), a **sanity / abstraction** meter, multiple endings, and unlockable
  characters & costumes.
- **Single-player, offline,** with a JSON save system.

## Tech

- Unity 6000 LTS (or 2022.3 LTS) · URP · Input System · Cinemachine · TextMeshPro
- ScriptableObject-driven data, persistent singleton `GameManager`, atomic JSON saves.
- Mobile-first: portrait UI scaling, lifecycle-aware saving, object pooling (later).

## Build Status — Iterative

This project is built one system at a time. Completed so far:

- ✅ **Step 1 — Project setup + Main Menu + Character Selector**
  - Folder structure & Unity `.gitignore`
  - `CharacterData` / `CharacterDatabase` / `AbilityType` (data layer)
  - `SaveData` / `SaveManager` (atomic JSON saves) / `GameManager` (persistent singleton)
  - `MainMenuManager` / `CharacterSelector` / `CharacterCardUI` (data-driven UI)
- ✅ **Step 2 — Touch controls + Pomni player controller**
  - `IInputSource` / `VirtualJoystick` / `TouchButton` / `MobileInput` (touch + editor keyboard)
  - `PlayerController2D` (coyote time, jump buffer, variable jump, character-driven tuning)
  - `PlayerAnimator` (safe Animator parameter driving)
  - Modular abilities: `PlayerAbility` base, `GlitchDashAbility` (Pomni), `AbilityController` + cooldown
- ✅ **Step 3 — Circus Hub scene**
  - `EpisodeData` / `EpisodeDatabase` (chapter data layer)
  - `GameManager` extended: episode selection/unlocking + sanity/abstraction system & events
  - `SceneLoader` (async scene loading with progress)
  - `HubManager`, `EpisodeSelector` / `EpisodeCardUI`, `SanityMeterUI`
- ⬜ Step 4 — Episode 1 (Pilot) level prototype
- ⬜ Step 5–… — Episodes 2–9, bosses, endings, polish

## Getting Started

See **[`docs/SETUP.md`](docs/SETUP.md)** for full instructions: creating the Unity
project, installing packages, building the character data assets, and wiring the
Main Menu scene.

## Repository Layout

```
Assets/Scripts/   Core · Data · UI · Characters · Gameplay
Assets/Scenes/    MainMenu, Hub, Episode levels
Assets/Prefabs/   CharacterCard, player, enemies …
Assets/Resources/Data/   CharacterDatabase (loaded at runtime)
docs/             Setup & design notes
```
