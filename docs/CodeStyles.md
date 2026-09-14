# Code Styles and Quality

Applies to every C# file under `Assets/_Project/Scripts/`. Ignore `Assets/TutorialInfo`, `Assets/TextMesh Pro` and package code for any guidance on formatting, quality and practices.

## Structure
- Every file gets a namespace matching its directory relative to `Scripts`, prefixed with `Branded` (`Scripts/Player` → `namespace Branded.Player`).
- One component per file; the file name matches the class.
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

## Events and input
- Prefer `UnityAction` over `Action`, including for event types and callback parameters; always use the `event` keyword.
- Components on different GameObjects communicate through `Branded.Core.GameEvents` instead of `FindAnyObjectByType`, `GetComponent` on another object, or enabling/disabling each other's components.
- Every `GameEvents` event gets a static `Raise<EventName>()` invoker method.
- Subscribe in `OnEnable` and unsubscribe in `OnDisable`. UI managers that hide their own GameObject subscribe in `Awake` and unsubscribe in `OnDestroy`.
- Prefer Input System events (`performed` / `canceled`) over polling `ReadValue` every frame.
- Domain reload is off: reset static events and caches in a `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` method.

## Logic
- Prefer guard clauses over nested conditionals.
- Cache components once in `Awake`; never call `GetComponent` in `Update`.
- Prefer hash IDs for Animator parameters and shader properties (`Animator.StringToHash`, `Shader.PropertyToID`), stored in `static readonly` fields.
- Logic that must see this frame's movement of another object (camera-facing bars, followers) runs in `LateUpdate`.
- Clear a stored `Coroutine` reference when that coroutine finishes.
- Comments explain why, not what. Keep them short and in English.
