using System;
using UnityEngine;

namespace Branded.Player
{
    // One sword attack: timing, hit shape and feel. PlayerCombat holds the combo chain,
    // the dash strike and the charged strike; SwordHitbox and SwordSwingVisual read the current one.
    [Serializable]
    public class SwingData
    {
        [field: SerializeField] public float Windup { get; private set; } = 0.08f;
        [field: SerializeField] public float Active { get; private set; } = 0.12f;
        [field: SerializeField] public float Recovery { get; private set; } = 0.22f;
        [field: SerializeField] public float DamageMultiplier { get; private set; } = 1f;

        [field: Header("Shape")]
        [field: SerializeField] public float Reach { get; private set; } = 1.3f;
        [field: SerializeField] public float Radius { get; private set; } = 1.6f;
        [field: SerializeField, Range(0f, 360f)] public float ArcAngle { get; private set; } = 170f;

        [field: Header("Feel")]
        [Tooltip("Forward push along the aim when the hit frame starts.")]
        [field: SerializeField] public float LungeDistance { get; private set; }
        [Tooltip("How sharply the lunge decays: 0 slides at one speed, 2 bursts and dies off.")]
        [field: SerializeField, Range(0f, 3f)] public float LungeDecay { get; private set; } = 1.4f;
        [Tooltip("Scales hitstop and camera shake on impact.")]
        [field: SerializeField] public float Impact { get; private set; } = 1f;
        [Tooltip("Scales how far the target is knocked back.")]
        [field: SerializeField] public float Knockback { get; private set; } = 1f;
        [field: SerializeField, Range(0f, 1f)] public float MoveSpeedMultiplier { get; private set; } = 0.25f;

        [field: Header("Chaining")]
        [Tooltip("Share of the recovery a buffered press may cut short. 0 waits out the whole recovery.")]
        [field: SerializeField, Range(0f, 1f)] public float RecoveryCancelFraction { get; private set; } = 0.55f;

        [field: Header("Greybox visual")]
        [Tooltip("Angle the windup loads to. Set it to the previous swing's VisualTo, so a chain flows as one motion.")]
        [field: SerializeField] public float VisualFrom { get; private set; } = 90f;
        [Tooltip("Angle the blade settles on after its follow-through, and where the next swing should start.")]
        [field: SerializeField] public float VisualTo { get; private set; } = -90f;
        [Tooltip("How far the blade carries past its end angle before settling back. 0 stops dead.")]
        [field: SerializeField, Range(0f, 3f)] public float VisualOvershoot { get; private set; } = 1.2f;
        [Tooltip("Falls on the centre line from overhead: the angles come from SwordSwingVisual, not from VisualFrom/To.")]
        [field: SerializeField] public bool Overhead { get; private set; }

        // Seconds into the recovery after which the chain may be cut into the next swing.
        public float RecoveryCancelTime => Recovery * RecoveryCancelFraction;
    }
}
