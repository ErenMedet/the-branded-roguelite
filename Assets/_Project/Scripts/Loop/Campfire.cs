using Branded.Core;
using Branded.Interaction;
using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Loop
{
    // Morning campfire. Resting at it ends the morning.
    public class Campfire : Interactable
    {
        [SerializeField, FormerlySerializedAs("fireLight")] Light _fireLightComponent;
        [SerializeField, FormerlySerializedAs("flickerAmount")] float _flickerAmount = 0.25f;
        [SerializeField, FormerlySerializedAs("flickerSpeed")] float _flickerSpeed = 8f;

        float _baseIntensity;
        bool _used;

        public override bool IsAvailable => !_used;

        void Start()
        {
            if (!_fireLightComponent) return;
            _baseIntensity = _fireLightComponent.intensity;
        }

        void Update()
        {
            if (!_fireLightComponent) return;
            float noise = Mathf.PerlinNoise(Time.time * _flickerSpeed, 0f) - 0.5f;
            _fireLightComponent.intensity = _baseIntensity * (1f + noise * 2f * _flickerAmount);
        }

        public override void Interact(PlayerInteractor user)
        {
            if (_used) return;
            _used = true;
            GameEvents.RaiseCampfireRested();
        }
    }
}
