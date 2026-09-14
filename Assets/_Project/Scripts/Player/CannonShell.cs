using System.Collections.Generic;
using Branded.Combat;
using UnityEngine;

namespace Branded.Player
{
    // The arm cannon's shell, Graves-style: flies straight, and where it hits (or runs out of range) it bursts
    // into a cone along its flight. The target it struck takes the impact damage instead of the cone damage.
    [RequireComponent(typeof(Projectile))]
    public class CannonShell : MonoBehaviour
    {
        [Header("Damage")]
        [SerializeField] float _impactDamage = 150f;
        [SerializeField] float _coneDamage = 100f;
        [SerializeField] LayerMask _targetLayers;

        [Header("Cone")]
        [SerializeField] float _coneLength = 9f;
        [SerializeField, Range(0f, 180f)] float _coneAngle = 110f;

        [Header("Visual")]
        [SerializeField] GameObject _blastPrefab;
        [SerializeField] float _blastDuration = 0.2f;

        Projectile _projectileComponent;
        readonly HashSet<IDamageable> _hitThisBlast = new();

        void Awake() => _projectileComponent = GetComponent<Projectile>();

        void OnEnable() => _projectileComponent.Impacted += OnImpacted;
        void OnDisable() => _projectileComponent.Impacted -= OnImpacted;

        public void Launch(Transform owner) => _projectileComponent.Launch(_impactDamage, owner);

        void OnImpacted(Vector3 point, IDamageable struck)
        {
            Vector3 direction = transform.forward;
            direction.y = 0f;
            direction.Normalize();

            _hitThisBlast.Clear();
            if (struck != null) _hitThisBlast.Add(struck);

            Collider[] hits = Physics.OverlapSphere(point, _coneLength, _targetLayers, QueryTriggerInteraction.Collide);
            foreach (var hit in hits)
            {
                Vector3 toTarget = hit.transform.position - point;
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude > 0.01f && Vector3.Angle(direction, toTarget) > _coneAngle * 0.5f) continue;

                var target = hit.GetComponentInParent<IDamageable>();
                if (target == null || !_hitThisBlast.Add(target)) continue;
                target.TakeDamage(_coneDamage, toTarget.sqrMagnitude > 0.01f ? toTarget.normalized : direction);
            }

            if (!_blastPrefab) return;
            var blast = Instantiate(_blastPrefab, point, Quaternion.LookRotation(direction));
            Destroy(blast, _blastDuration);
        }
    }
}
