using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Combat
{
    // Tints the renderers for a moment whenever this object takes a direct hit.
    public class DamageFlash : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("health")] HealthComponent _healthComponent;
        [SerializeField, FormerlySerializedAs("renderers")] Renderer[] _rendererComponents;
        [SerializeField, FormerlySerializedAs("flashColor")] Color _flashColor = Color.white;
        [SerializeField, FormerlySerializedAs("duration")] float _duration = 0.1f;

        RendererFlash _flash;

        void Awake()
        {
            if (!_healthComponent) _healthComponent = GetComponentInParent<HealthComponent>();
            if (_rendererComponents == null || _rendererComponents.Length == 0) _rendererComponents = GetComponentsInChildren<Renderer>();
            _flash = new RendererFlash(_rendererComponents);
        }

        void OnEnable() => _healthComponent.Damaged += OnDamaged;
        void OnDisable() => _healthComponent.Damaged -= OnDamaged;

        void OnDamaged(float amount, Vector3 hitDirection) => _flash.Flash(_flashColor, _duration);

        void Update() => _flash.Tick();
    }
}
