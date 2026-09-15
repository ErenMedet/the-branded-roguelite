using System.Collections.Generic;
using Branded.Core;
using UnityEngine;

namespace Branded.Combat
{
    // A patch on the ground that hurts whatever stands in it at a steady rhythm, then disappears.
    // The first tick waits one interval, so there is time to step or dash out.
    public class DamageZone : MonoBehaviour
    {
        [SerializeField] float _radius = 2.5f;
        [SerializeField] float _damage = 5f;
        [SerializeField] float _tickInterval = 0.5f;
        [SerializeField] float _duration = 3f;
        [SerializeField] LayerMask _targetLayers = 1; // Default: the player

        readonly HashSet<IDamageable> _hitThisTick = new();
        float _tickTimer;
        float _lifeTimer;

        void Awake()
        {
            _tickTimer = _tickInterval;
            _lifeTimer = _duration;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            _lifeTimer -= dt;
            if (_lifeTimer <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            _tickTimer -= dt;
            if (_tickTimer > 0f) return;
            _tickTimer += _tickInterval;
            Tick();
        }

        void Tick()
        {
            _hitThisTick.Clear();
            Collider[] hits = Physics.OverlapSphere(transform.position, _radius, _targetLayers, QueryTriggerInteraction.Ignore);
            foreach (var hit in hits)
            {
                var target = hit.GetComponentInParent<IDamageable>();
                if (target == null || !_hitThisTick.Add(target)) continue;

                target.TakeDamage(_damage, FlatMath.FlatDirection(transform.position, hit.transform.position, Vector3.forward));
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.9f, 0.3f, 0.4f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
}
