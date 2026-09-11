using System.Collections;
using Branded.Combat;
using UnityEngine;

namespace Branded.Enemies
{
    // Simple telegraphed melee: stop, show the warning, then hit if the player is still in reach.
    public class EnemyMeleeAttack : MonoBehaviour
    {
        [SerializeField] EnemyChaser chaser;
        [SerializeField] GameObject telegraph;
        [SerializeField] float damage = 10f;
        [SerializeField] float windup = 0.55f;
        [SerializeField] float cooldown = 1.2f;
        [SerializeField] float hitRange = 2.1f;

        public bool IsAttacking { get; private set; }

        float _cooldownTimer;

        void Awake()
        {
            if (!chaser) chaser = GetComponent<EnemyChaser>();
            if (telegraph) telegraph.SetActive(false);
        }

        void Update()
        {
            _cooldownTimer -= Time.deltaTime;
            if (!IsAttacking && _cooldownTimer <= 0f && chaser.InRange)
                StartCoroutine(Attack());
        }

        IEnumerator Attack()
        {
            IsAttacking = true;
            chaser.Halted = true;
            if (telegraph) telegraph.SetActive(true);

            float t = 0f;
            while (t < windup)
            {
                chaser.FaceTarget();
                t += Time.deltaTime;
                yield return null;
            }

            if (telegraph) telegraph.SetActive(false);
            TryHitTarget();
            Finish();
        }

        void TryHitTarget()
        {
            Transform target = chaser.Target;
            if (!target) return;

            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude > hitRange * hitRange) return;
            if (Vector3.Angle(transform.forward, toTarget) > 70f) return;

            var damageable = target.GetComponentInParent<IDamageable>();
            damageable?.TakeDamage(damage, toTarget.normalized);
        }

        // Called when the enemy gets staggered.
        public void Interrupt()
        {
            if (!IsAttacking) return;
            StopAllCoroutines();
            if (telegraph) telegraph.SetActive(false);
            Finish();
        }

        void Finish()
        {
            IsAttacking = false;
            chaser.Halted = false;
            _cooldownTimer = cooldown;
        }

        void OnDisable() => Interrupt();
    }
}
