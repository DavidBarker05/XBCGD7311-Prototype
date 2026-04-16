using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PipeSelectButton : MonoBehaviour
{
	[SerializeField]
	PipePlayerCharacter m_PipePlayer;
	[SerializeField]
	PipeSO m_Pipe;

	Button m_Button;
	TextMeshProUGUI m_ButtonText;

	uint m_PipeQuantity;

	void Awake()
	{
		m_Button = GetComponent<Button>();
		m_Button.onClick.AddListener(SelectPipe);
	}

	void OnDestroy() => m_Button.onClick.RemoveListener(SelectPipe);

    void Update()
    {
		m_PipeQuantity = m_PipePlayer.GetPipeQuantity(m_Pipe);
		m_ButtonText.text = $"{m_PipeQuantity}";
		m_Button.interactable = m_PipeQuantity > 0;
    }

	void SelectPipe() => m_PipePlayer.SelectPipe(m_Pipe);
}
