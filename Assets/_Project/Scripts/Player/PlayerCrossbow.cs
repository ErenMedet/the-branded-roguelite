using Branded.Combat;
using Branded.Core;
using UnityEngine;
using UnityEngine.Events;

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
        [SerializeField] PlayerGrip _gripComponent;
        [SerializeField] Transform _muzzleComponent;
        [SerializeField] Projectile _boltPrefabComponent;

        [Header("Firing")]
        [SerializeField] float _damage = 8f;
        [SerializeField] float _fireInterval = 0.15f;
        [SerializeField] int _magazineSize = 6;
        [SerializeField] float _reloadTime = 1.5f;

        public event UnityAction<int, int> BoltsChanged; // (left, magazine size)
        public event UnityAction ReloadStarted;
        public event UnityAction Broke;

        public bool IsBroken { get; private set; }
        public int BoltsLeft { get; private set; }
        public int MagazineSize => _magazineSize;
        public bool IsReloading => _reloadTimer > 0f;
        public float ReloadProgress => IsReloading ? 1f - _reloadTimer / _reloadTime : 1f;

        float _fireTimer;
        float _reloadTimer;

        void Awake()
        {
            if (!_inputComponent) _inputComponent = GetComponent<PlayerInputReader>();
            if (!_aimComponent) _aimComponent = GetComponent<PlayerAim>();
            if (!_motorComponent) _motorComponent = GetComponent<PlayerMotor>();
            if (!_combatComponent) _combatComponent = GetComponent<PlayerCombat>();
            if (!_gripComponent) _gripComponent = GetComponent<PlayerGrip>();
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
                if (_reloadTimer <= 0f) Refill();
                return;
            }

            if (IsBroken || !_inputComponent.IsFireHeld || _fireTimer > 0f) return;
            // The left hand is still on the sword until the draw finishes, so there is nothing to fire with.
            if (_gripComponent && !_gripComponent.IsCrossbowReady) return;
            if (_motorComponent.IsDashing || _combatComponent.Phase != ESwingPhase.None) return;
            Fire();
        }

        // What the next bolt would meet, for the reticle to report. The prefab's own radius, mask and
        // range are used, so what the crosshair promises cannot drift from what is actually fired.
        public EAimState PredictAim()
        {
            Vector3 origin = _muzzleComponent.position;
            Vector3 direction = _aimComponent.DirectionFrom(origin);
            float range = _boltPrefabComponent.Range;

            RaycastHit[] hits = Physics.SphereCastAll(origin, _boltPrefabComponent.Radius, direction, range,
                _boltPrefabComponent.HitLayers, QueryTriggerInteraction.Ignore);

            // The shooter's own colliders are skipped, as the bolt skips them in flight, and so is a cast
            // that starts already overlapping, which reports a useless hit at zero distance.
            float nearest = range;
            Collider struck = null;
            foreach (var hit in hits)
            {
                if (hit.distance <= 0f || hit.distance >= nearest) continue;
                if (hit.collider.transform.IsChildOf(transform)) continue;
                nearest = hit.distance;
                struck = hit.collider;
            }

            if (!struck) return EAimState.Clear;
            return struck.GetComponentInParent<IDamageable>() != null ? EAimState.Target : EAimState.Blocked;
        }

        void Fire()
        {
            _fireTimer = _fireInterval;
            _aimComponent.SnapToAim();
            Vector3 direction = _aimComponent.DirectionFrom(_muzzleComponent.position);
            var bolt = Instantiate(_boltPrefabComponent, _muzzleComponent.position, Quaternion.LookRotation(direction));
            bolt.Launch(_damage, transform);

            BoltsLeft--;
            BoltsChanged?.Invoke(BoltsLeft, _magazineSize);
            if (BoltsLeft > 0) return;
            _reloadTimer = _reloadTime;
            ReloadStarted?.Invoke();
        }

        void Refill()
        {
            BoltsLeft = _magazineSize;
            BoltsChanged?.Invoke(BoltsLeft, _magazineSize);
        }

        public void Break()
        {
            IsBroken = true;
            // A reload finishing while broken would overwrite the HUD's broken state; the repair refills anyway.
            _reloadTimer = 0f;
            // PlayerGrip owns the visual: a broken arm fails WantsCrossbow, so the stow hides it.
            Broke?.Invoke();
        }

        void OnArmRepaired()
        {
            IsBroken = false;
            _reloadTimer = 0f;
            Refill();
        }
    }
}
