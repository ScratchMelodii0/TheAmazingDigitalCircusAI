# The Amazing Digital Circus — Fangame · Setup Guide (Step 1)

This document covers everything needed to create the Unity project, install the
right packages, and wire up the **Main Menu + Character Selector** that ship in
Step 1.

> The C# in `Assets/Scripts/` is engine-ready. Unity itself generates the binary
> project files (`ProjectSettings/`, `.unity` scenes, `.meta` GUIDs) the first
> time you open the folder, so those are intentionally **not** committed here —
> follow the steps below to generate them locally.

---

## 1. Create the Unity Project

1. Install **Unity Hub** and a recommended editor version:
   - **Unity 6000.0 LTS** (preferred for mobile) or **Unity 2022.3 LTS** minimum.
   - During install, tick the **Android Build Support** (SDK/NDK + OpenJDK) and
     **iOS Build Support** modules.
2. In Unity Hub: **New Project ▸ 2D (URP)** template.
3. Set **Project name** to `TheAmazingDigitalCircusAI` and set the location to a
   *fresh* folder, then copy the contents of this repo's `Assets/`, `.gitignore`,
   and `docs/` into it (or clone this repo and point Unity Hub at it — Unity will
   regenerate `Library/` and `ProjectSettings/`).

If you used the plain **2D** template instead of **2D (URP)**, install URP via the
Package Manager (next section) and create a URP asset under
`Assets/Settings/` (**Create ▸ Rendering ▸ URP Asset (with Universal Renderer)**),
then assign it in **Project Settings ▸ Graphics ▸ Scriptable Render Pipeline**.

---

## 2. Required Packages

Open **Window ▸ Package Manager** (registry: *Unity Registry*) and install:

| Package | Why |
|---|---|
| **Universal RP** | Lightweight mobile-friendly rendering. |
| **Input System** | Touch / virtual-joystick controls (used in Step 2). |
| **Cinemachine** | Side-scrolling follow camera (Step 4). |
| **TextMeshPro** | All UI/dialogue text. Run *Window ▸ TextMeshPro ▸ Import TMP Essentials*. |
| **2D Animation** | Bone/sprite animation for characters (later). |
| **2D Tilemap Editor** | Level building (Step 4). |
| **2D Sprite** | Sprite editing (usually pre-installed in 2D templates). |

When prompted to enable the **new Input System backend**, choose **Both** (or
*Input System Package*) and let the editor restart.

---

## 3. Folder Structure

The repo already defines the structure (kept via `.gitkeep` files):

```
Assets/
├── Animations/
├── Audio/{Music,SFX}/
├── Prefabs/
├── Resources/Data/        ← CharacterDatabase asset lives here
├── Scenes/                ← MainMenu.unity, Hub.unity, Episode01.unity …
├── Scripts/
│   ├── Core/              ← GameManager, SaveManager, SaveData
│   ├── Characters/        ← player controller (Step 2)
│   ├── Data/              ← CharacterData / CharacterDatabase / AbilityType
│   ├── Gameplay/          ← sanity, puzzles, mini-games (later)
│   └── UI/                ← MainMenuManager, CharacterSelector, CharacterCardUI
├── Settings/              ← URP assets
├── Sprites/{Characters,Environment,UI}/
└── UI/                    ← fonts, UI atlases
```

---

## 4. Create the Character Data Assets

For each of the six cast members, create an asset:

1. **Assets ▸ Create ▸ Digital Circus ▸ Character Data**.
2. Fill in the fields. Suggested starting values:

| Character | id | Speed | Jump | Sanity Drain | Ability | Unlocked by default |
|---|---|---|---|---|---|---|
| Pomni | `pomni` | 8 | 15 | 1.0 | Glitch Dash | ✅ |
| Ragatha | `ragatha` | 7 | 13 | 0.8 | Patchwork Mend | ❌ |
| Jax | `jax` | 8 | 14 | 0.9 | Wall Jump & Pranks | ❌ |
| Gangle | `gangle` | 6 | 12 | 1.4 | Ribbon Glide | ❌ |
| Kinger | `kinger` | 6 | 14 | 1.5 | Erratic Blink | ❌ |
| Zooble | `zooble` | 7 | 13 | 1.0 | Modular Swap | ❌ |

3. **Placeholder art:** until real sprites exist, assign Unity's built-in
   `UISprite`/`Knob`, or make a solid-color 64×64 square (right-click in
   Project ▸ *Create ▸ Sprites ▸ Square*) and tint it with each character's
   **Theme Color**. Drop these in `Assets/Sprites/Characters/`.
4. **Assets ▸ Create ▸ Digital Circus ▸ Character Database**, name it
   `CharacterDatabase`, and place it in **`Assets/Resources/Data/`** (the path the
   `GameManager` loads from). Drag all six character assets into its list, Pomni
   first.

---

## 5. Build the Main Menu Scene

1. Create **`Assets/Scenes/MainMenu.unity`** and add it as scene 0 in
   **File ▸ Build Settings**.
2. Add a **Canvas** (Screen Space – Overlay). On its **Canvas Scaler** set:
   - UI Scale Mode → *Scale With Screen Size*
   - Reference Resolution → `1080 × 1920` (portrait)
   - Match → `0.5`
3. Under the Canvas create three child panels: `HomePanel`,
   `CharacterSelectPanel`, `SettingsPanel`.
4. **HomePanel** — add TMP buttons: Play, Continue, Settings, Quit + a title label.
5. **CharacterSelectPanel**:
   - Add a `CardContainer` child with a **Grid Layout Group** (e.g. cell 240×320,
     3 columns).
   - A **Detail** sub-panel with TMP texts for name/description/ability/unlock
     hint, plus a portrait `Image`.
   - A **Confirm** button and a **Back** button. Optionally a **Start Adventure**
     button.
6. Create the **CharacterCard prefab** (`Assets/Prefabs/CharacterCard.prefab`):
   a `Button` with child `Image` (portrait), child `TMP_Text` (name), a
   `LockOverlay` object, and a `SelectionFrame` outline. Add the
   **`CharacterCardUI`** component and wire its fields.
7. Add an empty **`MenuRoot`** object; attach **`MainMenuManager`** and assign the
   three panels and all buttons.
8. Attach **`CharacterSelector`** to `CharacterSelectPanel`; assign the card
   container, the CharacterCard prefab, the detail labels, and the confirm button.

Press **Play** — the grid builds itself from the database, locked characters show
as `???`, and selecting Pomni persists the choice to
`Application.persistentDataPath/circus_save.json`.

---

## 6. Mobile Project Settings

- **Edit ▸ Project Settings ▸ Player**:
  - *Resolution and Presentation* → Default Orientation: **Portrait** (or
    *Auto Rotation* if you want both).
  - Android → Minimum API Level 24+, Scripting Backend **IL2CPP**, target
    architectures **ARMv7 + ARM64**.
  - iOS → set a Bundle Identifier and target minimum iOS 12+.
- **Quality**: trim to one or two tiers for mobile and set them as default for
  Android/iOS.

---

## 7. Git Milestone

```bash
git add .
git commit -m "Step 1: project structure, save system, main menu + character selector"
git push -u origin claude/digital-circus-mobile-game-efbho4
```

Next step (on request): **virtual touch controls + the Pomni player controller**.
