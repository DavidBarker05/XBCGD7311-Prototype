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
#if UNITY_EDITOR
			string path = AssetDatabase.GetAssetPath(PipeType);
			string relative = path.Substring("Assets/Resources/".Length);
			string noExtension = Path.ChangeExtension(relative, null);
			return new SerializablePipeData() { pipeType = noExtension, pipeQuantity = PipeQuantity };
#else
			return new SerializablePipeData();
#endif
		}
	}
}

[System.Serializable]
public struct SerializablePipeGridData
{
	// Fields to be serialised
	public SerializableVectorTwoInt gridSize;
	public SerializableStartEndPipe[] startPipes;
	public SerializableStartEndPipe[] endPipes;
	public SerializablePipeData[] pipes;

	// Property to read the fields as meaningful data, don't serialise
	public readonly PipeGridData Deserialized
	{
		get
		{
			Vector2Int _gridSize = gridSize.Deserialized;
			StartEndPipe[] _startPipes = new StartEndPipe[startPipes.Length];
			for (int i = 0; i < _startPipes.Length; ++i)
			{
				_startPipes[i] = startPipes[i].Deserialized;
			}
			StartEndPipe[] _endPipes = new StartEndPipe[endPipes.Length];
			for (int i = 0; i < _endPipes.Length; ++i)
			{
				_endPipes[i] = endPipes[i].Deserialized;
			}
			PipeData[] _pipes = new PipeData[pipes.Length];
			for (int i = 0; i < _pipes.Length; ++i)
			{
				_pipes[i] = pipes[i].Deserialized;
			}
			return new PipeGridData() { GridSize = _gridSize, StartPipes = _startPipes, EndPipes = _endPipes, Pipes = _pipes };
		}
	}
}

[System.Serializable] // Serializable to show up in Unity, won't work with JSONs
public struct PipeGridData
{
	[field: SerializeField]
	public Vector2Int GridSize { get; set; }
	[field: SerializeField]
	public StartEndPipe[] StartPipes { get; set; }
	[field: SerializeField]
	public StartEndPipe[] EndPipes { get; set; }
	[field: SerializeField]
	public PipeData[] Pipes { get; set; }

	public SerializablePipeGridData Serialized
	{
		get
		{
			SerializableVectorTwoInt gridSize = new SerializableVectorTwoInt() { x = GridSize.x, y = GridSize.y };
			SerializableStartEndPipe[] startPipes = new SerializableStartEndPipe[StartPipes.Length];
			for (int i = 0; i < StartPipes.Length; ++i)
			{
				startPipes[i] = StartPipes[i].Serialized;
			}
			SerializableStartEndPipe[] endPipes = new SerializableStartEndPipe[EndPipes.Length];
			for (int i = 0; i < EndPipes.Length; ++i)
			{
				endPipes[i] = EndPipes[i].Serialized;
			}
			SerializablePipeData[] pipes = new SerializablePipeData[Pipes.Length];
			for (int i = 0; i < Pipes.Length; ++i)
			{
				pipes[i] = Pipes[i].Serialized;
			}
			return new SerializablePipeGridData() { gridSize = gridSize, startPipes = startPipes, endPipes = endPipes, pipes = pipes };
		}
	}
}
