using UnityEngine;

namespace Branded.Core
{
    // Movement and combat happen on the ground plane, so directions, distances and arcs ignore height.
    public static class FlatMath
    {
        const float MinSqrLength = 0.0001f;
        const float OnTopSqrDistance = 0.01f; // closer than this, a target counts as inside any arc

        public static Vector3 Flat(Vector3 vector)
        {
            vector.y = 0f;
            return vector;
        }

        // Normalized flat direction between two points, or the fallback when they overlap.
        public static Vector3 FlatDirection(Vector3 from, Vector3 to, Vector3 fallback)
        {
            Vector3 offset = Flat(to - from);
            return offset.sqrMagnitude > MinSqrLength ? offset.normalized : fallback;
        }

        public static float FlatDistance(Vector3 a, Vector3 b) => Flat(b - a).magnitude;

        // True when toTarget lies within arcAngle degrees centred on forward.
        public static bool WithinArc(Vector3 forward, Vector3 toTarget, float arcAngle)
        {
            toTarget = Flat(toTarget);
            return toTarget.sqrMagnitude <= OnTopSqrDistance || Vector3.Angle(Flat(forward), toTarget) <= arcAngle * 0.5f;
        }
    }
}
