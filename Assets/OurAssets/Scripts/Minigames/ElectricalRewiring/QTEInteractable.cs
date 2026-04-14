using UnityEngine;

public class QTEInteractable : MonoBehaviour
{
    public QTEManager qteManager;

    private bool playerInRange = false;
    private bool hasTriggered = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!hasTriggered)
            {
                hasTriggered = true;
                qteManager.StartQTE(this);
            }
        }
    }

    public void OnQTESuccess()
    {
        Debug.Log("SUCCESS - Objective completed");
        gameObject.SetActive(false);
    }

    public void OnQTEFailure()
    {
        Debug.Log("FAILURE - Try again");
        hasTriggered = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
