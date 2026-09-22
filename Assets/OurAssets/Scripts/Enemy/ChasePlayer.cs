using UnityEngine;
using UnityEngine.AI;

public class ChasePlayer : MonoBehaviour
{
    private static readonly int RunningHash = Animator.StringToHash("Running");

    public NavMeshAgent enemy;
    public Animator animator;
    public Transform player;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    void Start()
    {
        enemy = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (!ChaseMinigameStarter.Instance.ChaseMinigameIsRunning)
        {
            transform.SetPositionAndRotation(startPosition, startRotation);
            animator.SetBool(RunningHash, false);
            return;
        }

        if (player != null && !enemy.pathPending && Vector3.Distance(transform.position, player.position) > enemy.stoppingDistance)
        {
            enemy.SetDestination(player.position);
            animator.SetBool(RunningHash, true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) ChaseMinigameStarter.Instance.RestartChaseMinigame();
    }

    public void ResetToStart()
    {
        transform.SetPositionAndRotation(startPosition, startRotation);
    }
}