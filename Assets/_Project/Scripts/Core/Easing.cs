using UnityEngine;

namespace Branded.Core
{
    // Shared easing curves. Movement and swing visuals interpolate through these instead of a raw
    // Mathf.Lerp, so acceleration is felt: anticipation, an explosive middle and a settle at the end.
    // Every function takes and returns a normalized 0..1 value.
    public static class Easing
    {
        const float BackOvershoot = 1.70158f;

        // Fast start, decelerating settle. The default for anything that comes to rest.
        public static float OutCubic(float t)
        {
            t = 1f - Mathf.Clamp01(t);
            return 1f - t * t * t;
        }

        // Sharper than OutCubic: most of the motion happens in the first third.
        // Used for the blade's active frames, where the strike has to read as instant.
        public static float OutExpo(float t)
        {
            t = Mathf.Clamp01(t);
            return t >= 1f ? 1f : 1f - Mathf.Pow(2f, -10f * t);
        }

        // Slow start that builds up, for a windup that coils before it releases.
        public static float InCubic(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t * t;
        }

        // Ease in and out, for transitions with no impact of their own (move speed changes).
        public static float InOutQuad(float t)
        {
            t = Mathf.Clamp01(t);
            return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) * 0.5f;
        }

        // Overshoots past 1 before settling back, giving follow-through to a swing.
        public static float OutBack(float t, float overshoot = BackOvershoot)
        {
            t = Mathf.Clamp01(t) - 1f;
            return 1f + (overshoot + 1f) * t * t * t + overshoot * t * t;
        }

        // Speed profile for a burst that covers a fixed distance: starts fast, decays to nothing.
        // Normalized so integrating it across the full duration always covers exactly that distance,
        // which lets a dash or a lunge ease without drifting from its configured length.
        // power 0 is constant speed, 1 peaks at twice the average, 2 at three times.
        public static float DecaySpeed(float t, float power)
        {
            t = Mathf.Clamp01(t);
            power = Mathf.Max(0f, power);
            return (power + 1f) * Mathf.Pow(1f - t, power);
        }
    }
}
