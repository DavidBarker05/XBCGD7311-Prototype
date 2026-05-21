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
	[SerializeField]
	TextMeshProUGUI m_ButtonText;
	[SerializeField]
	GameObject m_SelectedIndicator;

	Button m_Button;

	void Awake()
	{
		m_Button = GetComponent<Button>();
		m_Button.onClick.AddListener(SelectPipe);
	}

	void Update()
	{
		uint pipeQuantity = m_PipePlayer.GetPipeQuantity(m_Pipe);
		m_ButtonText.text = $"{pipeQuantity}";
		m_Button.interactable = pipeQuantity > 0;
		m_SelectedIndicator.SetActive(m_PipePlayer.CurrentlySelectedPipe == m_Pipe);
	}

	void SelectPipe() => m_PipePlayer.SelectPipe(m_Pipe);
}
