using UnityEngine;

namespace Branded.Player
{
    // Finds the mouse point on the ground plane and turns Visual_Holder toward it.
    public class PlayerAim : MonoBehaviour
    {
        [SerializeField] PlayerInputReader input;
        [SerializeField] PlayerMotor motor;
        [Tooltip("Visual_Holder: only the visuals rotate, never Player_Root.")]
        [SerializeField] Transform visual;
        [SerializeField] float turnSpeed = 1440f;

        public Vector3 AimPoint { get; private set; }
        public Vector3 AimDirection { get; private set; } = Vector3.forward;

        // Set by combat while a swing is committed to a direction.
        public bool RotationLocked { get; set; }

        Camera _camera;

        void Awake()
        {
            if (!input) input = GetComponent<PlayerInputReader>();
            if (!motor) motor = GetComponent<PlayerMotor>();
        }

        void Update()
        {
            UpdateAimPoint();
            if (!visual || RotationLocked) return;

            Vector3 facing = motor && motor.IsDashing ? motor.DashDirection : AimDirection;
            Quaternion target = Quaternion.LookRotation(facing, Vector3.up);
            visual.rotation = Quaternion.RotateTowards(visual.rotation, target, turnSpeed * Time.deltaTime);
        }

        public void SnapToAim()
        {
            if (visual) visual.rotation = Quaternion.LookRotation(AimDirection, Vector3.up);
        }

        void UpdateAimPoint()
        {
            if (!_camera) _camera = Camera.main;
            if (!_camera) return;

            Ray ray = _camera.ScreenPointToRay(input.PointerScreenPosition);
            Plane ground = new Plane(Vector3.up, transform.position);
            if (!ground.Raycast(ray, out float enter)) return;

            AimPoint = ray.GetPoint(enter);
            Vector3 direction = AimPoint - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.0001f) AimDirection = direction.normalized;
        }
    }
}
