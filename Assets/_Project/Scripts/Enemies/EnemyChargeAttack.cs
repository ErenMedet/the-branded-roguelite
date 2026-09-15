using System.Collections;
using Branded.Combat;
using Branded.Core;
using Branded.Player;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Branded.Enemies
{
    // Telegraphed straight-line charge. It aims while a stripe on the ground shows the path, then commits
    // and rushes along it. Can't be staggered mid-rush, but stands exposed for a moment afterwards.
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyChargeAttack : EnemyAttack
    {
        [SerializeField, FormerlySerializedAs("telegraph")] Transform _telegraphComponent; // ground stripe pivot; its Z scale becomes the charge length
        [SerializeField, FormerlySerializedAs("minRange")] float _minRange = 2.5f;
        [SerializeField, FormerlySerializedAs("maxRange")] float _maxRange = 8f;
        [SerializeField, FormerlySerializedAs("windup")] float _windup = 0.7f;
        [SerializeField, FormerlySerializedAs("speed")] float _speed = 16f;
        [SerializeField, FormerlySerializedAs("overshoot")] float _overshoot = 2.5f;
        [SerializeField, FormerlySerializedAs("damage")] float _damage = 20f;
        [SerializeField, FormerlySerializedAs("hitRadius")] float _hitRadius = 1f;
        [SerializeField, FormerlySerializedAs("recovery")] float _recovery = 0.9f;
        [SerializeField, FormerlySerializedAs("cooldown")] float _cooldown = 2.5f;

        [Header("Opening")]
        [Tooltip("Only charge into an opening: the player mid-swing or facing away (hounds).")]
        [SerializeField] bool _onlyOnOpening;
        [Tooltip("How far behind the player's aim this enemy must be to count as an opening.")]
        [SerializeField] float _openingAngle = 100f;

        public override bool CanBeInterrupted => !_rushing;

        NavMeshAgent _navMeshAgentComponent;
        PlayerCombat _targetCombatComponent;
        PlayerAim _targetAimComponent;
        bool _rushing;

        protected override void Awake()
        {
            base.Awake();
            _navMeshAgentComponent = GetComponent<NavMeshAgent>();
            if (_telegraphComponent) _telegraphComponent.gameObject.SetActive(false);
            CooldownTimer = _cooldown * Random.Range(0.3f, 1f);
        }

        void Update()
        {
            TickCooldown();
            if (!IsReady) return;
            float distance = _chaserComponent.DistanceToTarget;
            if (distance < _minRange || distance > _maxRange || !_chaserComponent.HasLineOfSight()) return;
            if (_onlyOnOpening && !TargetIsOpen()) return;
            StartCoroutine(Charge());
        }

        bool TargetIsOpen()
        {
            Transform target = _chaserComponent.TargetComponent;
            if (!_targetCombatComponent) _targetCombatComponent = target.GetComponent<PlayerCombat>();
            if (!_targetAimComponent) _targetAimComponent = target.GetComponent<PlayerAim>();

            if (_targetCombatComponent && _targetCombatComponent.Phase != ESwingPhase.None) return true;
            if (!_targetAimComponent) return true;

            Vector3 fromTarget = FlatMath.Flat(transform.position - target.position);
            return Vector3.Angle(_targetAimComponent.AimDirection, fromTarget) > _openingAngle;
        }

        IEnumerator Charge()
        {
            StartAttack();
            float length = 0f;

            if (_telegraphComponent) _telegraphComponent.gameObject.SetActive(true);
            for (float t = 0f; t < _windup; t += Time.deltaTime)
            {
                _chaserComponent.FaceTarget();
                length = Mathf.Min(_chaserComponent.DistanceToTarget, _maxRange) + _overshoot;
                if (_telegraphComponent) _telegraphComponent.localScale = new Vector3(1f, 1f, length);
                yield return null;
            }
            if (_telegraphComponent) _telegraphComponent.gameObject.SetActive(false);

            _rushing = true;
            Vector3 direction = transform.forward;
            bool landed = false;
            for (float travelled = 0f; travelled < length;)
            {
                float step = _speed * Time.deltaTime;
                Vector3 before = transform.position;
                if (_navMeshAgentComponent.enabled && _navMeshAgentComponent.isOnNavMesh) _navMeshAgentComponent.Move(direction * step);
                if (!landed) landed = TryHit(direction);
                // Stopped short by a wall.
                if (step > 0f && (transform.position - before).sqrMagnitude < step * step * 0.1f) break;
                travelled += step;
                yield return null;
            }
            _rushing = false;

            yield return new WaitForSeconds(_recovery);
            FinishAttack(_cooldown);
        }

        bool TryHit(Vector3 direction)
        {
            Vector3 offset = FlatMath.Flat(_chaserComponent.TargetComponent.position - transform.position);
            if (offset.sqrMagnitude > _hitRadius * _hitRadius) return false;
            var damageable = _chaserComponent.TargetComponent.GetComponentInParent<IDamageable>();
            damageable?.TakeDamage(_damage, direction);
            return true;
        }

        public override void Interrupt()
        {
            if (!IsAttacking || _rushing) return;
            Stop();
        }

        void OnDisable()
        {
            _rushing = false;
            if (IsAttacking) Stop();
        }

        void Stop()
        {
            StopAllCoroutines();
            if (_telegraphComponent) _telegraphComponent.gameObject.SetActive(false);
            FinishAttack(_cooldown);
        }
    }
}
