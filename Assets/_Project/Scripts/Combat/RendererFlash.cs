using UnityEngine;

namespace Branded.Combat
{
    // Tints renderers for a moment. Uses unscaled time so the flash still shows during hitstop.
    // A plain class rather than a component: its owner calls Tick from its own Update.
    public class RendererFlash
    {
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        readonly Renderer[] _rendererComponents;
        readonly MaterialPropertyBlock _block = new();
        float _flashUntil;
        bool _flashing;

        public RendererFlash(Renderer[] rendererComponents) => _rendererComponents = rendererComponents;

        public void Flash(Color color, float duration)
        {
            _flashUntil = Time.unscaledTime + duration;
            _flashing = true;
            _block.SetColor(BaseColorId, color);
            foreach (var r in _rendererComponents) r.SetPropertyBlock(_block);
        }

        public void Tick()
        {
            if (!_flashing || Time.unscaledTime < _flashUntil) return;
            _flashing = false;
            foreach (var r in _rendererComponents) r.SetPropertyBlock(null);
        }
    }
}
