using Branded.Combat;
using UnityEngine;

namespace Branded.Enemies
{
    // Armored knight's shield: turns away hits from the front until one heavy enough (the arm cannon) breaks it.
    // Hits from behind always get through.
    public class EnemyShield : MonoBehaviour, IDamageBlocker
    {
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] GameObject _shieldVisual;
        [SerializeField, Range(0f, 360f)] float _blockArc = 120f;
        [Tooltip("A single hit at least this strong breaks the shield and gets through.")]
        [SerializeField] float _breakDamage = 60f;

        [Header("Block Flash")]
        [SerializeField] Color _flashColor = Color.white;
        [SerializeField] float _flashDuration = 0.1f;

        public bool IsBroken { get; private set; }

        Renderer _shieldRendererComponent;
        MaterialPropertyBlock _block;
        float _flashUntil;
        bool _flashing;

        void Awake()
        {
            if (_shieldVisual) _shieldRendererComponent = _shieldVisual.GetComponent<Renderer>();
            _block = new MaterialPropertyBlock();
        }

        public bool Blocks(float amount, Vector3 hitDirection)
        {
            if (IsBroken) return false;

            // hitDirection points from the attacker to this enemy, so a frontal hit comes in against our forward.
            Vector3 incoming = -hitDirection;
            incoming.y = 0f;
            if (incoming.sqrMagnitude < 0.0001f || Vector3.Angle(transform.forward, incoming) > _blockArc * 0.5f) return false;

            if (amount >= _breakDamage)
            {
                Break();
                return false;
            }

            Flash();
            return true;
        }

        void Break()
        {
            IsBroken = true;
            if (_shieldVisual) _shieldVisual.SetActive(false);
        }

        // Unscaled time so the flash still shows during hitstop.
        void Flash()
        {
            if (!_shieldRendererComponent) return;
            _flashUntil = Time.unscaledTime + _flashDuration;
            _flashing = true;
            _block.SetColor(BaseColorId, _flashColor);
            _shieldRendererComponent.SetPropertyBlock(_block);
        }

        void Update()
        {
            if (!_flashing || Time.unscaledTime < _flashUntil) return;
            _flashing = false;
            _shieldRendererComponent.SetPropertyBlock(null);
        }
    }
}
