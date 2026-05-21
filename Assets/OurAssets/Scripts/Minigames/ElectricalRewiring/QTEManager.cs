using UnityEngine;

public class QTEManager : MonoBehaviour
{
    public Canvas canvas;
    public GameObject qtePrefab;
    public Player player;
    public QTEPlayerCharacter qteCharacter;
    private GameObject currentQTE;

    private QTEInteractable currentInteractable;

    public void StartQTE(QTEInteractable interactable)
    {
        currentInteractable = interactable;

        currentQTE = Instantiate(qtePrefab, canvas.transform);
        PointerController pointer = currentQTE.GetComponentInChildren<PointerController>();
        player.ChangeCharacter(qteCharacter);
        pointer.Begin(this, qteCharacter);
        Time.timeScale = 0f;
    }

    public void Success()
    {
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
