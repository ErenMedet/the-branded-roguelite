using Branded.Combat;
using UnityEngine;

namespace Branded.Enemies
{
    // World-space bar above the enemy, hidden until it first takes damage. It faces the camera and sits a bit
    // toward it so nearby bodies don't cover it. A pale trail shows the chunk just lost, then catches up.
    public class EnemyHealthBar : MonoBehaviour
    {
        [SerializeField] HealthComponent health;
        [SerializeField] Transform fill;  // pivot on the bar's left edge, scaled on X
        [SerializeField] Transform trail; // same, lags behind the fill
        [SerializeField] float height = 2.4f;
        [SerializeField] float towardCamera = 0.5f;
        [SerializeField] float trailDelay = 0.35f;
        [SerializeField] float trailSpeed = 1.5f; // bar fraction per second

        Transform _camera;
        Renderer[] _renderers;
        bool _shown;
        float _value = 1f;
        float _trailValue = 1f;
        float _trailTimer;

        void Awake()
        {
            if (!health) health = GetComponentInParent<HealthComponent>();
            _renderers = GetComponentsInChildren<Renderer>(true);
        }

        void OnEnable()
        {
            health.OnHealthChanged += Changed;
            health.OnDeath += Hide;
        }

        void OnDisable()
        {
            health.OnHealthChanged -= Changed;
            health.OnDeath -= Hide;
        }

        void Start()
        {
            if (Camera.main) _camera = Camera.main.transform;
            SetShown(false);
            Changed(health.currentHealth, health.maxHealth);
            _trailValue = _value;
            SetBar(trail, _trailValue);
        }

        void Changed(float current, float max)
        {
            _value = max > 0f ? current / max : 0f;
            _trailTimer = trailDelay;
            SetBar(fill, _value);
            if (!_shown && _value < 1f) SetShown(true);
        }

        void SetShown(bool shown)
        {
            _shown = shown;
            foreach (var r in _renderers) r.enabled = shown;
        }

        void Hide() => gameObject.SetActive(false);

        void LateUpdate()
        {
            if (!_camera) return;

            Vector3 anchor = health.transform.position + Vector3.up * height - _camera.forward * towardCamera;
            transform.SetPositionAndRotation(anchor, _camera.rotation);

            if (_trailValue <= _value) _trailValue = _value;
            else if (_trailTimer > 0f) _trailTimer -= Time.deltaTime;
            else _trailValue = Mathf.MoveTowards(_trailValue, _value, trailSpeed * Time.deltaTime);
            SetBar(trail, _trailValue);
        }

        static void SetBar(Transform bar, float value) => bar.localScale = new Vector3(Mathf.Clamp01(value), 1f, 1f);
    }
}
