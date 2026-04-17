using UnityEngine;
using UnityEngine.AI;

public class ChasePlayer : MonoBehaviour
{
    public NavMeshAgent enemy;
    public Transform player;

	private Vector3 startPosition;
	private Quaternion startRotation;

	private void Awake()
	{
		startPosition = transform.position;
		startRotation = transform.rotation;
	}

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
        enemy = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
		if (!ChaseMinigameStarter.Instance.ChaseMinigameIsRunning)
		{
			transform.SetPositionAndRotation(startPosition, startRotation);
			return;
		}
        if (player != null && !enemy.pathPending && Vector3.Distance(transform.position, player.position) > enemy.stoppingDistance)
        {
            enemy.SetDestination(player.position);
        }
        
    }

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player")) ChaseMinigameStarter.Instance.StartChaseMinigame();
	}

	public void ResetToStart()
	{
		transform.SetPositionAndRotation(startPosition, startRotation);
	}
}
