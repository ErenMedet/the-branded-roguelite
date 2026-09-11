using UnityEngine;
using UnityEngine.AI;

namespace Branded.Enemies
{
    // Follows the player on the NavMesh. Walks to a reserved slot around the player (EnemySlotRing)
    // so groups surround the player instead of lining up behind each other.
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyChaser : MonoBehaviour
    {
        [SerializeField] float repathInterval = 0.2f;
        [SerializeField] float stopDistance = 1.6f;
        [SerializeField] float turnSpeed = 540f;

        public Transform Target { get; private set; }
        public float StopDistance => stopDistance;

        // Attack windups and knockback pause the chase through this.
        public bool Halted { get; set; }

        public bool InRange
        {
            get
            {
                if (!Target) return false;
                Vector3 offset = Target.position - transform.position;
                offset.y = 0f;
                return offset.sqrMagnitude <= (stopDistance + 0.15f) * (stopDistance + 0.15f);
            }
        }

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
            _ring = player.GetComponent<EnemySlotRing>();
            // With a ring the destination is the slot itself, so walk all the way onto it.
            if (_ring) _agent.stoppingDistance = 0.1f;
        }

        void OnDisable()
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
                _agent.SetDestination(_ring ? _ring.GetDestination(this) : Target.position);
            }

            bool arrived = !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance + 0.2f;
            if (InRange || arrived) FaceTarget();
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
