# Project Status — SoulsLikeDemo → Sci-Fi Tower Defense

_Last updated: 2026-10-01. Read this first at the start of every session._

## 1. Where the game is going

The third-person combat prototype is becoming a **sci-fi "Orcs Must Die!"-style third-person tower defense**, sold for **$8–10 on Steam**.

**Hooks**
- **Doom-style "get up close" rule:** stagger an enemy → finish it up close → it **heals you**. Gun kills drop **bolts** (money). Melee and guns both matter, and you can't camp.
- **Beefy player** who wades into crowds and survives by finishing enemies.
- **Weapons:** sword + sci-fi guns (blaster, shotgun, rocket launcher, BFG-style). Buy, upgrade and sell them between waves.
- **Helpers:** traps (e.g. **electric stun land mines**), **turrets**, **drones**. Traps and turrets are placed on a grid.
- **Movement:** the double jump, air control and jump attack stay. The jump attack becomes a **"Meteor Slam"** (shockwave, then upgrades to stun, which makes enemies finisher-ready). Levels have height (perches, catwalks, launch pads).

**Two tiers of enemies**
- **Fodder:** simple, in big numbers. **No full DOTS rewrite.** They're data-oriented: positions and state in NativeArrays, Burst jobs, a shared **flow field** to the core, a **spatial grid** for hit queries, **GPU instancing**, **vertex animation textures**, pooling. Target: a few hundred to a couple of thousand. **Build a stress test early** to set real numbers.
- **Elites and bosses:** the current detailed `EnemyStateMachine` enemies, about 10–20 at a time.

**Scope of a lean 1.0 (about 3 months at 6–8 hours a day)**
- 3 levels (each with its own enemy mix and boss), **1 playable character** (2nd and 3rd later), 6–8 traps, about 5 weapons, turrets and drones.
- **Demo around week 6** (aim for Steam Next Fest). Put the Steam page up early to collect wishlists.
- Planned order: W1 wave director + core + bolts + finisher-heal · W2 trap grid + shop · W3 fodder swarm + spatial grid (stress test) + attack tokens · W4 guns + level 1 playable · W5–6 polish level 1, Steam page, demo · W7–8 level 2 · W9–10 level 3 · W11–12 polish/launch.
- The current scene (`PlayTestScene`) becomes the **combat testbed**. The TD game gets its own scenes.

**Next step: a PLANNING phase before implementing** (see section 6).

## 2. Working agreements

- **Gameplay/combat code:** the user builds it, with step-by-step guidance (exact locations, small steps), **unless they ask me to write it** ("can you set it up / do it").
- **Editor, visual, animation, VFX, UI and tooling work:** I do it directly through the Unity CLI (`unity command eval_file`).
- **No magic numbers.** Use named consts or `[field: SerializeField]` Inspector values.
- **Never commit.** The user commits. Pushing only on request.
- **Always check Play mode before scene edits** (changes made during Play are lost). **Save the scene after editor changes.**
- **The Enemy prefab is the source of truth.** Edit `Assets/Prefab/Enemy.prefab`, not scene copies. With multiple enemies, `FindAnyObjectByType` picks an arbitrary one.

## 3. What's built (systems)

**Player** (`Assets/Scripts/StateMachine/Player`)
- **3-hit combo:** Light Combo A → B → Heavy C finisher (Synty Sword Combat clips). Combo windows come from Synty's WindUp/Hit sub-clip data. Hitbox events (`EnableWeapon`/`DisableWeapon`) are set in the FBX importer.
- **Recovery state:** plays ReturnToIdle after the last hit, cancellable by move/attack/dodge/jump (`PlayerRecoveryState`, `Attack.RecoveryAnimationName`).
- **Steering during attacks** when not locked on (`AttackDirectionSteering`).
- **Dodge roll** with i-frames. **Lock-on** (Targeter), with the camera switched by lock-on state (`LockOnCameraSwitcher`), not by animation.
- **Guard:** blocking stops **front** hits (they drain the guard meter, which is a `Poise` on the player). Back and side hits get through. When the guard breaks → `PlayerGuardBreakState` (A_Parry_Break). Block sparks and a block sound play.
- **Directional hit reactions** (`HitDirection.cs`): a light flinch, or a stagger if damage ≥ `HeavyHitDamage`.
- **Jumping:** Ratchet-style air control (`MovementWhileInAir`), a rise multiplier, a **double jump** (Mixamo flip, root Y/XZ bake turned OFF), **coyote time**, and a **lock-on gap** (lock-on kept over small gaps, dropped after `LoseTargetInAir`).
- **Jump attack:** `PlayerJumpAttackState` with phases Raise → Plunge (pose frozen, `ForceReceiver.Plunge`) → Slam on landing → recovery. It uses the trimmed clip `A_Attack_HeavyFlourish_JumpAttack_Sword` (frames 41–62). Triggered by a **fresh** attack press (`AttackPressedThisFrame`) from Jumping/Falling (the user commented it out in DoubleJump).

**Enemy** (`Assets/Scripts/StateMachine/Enemy`, knife grunt)
- Idle → Patrol (waypoints) → **Alert** (taunt + "!" + sound, skippable with `SkipAlert` for ambushes) → Chase → **Engage** (circles with strafe blend tree, random wait, punishes you for walking in) → **Attack** (windup **tell**: freeze + yellow flash, random hold, quick attacks, combo chance) → back to Engage.
- **Poise:** only staggers when broken. Directional stagger + **dizzy stars**. An additive **Flinch** layer plays on every hit.
- Knife **re-grip** (`WeaponGrip` + `WeaponGripSwitch`), and the **GuardArm** layer keeps the knife pointed forward while moving.
- World-space **health bar** (`HealthBarUI` + `UI_EnemyHealthBar`, Synty style, white damage chunk).

**Combat core** (`Assets/Scripts/Combat`): `Health` (blocking, last-hit info, read-only getters for UI), `Poise`, `WeaponDamage` (hit stop goes through Feel), `HitFlash`, `HitDirection`, `LayerWeightBlend`, `WeaponGrip(Switch)`.

**Game feel (Feel / More Mountains)**
- A `GameFeel` object with **MMTimeManager**, which owns time scale: hit stop = `MMFreezeFrameEvent`. The old `HitStop.cs` is deleted.
- Player feedbacks (`CaptainSam/Feedbacks`): **Jump** = stretch + dust + sound. **Double Jump** = stretch + dust + `VFX_AirRing` + higher-pitched sound. **Land** = squash + dust + thud + small camera kick, scaled by fall time (`Min/MaxLandIntensity`, `AirTimeForMaxLandIntensity`). **Slam** (`SlamFeedback`, played by the jump attack on landing) = big squash + dust + `VFX_SlamShockwave` (3 m yellow ring, cracks, sparks) + camera kick + chromatic + 0.05 s freeze + boom, all on contact. **HitConfirm** (on `WeaponDamage.hitFeedback`, player sword only) = chromatic kick + impact sound. **Hurt** = red vignette + chromatic aberration. The volume profile is `Assets/Settings/GameVolumeProfile.asset`.
- Enemy prefab: `SM_Chr_Alien_01/Feedbacks/HitReactFeedback` squashes the `Root` bone on every hit (`EnemyStateMachine.HitReactFeedback`).
- **Gotcha:** Enter Play Mode Options has domain + scene reload OFF, so in-memory objects go into Play as-is. Feel feedbacks added by script (`AddFeedback`) have null lists (`RandomParticlePrefabs`, `RandomSfx`) until the scene is reloaded → NRE at `MMF_Player.Start` that silences the whole player. After adding feedbacks by script: fill null lists/arrays, save, and reopen the scene before testing.
- **Gotcha 2:** Feel `MMF_ParticlesInstantiation` in OnDemand mode with *Cached Recycle* replays ONE instance. Particle prefabs used by feedbacks must have **Stop Action = None** (not Destroy), or the effect plays once and the exception kills every feedback after it in the list.
- **Movement feedbacks go through `PlayerStateMachine.PlayMovementFeedback(...)`**, which stops any running squash and resets the `Root` bone to rest first. Feel's squash remembers the current scale as "normal", so overlapping squashes made the player shrink permanently.
- Sounds currently borrowed from Feel's demo folders (`FeelDuckBoom`, `FeelBounceJump/Landing`, `FeelStrikeHit`, `FeelBarbarianHit`); swap for final SFX later.
- **Slam beat** (`PlayerJumpAttackState`): hang `JumpAttackHangTime` 0.15 s with the sword going up (`ForceReceiver.Hover`) → dive at `JumpAttackPlungeSpeed` 24 frozen at the overhead pose (`HoldPoseTime` 0.143 = frame 44) → on contact **snap** to `JumpAttackImpactTime` 0.333 (frame 48, sword in the ground) and freeze `JumpAttackImpactHoldTime` 0.18 s → follow-through. Clip frames 41–62, hitbox events 0.34 / 0.48. Slam feedback fires on contact (no delays).

**Visuals**
- **Cel shader** (`Assets/Shaders/CelShader.shadergraph`), cel materials in `Assets/Materials/Cel`.
- **Comic ink outlines** (`Assets/Shaders/ComicOutline.shader`, a full-screen pass on `PC_Renderer`, before transparents; tune it on `ComicOutline.mat`).
- **Toon VFX shader** (`Assets/Shaders/ToonVFX.shader`): comic sword trails, hit stars (`VFX_HitStar` yellow / `_Player` red), block sparks, alert "!", stun stars, dust. Blood was removed on purpose.

**HUD**
- `Assets/Prefab/UI/UI_GameHUD.prefab`, based on Synty **Screen_HUD_SciFiSoldier_Minimal_01** (3840×2160 reference, match height), **no compass**.
- Health line (`HUDBar`), guard hex dial with armor icon (`HUDDial`), notification feed (`EventLogUI.Show("...")`: "Guard Broken!", "Enemy Defeated"), lock-on marker = objective triangle + distance (`LockOnIndicator`).
- Hidden for later: timer, compass, crosshair, subtitles, weapon panel.
- `UI_PlayerHUD.prefab` is the old version, unused, and can be deleted.

## 4. Open / pending items

1. **HUD (on hold, recolor NOT applied):** the user likes the boot-up intro (`Techy_Stuff`) and wants to replay it **per wave** later; the blue line decorations should go. Target HUD = health + guard + weapons only. `docs/tools/RecolorGameHUD.eval.cs.txt` is outdated (it turns the intro off) — revise before use.
2. **Slam AoE damage (user is building it):** the slam needs a small radius hit (3 m to match the ring) on landing: overlap → `Health`/`Poise`/`ForceReceiver` on each enemy once. The visuals are done.
3. **Push to GitHub:** `origin` currently points to the empty `CaptainSamReturns` repo (created by accident). The user wants **SoulsLikeDemo**, but `github.com/KAOSN00B/SoulsLikeDemo` returned "not found", so the exact URL is needed. Git LFS is set up. The user commits first; don't force-push.
4. Things to review in Play mode: the new feel pass (slam, hits, jumps/landings), the jump attack timing, the HUD, the alert flow with two enemies.
5. Deferred: enemy death effect (options: bolt burst / comic poof / robot pop), enemy return-to-idle after attacks, the player prefab still has scene overrides (consider Apply All on CaptainSam).

## 5. Asset notes

- Synty packs: Polygon SciFi City/Space, Animation Sword Combat (Polygon), Base Locomotion, PolygonParticleFX, Interface SciFi Soldier HUD. Feel is in `Assets/Feel`.
- **Synty hit clip letters = the direction the body is pushed** (a hit from the front uses the "B" clip).
- Synty attack FBXs have WindUp / Hit / FollowThrough sub-clips. Use them for event timing. Trimmed clips are added as **new importer clips** (the originals are untouched).
- Mixamo clips used in place need **Root Transform Position Y/XZ "Bake Into Pose" OFF** (counter-intuitive).

## 6. Next session: planning-phase goals (before implementing)

1. ~~HUD recolor~~ (on hold, see 4.1). Sort out the push (needs the SoulsLikeDemo URL).
2. **Planning done (2026-10-01):** GDD, content list, architecture, 12-week plan, hook + Steam/Next Fest timing live in the doc **"Sci-Fi TD — 1.0 Planning"**: https://claude.ai/code/artifact/95f4e743-a78d-4c2b-8f96-97acfdc8c38b (user decisions are in its section 6; launch date TBD; price open).
3. **Current focus: finalize combat** before building TD systems (feel pass done; slam AoE next).
