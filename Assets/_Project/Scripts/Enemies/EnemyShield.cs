using Branded.Combat;
using Branded.Core;
using UnityEngine;

namespace Branded.Enemies
{
    // Armored knight's shield: turns away hits from the front until one heavy enough (the arm cannon) breaks it.
    // Hits from behind always get through.
    public class EnemyShield : MonoBehaviour, IDamageBlocker
    {
        [SerializeField] GameObject _shieldVisual;
        [SerializeField, Range(0f, 360f)] float _blockArc = 120f;
        [Tooltip("A single hit at least this strong breaks the shield and gets through.")]
        [SerializeField] float _breakDamage = 60f;

        [Header("Block Flash")]
        [SerializeField] Color _flashColor = Color.white;
        [SerializeField] float _flashDuration = 0.1f;

        public bool IsBroken { get; private set; }

        RendererFlash _flash;

        void Awake()
        {
            if (!_shieldVisual || !_shieldVisual.TryGetComponent(out Renderer shieldRenderer)) return;
            _flash = new RendererFlash(new[] { shieldRenderer });
        }

        public bool Blocks(float amount, Vector3 hitDirection)
        {
            if (IsBroken) return false;

            // hitDirection points from the attacker to this enemy, so a frontal hit comes in against our forward.
            Vector3 incoming = FlatMath.Flat(-hitDirection);
            if (incoming.sqrMagnitude < 0.0001f || !FlatMath.WithinArc(transform.forward, incoming, _blockArc)) return false;

            if (amount >= _breakDamage)
            {
                Break();
                return false;
            }

            _flash?.Flash(_flashColor, _flashDuration);
            return true;
        }

        void Break()
        {
            IsBroken = true;
            if (_shieldVisual) _shieldVisual.SetActive(false);
        }

        void Update() => _flash?.Tick();
    }
}
