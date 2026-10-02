using UnityEngine;
using UnityEngine.AI;
using RetwineMake.Combat;
using RetwineMake.Player;

namespace RetwineMake.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class GuardAI : MonoBehaviour
    {
        enum State { Patrol, Chase, Attack }

        [Header("Patrol")]
        [SerializeField] Transform[] patrolPoints;
        [SerializeField] float patrolSpeed = 2f;
        [SerializeField] float waypointTolerance = 0.5f;
        [SerializeField] float waypointPause = 1f;

        [Header("Detection")]
        [SerializeField] float viewDistance = 12f;
        [SerializeField] float viewAngle = 70f;
        [SerializeField] float eyeHeight = 1.6f;
        [SerializeField] LayerMask obstructionMask = ~0;
        [SerializeField] float loseSightTime = 3f;

        [Header("Combat")]
        [SerializeField] float attackRange = 8f;
        [SerializeField] float chaseSpeed = 4f;
        [SerializeField] float fireCooldown = 1f;
        [SerializeField] float damage = 10f;
        [SerializeField, Range(0f, 1f)] float accuracy = 0.75f;

        NavMeshAgent agent;
        Health health;
        GuardWeapon weapon;
        Transform player;
        State state = State.Patrol;
        int currentPoint;
        float waypointWaitUntil;
        float lastSeenTime = -999f;
        float nextFireTime;
        bool isDead;

        public string CurrentStateName => state.ToString();

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            health = GetComponent<Health>();
            weapon = GetComponentInChildren<GuardWeapon>();

            var playerObj = GameObject.Find("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        void OnEnable()
        {
            if (health != null)
                health.OnDeath += HandleDeath;
        }

        void OnDisable()
        {
            if (health != null)
                health.OnDeath -= HandleDeath;
        }

        void Start()
        {
            EnterPatrol();
        }

        void Update()
        {
            if (isDead || player == null)
                return;

            bool canSeePlayer = CanSeePlayer();
            if (canSeePlayer)
                lastSeenTime = Time.time;

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            switch (state)
            {
                case State.Patrol:
                    Patrol();
                    if (canSeePlayer)
                        EnterChase();
                    break;

                case State.Chase:
                    agent.SetDestination(player.position);
                    if (canSeePlayer && distanceToPlayer <= attackRange)
                        EnterAttack();
                    else if (Time.time - lastSeenTime > loseSightTime)
                        EnterPatrol();
                    break;

                case State.Attack:
                    FaceTarget();
                    if (!canSeePlayer || distanceToPlayer > attackRange * 1.2f)
                        EnterChase();
                    else if (Time.time >= nextFireTime)
                        FireAtPlayer();

                    if (Time.time - lastSeenTime > loseSightTime)
                        EnterPatrol();
                    break;
            }
        }

        bool CanSeePlayer()
        {
            Vector3 eyePos = transform.position + Vector3.up * eyeHeight + transform.forward * 0.5f;
            Vector3 toPlayer = player.position - eyePos;
            float distance = toPlayer.magnitude;

            if (distance > viewDistance)
                return false;

            float angle = Vector3.Angle(transform.forward, toPlayer);
            if (angle > viewAngle * 0.5f)
                return false;

            if (Physics.Raycast(eyePos, toPlayer.normalized, out var hit, distance, obstructionMask, QueryTriggerInteraction.Ignore))
                return hit.transform == player || hit.transform.IsChildOf(player);

            return true;
        }

        void Patrol()
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
                return;

            if (Time.time < waypointWaitUntil)
                return;

            if (!agent.pathPending && agent.remainingDistance < waypointTolerance)
            {
                currentPoint = (currentPoint + 1) % patrolPoints.Length;
                waypointWaitUntil = Time.time + waypointPause;
                agent.SetDestination(patrolPoints[currentPoint].position);
            }
        }

        void FaceTarget()
        {
            Vector3 direction = player.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.01f)
                return;

            var targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
        }

        void FireAtPlayer()
        {
            nextFireTime = Time.time + fireCooldown;
            weapon?.PlayFireAnimation();

            if (Random.value <= accuracy)
            {
                var damageable = player.GetComponentInParent<IDamageable>();
                damageable?.ApplyDamage(damage, player.position, Vector3.up);
                Debug.Log($"[GuardAI] {name} hit the player for {damage}.");
            }
            else
            {
                Debug.Log($"[GuardAI] {name} missed.");
            }
        }

        void EnterPatrol()
        {
            state = State.Patrol;
            agent.isStopped = false;
            agent.speed = patrolSpeed;
            agent.autoBraking = false;

            if (patrolPoints != null && patrolPoints.Length > 0)
                agent.SetDestination(patrolPoints[currentPoint].position);
        }

        void EnterChase()
        {
            state = State.Chase;
            agent.isStopped = false;
            agent.speed = chaseSpeed;
        }

        void EnterAttack()
        {
            state = State.Attack;
            agent.isStopped = true;
        }

        void HandleDeath()
        {
            isDead = true;
            agent.isStopped = true;
            enabled = false;
        }
    }
}
