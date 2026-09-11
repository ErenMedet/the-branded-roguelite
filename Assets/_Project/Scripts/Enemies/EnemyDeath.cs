using System.Collections;
using Branded.Combat;
using UnityEngine;
using UnityEngine.AI;

namespace Branded.Enemies
{
    // Shuts the enemy down on death, shrinks it away, then destroys it.
    public class EnemyDeath : MonoBehaviour
    {
        [SerializeField] HealthComponent health;
        [SerializeField] float shrinkDuration = 0.18f;

        void Awake()
        {
            if (!health) health = GetComponent<HealthComponent>();
        }

        void OnEnable() => health.OnDeath += Die;
        void OnDisable() => health.OnDeath -= Die;

        void Die()
        {
            foreach (var behaviour in GetComponents<MonoBehaviour>())
                if (behaviour != this && behaviour != health) behaviour.enabled = false;
            foreach (var col in GetComponentsInChildren<Collider>()) col.enabled = false;
            if (TryGetComponent(out NavMeshAgent agent)) agent.enabled = false;

            StartCoroutine(ShrinkAndDestroy());
        }

        IEnumerator ShrinkAndDestroy()
        {
            Vector3 start = transform.localScale;
            float t = 0f;
            while (t < shrinkDuration)
            {
                t += Time.deltaTime;
                transform.localScale = Vector3.Lerp(start, Vector3.zero, t / shrinkDuration);
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
