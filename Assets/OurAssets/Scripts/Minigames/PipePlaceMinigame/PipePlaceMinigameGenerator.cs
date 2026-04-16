using System.Collections.Generic;
using UnityEngine;

public class PipePlaceMinigameGenerator : MonoBehaviour
{
	// Only going to use pre-generated puzzles for now
	//[SerializeField]
	//bool m_UsePreGeneratedPuzzles = true;
	//[SerializeField]
	//bool m_UseProcedurallyGeneratedPuzzles = true;
	[SerializeField]
	PipeGrid m_PipeGrid;
	[SerializeField]
	Player m_Player;
	[SerializeField]
	PipePlayerCharacter m_PipePlayerCharacter;
	[SerializeField]
	List<TextAsset> m_PreGeneratedPuzzles;

	public void StartPipeMinigame()
	{
		int puzzleNumber = Random.Range(0, m_PreGeneratedPuzzles.Count);
		PipeGridData gridData = JsonUtility.FromJson<SerializablePipeGridData>(m_PreGeneratedPuzzles[puzzleNumber].text).Deserialized;
		foreach (PipeData pipe in gridData.Pipes)
		{
			m_PipePlayerCharacter.SetPipeQuantity(pipe.PipeType, pipe.PipeQuantity);
		}
		m_PipeGrid.StartMinigame(gridData);
		m_Player.ChangeActionMap("PipePlayer");
	}
}
