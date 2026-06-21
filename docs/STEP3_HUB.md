# Step 3 — The Circus Hub

The Hub is the central tent the player returns to between chapters. It shows the
active character and the **sanity / abstraction** meter, hosts character- and
episode-select, and launches episodes via async loading.

## New / changed scripts

| Script | Role |
|---|---|
| `Data/EpisodeData.cs` | ScriptableObject per chapter (title, scene, unlock, theme). |
| `Data/EpisodeDatabase.cs` | Ordered episode list, loadable from `Resources/Data/`. |
| `Core/GameManager.cs` *(extended)* | Episode selection/unlock + sanity system (`ModifySanity`, `ResetSanity`, `OnSanityChanged`, `OnAbstracted`, `CompleteEpisode`). |
| `Core/SceneLoader.cs` | Async scene loading with 0–1 progress + double-load guard. |
| `UI/SanityMeterUI.cs` | Filled-image meter that lerps + recolors (green→yellow→red). |
| `UI/EpisodeCardUI.cs` | One episode entry (number, title, lock, selection frame). |
| `UI/EpisodeSelector.cs` | Builds the chapter list, detail panel, confirms a playable episode. |
| `UI/HubManager.cs` | Orchestrates the Hub: character display, panels, episode launch. |

## Create the Episode assets

1. **Assets ▸ Create ▸ Digital Circus ▸ Episode Data** — make nine, one per episode:

| # | id | Title | Scene | Unlocked by default |
|---|---|---|---|---|
| 1 | `ep01_pilot` | Pilot | `Episode01` | ✅ |
| 2 | `ep02_candy` | Candy Canyon | `Episode02` | ❌ |
| 3 | `ep03_manor` | Mildenhall Manor | `Episode03` | ❌ |
| 4 | `ep04_fishtank`| Spudboy / Fishtank | `Episode04` | ❌ |
| 5 | `ep05_digital` | Digital Doodles | `Episode05` | ❌ |
| 6 | `ep06` | Episode 6 | `Episode06` | ❌ |
| 7 | `ep07` | Episode 7 | `Episode07` | ❌ |
| 8 | `ep08` | Episode 8 | `Episode08` | ❌ |
| 9 | `ep09_lastact`| The Last Act | `Episode09` | ❌ |

2. **Assets ▸ Create ▸ Digital Circus ▸ Episode Database**, name it
   `EpisodeDatabase`, place it in **`Assets/Resources/Data/`**, and drag the nine
   episodes in (Pilot first). Placeholder thumbnails: colored squares.

## Build the Hub scene

1. Create **`Assets/Scenes/Hub.unity`** and add it to **Build Settings**
   (so the Main Menu's "Play / Start Adventure" can load it).
2. Canvas (1080×1920, *Scale With Screen Size*). Children:
   - **HubRoot** — character portrait + name, a **SanityMeter** (Image with
     *Image Type = Filled, Horizontal* + a `SanityMeterUI`), and buttons:
     *Select Episode*, *Change Character*, *Settings*, *Back to Menu*. Add a
     `welcomeLabel` TMP for Caine's greeting.
   - **CharacterSelectPanel** — reuse the **CharacterSelector** + CharacterCard
     prefab from Step 1, plus a Back button.
   - **EpisodeSelectPanel** — a `CardContainer` (Vertical/Grid Layout), the
     **EpisodeCard** prefab (`EpisodeCardUI`), a detail sub-panel
     (title/subtitle/description/thumbnail), a **Play** button, and a Back button.
     Add `EpisodeSelector` and wire its fields.
   - **LoadingPanel** — full-screen overlay + a `Slider` (`loadingProgressBar`).
3. Add a **HubManager** object; wire all panels, the character display refs, the
   `EpisodeSelector`, the loading bar, and every button.
4. Make the **EpisodeCard prefab** (`Assets/Prefabs/EpisodeCard.prefab`): a Button
   with Thumbnail/Number/Title children, a LockOverlay, a SelectionFrame, and the
   `EpisodeCardUI` component wired.

## How it flows

- Entering the Hub calls `GameManager.ResetSanity()` (the tent is safe).
- *Change Character* opens the Step 1 selector; confirming it persists the choice and
  the Hub portrait refreshes via `OnSelectedCharacterChanged`.
- *Select Episode* → pick an unlocked chapter → **Play** → `HubManager` shows the
  loading panel and `SceneLoader` streams in the episode scene.
- Completing a level later calls `GameManager.CompleteEpisode(index)` to unlock the
  next chapter; locked episodes show as "Locked" with a hint.
- The sanity meter is data-driven: anything that calls `GameManager.ModifySanity(-x)`
  (stress events in levels) animates the bar and fires `OnAbstracted` at zero.

## Git milestone

```bash
git add .
git commit -m "Step 3: Circus Hub scene, episode data + selection, sanity system, async loading"
git push -u origin claude/digital-circus-mobile-game-efbho4
```

Next step (on request): **Episode 1 (Pilot) level prototype** — tilemap level,
exit-key collection, Gloink hazards, the active sanity drain, and a level-complete flow.
