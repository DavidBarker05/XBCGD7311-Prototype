using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(TMP_Text))]
public class HyperlinkHandler : MonoBehaviour, IPointerClickHandler
{
    TMP_Text m_TextMeshPro;

    void Awake() => m_TextMeshPro = GetComponent<TMP_Text>();

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 pointerPosition = Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero;
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(m_TextMeshPro, pointerPosition, null);
        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = m_TextMeshPro.textInfo.linkInfo[linkIndex];
            Application.OpenURL(linkInfo.GetLinkID());
        }
    }
}
