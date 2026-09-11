using System.Collections;
using Branded.Combat;
using Branded.Meta;
using Branded.UI;
using UnityEngine;

namespace Branded.Player
{
    // Death drags the player back to Godo's forge: the run's boons go with the scene, demon ash is already banked.
    // A Death Defiance charge from Puck revives on the spot instead.
    public class PlayerDeath : MonoBehaviour, IInvulnerabilitySource
    {
        [SerializeField] HealthComponent health;
        [SerializeField] PlayerUpgrades upgrades;
        [SerializeField, Range(0.1f, 1f)] float defianceHealth = 0.5f;
        [SerializeField] float defianceInvulnerability = 1.5f;
        [SerializeField] float collapseDelay = 0.8f; // the body stays on screen before the fade

        float _invulnerableUntil;

        public bool IsInvulnerable => Time.time < _invulnerableUntil;

        void Awake()
        {
            if (!health) health = GetComponent<HealthComponent>();
            if (!upgrades) upgrades = GetComponent<PlayerUpgrades>();
        }

        void OnEnable() => health.OnDeath += Die;
        void OnDisable() => health.OnDeath -= Die;

        void Die()
        {
            if (upgrades && upgrades.TryUseDefiance())
            {
                health.Revive(health.maxHealth * defianceHealth);
                _invulnerableUntil = Time.time + defianceInvulnerability;
                return;
            }

            foreach (var behaviour in GetComponents<MonoBehaviour>())
                if (behaviour != this && behaviour != health) behaviour.enabled = false;
            StartCoroutine(ReturnToForge());
        }

        IEnumerator ReturnToForge()
        {
            yield return new WaitForSecondsRealtime(collapseDelay);
            var screen = FindAnyObjectByType<ScreenTransition>();
            if (screen) yield return screen.PlayDeath();
            SceneFlow.Load(SceneFlow.Hub);
        }
    }
}
