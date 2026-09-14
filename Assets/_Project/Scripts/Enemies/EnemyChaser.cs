using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Branded.Enemies
{
    // Follows the player on the NavMesh. Melee enemies walk to a reserved slot around the player
    // (EnemySlotRing) so groups surround the player instead of lining up behind each other.
    // Ranged enemies set _keepDistance and hold that range instead.
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyChaser : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("repathInterval")] float _repathInterval = 0.2f;
        [SerializeField, FormerlySerializedAs("stopDistance")] float _stopDistance = 1.6f;
        [SerializeField, FormerlySerializedAs("turnSpeed")] float _turnSpeed = 540f;
        [Tooltip("> 0: hold this distance from the player instead of taking a ring slot.")]
        [SerializeField, FormerlySerializedAs("keepDistance")] float _keepDistance = 0f;
        [SerializeField, FormerlySerializedAs("sightBlockers")] LayerMask _sightBlockers = 1; // Default: level geometry (and the player, who counts as visible)
        [Tooltip("Off: run straight at the player without taking a ring slot (latching spirits).")]
        [SerializeField] bool _useSlotRing = true;

        [Header("Orbit")]
        [Tooltip("> 0: circle the player at this radius instead of closing in (hounds waiting for an opening).")]
        [SerializeField] float _orbitRadius = 0f;
        [Tooltip("Degrees per second around the player.")]
        [SerializeField] float _orbitSpeed = 60f;

        public Transform TargetComponent { get; private set; }
        public float StopDistance => _stopDistance;

        // Attack windups and knockback pause the chase through this.
        public bool Halted { get; set; }

        public float DistanceToTarget
        {
            get
            {
                if (!TargetComponent) return float.MaxValue;
                Vector3 offset = TargetComponent.position - transform.position;
                offset.y = 0f;
                return offset.magnitude;
            }
        }

        public bool InRange => DistanceToTarget <= _stopDistance + 0.15f;

        NavMeshAgent _navMeshAgentComponent;
        EnemySlotRing _slotRingComponent;
        float _repathTimer;
        float _orbitSign;

        void Awake()
        {
            _navMeshAgentComponent = GetComponent<NavMeshAgent>();
            _navMeshAgentComponent.stoppingDistance = _stopDistance;
            _navMeshAgentComponent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            _navMeshAgentComponent.avoidancePriority = Random.Range(30, 70);
            _orbitSign = Random.value < 0.5f ? -1f : 1f;
        }

        void Start()
        {
            var player = GameObject.FindWithTag("Player");
            if (!player) return;
            TargetComponent = player.transform;
            if (_keepDistance > 0f || _orbitRadius > 0f)
            {
                _navMeshAgentComponent.stoppingDistance = 0.3f;
                return;
            }
            if (!_useSlotRing) return;
            _slotRingComponent = player.GetComponent<EnemySlotRing>();
            // With a ring the destination is the slot itself, so walk all the way onto it.
            if (_slotRingComponent) _navMeshAgentComponent.stoppingDistance = 0.1f;
        }

        void OnDisable() => ReleaseSlot();

        // Lets a teleporting enemy pick the slot nearest to where it reappears.
        public void ReleaseSlot()
        {
            if (!_slotRingComponent) return;
            _slotRingComponent.Release(this);
        }

        void Update()
        {
            if (!TargetComponent || !_navMeshAgentComponent.enabled || !_navMeshAgentComponent.isOnNavMesh) return;

            _navMeshAgentComponent.isStopped = Halted;
            if (Halted) return;

            _repathTimer -= Time.deltaTime;
            if (_repathTimer <= 0f)
            {
                _repathTimer = _repathInterval;
                _navMeshAgentComponent.SetDestination(PickDestination());
            }

            bool arrived = !_navMeshAgentComponent.pathPending && _navMeshAgentComponent.remainingDistance <= _navMeshAgentComponent.stoppingDistance + 0.2f;
            if (InRange || arrived) FaceTarget();
        }

        Vector3 PickDestination()
        {
            if (_orbitRadius > 0f) return OrbitPoint();
            if (_keepDistance <= 0f) return _slotRingComponent ? _slotRingComponent.GetDestination(this) : TargetComponent.position;

            // No clear shot: close in until there is one.
            if (!HasLineOfSight()) return TargetComponent.position;

            Vector3 away = transform.position - TargetComponent.position;
            away.y = 0f;
            if (away.sqrMagnitude < 0.01f) away = Vector3.forward;
            Vector3 spot = TargetComponent.position + away.normalized * _keepDistance;
            return NavMesh.SamplePosition(spot, out NavMeshHit hit, 2f, NavMesh.AllAreas) ? hit.position : TargetComponent.position;
        }

        // A point a little ahead along the circle, so the agent keeps moving around the player.
        Vector3 OrbitPoint()
        {
            Vector3 away = transform.position - TargetComponent.position;
            away.y = 0f;
            if (away.sqrMagnitude < 0.01f) away = Vector3.forward;

            float step = _orbitSpeed * _repathInterval * 3f * _orbitSign;
            Vector3 spot = TargetComponent.position + Quaternion.Euler(0f, step, 0f) * away.normalized * _orbitRadius;
            if (NavMesh.SamplePosition(spot, out NavMeshHit hit, 1.5f, NavMesh.AllAreas)) return hit.position;

            // Blocked by a wall: go around the other way.
            _orbitSign = -_orbitSign;
            return transform.position;
        }

        public bool HasLineOfSight()
        {
            if (!TargetComponent) return false;
            Vector3 from = transform.position + Vector3.up;
            Vector3 to = TargetComponent.position + Vector3.up;
            if (!Physics.Linecast(from, to, out RaycastHit hit, _sightBlockers, QueryTriggerInteraction.Ignore)) return true;
            return hit.transform == TargetComponent || hit.transform.IsChildOf(TargetComponent);
        }

        public void FaceTarget()
        {
            if (!TargetComponent) return;
            Vector3 direction = TargetComponent.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f) return;
            Quaternion look = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, look, _turnSpeed * Time.deltaTime);
        }
    }
}
