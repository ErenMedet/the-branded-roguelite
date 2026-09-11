using System.Collections;
using Branded.Combat;
using UnityEngine;
using UnityEngine.AI;

namespace Branded.Enemies
{
    // Shuts the enemy down on death, shrinks it away, then destroys it.
    // At dawn it evaporates instead: rises and fades without counting as a death.
    public class EnemyDeath : MonoBehaviour
    {
        [SerializeField] HealthComponent health;
        [SerializeField] float shrinkDuration = 0.18f;
        [SerializeField] float evaporateDuration = 0.7f;
        [SerializeField] float evaporateRise = 1.2f;

        bool _gone;

        void Awake()
        {
            if (!health) health = GetComponent<HealthComponent>();
        }

        void OnEnable() => health.OnDeath += Die;
        void OnDisable() => health.OnDeath -= Die;

        void Die()
        {
            if (_gone) return;
            Shutdown();
            StartCoroutine(Vanish(0f, shrinkDuration, 0f));
        }

        public void Evaporate(float delay)
        {
            if (_gone || health.IsDead) return;
            Shutdown();
            StartCoroutine(Vanish(delay, evaporateDuration, evaporateRise));
        }

        void Shutdown()
        {
            _gone = true;
            foreach (var behaviour in GetComponents<MonoBehaviour>())
                if (behaviour != this && behaviour != health) behaviour.enabled = false;
            foreach (var col in GetComponentsInChildren<Collider>()) col.enabled = false;
            if (TryGetComponent(out NavMeshAgent agent)) agent.enabled = false;
        }

        IEnumerator Vanish(float delay, float duration, float rise)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            Vector3 startPosition = transform.position;
            Vector3 startScale = transform.localScale;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / duration);
                transform.position = startPosition + Vector3.up * (rise * k);
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, k);
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
