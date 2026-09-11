using System.Collections;
using Branded.Combat;
using Branded.Player;
using UnityEngine;
using UnityEngine.AI;

namespace Branded.Enemies
{
    // Sinks into the ground, travels unseen, then bursts out behind the player. A marker shows the exit
    // point for a moment first so the ambush stays readable and can be dodged with a dash.
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyBurrowAmbush : EnemyAttack, IInvulnerabilitySource
    {
        static readonly float[] ExitAngles = { 0f, 40f, -40f, 80f, -80f };

        [SerializeField] EnemyChaser chaser;
        [SerializeField] Transform visual;      // sunk below the floor while burrowed
        [SerializeField] GameObject exitMarker; // ground disc at the exit point, sized to the hit radius
        [SerializeField] GameObject healthBar;
        [SerializeField] float sinkDepth = 2.3f;
        [SerializeField] float sinkDuration = 0.4f;
        [SerializeField] float undergroundTime = 1f;
        [SerializeField] float markerTime = 0.6f;
        [SerializeField] float riseDuration = 0.15f;
        [SerializeField] float behindDistance = 2f;
        [SerializeField] float damage = 15f;
        [SerializeField] float hitRadius = 1.7f;
        [SerializeField] float recovery = 0.6f;
        [SerializeField] Vector2 surfaceTime = new(4f, 7f); // seconds spent fighting above ground between burrows

        public bool IsInvulnerable { get; private set; }

        // The whole ambush ignores staggers; it is invulnerable underground anyway.
        public override bool CanBeInterrupted => false;

        NavMeshAgent _agent;
        Collider[] _colliders;
        PlayerAim _targetAim;
        Vector3 _visualRest;
        float _surfaceTimer;

        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            if (!chaser) chaser = GetComponent<EnemyChaser>();
            _colliders = GetComponents<Collider>();
            _visualRest = visual.localPosition;
            if (exitMarker) exitMarker.SetActive(false);
            _surfaceTimer = Random.Range(1.5f, 3f);
        }

        void Update()
        {
            if (IsAttacking || !chaser.Target) return;
            _surfaceTimer -= Time.deltaTime;
            if (_surfaceTimer <= 0f && !chaser.Halted) StartCoroutine(Ambush());
        }

        IEnumerator Ambush()
        {
            IsAttacking = true;
            chaser.Halted = true;

            SetExposed(false);
            yield return MoveVisual(0f, -sinkDepth, sinkDuration);
            _agent.enabled = false;
            chaser.ReleaseSlot();

            yield return new WaitForSeconds(undergroundTime);

            transform.position = FindExitPoint();
            FaceTargetNow();
            if (exitMarker) exitMarker.SetActive(true);
            yield return new WaitForSeconds(markerTime);

            _agent.enabled = true;
            yield return MoveVisual(-sinkDepth, 0f, riseDuration);
            if (exitMarker) exitMarker.SetActive(false);
            SetExposed(true);
            TryHit();

            yield return new WaitForSeconds(recovery);
            Finish();
        }

        Vector3 FindExitPoint()
        {
            Transform target = chaser.Target;
            if (!_targetAim) _targetAim = target.GetComponent<PlayerAim>();
            Vector3 back = _targetAim ? -_targetAim.AimDirection : transform.position - target.position;
            back.y = 0f;
            back = back.sqrMagnitude > 0.001f ? back.normalized : Vector3.back;

            foreach (float angle in ExitAngles)
            {
                Vector3 candidate = target.position + Quaternion.Euler(0f, angle, 0f) * back * behindDistance;
                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 0.8f, NavMesh.AllAreas)) return hit.position;
            }
            return NavMesh.SamplePosition(target.position, out NavMeshHit fallback, 3f, NavMesh.AllAreas) ? fallback.position : transform.position;
        }

        void FaceTargetNow()
        {
            Vector3 direction = chaser.Target.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(direction);
        }

        void TryHit()
        {
            Vector3 offset = chaser.Target.position - transform.position;
            offset.y = 0f;
            if (offset.sqrMagnitude > hitRadius * hitRadius) return;
            var damageable = chaser.Target.GetComponentInParent<IDamageable>();
            damageable?.TakeDamage(damage, offset.sqrMagnitude > 0.001f ? offset.normalized : transform.forward);
        }

        void SetExposed(bool exposed)
        {
            IsInvulnerable = !exposed;
            foreach (var col in _colliders) col.enabled = exposed;
            if (healthBar) healthBar.SetActive(exposed);
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

        void SetDepth(float depth) => visual.localPosition = _visualRest + Vector3.up * depth;

        public override void Interrupt() { }

        void Finish()
        {
            IsAttacking = false;
            chaser.Halted = false;
            _surfaceTimer = Random.Range(surfaceTime.x, surfaceTime.y);
        }

        void OnDisable()
        {
            if (!IsAttacking) return;
            StopAllCoroutines();
            SetDepth(0f);
            if (exitMarker) exitMarker.SetActive(false);
            IsInvulnerable = false;
            IsAttacking = false;
        }
    }
}
