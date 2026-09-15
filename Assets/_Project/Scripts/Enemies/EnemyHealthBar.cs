using Branded.Combat;
using Branded.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Enemies
{
    // World-space bar above the enemy, hidden until it first takes damage. It faces the camera and sits a bit
    // toward it so nearby bodies don't cover it. A pale trail shows the chunk just lost, then catches up.
    public class EnemyHealthBar : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("health")] HealthComponent _healthComponent;
        [SerializeField, FormerlySerializedAs("fill")] Transform _fillComponent;   // pivot on the bar's left edge, scaled on X
        [SerializeField, FormerlySerializedAs("trail")] Transform _trailComponent; // same, lags behind the fill
        [SerializeField, FormerlySerializedAs("height")] float _height = 2.4f;
        [SerializeField, FormerlySerializedAs("towardCamera")] float _towardCamera = 0.5f;
        [SerializeField, FormerlySerializedAs("trailDelay")] float _trailDelay = 0.35f;
        [SerializeField, FormerlySerializedAs("trailSpeed")] float _trailSpeed = 1.5f; // bar fraction per second

        Transform _cameraTransformComponent;
        Renderer[] _rendererComponents;
        BarTrail _trail;
        bool _shown;

        void Awake()
        {
            if (!_healthComponent) _healthComponent = GetComponentInParent<HealthComponent>();
            _rendererComponents = GetComponentsInChildren<Renderer>(true);
            _trail = new BarTrail(_trailDelay, _trailSpeed);
        }

        void OnEnable()
        {
            _healthComponent.HealthChanged += OnHealthChanged;
            _healthComponent.Died += OnDied;
        }

        void OnDisable()
        {
            _healthComponent.HealthChanged -= OnHealthChanged;
            _healthComponent.Died -= OnDied;
        }

        void Start()
        {
            if (Camera.main) _cameraTransformComponent = Camera.main.transform;
            SetShown(false);
            OnHealthChanged(_healthComponent.CurrentHealth, _healthComponent.MaxHealth);
            _trail.Snap();
            SetBar(_trailComponent, _trail.Value);
        }

        void OnHealthChanged(float current, float max)
        {
            float value = max > 0f ? current / max : 0f;
            _trail.SetTarget(value);
            SetBar(_fillComponent, value);
            if (!_shown && value < 1f) SetShown(true);
        }

        void SetShown(bool shown)
        {
            _shown = shown;
            foreach (var r in _rendererComponents) r.enabled = shown;
        }

        void OnDied() => gameObject.SetActive(false);

        void LateUpdate()
        {
            if (!_cameraTransformComponent) return;

            Vector3 anchor = _healthComponent.transform.position + Vector3.up * _height - _cameraTransformComponent.forward * _towardCamera;
            transform.SetPositionAndRotation(anchor, _cameraTransformComponent.rotation);
            SetBar(_trailComponent, _trail.Tick(Time.deltaTime));
        }

        static void SetBar(Transform bar, float value) => bar.localScale = new Vector3(Mathf.Clamp01(value), 1f, 1f);
    }
}
