using System.Collections;
using Branded.Combat;
using UnityEngine;
using UnityEngine.AI;

namespace Branded.Enemies
{
    // Telegraphed straight-line charge. It aims while a stripe on the ground shows the path, then commits
    // and rushes along it. Can't be staggered mid-rush, but stands exposed for a moment afterwards.
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyChargeAttack : EnemyAttack
    {
        [SerializeField] EnemyChaser chaser;
        [SerializeField] Transform telegraph; // ground stripe pivot; its Z scale becomes the charge length
        [SerializeField] float minRange = 2.5f;
        [SerializeField] float maxRange = 8f;
        [SerializeField] float windup = 0.7f;
        [SerializeField] float speed = 16f;
        [SerializeField] float overshoot = 2.5f;
        [SerializeField] float damage = 20f;
        [SerializeField] float hitRadius = 1f;
        [SerializeField] float recovery = 0.9f;
        [SerializeField] float cooldown = 2.5f;

        public override bool CanBeInterrupted => !_rushing;

        NavMeshAgent _agent;
        float _cooldownTimer;
        bool _rushing;

        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            if (!chaser) chaser = GetComponent<EnemyChaser>();
            if (telegraph) telegraph.gameObject.SetActive(false);
            _cooldownTimer = cooldown * Random.Range(0.3f, 1f);
        }

        void Update()
        {
            _cooldownTimer -= Time.deltaTime;
            if (IsAttacking || _cooldownTimer > 0f || chaser.Halted || !chaser.Target) return;
            float distance = chaser.DistanceToTarget;
            if (distance >= minRange && distance <= maxRange && chaser.HasLineOfSight())
                StartCoroutine(Charge());
        }

        IEnumerator Charge()
        {
            IsAttacking = true;
            chaser.Halted = true;
            float length = 0f;

            if (telegraph) telegraph.gameObject.SetActive(true);
            for (float t = 0f; t < windup; t += Time.deltaTime)
            {
                chaser.FaceTarget();
                length = Mathf.Min(chaser.DistanceToTarget, maxRange) + overshoot;
                if (telegraph) telegraph.localScale = new Vector3(1f, 1f, length);
                yield return null;
            }
            if (telegraph) telegraph.gameObject.SetActive(false);

            _rushing = true;
            Vector3 direction = transform.forward;
            bool landed = false;
            for (float travelled = 0f; travelled < length;)
            {
                float step = speed * Time.deltaTime;
                Vector3 before = transform.position;
                if (_agent.enabled && _agent.isOnNavMesh) _agent.Move(direction * step);
                if (!landed) landed = TryHit(direction);
                // Stopped short by a wall.
                if (step > 0f && (transform.position - before).sqrMagnitude < step * step * 0.1f) break;
                travelled += step;
                yield return null;
            }
            _rushing = false;

            yield return new WaitForSeconds(recovery);
            Finish();
        }

        bool TryHit(Vector3 direction)
        {
            Vector3 offset = chaser.Target.position - transform.position;
            offset.y = 0f;
            if (offset.sqrMagnitude > hitRadius * hitRadius) return false;
            var damageable = chaser.Target.GetComponentInParent<IDamageable>();
            damageable?.TakeDamage(damage, direction);
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
            if (telegraph) telegraph.gameObject.SetActive(false);
            Finish();
        }

        void Finish()
        {
            IsAttacking = false;
            chaser.Halted = false;
            _cooldownTimer = cooldown;
        }
    }
}
