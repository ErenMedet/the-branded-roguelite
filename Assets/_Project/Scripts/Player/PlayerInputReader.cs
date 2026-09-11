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

        InputAction _move;
        InputAction _point;
        InputAction _dash;
        InputAction _attack;

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

            _dash.performed += OnDash;
            _attack.performed += OnAttack;
        }

        void OnEnable()
        {
            _move.Enable();
            _point.Enable();
            _dash.Enable();
            _attack.Enable();
        }

        void OnDisable()
        {
            _move.Disable();
            _point.Disable();
            _dash.Disable();
            _attack.Disable();
        }

        void OnDestroy()
        {
            _dash.performed -= OnDash;
            _attack.performed -= OnAttack;
            _move.Dispose();
            _point.Dispose();
            _dash.Dispose();
            _attack.Dispose();
        }

        void Update()
        {
            Move = Vector2.ClampMagnitude(_move.ReadValue<Vector2>(), 1f);
            PointerScreenPosition = _point.ReadValue<Vector2>();
        }

        void OnDash(InputAction.CallbackContext _) => DashPressed?.Invoke();
        void OnAttack(InputAction.CallbackContext _) => AttackPressed?.Invoke();
    }
}
