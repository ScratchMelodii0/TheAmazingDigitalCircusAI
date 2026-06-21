# Step 2 — Touch Controls + Pomni Player Controller

This step adds a mobile input layer, a physics-based 2D platformer controller that
reads tuning from the selected character, an animator driver, and a modular ability
system with **Pomni's Glitch Dash** implemented.

## New scripts

| Script | Role |
|---|---|
| `Gameplay/Input/IInputSource.cs` | Interface the controller depends on (decouples input). |
| `Gameplay/Input/VirtualJoystick.cs` | On-screen analog stick (pointer events, multi-touch safe). |
| `Gameplay/Input/TouchButton.cs` | On-screen button reporting press / held / this-frame. |
| `Gameplay/Input/MobileInput.cs` | Aggregates stick + buttons into `IInputSource`; keyboard fallback in editor. |
| `Characters/PlayerController2D.cs` | Rigidbody2D controller: coyote time, jump buffer, variable jump, char-driven tuning. |
| `Characters/PlayerAnimator.cs` | Safely drives Animator params from controller state. |
| `Characters/Abilities/PlayerAbility.cs` | Abstract ability base (coroutine-driven). |
| `Characters/Abilities/GlitchDashAbility.cs` | Pomni's dash (full implementation). |
| `Characters/Abilities/PlaceholderAbility.cs` | Temporary hop for not-yet-built abilities. |
| `Characters/Abilities/AbilityController.cs` | Input → ability factory + cooldown. |

## Unity version note

The controller uses `Rigidbody2D.linearVelocity` (the Unity 6 API), matching the
recommended **Unity 6000 LTS**. If you target **Unity 2022.3 LTS**, do a
find-and-replace of `linearVelocity` → `velocity` in `PlayerController2D.cs` and
`GlitchDashAbility.cs` (the 2022.3 property name).

## Prerequisite: enable the Input System

**Edit ▸ Project Settings ▸ Player ▸ Active Input Handling → "Both"** (or *Input
System Package*). The keyboard fallback compiles against whichever backend is
active, so either setting works; "Both" is safest. Install the **Input System**
package if you haven't (see `docs/SETUP.md`).

## Scene setup — a test level

Until the real Hub/levels exist, make a scratch scene to feel the controls:

1. **Ground:** a long Sprite (square) with a `BoxCollider2D`. Put it on a new layer
   called **Ground**.
2. **Player object** (`Assets/Prefabs/Player.prefab` recommended):
   - Add `Rigidbody2D` → Gravity Scale ≈ 3, **Freeze Rotation Z**, Collision
     Detection = Continuous, Interpolate = Interpolate.
   - Add a `CapsuleCollider2D` (or Box) sized to the body.
   - Child empty **`GroundCheck`** placed at the feet.
   - Child **`Sprite`** with a `SpriteRenderer` (placeholder square; real body sprite
     comes from `CharacterData.BodySprite` at runtime).
   - Add **`PlayerController2D`**: assign Sprite, GroundCheck, set **Ground Layer**
     to the Ground layer, and assign the **MobileInput** (created below).
   - Add **`AbilityController`**: assign the same MobileInput.
   - (Optional) Add **`PlayerAnimator`** + an Animator with the params listed in the
     script header.
3. **HUD Canvas** (Screen Space – Overlay, Canvas Scaler = Scale With Screen Size,
   1080×1920):
   - **JoystickBackground** Image (bottom-left) with a child **Handle** Image →
     add `VirtualJoystick`, assign the handle.
   - **JumpButton** Image (bottom-right) → add `TouchButton`.
   - **AbilityButton** Image (bottom-right) → add `TouchButton`.
   - **GameplayInput** empty object → add `MobileInput`, assign the joystick + both
     buttons.
   - Ensure there is an **EventSystem** in the scene (Unity adds one with the Canvas;
     it uses the Input System UI module when that backend is active).
4. Press **Play**: move with the stick / **A-D**, jump with the button / **Space**,
   dash with the ability button / **Left-Shift** or **J**. The controller pulls
   Pomni's speed, jump, and Glitch Dash from her `CharacterData` asset.

## Notes & next hooks

- `AbilityController.CooldownNormalized` is ready to drive a HUD radial cooldown fill.
- `PlayerController2D.ControlEnabled` lets dialogue/cutscenes lock movement (used in
  later steps); `SetExternalMotionOverride` is how abilities borrow physics.
- Other characters' abilities currently use `PlaceholderAbility` and will get
  dedicated classes (wall-jump, glide, blink, modular swap) in later steps.

## Git milestone

```bash
git add .
git commit -m "Step 2: mobile touch controls + Pomni player controller & ability system"
git push -u origin claude/digital-circus-mobile-game-efbho4
```

Next step (on request): **the Circus Hub scene** — central menu/world that ties
character select, episode select, and the sanity meter together.
