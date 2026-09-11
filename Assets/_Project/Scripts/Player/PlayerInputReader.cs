using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Branded.Player
{
    // Reads raw player input and exposes it as values/events. No gameplay logic here.
    public class PlayerInputReader : MonoBehaviour
    {
        public Vector2 Move { get; private set; }
        public Vector2 PointerScreenPosition { get; private set; }

        public event Action DashPressed;
        public event Action AttackPressed;
        public event Action InteractPressed;

        InputAction _move;
        InputAction _point;
        InputAction _dash;
        InputAction _attack;
        InputAction _interact;

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
            _interact = new InputAction("Interact", InputActionType.Button, "<Keyboard>/e");

            _dash.performed += OnDash;
            _attack.performed += OnAttack;
            _interact.performed += OnInteract;
        }

        void OnEnable()
        {
            _move.Enable();
            _point.Enable();
            _dash.Enable();
            _attack.Enable();
            _interact.Enable();
        }

        // Disabling the reader locks player control (e.g. during dialogue), so nothing may stay held.
        void OnDisable()
        {
            Move = Vector2.zero;
            _move.Disable();
            _point.Disable();
            _dash.Disable();
            _attack.Disable();
            _interact.Disable();
        }

        void OnDestroy()
        {
            _dash.performed -= OnDash;
            _attack.performed -= OnAttack;
            _interact.performed -= OnInteract;
            _move.Dispose();
            _point.Dispose();
            _dash.Dispose();
            _attack.Dispose();
            _interact.Dispose();
        }

        void Update()
        {
            Move = Vector2.ClampMagnitude(_move.ReadValue<Vector2>(), 1f);
            PointerScreenPosition = _point.ReadValue<Vector2>();
        }

        void OnDash(InputAction.CallbackContext _) => DashPressed?.Invoke();
        void OnAttack(InputAction.CallbackContext _) => AttackPressed?.Invoke();
        void OnInteract(InputAction.CallbackContext _) => InteractPressed?.Invoke();
    }
}
