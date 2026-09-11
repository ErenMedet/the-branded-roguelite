using System;
using Branded.Player;
using UnityEngine;

namespace Branded.Loop
{
    // Morning campfire. Resting at it (interact in range) ends the morning.
    public class Campfire : MonoBehaviour
    {
        [SerializeField] float interactRadius = 2.5f;
        [SerializeField] Light fireLight;
        [SerializeField] float flickerAmount = 0.25f;
        [SerializeField] float flickerSpeed = 8f;

        public event Action Rested;

        Transform _player;
        PlayerInputReader _input;
        float _baseIntensity;
        bool _used;
        GUIStyle _style;

        void Start()
        {
            var player = GameObject.FindWithTag("Player");
            if (player)
            {
                _player = player.transform;
                _input = player.GetComponent<PlayerInputReader>();
                if (_input) _input.InteractPressed += OnInteract;
            }
            if (fireLight) _baseIntensity = fireLight.intensity;
        }

        void OnDestroy()
        {
            if (_input) _input.InteractPressed -= OnInteract;
        }

        void Update()
        {
            if (!fireLight) return;
            float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f) - 0.5f;
            fireLight.intensity = _baseIntensity * (1f + noise * 2f * flickerAmount);
        }

        bool PlayerInRange()
        {
            if (!_player) return false;
            Vector3 offset = _player.position - transform.position;
            offset.y = 0f;
            return offset.sqrMagnitude <= interactRadius * interactRadius;
        }

        void OnInteract()
        {
            if (_used || !PlayerInRange()) return;
            _used = true;
            Rested?.Invoke();
        }

        // Temporary prompt until the real UI (Aşama 4).
        void OnGUI()
        {
            if (_used || !PlayerInRange()) return;
            var cam = Camera.main;
            if (!cam) return;
            Vector3 screen = cam.WorldToScreenPoint(transform.position + Vector3.up * 1.6f);
            _style ??= new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(screen.x - 150f, Screen.height - screen.y - 20f, 300f, 40f), "[E] Dinlen", _style);
        }
    }
}
