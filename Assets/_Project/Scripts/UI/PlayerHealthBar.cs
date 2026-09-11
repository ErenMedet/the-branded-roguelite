using Branded.Combat;
using TMPro;
using UnityEngine;

namespace Branded.UI
{
    // HUD health bar. A pale trail shows the chunk just lost, then catches up (same feel as the enemy bars).
    public class PlayerHealthBar : MonoBehaviour
    {
        [SerializeField] HealthComponent health; // empty = the Player-tagged object
        [SerializeField] RectTransform fill;     // stretched; its right anchor follows the value
        [SerializeField] RectTransform trail;
        [SerializeField] TMP_Text label;
        [SerializeField] float trailDelay = 0.35f;
        [SerializeField] float trailSpeed = 1f; // bar fraction per second

        float _value = 1f;
        float _trailValue = 1f;
        float _trailTimer;

        void Awake()
        {
            if (health) return;
            var player = GameObject.FindWithTag("Player");
            if (player) health = player.GetComponent<HealthComponent>();
        }

        void OnEnable()
        {
            if (!health) return;
            health.OnHealthChanged += Changed;
            Changed(health.currentHealth, health.maxHealth); // catch up on anything missed while hidden
        }

        void OnDisable()
        {
            if (health) health.OnHealthChanged -= Changed;
        }

        void Start()
        {
            if (!health) return;
            Changed(health.currentHealth, health.maxHealth);
            _trailValue = _value;
            SetBar(trail, _trailValue);
        }

        void Changed(float current, float max)
        {
            _value = max > 0f ? current / max : 0f;
            _trailTimer = trailDelay;
            SetBar(fill, _value);
            if (label) label.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }

        void Update()
        {
            if (_trailValue <= _value) _trailValue = _value;
            else if (_trailTimer > 0f) _trailTimer -= Time.unscaledDeltaTime;
            else _trailValue = Mathf.MoveTowards(_trailValue, _value, trailSpeed * Time.unscaledDeltaTime);
            SetBar(trail, _trailValue);
        }

        static void SetBar(RectTransform bar, float value) => bar.anchorMax = new Vector2(Mathf.Clamp01(value), 1f);
    }
}
