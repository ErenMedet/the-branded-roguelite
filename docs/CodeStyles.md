# Code Styles and Quality

Applies to every C# file under `Assets/_Project/Scripts/`. Ignore `Assets/TutorialInfo`, `Assets/TextMesh Pro` and package code for any guidance on formatting, quality and practices.

## Structure
- Every file gets a namespace matching its directory relative to `Scripts`, prefixed with `Branded` (`Scripts/Player` → `namespace Branded.Player`).
- One component per file; the file name matches the class.
- Logic shared by several components that needs no GameObject of its own goes in a plain C# class the components own (`RendererFlash`, `BarTrail`), not a copy in each.
- Unless specified otherwise, a newly requested component starts as an empty class: no methods, properties or fields.
- Prefer updating an existing component over creating a new file.

## Naming
- Private fields, serialized ones included, are prefixed with `_`: `[SerializeField] float _moveSpeed`.
- Fields or properties storing a component are suffixed with `Component` (`Components` for arrays and lists): `_animatorComponent`, `_rendererComponents`, `TargetComponent`.
- Enums are prefixed with `E`: `ECyclePhase`.
- Constants and `static readonly` values use PascalCase.
- Events are not prefixed with `On` (`Died`, not `OnDeath`); their handlers are named `On<EventName>` (`OnDied`).

## Properties and serialization
- Prefer public properties with private setters over public fields.
- ScriptableObject and `[Serializable]` data expose values as `[field: SerializeField] public T Name { get; private set; }`.
  Exception: save-file DTOs (`PlayerData`) keep plain public fields, because JsonUtility writes field names to disk.
- When renaming a serialized field, add `[FormerlySerializedAs("oldName")]` so scenes, prefabs and assets keep their values.
- A serialized field added to a type that prefabs already use is deserialized as `default(T)`, not as its C# initializer: the value in the prefab wins, and a missing key reads as 0. After adding one, set it on every prefab that uses the type before testing, or a new tuning knob silently arrives at zero.

## Events and input
- Prefer `UnityAction` over `Action`, including for event types and callback parameters; always use the `event` keyword.
- Components on different GameObjects communicate through `Branded.Core.GameEvents` instead of `FindAnyObjectByType`, `GetComponent` on another object, or enabling/disabling each other's components.
  Exception: finding the player once with `GameObject.FindWithTag("Player")` in `Awake` or `Start` and caching its components is allowed, since enemies and the HUD need the player before any event could introduce it.
- Every `GameEvents` event gets a static `Raise<EventName>()` invoker method.
- Subscribe in `OnEnable` and unsubscribe in `OnDisable`. UI managers that hide their own GameObject subscribe in `Awake` and unsubscribe in `OnDestroy`.
- Prefer Input System events (`performed` / `canceled`) over polling `ReadValue` every frame.
- A UI opened by the interact key waits one frame before enabling its own key action (`UpgradePanelUI`, `DialogueManager`), so the press that opened it doesn't also advance it.
- Domain reload is off: reset static events and caches in a `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` method.

## Logic
- Prefer guard clauses over nested conditionals.
- Cache components once in `Awake`; never call `GetComponent` in `Update`.
- Prefer hash IDs for Animator parameters and shader properties (`Animator.StringToHash`, `Shader.PropertyToID`), stored in `static readonly` fields.
- Logic that must see this frame's movement of another object (camera-facing bars, followers) runs in `LateUpdate`.
- Clear a stored `Coroutine` reference when that coroutine finishes.
- Flat (ground-plane) directions, distances and arc checks go through `Branded.Core.FlatMath` instead of zeroing `y` by hand.
- Enemy attacks derive from `EnemyAttack` and use its `IsReady`, `StartAttack` and `FinishAttack` helpers, so the chaser's `Halted` flag and cooldowns stay consistent.
- Anything the player watches move (swing angles, committed turns, time scale after a hitstop) interpolates through `Branded.Core.Easing`, never a raw `Mathf.Lerp` on normalized time: a constant rate reads as robotic. Windups ease out into a held pose, strikes use `OutExpo` and carry past their end angle, recoveries settle back.
- A burst that must cover a fixed distance (dash, swing lunge, knockback) drives its speed with `Easing.DecaySpeed` instead of `distance / duration`, so it leaves fast and bleeds off while still landing exactly where it was tuned to.
- An attack's recovery ends in a cancel window (`SwingData.RecoveryCancelFraction`), so a buffered press starts the next swing instead of waiting the animation out. A chain with no cancel window eats inputs and feels stiff.
- An aim point for a flat-flying shot is taken where the camera ray crosses a plane at the muzzle's own height (`PlayerAim._aimHeight`), never by raycasting into scene geometry: with the camera angled down, geometry hands back a point on the ground that the shot passes above and carries well past, so the reticle promises a range nothing keeps. Once the aim point is taken that way it lands on screen centre by construction, and the reticle stays there: a reticle pinned to the predicted impact is equally true but jumps whenever the shot crosses an obstacle, which reads as a twitch. What is in the way is reported by tinting the reticle (`EAimState`), never by moving it.
- A rotation wider than half a turn is applied as a signed delta (`from * Quaternion.Euler(sweep * eased, ...)`), never slerped to an end pose: `Quaternion.Slerp` and `RotateTowards` always take the short way round, so a 200-degree swing silently reverses and travels the other 160.
- A speed ceiling on an eased motion shortens how far it travels, never how fast it may go: clamping with `RotateTowards` or `MoveTowards` against a flat ceiling drags the whole motion at that one rate and throws the easing away. Compare the two curves at their opening speed and scale the target down until it fits.
- Comments explain why, not what. Keep them short and in English.
