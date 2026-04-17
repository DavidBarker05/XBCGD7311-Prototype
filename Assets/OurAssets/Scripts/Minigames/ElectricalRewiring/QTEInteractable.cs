using System.Security.Cryptography;
using UnityEngine;

public class QTEInteractable : Interactable
{
    public QTEManager qteManager;
    private bool hasTriggered = false;
	private Player player;

	public QTEPlayerCharacter QTEPlayer { get; private set; }

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
		if (inputParameters.Length != 2)
		{
#if UNITY_EDITOR
			Debug.LogWarning($"WARNING: QTEInteractable objects needs 1 input parameter. Received {inputParameters.Length} input parameters");
#endif
		}
		else
		{
			if (inputParameters[0] is Player player && inputParameters[1] is QTEPlayerCharacter qtePlayer)
			{
				if (!hasTriggered)
				{
					hasTriggered = true;
					this.player = player;
					QTEPlayer = qtePlayer;
					this.player.ChangeActionMap("QTEPlayer");
					qteManager.StartQTE(this);
				}
			}
			else
			{
#if UNITY_EDITOR
				Debug.LogWarning($"WARNING: Input parameter 0 needs to be a QTEPlayerCharacter. Received {inputParameters[0]} type {inputParameters[0].GetType()} as input parameter 0");
#endif
			}
		}
		// David - Return nothing for now, if you want the qtePlayer to receive information
		// then output an array of objects instead
		return null; 
    }

    public void OnQTESuccess()
    {
        Debug.Log("SUCCESS - Objective completed");
		player.ChangeActionMap("Player");
		player = null;
		QTEPlayer = null;
        gameObject.SetActive(false);
    }

    public void OnQTEFailure()
    {
        Debug.Log("FAILURE - Try again");
		player.ChangeActionMap("Player");
		player = null;
		QTEPlayer = null;
        hasTriggered = false;
    }
}
