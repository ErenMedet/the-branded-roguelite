using UnityEngine;
using UnityEngine.AI;

namespace Branded.Enemies
{
    // Follows the player on the NavMesh. Melee enemies walk to a reserved slot around the player
    // (EnemySlotRing) so groups surround the player instead of lining up behind each other.
    // Ranged enemies set keepDistance and hold that range instead.
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyChaser : MonoBehaviour
    {
        [SerializeField] float repathInterval = 0.2f;
        [SerializeField] float stopDistance = 1.6f;
        [SerializeField] float turnSpeed = 540f;
        [Tooltip("> 0: hold this distance from the player instead of taking a ring slot.")]
        [SerializeField] float keepDistance = 0f;
        [SerializeField] LayerMask sightBlockers = 1; // Default: level geometry (and the player, who counts as visible)

        public Transform Target { get; private set; }
        public float StopDistance => stopDistance;

        // Attack windups and knockback pause the chase through this.
        public bool Halted { get; set; }

        public float DistanceToTarget
        {
            get
            {
                if (!Target) return float.MaxValue;
                Vector3 offset = Target.position - transform.position;
                offset.y = 0f;
                return offset.magnitude;
            }
        }

        public bool InRange => DistanceToTarget <= stopDistance + 0.15f;

        NavMeshAgent _agent;
        EnemySlotRing _ring;
        float _repathTimer;

        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.stoppingDistance = stopDistance;
            _agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            _agent.avoidancePriority = Random.Range(30, 70);
        }

        void Start()
        {
            var player = GameObject.FindWithTag("Player");
            if (!player) return;
            Target = player.transform;
            if (keepDistance > 0f)
            {
                _agent.stoppingDistance = 0.3f;
                return;
            }
            _ring = player.GetComponent<EnemySlotRing>();
            // With a ring the destination is the slot itself, so walk all the way onto it.
            if (_ring) _agent.stoppingDistance = 0.1f;
        }

        void OnDisable() => ReleaseSlot();

        // Lets a teleporting enemy pick the slot nearest to where it reappears.
        public void ReleaseSlot()
        {
            if (_ring) _ring.Release(this);
        }

        void Update()
        {
            if (!Target || !_agent.enabled || !_agent.isOnNavMesh) return;

            _agent.isStopped = Halted;
            if (Halted) return;

            _repathTimer -= Time.deltaTime;
            if (_repathTimer <= 0f)
            {
                _repathTimer = repathInterval;
                _agent.SetDestination(PickDestination());
            }

            bool arrived = !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance + 0.2f;
            if (InRange || arrived) FaceTarget();
        }

        Vector3 PickDestination()
        {
            if (keepDistance <= 0f) return _ring ? _ring.GetDestination(this) : Target.position;

            // No clear shot: close in until there is one.
            if (!HasLineOfSight()) return Target.position;

            Vector3 away = transform.position - Target.position;
            away.y = 0f;
            if (away.sqrMagnitude < 0.01f) away = Vector3.forward;
            Vector3 spot = Target.position + away.normalized * keepDistance;
            return NavMesh.SamplePosition(spot, out NavMeshHit hit, 2f, NavMesh.AllAreas) ? hit.position : Target.position;
        }

        public bool HasLineOfSight()
        {
            if (!Target) return false;
            Vector3 from = transform.position + Vector3.up;
            Vector3 to = Target.position + Vector3.up;
            if (!Physics.Linecast(from, to, out RaycastHit hit, sightBlockers, QueryTriggerInteraction.Ignore)) return true;
            return hit.transform == Target || hit.transform.IsChildOf(Target);
        }

        public void FaceTarget()
        {
            if (!Target) return;
            Vector3 direction = Target.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f) return;
            Quaternion look = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, look, turnSpeed * Time.deltaTime);
        }
    }
}
