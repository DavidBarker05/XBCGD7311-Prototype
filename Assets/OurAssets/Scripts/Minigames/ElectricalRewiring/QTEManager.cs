using UnityEngine;

public class QTEManager : MonoBehaviour
{
    public Canvas canvas;
    public GameObject qtePrefab;
    public Player player;
    public QTEPlayerCharacter qteCharacter;
    private GameObject currentQTE;

    private QTEInteractable currentInteractable;

    [Header("Difficulty Scaling")]
    public float baseMoveSpeed = 150f;
    public float speedIncreasePerSuccess = 0.25f;
    private int successCount = 0;

    public void StartQTE(QTEInteractable interactable)
    {
        currentInteractable = interactable;

        currentQTE = Instantiate(qtePrefab, canvas.transform);
        PointerController pointer = currentQTE.GetComponentInChildren<PointerController>();
        player.ChangeCharacter(qteCharacter);

        float currentSpeed = baseMoveSpeed + (speedIncreasePerSuccess * successCount);
        pointer.Begin(this, qteCharacter, currentSpeed);

        Time.timeScale = 0f;
    }

    public void Success()
    {
        successCount++;
        currentInteractable.OnQTESuccess();
        EndQTE();
    }

    public void Falilure()
    {
        currentInteractable.OnQTEFailure();
        EndQTE();
    }

    void EndQTE()
    {
        Destroy(currentQTE);
        Time.timeScale = 1f;
    }
}