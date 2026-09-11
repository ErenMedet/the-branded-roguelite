using UnityEngine;

namespace Branded.Combat
{
    // Tints renderers for a moment when damaged. Uses unscaled time so the flash shows during hitstop.
    public class DamageFlash : MonoBehaviour
    {
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] HealthComponent health;
        [SerializeField] Renderer[] renderers;
        [SerializeField] Color flashColor = Color.white;
        [SerializeField] float duration = 0.1f;

        MaterialPropertyBlock _block;
        float _flashUntil;
        bool _flashing;

        void Awake()
        {
            if (!health) health = GetComponentInParent<HealthComponent>();
            if (renderers == null || renderers.Length == 0) renderers = GetComponentsInChildren<Renderer>();
            _block = new MaterialPropertyBlock();
        }

        void OnEnable() => health.OnDamaged += Flash;
        void OnDisable() => health.OnDamaged -= Flash;

        void Flash(float amount, Vector3 hitDirection)
        {
            _flashUntil = Time.unscaledTime + duration;
            _flashing = true;
            _block.SetColor(BaseColorId, flashColor);
            foreach (var r in renderers) r.SetPropertyBlock(_block);
        }

        void Update()
        {
            if (!_flashing || Time.unscaledTime < _flashUntil) return;
            _flashing = false;
            foreach (var r in renderers) r.SetPropertyBlock(null);
        }
    }
}
