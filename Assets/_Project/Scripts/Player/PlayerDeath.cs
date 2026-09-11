using System.Collections;
using Branded.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Branded.Player
{
    // Temporary until the Godo's forge death loop (Aşama 5): stop control and reload the scene.
    public class PlayerDeath : MonoBehaviour
    {
        [SerializeField] HealthComponent health;
        [SerializeField] float reloadDelay = 1.5f;

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
            StartCoroutine(ReloadAfterDelay());
        }

        IEnumerator ReloadAfterDelay()
        {
            yield return new WaitForSecondsRealtime(reloadDelay);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
