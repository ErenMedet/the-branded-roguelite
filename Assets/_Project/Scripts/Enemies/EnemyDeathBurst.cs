using Branded.Combat;
using UnityEngine;

namespace Branded.Enemies
{
    // Flesh heap: bursts on death and leaves a damaging patch behind, so killing it up close is risky.
    // Evaporating at dawn isn't a death and leaves nothing.
    public class EnemyDeathBurst : MonoBehaviour
    {
        [SerializeField] HealthComponent _healthComponent;
        [SerializeField] DamageZone _zonePrefabComponent;

        void Awake()
        {
            if (!_healthComponent) _healthComponent = GetComponent<HealthComponent>();
        }

        void OnEnable() => _healthComponent.Died += OnDied;
        void OnDisable() => _healthComponent.Died -= OnDied;

        void OnDied()
        {
            if (!_zonePrefabComponent) return;
            Instantiate(_zonePrefabComponent, transform.position, Quaternion.identity);
        }
    }
}
