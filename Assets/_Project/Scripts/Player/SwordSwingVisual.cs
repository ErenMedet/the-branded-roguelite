using UnityEngine;

namespace Branded.Player
{
    // Greybox only: sweeps the sword pivot through an arc following PlayerCombat's phases.
    // Replaced by real animation once the character model arrives.
    public class SwordSwingVisual : MonoBehaviour
    {
        [SerializeField] PlayerCombat combat;
        [SerializeField] Transform swordPivot;
        [SerializeField] float restAngle = 110f;
        [SerializeField] float arcAngle = 160f;

        void Awake()
        {
            if (!combat) combat = GetComponentInParent<PlayerCombat>();
        }

        void LateUpdate()
        {
            if (!combat || !swordPivot) return;

            float start = -arcAngle * 0.5f;
            float end = arcAngle * 0.5f;
            float t = combat.PhaseProgress;

            float yaw = combat.Phase switch
            {
                PlayerCombat.SwingPhase.Windup => Mathf.Lerp(restAngle, start, t),
                PlayerCombat.SwingPhase.Active => Mathf.Lerp(start, end, t),
                PlayerCombat.SwingPhase.Recovery => Mathf.Lerp(end, restAngle, t * t),
                _ => restAngle,
            };

            swordPivot.localRotation = Quaternion.Euler(0f, yaw, 0f);
        }
    }
}
