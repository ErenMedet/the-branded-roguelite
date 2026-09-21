# The Branded

Third-person hack & slash roguelite: Hades' run loop in Berserk's post-Eclipse mood. Unity 6000.6.0f1, URP, C#.
`docs/GDD.md` is the source of truth for gameplay. Don't add design ideas nobody asked for; fill technical gaps only.

## Project context
- Code lives in `Assets/_Project/Scripts/<Folder>`, data (ScriptableObjects) in `Assets/_Project/Data`, prefabs in `Assets/_Project/Prefabs`.
- Scenes: `Hub_GodotForge` (first build scene, the hub: Godot's old mine with the forge, his decorative cottage with a water wheel, a grove, armory shed, waterfall with a walkable cave behind it, river bank and Hill of Swords; talkable NPCs Godot, Erica, Rickert and Casca) and `Greybox_Asama1` (the run: night/morning loop).
- Stack: Input System (actions built in code), Cinemachine 6, NavMesh, uGUI + TextMeshPro, CharacterController for the player (no Rigidbody).
- Components on different GameObjects talk through `Branded.Core.GameEvents` (observer pattern).
- Permanent progress: `Branded.Meta.Progress` writes `save.json` to `Application.persistentDataPath`.
- Enter Play Mode runs with domain reload OFF, so statics outlive a play session.
- Unity is driven through the UnityMCP server; its tools are pre-approved in `.claude/settings.json`.

## Code styles
@docs/CodeStyles.md

## Workflow
- **Plan → Review → Refine → Execute.** For a change touching more than one file, first give a short plan and wait for approval:
  - **Context**: my understanding of the request.
  - **Changes**: every file created or modified, including scene and prefab edits.
  - **Verification**: how I will confirm it works (compile, console, Play Mode test).
- Be explicit about create vs update. Prefer updating an existing component over creating a new file.
- Don't add components to scenes or prefabs, or edit scene objects, unless the plan says so.
- Before using an unfamiliar Unity API, study it: parent class, namespace, the methods it expects, then the Scripting API docs. For APIs newer than my knowledge, read the docs page first.
- Use official Unity terminology in plans and comments (serialized field, Animator component, trigger collider...).
- Design events in three steps: create the event (name, where it lives) → decide where it is raised → decide who handles it and what they do.
- Debugging: state what works, what doesn't and the steps to reproduce. Verify in Play Mode and check the console before calling something done.
- Review the code after each finished feature. A fix that could repeat becomes a rule in `docs/CodeStyles.md`.
- Start a new session for unrelated tasks; reuse the session when building on the same work; `/compact` long sessions.
