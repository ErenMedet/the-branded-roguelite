using Branded.Combat;
using UnityEngine;

namespace Branded.Enemies
{
    // A fallen escort's soul drifting to its cultist. It can be cut down on the way, and fades if the cultist is gone.
    [RequireComponent(typeof(HealthComponent))]
    public class ReviveSoul : MonoBehaviour
    {
        [SerializeField] float _speed = 3.5f;
        [SerializeField] float _arriveDistance = 0.6f;
        [SerializeField] float _height = 1f;

        HealthComponent _healthComponent;
        EnemyCultist _cultistComponent;
        GameObject _escortPrefab;

        void Awake() => _healthComponent = GetComponent<HealthComponent>();

        void OnEnable() => _healthComponent.Died += OnDied;
        void OnDisable() => _healthComponent.Died -= OnDied;

        public void Launch(EnemyCultist cultist, GameObject escortPrefab)
        {
            _cultistComponent = cultist;
            _escortPrefab = escortPrefab;
        }

        void Update()
        {
            // Destroy only lands at the end of the frame, so a soul cut down this frame must not still arrive.
            if (_healthComponent.IsDead) return;
            if (!_cultistComponent || !_cultistComponent.isActiveAndEnabled)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 target = _cultistComponent.transform.position + Vector3.up * _height;
            transform.position = Vector3.MoveTowards(transform.position, target, _speed * Time.deltaTime);
            if ((target - transform.position).sqrMagnitude > _arriveDistance * _arriveDistance) return;

            _cultistComponent.Revive(_escortPrefab);
            Destroy(gameObject);
        }

        void OnDied() => Destroy(gameObject);
    }
}
