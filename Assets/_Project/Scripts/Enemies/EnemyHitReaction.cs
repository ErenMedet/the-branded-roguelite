using System.Collections;
using Branded.Combat;
using UnityEngine;
using UnityEngine.AI;

namespace Branded.Enemies
{
    // Knockback + short stagger when hit. Cancels a pending attack unless that attack has super armor.
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyHitReaction : MonoBehaviour
    {
        [SerializeField] HealthComponent health;
        [SerializeField] EnemyChaser chaser;
        [SerializeField] float knockbackDistance = 1.2f;
        [SerializeField] float knockbackDuration = 0.12f;
        [SerializeField] float staggerDuration = 0.3f;

        NavMeshAgent _agent;
        EnemyAttack[] _attacks;

        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            if (!health) health = GetComponent<HealthComponent>();
            if (!chaser) chaser = GetComponent<EnemyChaser>();
            _attacks = GetComponents<EnemyAttack>();
        }

        void OnEnable() => health.OnDamaged += React;
        void OnDisable() => health.OnDamaged -= React;

        void React(float amount, Vector3 hitDirection)
        {
            if (health.IsDead) return;
            foreach (var attack in _attacks)
                if (attack.IsAttacking && !attack.CanBeInterrupted) return;

            foreach (var attack in _attacks) attack.Interrupt();
            StopAllCoroutines();
            StartCoroutine(Knockback(hitDirection));
        }

        IEnumerator Knockback(Vector3 direction)
        {
            chaser.Halted = true;
            direction.y = 0f;
            direction.Normalize();

            float speed = knockbackDistance / knockbackDuration;
            float t = 0f;
            while (t < knockbackDuration)
            {
                if (_agent.enabled && _agent.isOnNavMesh) _agent.Move(direction * (speed * Time.deltaTime));
                t += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(Mathf.Max(0f, staggerDuration - knockbackDuration));
            chaser.Halted = false;
        }
    }
}
