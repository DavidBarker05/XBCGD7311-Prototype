using System.IO;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public struct SerializableVectorTwoInt
{
	// Fields to be serialised
	public int x;
	public int y;

	// Property to read the fields as meaningful data, don't serialise
	public readonly Vector2Int Deserialized => new Vector2Int(x, y);
}

[System.Serializable]
public struct SerializableStartEndPipe
{
	// Fields to be serialised
	public SerializableVectorTwoInt cellPosition;
	public int entranceExitSide;

	// Property to read the fields as meaningful data, don't serialise
	public readonly StartEndPipe Deserialized => new StartEndPipe() { CellPosition = cellPosition.Deserialized, EntranceExitSide = (PipeSide)entranceExitSide };
}

[System.Serializable] // Serializable to show up in Unity, won't work with JSONs
public struct StartEndPipe
{
	[field: SerializeField]
	public Vector2Int CellPosition { get; set; }
	[field: SerializeField]
	public PipeSide EntranceExitSide { get; set; }

	public readonly SerializableStartEndPipe Serialized => new SerializableStartEndPipe() { cellPosition = new SerializableVectorTwoInt() { x = CellPosition.x, y = CellPosition.y }, entranceExitSide = (int)EntranceExitSide };
}

[System.Serializable]
public struct SerializablePipeData
{
	// Fields to be serialised
	public string pipeType;
	public uint pipeQuantity;

	// Property to read the fields as meaningful data, don't serialise
	public PipeData Deserialized => new PipeData() { PipeType = Resources.Load<PipeSO>(pipeType), PipeQuantity = pipeQuantity };
}

[System.Serializable] // Serializable to show up in Unity, won't work with JSONs
public struct PipeData
{
	[field: SerializeField]
	public PipeSO PipeType { get; set; }
	[field: SerializeField]
	public uint PipeQuantity { get; set; }

	public SerializablePipeData Serialized
	{
		get
		{
			string path = AssetDatabase.GetAssetPath(PipeType);
			string relative = path.Substring("Assets/Resources/".Length);
			string noExtension = Path.ChangeExtension(relative, null);
			return new SerializablePipeData() { pipeType = noExtension, pipeQuantity = PipeQuantity };
		}
	}
}

[System.Serializable]
public struct SerializablePipeGridData
{
	// Fields to be serialised
	public SerializableVectorTwoInt gridSize;
	public SerializableStartEndPipe startPipe;
	public SerializableStartEndPipe endPipe;
	public SerializablePipeData[] pipes;

	// Property to read the fields as meaningful data, don't serialise
	public readonly PipeGridData Deserialized
	{
		get
		{
			Vector2Int _gridSize = gridSize.Deserialized;
			StartEndPipe _startPipe = startPipe.Deserialized;
			StartEndPipe _endPipe = endPipe.Deserialized;
			PipeData[] _pipes = new PipeData[pipes.Length];
			for (int i = 0; i < _pipes.Length; ++i)
			{
				_pipes[i] = pipes[i].Deserialized;
			}
			return new PipeGridData() { GridSize = _gridSize, StartPipe = _startPipe, EndPipe = _endPipe, Pipes = _pipes };
		}
	}
}

[System.Serializable] // Serializable to show up in Unity, won't work with JSONs
public struct PipeGridData
{
	[field: SerializeField]
	public Vector2Int GridSize { get; set; }
	[field: SerializeField]
	public StartEndPipe StartPipe { get; set; }
	[field: SerializeField]
	public StartEndPipe EndPipe { get; set; }
	[field: SerializeField]
	public PipeData[] Pipes { get; set; }

	public SerializablePipeGridData Serialized
	{
		get
		{
			SerializableVectorTwoInt gridSize = new SerializableVectorTwoInt() { x = GridSize.x, y = GridSize.y };
			SerializableStartEndPipe startPipe = StartPipe.Serialized;
			SerializableStartEndPipe endPipe = EndPipe.Serialized;
			SerializablePipeData[] pipes = new SerializablePipeData[Pipes.Length];
			for (int i = 0; i < Pipes.Length; ++i)
			{
				pipes[i] = Pipes[i].Serialized;
			}
			return new SerializablePipeGridData() { gridSize = gridSize, startPipe = startPipe, endPipe = endPipe, pipes = pipes };
		}
	}
}
