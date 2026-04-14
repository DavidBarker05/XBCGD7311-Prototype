using UnityEngine;

public class QTEInteractable : Interactable
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
        //if (playerInRange && Input.GetKeyDown(KeyCode.E))
        //{
        //    
        //}
    }

    public override object[] Interact(params object[] inputParameters)
    {
        if (!hasTriggered)
        {
            if (!hasTriggered)
            {
                hasTriggered = true;
                qteManager.StartQTE(this);
            }
        }
        // David - Return nothing for now, if you want the player to receive information
        // then output an array of objects instead
        return null; 
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
