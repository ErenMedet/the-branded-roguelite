using System;
using Branded.Interaction;
using UnityEngine;

namespace Branded.Loop
{
    // Morning campfire. Resting at it ends the morning.
    public class Campfire : Interactable
    {
        [SerializeField] Light fireLight;
        [SerializeField] float flickerAmount = 0.25f;
        [SerializeField] float flickerSpeed = 8f;

        public event Action Rested;

        float _baseIntensity;
        bool _used;

        public override bool IsAvailable => !_used;

        void Start()
        {
            if (fireLight) _baseIntensity = fireLight.intensity;
        }

        void Update()
        {
            if (!fireLight) return;
            float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f) - 0.5f;
            fireLight.intensity = _baseIntensity * (1f + noise * 2f * flickerAmount);
        }

        public override void Interact(PlayerInteractor user)
        {
            if (_used) return;
            _used = true;
            Rested?.Invoke();
        }
    }
}
