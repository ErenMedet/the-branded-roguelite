using System.Collections;
using Branded.Combat;
using Branded.Core;
using Branded.Meta;
using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Player
{
    // Death drags the player back to Godot's forge: the run's boons go with the scene, demon ash is already banked.
    // A Death Defiance charge from Puck revives on the spot instead.
    public class PlayerDeath : MonoBehaviour, IInvulnerabilitySource
    {
        [SerializeField, FormerlySerializedAs("health")] HealthComponent _healthComponent;
        [SerializeField, FormerlySerializedAs("upgrades")] PlayerUpgrades _upgradesComponent;
        [SerializeField, Range(0.1f, 1f), FormerlySerializedAs("defianceHealth")] float _defianceHealth = 0.5f;
        [SerializeField, FormerlySerializedAs("defianceInvulnerability")] float _defianceInvulnerability = 1.5f;
        [SerializeField, FormerlySerializedAs("collapseDelay")] float _collapseDelay = 0.8f; // the body stays on screen before the fade

        float _invulnerableUntil;

        public bool IsInvulnerable => Time.time < _invulnerableUntil;

        void Awake()
        {
            if (!_healthComponent) _healthComponent = GetComponent<HealthComponent>();
            if (!_upgradesComponent) _upgradesComponent = GetComponent<PlayerUpgrades>();
        }

        void OnEnable()
        {
            _healthComponent.Died += OnDied;
            GameEvents.DeathScreenFinished += OnDeathScreenFinished;
        }

        void OnDisable()
        {
            _healthComponent.Died -= OnDied;
            GameEvents.DeathScreenFinished -= OnDeathScreenFinished;
        }

        void OnDied()
        {
            if (_upgradesComponent && _upgradesComponent.TryUseDefiance())
            {
                _healthComponent.Revive(_healthComponent.MaxHealth * _defianceHealth);
                _invulnerableUntil = Time.time + _defianceInvulnerability;
                return;
            }

            foreach (var behaviour in GetComponents<MonoBehaviour>())
                if (behaviour != this && behaviour != _healthComponent) behaviour.enabled = false;
            StartCoroutine(Collapse());
        }

        // ScreenTransition answers PlayerDied with the death lines, then raises DeathScreenFinished.
        IEnumerator Collapse()
        {
            yield return new WaitForSecondsRealtime(_collapseDelay);
            GameEvents.RaisePlayerDied();
        }

        void OnDeathScreenFinished() => SceneFlow.Load(SceneFlow.Hub);
    }
}
