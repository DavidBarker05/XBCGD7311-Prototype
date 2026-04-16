using UnityEngine;

[System.Serializable]
public struct SerializableVectorTwoInt
{
	// Fields to be serialised
	public int x;
	public int y;
}

[System.Serializable]
public struct SerializablePipeSide
{
	// Fields to be serialised
	public string side;

	// Properties to read the fields as meaningful data, don't serialise
	public readonly PipeSide Side
	{
		get
		{
			string sideLower = side.ToLower();
			PipeSide pipeSide = sideLower switch
			{
				"left" => PipeSide.Left,
				"top" => PipeSide.Top,
				"right" => PipeSide.Right,
				"bottom" => PipeSide.Bottom,
				_ => throw new System.IO.InvalidDataException($"{side} is not a valid pipeSide")
			};
			return pipeSide;
		}
	}
}

[System.Serializable]
public struct SerializableStartEndPipe
{
	// Fields to be serialised
	public SerializableVectorTwoInt cellPosition;
	public SerializablePipeSide entranceExitSide;
}

[System.Serializable]
public struct PipeData
{
	// Fields to be serialised
	public string pipeType;
	public int pipeQuantity;
}

[System.Serializable]
public struct PipeGridData
{
	// Fields to be serialised
	public SerializableVectorTwoInt gridSize;
	public SerializableStartEndPipe startPipe;
	public SerializableStartEndPipe endPipe;

	public PipeData[] pipes;

	// Properties to read the fields as meaningful data, don't serialise
	public readonly Vector2Int GridSize => new Vector2Int(gridSize.x, gridSize.y);
	public readonly int StartX => startPipe.cellPosition.x;
	public readonly int StartY => startPipe.cellPosition.y;
	public readonly PipeSide EntranceSide => startPipe.entranceExitSide.Side;
	public readonly int EndX => endPipe.cellPosition.x;
	public readonly int EndY => endPipe.cellPosition.y;
	public readonly PipeSide ExitSide => endPipe.entranceExitSide.Side;
}
