# The Branded (Mühürlü)

**Third-person hack & slash roguelite**: the run loop of Hades in the post-Eclipse mood of Berserk.

You play a warrior who walked out of the Eclipse massacre with one arm and one eye. The **Brand of Sacrifice** on his neck bleeds every night and pulls the dead and the demons toward the smell of his blood. Nights are spent in savage melee; mornings are spent breathing by a campfire.

> Built with Unity 6 (URP). Currently at the **greybox** stage: the models are capsules and cubes, the portraits are placeholders.

---

## Game Loop

The game is built on the contrast between **Night Savagery** and **Morning Calm**.

```mermaid
flowchart TD
    subgraph HUB ["🏠 GODOT'S MINE (Permanent Safe Area / Hub)"]
        WakeUp["✨ Awakening: After the Eclipse<br/>• One Arm & One Eye Lost<br/>• The Brand of Sacrifice Bleeding"]
        Godot["🔨 Godot's Forge (Meta-Progression)<br/>• Dragonslayer Base Damage<br/>• Prosthetic Arm Mods: Cannon & Repeater Crossbow"]
        Puck["🧚 Puck & Healing Elf Dust<br/>• Permanent Health Capacity & Death Defiance"]
        Depart["🚪 Down the Mountain Path: Start the Run"]
        WakeUp --> Godot
        WakeUp --> Puck
        Godot --> Depart
        Puck --> Depart
    end

    subgraph RUN ["⚔️ RUN LOOP: BIOME 1 - THE CURSED LANDS"]
        subgraph NIGHT1 ["🌑 NIGHT 1: FOG AND RESTLESS SPIRITS"]
            N1_Start["🩸 The Brand Bleeds & Fog Falls<br/>(Vision narrows to the torch, a heartbeat rises)"]
            N1_Combat["⚔️ Savage Melee<br/>• Skeletons Clawing Out of the Ground & Shade Spirits<br/>• Heavy Dragonslayer Swings, Hitstop & Screen Shake"]
            N1_Survive["☀️ Dawn Breaks<br/>(The remaining demons evaporate screaming)"]
            N1_Start --> N1_Combat --> N1_Survive
        end

        subgraph DAWN1 ["🌅 MORNING 1: A QUIET CAMP"]
            D1_Camp["🔥 The Campfire Is Lit<br/>(Birdsong, calm acoustic music, a moment to breathe)"]
            D1_Dialogue["💬 2D Portrait Dialogue (Hades-style UI)<br/>(A talk with Rickert / a wandering mercenary)"]
            D1_Boon["🎁 Pick a Temporary Run Boon<br/>• Flame Oil on the Sword (Burn Damage)<br/>• Swift Dash Charm<br/>• Healing Bandage (+Health)"]
            D1_Camp --> D1_Dialogue --> D1_Boon
        end

        subgraph NIGHT2 ["🌑 NIGHT 2: DEMON HOUNDS AND ARMOURED PACKS"]
            N2_Start["🩸 The Brand Throbs Violently<br/>(Screen edges redden, red fog)"]
            N2_Combat["⚔️ High-Tension Fighting<br/>• Fast Demon Hounds & Armoured Dead Knights<br/>• Left Arm: Repeater Crossbow & Point-Blank Cannon"]
            N2_Survive["☀️ Dawn Breaks"]
            N2_Start --> N2_Combat --> N2_Survive
        end

        subgraph DAWN2 ["🌅 MORNING 2: RUINED SHRINE & THE SKULL KNIGHT"]
            D2_Shrine["🗿 Ruins of an Ancient Shrine"]
            D2_Skull["💬 The Skull Knight Appears<br/>(2D Portrait, a Prophetic Dialogue)"]
            D2_Relic["⚡ Pick an Ancient Relic Boon<br/>(A Large Temporary Buff Before the Apostle)"]
            D2_Shrine --> D2_Skull --> D2_Relic
        end

        subgraph BOSS_NIGHT ["🔥 NIGHT 3: BIOME BOSS - THE GREAT APOSTLE"]
            Boss_Spawn["👁️ The Sky Turns Blood Red<br/>(A Colossal Demon Apostle Lands)"]
            Boss_Fight["⚔️ A Multi-Phase Boss Fight<br/>• Dash Out of Area Attacks<br/>• Berserk Rage: High Damage & Draining Health"]
            Boss_Victory["🏆 The Apostle Is Slain!<br/>(A Great Demon Heart & Rare Black Ore)"]
            Boss_Spawn --> Boss_Fight --> Boss_Victory
        end

        Depart --> N1_Start
        N1_Survive --> D1_Camp
        D1_Boon --> N2_Start
        N2_Survive --> D2_Shrine
        D2_Relic --> Boss_Spawn
    end

    subgraph DEATH_SYSTEM ["💀 DEATH AND RETURN"]
        DeathEvent["⚰️ Defeated (Health Reached Zero)"]
        DragBack["Dark spirits drag you down...<br/>But the Brand's hunger for vengeance will not let you die!"]
        Respawn["🩸 Waking Up Bloodied at Godot's Forge<br/>(Temporary oils reset, Demon Ash is kept)"]
        DeathEvent --> DragBack --> Respawn
        Respawn --> Godot
    end

    N1_Combat -.->|Death| DeathEvent
    N2_Combat -.->|Death| DeathEvent
    Boss_Fight -.->|Death| DeathEvent

    Boss_Victory --> NextBiome["🌟 GREAT DAWN: ON TO BIOME 2<br/>(Or Return to the Mine With the Spoils)"]
    NextBiome -.-> Godot
```

For the full design, map, combat and architecture detail: **[Design Document (GDD)](docs/GDD.md)**

---

## Roadmap

| Stage | Contents | Status |
|---|---|---|
| 1. Greybox Foundations | WASD movement, turning, dash, sword swing with input buffering | ✅ |
| 2. Damage and the First Enemy | `IDamageable`, a NavMesh enemy, hitstop and hit feel | ✅ |
| 3. Night/Morning Cycle | Enemy waves, daybreak, campfire | ✅ |
| 4. Dialogue and UI | 2D portrait dialogue, health bar, temporary sword-oil pick | ✅ |
| 5. Godot's Workshop | Hub scene (mine + cottage + grove), permanent upgrades, death loop | ✅ |
| 6. Combat Depth | Combo chain with cancel windows, charged strike, spin attack, dash strike | ✅ |
| 7. Camera Change | From a fixed isometric angle to a Witcher 3-style orbital third-person camera | ✅ |
| 8. Character Model | A rigged, animated character to replace the greybox capsules | ⏳ |

## What Is in the Game Right Now

- **Combat:** A sword combo with input buffering (scanned with `Physics.OverlapSphere`). Every swing's recovery ends in a cancel window, so the chain flows without stalling. Holding the attack button charges a heavy strike, `Q` throws a spin attack (1.5× damage when linked out of a combo), and attacking right after a dash gives a dedicated dash strike. Dash has i-frames; hits produce hitstop and camera shake.
- **Enemies:** A NavMesh swarm that surrounds the player — tank, charger, ranged and burrowing types; shade spirits that latch on and slow you, hounds that wait for an opening, knights who block from the front, a troll with a wide club, a cultist who revives his escort, and a flesh pile that leaves a damaging pool when it dies. Health bars stay hidden until the first hit.
- **Night/Morning:** Waves of enemies, demons evaporating at daybreak, the transition into morning light and the campfire.
- **Camp:** Hades-style portrait dialogue, then a choice of three boon cards (Flame Oil, Swift Dash Charm, Healing Bandage).
- **HUD:** A health bar that shows damage as a fading trail, and a Demon Ash counter.
- **Godot's Mine:** The game opens at the forge inside the old mine, next to Godot's water-wheel cottage, with a grove, an armoury, a waterfall and the Hill of Swords around it. *Dragonslayer* (base damage) is bought at Godot's forge, *Permanent Health Capacity* and *Death Defiance* from Puck, all with Demon Ash; the run starts down the mountain path to the south.
- **Death loop:** On death, the dark-spirits text, then waking at Godot's forge. Temporary oils reset; ash and upgrades persist (`save.json`).

## Controls

| Key | Action |
|---|---|
| `W A S D` / Arrow keys | Move |
| Mouse | Orbits the camera around the character; the cursor is locked and you aim where the camera looks |
| Left click | Sword attack (combo) |
| Left click (hold) | Charged heavy strike |
| `Q` | Spin attack; links out of a combo for extra damage |
| Right click (hold) | Repeater crossbow on the left arm (magazine + reload) |
| Middle click | Arm cannon: one shot per camp, breaks the crossbow, knocks you back |
| `Space` | Dash; a left click straight after it becomes a dash strike |
| `E` | Interact (campfire, Godot, Puck, mountain path) |
| `Esc` / `E` | Close the upgrade panel |
| `E` / `Space` / Left click | Advance dialogue |

## Technical Stack

- **Engine:** Unity `6000.6.0f1`, Universal Render Pipeline `17.6.0`
- **Input:** Input System `1.20.0` (actions are built in code; there is no `.inputactions` asset)
- **Camera:** Cinemachine `6.6.0` — a Witcher 3-style orbital third-person camera; the orbit centres above the character's head, and the character stays in the lower half of the frame
- **Character:** `CharacterController` (no Rigidbody)
- **AI:** NavMesh
- **UI:** uGUI + TextMeshPro
- **Data:** Nights, boons, upgrades and dialogue are `ScriptableObject` assets; permanent progress is saved as JSON

> The in-game text (dialogue, HUD labels) is in Turkish. The code, comments and documentation are in English.

## Project Structure

```
Assets/_Project/
├── Art/           Portraits/ (dialogue portraits), UI/ (interface art)
├── Data/          ScriptableObject data (Boons, Dialogue, Nights, Upgrades)
├── Materials/
├── Prefabs/       Player, enemies, world objects, UI
├── Scenes/        Hub_GodotForge.unity, Greybox_Asama1.unity
└── Scripts/
    ├── Core/      GameEvents (the cross-system event channel), FlatMath, Easing
    ├── Player/    Movement, dash, input, camera, aim, sword, crossbow, death
    ├── Combat/    IDamageable, HealthComponent, hitboxes, Projectile
    ├── Enemies/   Enemy AI and types
    ├── Loop/      Night/morning cycle, waves, campfire
    ├── Dialogue/  Dialogue data and manager
    ├── Boons/     Boon data and effects
    ├── Meta/      Permanent progress, saving, upgrades, scene transitions
    ├── Interaction/ Interactable objects and the player side
    ├── Hub/       Upgrade stations, hub exit
    ├── UI/        Health bar, boon cards, upgrade panel, screen fades
    └── DevTools/  Test helpers
```

Code conventions: **[docs/CodeStyles.md](docs/CodeStyles.md)**

## Running It

1. Clone the repository and open it from Unity Hub with **Unity 6000.6.0f1**.
2. Open `Assets/_Project/Scenes/Hub_GodotForge.unity` (the run scene, `Greybox_Asama1.unity`, can also be opened directly).
3. Press Play.

The repository uses **Git LFS** for binary assets (`.png`, `.ttf`, `.fbx` and similar). Run `git lfs install` before cloning, or those files arrive as text pointers.

## Licence

MIT — see [LICENSE](LICENSE). Berserk is created by Kentaro Miura; this is a non-commercial fan project for learning purposes and is not affiliated with the rights holders.
