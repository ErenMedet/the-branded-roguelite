using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Combat
{
    // Tints renderers for a moment when damaged. Uses unscaled time so the flash shows during hitstop.
    public class DamageFlash : MonoBehaviour
    {
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField, FormerlySerializedAs("health")] HealthComponent _healthComponent;
        [SerializeField, FormerlySerializedAs("renderers")] Renderer[] _rendererComponents;
        [SerializeField, FormerlySerializedAs("flashColor")] Color _flashColor = Color.white;
        [SerializeField, FormerlySerializedAs("duration")] float _duration = 0.1f;

        MaterialPropertyBlock _block;
        float _flashUntil;
        bool _flashing;

        void Awake()
        {
            if (!_healthComponent) _healthComponent = GetComponentInParent<HealthComponent>();
            if (_rendererComponents == null || _rendererComponents.Length == 0) _rendererComponents = GetComponentsInChildren<Renderer>();
            _block = new MaterialPropertyBlock();
        }

        void OnEnable() => _healthComponent.Damaged += OnDamaged;
        void OnDisable() => _healthComponent.Damaged -= OnDamaged;

        void OnDamaged(float amount, Vector3 hitDirection)
        {
            _flashUntil = Time.unscaledTime + _duration;
            _flashing = true;
            _block.SetColor(BaseColorId, _flashColor);
            foreach (var r in _rendererComponents) r.SetPropertyBlock(_block);
        }

        void Update()
        {
            if (!_flashing || Time.unscaledTime < _flashUntil) return;
            _flashing = false;
            foreach (var r in _rendererComponents) r.SetPropertyBlock(null);
        }
    }
}
