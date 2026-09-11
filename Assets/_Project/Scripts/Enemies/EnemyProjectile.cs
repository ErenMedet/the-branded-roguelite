using Branded.Combat;
using UnityEngine;

namespace Branded.Enemies
{
    // Straight-flying shot. Sweeps a sphere each frame so a fast shot can't tunnel through the player or a wall.
    public class EnemyProjectile : MonoBehaviour
    {
        [SerializeField] float speed = 11f;
        [SerializeField] float radius = 0.25f;
        [SerializeField] float lifetime = 3f;
        [SerializeField] LayerMask hitLayers = 1; // Default: the player and level geometry

        float _damage;

        public void Launch(float damage)
        {
            _damage = damage;
            Destroy(gameObject, lifetime);
        }

        void Update()
        {
            float step = speed * Time.deltaTime;
            if (Physics.SphereCast(transform.position, radius, transform.forward, out RaycastHit hit, step, hitLayers, QueryTriggerInteraction.Ignore))
            {
                var damageable = hit.collider.GetComponentInParent<IDamageable>();
                damageable?.TakeDamage(_damage, transform.forward);
                Destroy(gameObject);
                return;
            }
            transform.position += transform.forward * step;
        }
    }
}
