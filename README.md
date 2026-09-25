# Juice Galaxy

A chaotic, physics-driven VR action game for **Meta Quest**, built natively in Unity (URP + OpenXR).
You play a highly elastic, floppy little creature exploring a school and a galaxy of floating
planetoids, smacking bizarre creatures around with momentum-based melee, and chugging **Juice** -
the game's single resource for health, currency and unlocking new abilities.

You spawn in a fever-dream classroom watched over by **Mrs. Slithers**, a floppy, weird snake
teacher who lurks by her blackboard. Out the door is a playground with crate stacks (they give
Juice, but a toppling stack can crush you), a hue-cycling rainbow bouncy ball, and **Ingot**, who
will teach you to fly if you hold the **A** button near him. Once you've learned to fly, the
surrounding planetoids are yours to explore.

This whole project is generated **entirely from code** - there is one hand-authored Unity scene
(`Assets/Scenes/Bootstrap.unity`) containing a single empty GameObject running `GameBootstrap.cs`.
Everything else - the school, the playground, the planetoids, the player's XR rig, the floppy
character rigs, Mrs. Slithers, Ingot, the enemies, and the HUD - is built at runtime in `Awake()`
using primitive meshes, procedurally generated low-poly icospheres, and procedurally generated
textures. There are no imported art assets to go stale or break.

## What's implemented

- **Native Quest VR**: OpenXR + Unity XR Plugin Management, head/hand tracking built from the
  Input System's XR device layouts (no prefabs required).
- **Floppy, physics-driven protagonist**: a `CharacterController`-based locomotion rig (reliable
  movement/collision) with a purely cosmetic elastic body - jiggly tentacle-sleeve arms trailing
  from your tracked hands and a floppy tail, all built from spring-jointed physics chains
  (`FloppyChain.cs`).
- **Momentum-based melee combat**: your fists (and Mrs. Slithers' head, and every creature) track
  their own real-world velocity every physics step; damage scales with how fast you actually swing
  (`MomentumMeleeHitbox.cs`).
- **Juice**: one resource that is simultaneously your health pool and your progression currency
  (`JuiceSystem.cs`). Crates and defeated creatures drop Juice pickups; combat and getting crushed
  drain it.
- **Flight**: locked until Ingot teaches you. Hold the right controller's **A** button near him for
  ~1.5s to unlock it, then hold **A** anywhere to fly in your look direction (`PlayerFlight.cs`,
  `IngotFlightTutor.cs`).
- **The school**: a low-poly classroom matching the reference art direction - mottled tan walls, a
  teal/blue checkered floor, three colorful cut-out windows, rows of desks, and a blackboard reading
  "Mrs. S..." (`SchoolBuilder.cs`).
- **The playground**: crate stacks that reward Juice when punched apart but can crush you if they
  topple onto you (`CrateStack.cs`, `CrushHazard.cs`), and a rainbow bouncy ball
  (`RainbowBouncyBall.cs`).
- **Mrs. Slithers**: a big floppy, weird snake NPC/mini-boss with a spring-jointed noodle body,
  cartoon eyes, fangs and a tongue, who idles, notices, and lunges at the player
  (`MrsSlithersNPC.cs`, `MrsSlithersAI.cs`).
- **Ingot**: a friendly golden bar-shaped NPC with floppy limbs who teaches flight
  (`IngotNPC.cs`).
- **Wobbly creatures**: small bizarre blob enemies wandering the playground and planetoids that
  wander/chase/lunge and drop Juice on death (`WobblyCreature.cs`, `CreatureAI.cs`).
- **HUD**: segmented health/Juice bars pinned to the top-left of view, styled after the reference
  screenshot (`JuiceBarUI.cs`).

## Project layout

```
Assets/
  Scenes/Bootstrap.unity      <- the only hand-authored scene; just runs GameBootstrap
  Scripts/
    Core/                     GameBootstrap, GameManager, JuiceSystem
    XR/                       Runtime-built XR head/hand tracking + input actions
    Player/                   Locomotion, momentum melee fists, flight, floppy visuals
    Combat/                   Health, MomentumMeleeHitbox, CrushHazard
    World/                    School, playground, planetoids, crates, pickups, bouncy ball
    NPC/                      Mrs. Slithers, Ingot, wobbly creatures + their AI
    UI/                       World-space labels, HUD bars
    Utils/                    Procedural meshes/textures, primitive builders, FloppyChain
```

## Opening the project

This was built without access to the Unity Editor, so a few one-time setup steps that the Editor
normally does interactively haven't been done yet:

1. **Install Unity 2022.3 LTS** (any recent patch; the project pins `2022.3.50f1` but Unity will
   offer to open it with whatever 2022.3.x you have installed) via Unity Hub, with the **Android
   Build Support** module (+ its SDK/NDK/OpenJDK components) checked during installation.
2. Open this folder as a Unity project. Unity will import the packages from `Packages/manifest.json`
   (URP, OpenXR, XR Interaction Toolkit, XR Core Utils, Input System, uGUI) - this can take a few
   minutes the first time.
3. **Switch platform to Android**: `File > Build Settings > Android > Switch Platform`.
4. **Enable OpenXR for Android**: `Edit > Project Settings > XR Plug-in Management` → check the
   Android tab → enable **OpenXR** → under OpenXR's Android settings, add the **Meta Quest Support**
   feature group and the **Oculus Touch Controller Profile** interaction profile.
5. **Activate URP**: `Edit > Project Settings > Graphics` → if no Render Pipeline Asset is assigned,
   create one via `Assets > Create > Rendering > URP Asset (with Universal Renderer)` and assign it
   there (and under Quality settings). The runtime-generated materials look for the URP Lit/Unlit
   shaders and fall back gracefully if URP isn't active yet.
6. **Player Settings for Quest**: set the package name (`Edit > Project Settings > Player > Android
   > Other Settings > Package Name`, e.g. `com.yourname.juicegalaxy`), minimum API level 29+, and
   Scripting Backend to IL2CPP with the ARM64 architecture (required by Quest).
7. Confirm `Assets/Scenes/Bootstrap.unity` is the only scene in `File > Build Settings` (it should
   already be listed via `ProjectSettings/EditorBuildSettings.asset`).
8. **Build & Run** with your Quest connected over USB (Developer Mode enabled), or `File > Build
   Settings > Build` to produce an APK and side-load it.

## Controls

- **Left thumbstick**: move (relative to where you're looking)
- **Right thumbstick (flick left/right)**: snap turn
- **Swing your hands**: momentum melee - the faster you swing, the harder you hit
- **Hold A (right controller)**: fly, once Ingot has taught you how
- **Walk into Ingot and hold A** for ~1.5 seconds: unlocks flight

## Known limitations

- This was authored entirely outside the Unity Editor (no Editor/Android SDK available in this
  environment), so it has not been compiled, opened, or run yet. Everything above follows Unity's
  well-documented APIs and formats, but please open it in the Editor and check the Console for any
  first-run errors before flashing it to a headset.
- All visuals are procedurally generated low-poly primitives/textures rather than authored art, in
  keeping with the chunky, fever-dream aesthetic of the reference screenshot.
- There's no full-body/leg IK, save/load, or menu system yet - this is a playable core loop, not a
  finished, content-complete game.
