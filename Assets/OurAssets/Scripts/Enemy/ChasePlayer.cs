using UnityEngine;
using UnityEngine.AI;

public class ChasePlayer : MonoBehaviour
{
    public NavMeshAgent enemy;
    public Transform player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemy = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if(player != null)
        {
            enemy.SetDestination(player.position);
        }
        
    }
}
