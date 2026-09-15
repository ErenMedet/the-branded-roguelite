using System.Collections;
using Branded.Combat;
using Branded.Core;
using Branded.Player;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Branded.Enemies
{
    // Sinks into the ground, travels unseen, then bursts out behind the player. A marker shows the exit
    // point for a moment first so the ambush stays readable and can be dodged with a dash.
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyBurrowAmbush : EnemyAttack, IInvulnerabilitySource
    {
        static readonly float[] ExitAngles = { 0f, 40f, -40f, 80f, -80f };

        [SerializeField, FormerlySerializedAs("visual")] Transform _visualComponent; // sunk below the floor while burrowed
        [SerializeField, FormerlySerializedAs("exitMarker")] GameObject _exitMarker;  // ground disc at the exit point, sized to the hit radius
        [SerializeField, FormerlySerializedAs("healthBar")] GameObject _healthBar;
        [SerializeField, FormerlySerializedAs("sinkDepth")] float _sinkDepth = 2.3f;
        [SerializeField, FormerlySerializedAs("sinkDuration")] float _sinkDuration = 0.4f;
        [SerializeField, FormerlySerializedAs("undergroundTime")] float _undergroundTime = 1f;
        [SerializeField, FormerlySerializedAs("markerTime")] float _markerTime = 0.6f;
        [SerializeField, FormerlySerializedAs("riseDuration")] float _riseDuration = 0.15f;
        [SerializeField, FormerlySerializedAs("behindDistance")] float _behindDistance = 2f;
        [SerializeField, FormerlySerializedAs("damage")] float _damage = 15f;
        [SerializeField, FormerlySerializedAs("hitRadius")] float _hitRadius = 1.7f;
        [SerializeField, FormerlySerializedAs("recovery")] float _recovery = 0.6f;
        [SerializeField, FormerlySerializedAs("surfaceTime")] Vector2 _surfaceTime = new(4f, 7f); // seconds spent fighting above ground between burrows

        public bool IsInvulnerable { get; private set; }

        // The whole ambush ignores staggers; it is invulnerable underground anyway.
        public override bool CanBeInterrupted => false;

        NavMeshAgent _navMeshAgentComponent;
        Collider[] _colliderComponents;
        PlayerAim _targetAimComponent;
        Vector3 _visualRest;

        protected override void Awake()
        {
            base.Awake();
            _navMeshAgentComponent = GetComponent<NavMeshAgent>();
            _colliderComponents = GetComponents<Collider>();
            _visualRest = _visualComponent.localPosition;
            if (_exitMarker) _exitMarker.SetActive(false);
            CooldownTimer = Random.Range(1.5f, 3f);
        }

        void Update()
        {
            TickCooldown();
            if (!IsReady) return;
            StartCoroutine(Ambush());
        }

        IEnumerator Ambush()
        {
            StartAttack();

            SetExposed(false);
            yield return MoveVisual(0f, -_sinkDepth, _sinkDuration);
            _navMeshAgentComponent.enabled = false;
            _chaserComponent.ReleaseSlot();

            yield return new WaitForSeconds(_undergroundTime);

            transform.position = FindExitPoint();
            FaceTargetNow();
            if (_exitMarker) _exitMarker.SetActive(true);
            yield return new WaitForSeconds(_markerTime);

            _navMeshAgentComponent.enabled = true;
            yield return MoveVisual(-_sinkDepth, 0f, _riseDuration);
            if (_exitMarker) _exitMarker.SetActive(false);
            SetExposed(true);
            TryHit();

            yield return new WaitForSeconds(_recovery);
            FinishAttack(NextSurfaceTime());
        }

        float NextSurfaceTime() => Random.Range(_surfaceTime.x, _surfaceTime.y);

        Vector3 FindExitPoint()
        {
            Transform target = _chaserComponent.TargetComponent;
            if (!_targetAimComponent) _targetAimComponent = target.GetComponent<PlayerAim>();
            Vector3 back = _targetAimComponent
                ? -_targetAimComponent.AimDirection
                : FlatMath.FlatDirection(target.position, transform.position, Vector3.back);

            foreach (float angle in ExitAngles)
            {
                Vector3 candidate = target.position + Quaternion.Euler(0f, angle, 0f) * back * _behindDistance;
                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 0.8f, NavMesh.AllAreas)) return hit.position;
            }
            return NavMesh.SamplePosition(target.position, out NavMeshHit fallback, 3f, NavMesh.AllAreas) ? fallback.position : transform.position;
        }

        void FaceTargetNow()
        {
            Vector3 direction = FlatMath.Flat(_chaserComponent.TargetComponent.position - transform.position);
            if (direction.sqrMagnitude <= 0.001f) return;
            transform.rotation = Quaternion.LookRotation(direction);
        }

        void TryHit()
        {
            Vector3 targetPosition = _chaserComponent.TargetComponent.position;
            if (FlatMath.FlatDistance(transform.position, targetPosition) > _hitRadius) return;
            var damageable = _chaserComponent.TargetComponent.GetComponentInParent<IDamageable>();
            damageable?.TakeDamage(_damage, FlatMath.FlatDirection(transform.position, targetPosition, transform.forward));
        }

        void SetExposed(bool exposed)
        {
            IsInvulnerable = !exposed;
            foreach (var col in _colliderComponents) col.enabled = exposed;
            if (_healthBar) _healthBar.SetActive(exposed);
        }

        IEnumerator MoveVisual(float from, float to, float duration)
        {
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                SetDepth(Mathf.Lerp(from, to, t / duration));
                yield return null;
            }
            SetDepth(to);
        }

        void SetDepth(float depth) => _visualComponent.localPosition = _visualRest + Vector3.up * depth;

        public override void Interrupt() { }

        void OnDisable()
        {
            if (!IsAttacking) return;
            StopAllCoroutines();
            SetDepth(0f);
            if (_exitMarker) _exitMarker.SetActive(false);
            IsInvulnerable = false;
            FinishAttack(NextSurfaceTime());
        }
    }
}
