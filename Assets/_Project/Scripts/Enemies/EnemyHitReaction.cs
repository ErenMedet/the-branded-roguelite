using System.Collections;
using Branded.Combat;
using Branded.Core;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Branded.Enemies
{
    // Knockback + short stagger when hit. Cancels a pending attack unless that attack has super armor.
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyHitReaction : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("health")] HealthComponent _healthComponent;
        [SerializeField, FormerlySerializedAs("chaser")] EnemyChaser _chaserComponent;
        [SerializeField, FormerlySerializedAs("knockbackDistance")] float _knockbackDistance = 1.2f;
        [SerializeField, FormerlySerializedAs("knockbackDuration")] float _knockbackDuration = 0.12f;
        [SerializeField, FormerlySerializedAs("staggerDuration")] float _staggerDuration = 0.3f;

        NavMeshAgent _navMeshAgentComponent;
        EnemyAttack[] _attackComponents;

        void Awake()
        {
            _navMeshAgentComponent = GetComponent<NavMeshAgent>();
            if (!_healthComponent) _healthComponent = GetComponent<HealthComponent>();
            if (!_chaserComponent) _chaserComponent = GetComponent<EnemyChaser>();
            _attackComponents = GetComponents<EnemyAttack>();
        }

        void OnEnable() => _healthComponent.Damaged += OnDamaged;
        void OnDisable() => _healthComponent.Damaged -= OnDamaged;

        void OnDamaged(float amount, Vector3 hitDirection, float knockback)
        {
            if (_healthComponent.IsDead) return;
            foreach (var attack in _attackComponents)
                if (attack.IsAttacking && !attack.CanBeInterrupted) return;

            foreach (var attack in _attackComponents) attack.Interrupt();
            StopAllCoroutines();
            StartCoroutine(Knockback(hitDirection, knockback));
        }

        IEnumerator Knockback(Vector3 direction, float scale)
        {
            _chaserComponent.Halted = true;
            direction = FlatMath.Flat(direction).normalized;

            float speed = _knockbackDistance * Mathf.Max(0f, scale) / _knockbackDuration;
            float t = 0f;
            while (t < _knockbackDuration)
            {
                if (_navMeshAgentComponent.enabled && _navMeshAgentComponent.isOnNavMesh) _navMeshAgentComponent.Move(direction * (speed * Time.deltaTime));
                t += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(Mathf.Max(0f, _staggerDuration - _knockbackDuration));
            _chaserComponent.Halted = false;
        }
    }
}
