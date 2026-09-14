using Branded.Combat;
using Branded.Core;
using UnityEngine;

namespace Branded.Player
{
    // Repeating crossbow on the prosthetic left arm: fires while right mouse is held, reloads when the magazine is empty.
    // The arm cannon breaks it; the camp's repair bench (ArmRepaired) fixes it.
    public class PlayerCrossbow : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] PlayerInputReader _inputComponent;
        [SerializeField] PlayerAim _aimComponent;
        [SerializeField] PlayerMotor _motorComponent;
        [SerializeField] PlayerCombat _combatComponent;
        [SerializeField] Transform _muzzleComponent;
        [SerializeField] GameObject _crossbowVisual;
        [SerializeField] Projectile _boltPrefabComponent;

        [Header("Firing")]
        [SerializeField] float _damage = 8f;
        [SerializeField] float _fireInterval = 0.15f;
        [SerializeField] int _magazineSize = 6;
        [SerializeField] float _reloadTime = 1.5f;

        public bool IsBroken { get; private set; }
        public int BoltsLeft { get; private set; }
        public bool IsReloading => _reloadTimer > 0f;

        float _fireTimer;
        float _reloadTimer;

        void Awake()
        {
            if (!_inputComponent) _inputComponent = GetComponent<PlayerInputReader>();
            if (!_aimComponent) _aimComponent = GetComponent<PlayerAim>();
            if (!_motorComponent) _motorComponent = GetComponent<PlayerMotor>();
            if (!_combatComponent) _combatComponent = GetComponent<PlayerCombat>();
            BoltsLeft = _magazineSize;
        }

        void OnEnable() => GameEvents.ArmRepaired += OnArmRepaired;
        void OnDisable() => GameEvents.ArmRepaired -= OnArmRepaired;

        void Update()
        {
            _fireTimer -= Time.deltaTime;
            if (IsReloading)
            {
                _reloadTimer -= Time.deltaTime;
                if (_reloadTimer <= 0f) BoltsLeft = _magazineSize;
                return;
            }

            if (IsBroken || !_inputComponent.IsFireHeld || _fireTimer > 0f) return;
            if (_motorComponent.IsDashing || _combatComponent.Phase != PlayerCombat.ESwingPhase.None) return;
            Fire();
        }

        void Fire()
        {
            _fireTimer = _fireInterval;
            _aimComponent.SnapToAim();
            Vector3 direction = _aimComponent.DirectionFrom(_muzzleComponent.position);
            var bolt = Instantiate(_boltPrefabComponent, _muzzleComponent.position, Quaternion.LookRotation(direction));
            bolt.Launch(_damage, transform);

            BoltsLeft--;
            if (BoltsLeft > 0) return;
            _reloadTimer = _reloadTime;
        }

        public void Break()
        {
            IsBroken = true;
            if (_crossbowVisual) _crossbowVisual.SetActive(false);
        }

        void OnArmRepaired()
        {
            IsBroken = false;
            BoltsLeft = _magazineSize;
            _reloadTimer = 0f;
            if (_crossbowVisual) _crossbowVisual.SetActive(true);
        }
    }
}
