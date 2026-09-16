using Branded.Core;
using Branded.Meta;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Branded.Player
{
    // Reads raw player input and exposes it as values/events. No gameplay logic here.
    // The camp, the upgrade panel and leaving the hub lock gameplay input through GameEvents.
    public class PlayerInputReader : MonoBehaviour
    {
        public Vector2 Move { get; private set; }
        public Vector2 PointerScreenPosition { get; private set; }
        public bool IsLocked { get; private set; }
        public bool IsFireHeld { get; private set; }
        // Held long enough, an attack press charges the sword instead of swinging.
        public bool IsAttackHeld { get; private set; }

        public event UnityAction DashPressed;
        public event UnityAction AttackPressed;
        public event UnityAction SpinPressed;
        public event UnityAction CannonPressed;
        public event UnityAction InteractPressed;

        InputAction _move;
        InputAction _point;
        InputAction _dash;
        InputAction _attack;
        InputAction _spin;
        InputAction _fire;
        InputAction _cannon;
        InputAction _interact;
        bool _gameplayRegistered;

        void Awake()
        {
            _move = new InputAction("Move", InputActionType.Value);
            _move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            _move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");

            _point = new InputAction("Point", InputActionType.PassThrough, "<Pointer>/position");

            _dash = new InputAction("Dash", InputActionType.Button, "<Keyboard>/space");
            _attack = new InputAction("Attack", InputActionType.Button, "<Mouse>/leftButton");
            _spin = new InputAction("Spin", InputActionType.Button, "<Keyboard>/q");
            _fire = new InputAction("Fire", InputActionType.Button, "<Mouse>/rightButton");
            _cannon = new InputAction("Cannon", InputActionType.Button, "<Mouse>/middleButton");
            _interact = new InputAction("Interact", InputActionType.Button, "<Keyboard>/e");
        }

        void OnEnable()
        {
            _move.Enable();
            _point.Enable();
            _dash.Enable();
            _attack.Enable();
            _spin.Enable();
            _fire.Enable();
            _cannon.Enable();
            _interact.Enable();

            _point.performed += OnPoint;
            RegisterGameplay();

            GameEvents.CampStarted += OnCampStarted;
            GameEvents.CampEnded += OnCampEnded;
            GameEvents.UpgradePanelOpened += OnUpgradePanelOpened;
            GameEvents.UpgradePanelClosed += OnUpgradePanelClosed;
            GameEvents.HubExited += OnHubExited;
            GameEvents.DialogueStarted += OnDialogueStarted;
            GameEvents.DialogueEnded += OnDialogueEnded;
        }

        // Death disables the reader, so nothing may stay held.
        void OnDisable()
        {
            GameEvents.CampStarted -= OnCampStarted;
            GameEvents.CampEnded -= OnCampEnded;
            GameEvents.UpgradePanelOpened -= OnUpgradePanelOpened;
            GameEvents.UpgradePanelClosed -= OnUpgradePanelClosed;
            GameEvents.HubExited -= OnHubExited;
            GameEvents.DialogueStarted -= OnDialogueStarted;
            GameEvents.DialogueEnded -= OnDialogueEnded;

            _point.performed -= OnPoint;
            UnregisterGameplay();

            _move.Disable();
            _point.Disable();
            _dash.Disable();
            _attack.Disable();
            _spin.Disable();
            _fire.Disable();
            _cannon.Disable();
            _interact.Disable();
        }

        void OnDestroy()
        {
            _move.Dispose();
            _point.Dispose();
            _dash.Dispose();
            _attack.Dispose();
            _spin.Dispose();
            _fire.Dispose();
            _cannon.Dispose();
            _interact.Dispose();
        }

        void OnCampStarted() => Lock();
        void OnCampEnded() => Unlock();
        void OnUpgradePanelOpened(string stationName, UpgradeData[] upgrades) => Lock();
        void OnUpgradePanelClosed() => Unlock();
        void OnHubExited(float fadeDuration) => Lock();
        void OnDialogueStarted() => Lock();
        void OnDialogueEnded() => Unlock();

        void Lock()
        {
            IsLocked = true;
            UnregisterGameplay();
        }

        void Unlock()
        {
            IsLocked = false;
            RegisterGameplay();
        }

        void RegisterGameplay()
        {
            if (IsLocked || _gameplayRegistered) return;
            _gameplayRegistered = true;

            _move.performed += OnMove;
            _move.canceled += OnMove;
            _dash.performed += OnDash;
            _attack.performed += OnAttack;
            _attack.canceled += OnAttackReleased;
            _spin.performed += OnSpin;
            _fire.performed += OnFire;
            _fire.canceled += OnFire;
            _cannon.performed += OnCannon;
            _interact.performed += OnInteract;

            // A key held down while input was locked sends no new event, so pick it up once here.
            Move = Vector2.ClampMagnitude(_move.ReadValue<Vector2>(), 1f);
            IsFireHeld = _fire.IsPressed();
            IsAttackHeld = _attack.IsPressed();
        }

        void UnregisterGameplay()
        {
            Move = Vector2.zero;
            IsFireHeld = false;
            IsAttackHeld = false;
            if (!_gameplayRegistered) return;
            _gameplayRegistered = false;

            _move.performed -= OnMove;
            _move.canceled -= OnMove;
            _dash.performed -= OnDash;
            _attack.performed -= OnAttack;
            _attack.canceled -= OnAttackReleased;
            _spin.performed -= OnSpin;
            _fire.performed -= OnFire;
            _fire.canceled -= OnFire;
            _cannon.performed -= OnCannon;
            _interact.performed -= OnInteract;
        }

        void OnMove(InputAction.CallbackContext context) => Move = Vector2.ClampMagnitude(context.ReadValue<Vector2>(), 1f);
        void OnPoint(InputAction.CallbackContext context) => PointerScreenPosition = context.ReadValue<Vector2>();
        void OnDash(InputAction.CallbackContext _) => DashPressed?.Invoke();
        void OnAttack(InputAction.CallbackContext _)
        {
            IsAttackHeld = true;
            AttackPressed?.Invoke();
        }

        void OnAttackReleased(InputAction.CallbackContext _) => IsAttackHeld = false;
        void OnSpin(InputAction.CallbackContext _) => SpinPressed?.Invoke();
        void OnFire(InputAction.CallbackContext context) => IsFireHeld = context.performed;
        void OnCannon(InputAction.CallbackContext _) => CannonPressed?.Invoke();
        void OnInteract(InputAction.CallbackContext _) => InteractPressed?.Invoke();
    }
}
