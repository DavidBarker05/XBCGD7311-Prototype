using UnityEngine;

public class QTEManager : MonoBehaviour
{
    public Canvas canvas;
    public GameObject qtePrefab;
    private GameObject currentQTE;

    private QTEInteractable currentInteractable;

    public void StartQTE(QTEInteractable interactable)
    {
        currentInteractable = interactable;

        currentQTE = Instantiate(qtePrefab, canvas.transform);
        PointerController pointer = currentQTE.GetComponentInChildren<PointerController>();
        pointer.Begin(this);
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
