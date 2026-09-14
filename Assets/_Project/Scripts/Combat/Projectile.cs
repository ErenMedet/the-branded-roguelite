using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Branded.Combat
{
    // Straight-flying shot, used by enemies and the player's arm weapons.
    // Sweeps a sphere each frame so a fast shot can't tunnel through a target or a wall.
    public class Projectile : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("speed")] float _speed = 11f;
        [SerializeField, FormerlySerializedAs("radius")] float _radius = 0.25f;
        [SerializeField, FormerlySerializedAs("lifetime")] float _lifetime = 3f;
        [SerializeField, FormerlySerializedAs("hitLayers")] LayerMask _hitLayers = 1; // Default: the player and level geometry

        // (impact point, what was hit). Also raised with null when the lifetime runs out, so shells can burst at max range.
        public event UnityAction<Vector3, IDamageable> Impacted;

        float _damage;
        float _timer;
        Transform _ownerComponent;

        // The owner's own colliders are skipped, since a shot from an arm starts inside or beside its shooter.
        public void Launch(float damage, Transform owner = null)
        {
            _damage = damage;
            _ownerComponent = owner;
            _timer = _lifetime;
        }

        void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                Impact(transform.position, null);
                return;
            }

            float step = _speed * Time.deltaTime;
            if (!TryFindHit(step, out RaycastHit hit))
            {
                transform.position += transform.forward * step;
                return;
            }

            var damageable = hit.collider.GetComponentInParent<IDamageable>();
            damageable?.TakeDamage(_damage, transform.forward);
            Impact(hit.point, damageable);
        }

        bool TryFindHit(float step, out RaycastHit closest)
        {
            closest = default;
            if (!_ownerComponent)
                return Physics.SphereCast(transform.position, _radius, transform.forward, out closest, step, _hitLayers, QueryTriggerInteraction.Ignore);

            bool found = false;
            closest.distance = float.MaxValue;
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, _radius, transform.forward, step, _hitLayers, QueryTriggerInteraction.Ignore);
            foreach (var hit in hits)
            {
                if (hit.collider.transform.IsChildOf(_ownerComponent) || hit.distance >= closest.distance) continue;
                closest = hit;
                found = true;
            }
            return found;
        }

        void Impact(Vector3 point, IDamageable struck)
        {
            Impacted?.Invoke(point, struck);
            Destroy(gameObject);
        }
    }
}
