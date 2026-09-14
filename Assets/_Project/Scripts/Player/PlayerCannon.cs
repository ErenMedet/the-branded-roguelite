using Branded.Core;
using Unity.Cinemachine;
using UnityEngine;

namespace Branded.Player
{
    // Hidden cannon in the prosthetic left arm, for when the swarm closes in: one shot per camp repair.
    // The blast wrecks the crossbow and throws the player back.
    public class PlayerCannon : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] PlayerInputReader _inputComponent;
        [SerializeField] PlayerAim _aimComponent;
        [SerializeField] PlayerMotor _motorComponent;
        [SerializeField] PlayerCrossbow _crossbowComponent;
        [SerializeField] CinemachineImpulseSource _impulseComponent;
        [SerializeField] Transform _muzzleComponent;
        [SerializeField] CannonShell _shellPrefabComponent;

        [Header("Recoil")]
        [SerializeField] float _recoilDistance = 3f;
        [SerializeField] float _recoilDuration = 0.2f;
        [SerializeField] float _shakeForce = 1f;

        public bool IsCharged { get; private set; } = true;

        void Awake()
        {
            if (!_inputComponent) _inputComponent = GetComponent<PlayerInputReader>();
            if (!_aimComponent) _aimComponent = GetComponent<PlayerAim>();
            if (!_motorComponent) _motorComponent = GetComponent<PlayerMotor>();
            if (!_crossbowComponent) _crossbowComponent = GetComponent<PlayerCrossbow>();
            if (!_impulseComponent) _impulseComponent = GetComponent<CinemachineImpulseSource>();
        }

        void OnEnable()
        {
            _inputComponent.CannonPressed += OnCannonPressed;
            GameEvents.ArmRepaired += OnArmRepaired;
        }

        void OnDisable()
        {
            _inputComponent.CannonPressed -= OnCannonPressed;
            GameEvents.ArmRepaired -= OnArmRepaired;
        }

        void OnCannonPressed()
        {
            if (!IsCharged || _motorComponent.IsDashing) return;
            IsCharged = false;

            _aimComponent.SnapToAim();
            Vector3 direction = _aimComponent.DirectionFrom(_muzzleComponent.position);
            var shell = Instantiate(_shellPrefabComponent, _muzzleComponent.position, Quaternion.LookRotation(direction));
            shell.Launch(transform);

            _motorComponent.Push(-direction, _recoilDistance, _recoilDuration);
            if (_impulseComponent) _impulseComponent.GenerateImpulseWithVelocity(-direction * _shakeForce);
            if (_crossbowComponent) _crossbowComponent.Break();
        }

        void OnArmRepaired() => IsCharged = true;
    }
}
