using UnityEngine;
using UnityEngine.AI;

public class ChasePlayer : MonoBehaviour
{
    private static readonly int RunningHash = Animator.StringToHash("Running");

    public Animator animator;
    public Transform Player { get; set; }
    private NavMeshAgent enemy;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
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

        if (Player != null && !enemy.pathPending && Vector3.Distance(transform.position, Player.position) > enemy.stoppingDistance)
        {
            enemy.SetDestination(Player.position);
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