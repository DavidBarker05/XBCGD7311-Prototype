using System.Collections.Generic;
using UnityEngine;

public class PipePlaceMinigameGenerator : MonoBehaviour
{
	[SerializeField]
	PipeGrid m_PipeGrid;
	[SerializeField]
	Player m_Player;
	[SerializeField]
	PipePlayerCharacter m_PipePlayerCharacter;
	[SerializeField]
	List<TextAsset> m_PreGeneratedPuzzles;

	[Header("Procedural Puzzles")]
	[SerializeField, Range(0f, 1f)]
	float m_ProceduralPuzzleChance = 0.5f;
	[SerializeField, Min(3)]
	int m_ProceduralGridSizeMin = 6;
	[SerializeField, Min(3)]
	int m_ProceduralGridSizeMax = 8;
	[SerializeField, Min(0)]
	int m_ProceduralExtraPiecesMin = 0;
	[SerializeField, Min(0)]
	int m_ProceduralExtraPiecesMax = 2;

	public void StartPipeMinigame(float wallKnockSpeedMultiplier = 1f)
	{
		m_Player.ChangeCharacter(m_PipePlayerCharacter);
		bool bUseProcedural = m_PreGeneratedPuzzles.Count == 0 || Random.value < m_ProceduralPuzzleChance;
		PipeGridData gridData = bUseProcedural ? GenerateProceduralPuzzle() : LoadPreGeneratedPuzzle();
		m_PipePlayerCharacter.ClearPipeQuantities();
		foreach (PipeData pipe in gridData.Pipes)
		{
			m_PipePlayerCharacter.SetPipeQuantity(pipe.PipeType, pipe.PipeQuantity);
		}
		m_PipeGrid.StartMinigame(gridData, wallKnockSpeedMultiplier);
	}

	PipeGridData LoadPreGeneratedPuzzle()
	{
		int puzzleNumber = Random.Range(0, m_PreGeneratedPuzzles.Count);
		return JsonUtility.FromJson<SerializablePipeGridData>(m_PreGeneratedPuzzles[puzzleNumber].text).Deserialized;
	}

	PipeGridData GenerateProceduralPuzzle()
	{
		int minSteps = Mathf.CeilToInt(m_ProceduralGridSizeMin / 2f);
		int maxSteps = Mathf.FloorToInt(m_ProceduralGridSizeMax / 2f);
		int size = Random.Range(minSteps, maxSteps + 1) * 2;
		Vector2Int gridSize = new Vector2Int(size, size);
		int numStartPipes = Random.Range(1, 3);
		int numEndPipes = Random.Range(1, 3);
		return ProceduralPipeGridGenerator.Generate(gridSize, numStartPipes, numEndPipes, m_ProceduralExtraPiecesMin, m_ProceduralExtraPiecesMax);
	}
}
