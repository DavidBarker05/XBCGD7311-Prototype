using System.Collections.Generic;
using UnityEngine;
using Util.ArrayUtils;
using Util.SystemUtils;

[RequireComponent(typeof(PlaneGridGenerator), typeof(Grid))]
public class PipeGrid : MonoBehaviour
{
    [SerializeField]
    Transform m_UnscaledTransform;
    [SerializeField]
    Pipe m_PipePrefab;
    [SerializeField]
    PipeSO m_EmptyPipe;
    [SerializeField]
    PipeSO m_OutsidePipe;
    [SerializeField]
    GameObject m_PipeUI;
    [SerializeField]
    Player m_Player;
    [SerializeField]
    FirstPersonPlayerCharacter m_FirstPersonPlayerCharacter;
    [SerializeField]
    PipePlayerCharacter m_PipePlayerCharacter;
    [Header("Money Reward")]
    [SerializeField, Min(0f)]
    float m_MinMoneyReward = 40f;
    [SerializeField, Min(0f)]
    float m_MaxMoneyReward = 80f;

    float m_SpeedMultiplier = 1f;

    PlaneGridGenerator m_PlaneGrid;
    Vector2Int Size => m_PlaneGrid.GridSize;
    Grid m_Grid;

    Pipe[,] m_PipeCells;

    public struct ResolvedStartEndPipe
    {
        public Pipe PipeCell;
        public Vector2Int ArrayIndex;
        public PipeSide EntranceExitSide;

        public readonly PipeRotationAngle HoleSideToOutsidePipeAngle => EntranceExitSide switch
        {
            PipeSide.Left => PipeRotationAngle.OneEighty,
            PipeSide.Top => PipeRotationAngle.TwoSeventy,
            PipeSide.Right => PipeRotationAngle.Zero,
            PipeSide.Bottom => PipeRotationAngle.Ninety,
            _ => throw new System.NotImplementedException()
        };

        public readonly Vector3Int HoleSideToOutsidePipePos => EntranceExitSide switch
        {
            PipeSide.Left => new Vector3Int(-1, 0, 0),
            PipeSide.Top => new Vector3Int(0, 0, 1),
            PipeSide.Right => new Vector3Int(1, 0, 0),
            PipeSide.Bottom => new Vector3Int(0, 0, -1),
            _ => throw new System.NotImplementedException()
        };
    }
    ResolvedStartEndPipe[] m_StartPipes;
    Pipe[] m_StartOutsidePipes;
    ResolvedStartEndPipe[] m_EndPipes;
    Pipe[] m_EndOutsidePipes;

    #region Delete & Init
    void DeletePipes(ref Pipe[,] pipeCells)
    {
        for (int x = pipeCells.GetLength(0) - 1; x >= 0; --x)
        {
            for (int y = pipeCells.GetLength(1) - 1; y >= 0; --y)
            {
                if (pipeCells[x, y]) Destroy(pipeCells[x, y].gameObject);
            }
        }
        pipeCells = null;
        DeleteOutsidePipes(ref m_StartOutsidePipes);
        DeleteOutsidePipes(ref m_EndOutsidePipes);
    }

    void DeleteOutsidePipes(ref Pipe[] outsidePipes)
    {
        if (outsidePipes == null) return;
        foreach (Pipe outsidePipe in outsidePipes) if (outsidePipe) Destroy(outsidePipe.gameObject);
        outsidePipes = null;
    }

    void InitCells(ref Pipe[,] pipeCells, ref Grid grid, Vector2Int size)
    {
        if (pipeCells != null && pipeCells.Length > 0) DeletePipes(ref pipeCells);
        pipeCells = new Pipe[size.x, size.y];
        for (int x = 0; x < size.x; ++x)
        {
            for (int y = 0; y < size.y; ++y)
            {
                GameObject go = Instantiate(m_PipePrefab.gameObject, m_UnscaledTransform);
                Vector3Int cp = ArrayIndex2DToCellPosition(x, y);
                Vector3 wp = grid.CellToWorld(cp);
                go.transform.position = wp;
                Pipe pipe = go.GetComponent<Pipe>();
                pipe.CurrentPipeSO = m_EmptyPipe;
                pipe.CurrentPipeAngle = PipeRotationAngle.Zero;
                m_PipeCells[x, y] = pipe;
            }
        }
    }

    Pipe[] SpawnOutsidePipes(int count)
    {
        Pipe[] outsidePipes = new Pipe[count];
        for (int i = 0; i < count; ++i)
        {
            GameObject go = Instantiate(m_PipePrefab.gameObject, m_UnscaledTransform);
            Pipe outsidePipe = go.GetComponent<Pipe>();
            outsidePipe.CurrentPipeSO = m_OutsidePipe;
            outsidePipes[i] = outsidePipe;
        }
        return outsidePipes;
    }
    #endregion Delete & Init

    #region Start & End Minigame
    ResolvedStartEndPipe[] SetUpStartEndPipes(StartEndPipe[] pipeDatas, out Pipe[] outsidePipes)
    {
        outsidePipes = SpawnOutsidePipes(pipeDatas.Length);
        ResolvedStartEndPipe[] startEndPipes = new ResolvedStartEndPipe[pipeDatas.Length];
        for (int i = 0; i < pipeDatas.Length; ++i)
        {
            Pipe pipeCell = GetPipe(pipeDatas[i].CellPosition.x, pipeDatas[i].CellPosition.y);
            Sys.Assert(pipeCell, $"({pipeDatas[i].CellPosition}) was not a valid index");
            ResolvedStartEndPipe startEndPipe = new ResolvedStartEndPipe() { PipeCell = pipeCell, ArrayIndex = pipeDatas[i].CellPosition, EntranceExitSide = pipeDatas[i].EntranceExitSide };
            startEndPipes[i] = startEndPipe;
            Vector3Int outsidePosCP = ArrayIndex2DToCellPosition(pipeDatas[i].CellPosition.x, pipeDatas[i].CellPosition.y) + startEndPipe.HoleSideToOutsidePipePos;
            outsidePipes[i].transform.position = m_Grid.CellToWorld(outsidePosCP);
            outsidePipes[i].CurrentPipeAngle = startEndPipe.HoleSideToOutsidePipeAngle;
        }
        return startEndPipes;
    }

    public void StartMinigame(PipeGridData pipeGridData, float wallKnockSpeedMultiplier = 1f)
    {
        if (!m_PlaneGrid) m_PlaneGrid = GetComponent<PlaneGridGenerator>();
        if (!m_Grid) m_Grid = GetComponent<Grid>();
        m_PlaneGrid.GridSize = pipeGridData.GridSize;
        m_SpeedMultiplier = wallKnockSpeedMultiplier;
        m_UnscaledTransform.gameObject.SetActive(true);
        if (m_PipeUI) m_PipeUI.SetActive(true);
        InitCells(ref m_PipeCells, ref m_Grid, Size);
        m_StartPipes = SetUpStartEndPipes(pipeGridData.StartPipes, out m_StartOutsidePipes);
        m_EndPipes = SetUpStartEndPipes(pipeGridData.EndPipes, out m_EndOutsidePipes);
    }

    void EndMinigame(List<Pipe> path) // path is in case we want to do some kind of flowing animation
    {
        if (m_PipeUI) m_PipeUI.SetActive(false);
        m_PipePlayerCharacter.DeleteCellIndicator();
        if (m_Player && m_FirstPersonPlayerCharacter) m_Player.ChangeCharacter(m_FirstPersonPlayerCharacter);
        AwardMoney();
        MinigameManager.Instance?.OnMinigameBeaten();
        HouseProgressTracker.ReportMinigameCompleted(MinigameType.WallKnockAndPipes);
        TutorialMinigameManager.Instance?.ReportMinigameCompleted(MinigameType.WallKnockAndPipes);
        DeletePipes(ref m_PipeCells);
        m_UnscaledTransform.gameObject.SetActive(false);
    }

    void AwardMoney()
    {
        if (TutorialMinigameManager.Instance) return;
        int totalOpenings = m_StartPipes.Length + m_EndPipes.Length;
        float sizeT = Mathf.InverseLerp(6f, 8f, (Size.x + Size.y) / 2f);
        float openingsT = Mathf.InverseLerp(2f, 4f, totalOpenings);
        float difficultyT = (sizeT + openingsT) / 2f;
        float baseMoney = Mathf.Lerp(m_MinMoneyReward, m_MaxMoneyReward, difficultyT);
        MinigameMoneyReward.Award(baseMoney, m_SpeedMultiplier);
    }
    #endregion Start & End Minigame

    #region GetIndex & GetPipe
    public (int x, int y) GetIndexOf(Pipe pipe)
    {
        int[] indices = m_PipeCells.MultiIndexOf(pipe);
        if (indices[0] >= 0 && indices[1] >= 0) return (indices[0], indices[1]);
        for (int x = 0; x < m_PipeCells.GetLength(0); ++x)
        {
            for (int y = 0; y < m_PipeCells.GetLength(1); ++y)
            {
                if (m_PipeCells[x, y] == pipe) return (x, y);
            }
        }
        return (-1, -1);
    }

    #region GetPipe
    public Pipe GetPipe(int x, int y) => m_PipeCells.ContainsIndex(x, y) ? m_PipeCells[x, y] : null;

    public Pipe GetPipe(Vector3Int cellPosition)
    {
        (int x, int y) = CellPositionToArrayIndex2D(cellPosition);
        return GetPipe(x, y);
    }
    #endregion GetPipe
    #endregion GetIndex & GetPipe

    #region CellPos & Index Conversion
    public int CellPosAxisToArrayIndex(int cellPosAxis, int gridSizeAxis) => cellPosAxis + Mathf.CeilToInt(gridSizeAxis / 2f);

    public int ArrayIndexToCellPosAxis(int index, int gridSizeAxis) => index - Mathf.CeilToInt(gridSizeAxis / 2f);

    public (int x, int y) CellPositionToArrayIndex2D(Vector3Int cellPosition)
    {
        int x = CellPosAxisToArrayIndex(cellPosition.x, Size.x),
            y = CellPosAxisToArrayIndex(cellPosition.z, Size.y);
        return (x, y);
    }

    public Vector3Int ArrayIndex2DToCellPosition(int x, int y)
    {
        Vector3Int pos = Vector3Int.zero;
        pos.x = ArrayIndexToCellPosAxis(x, Size.x);
        pos.z = ArrayIndexToCellPosAxis(y, Size.y);
        return pos;
    }
    #endregion CellPos & Index Conversion

    #region CellIsEmpty
    public bool CellIsEmpty(int x, int y)
    {
        if (x < m_PipeCells.GetLength(0) && y < m_PipeCells.GetLength(1))
            return m_PipeCells[x, y].CurrentPipeSO == m_EmptyPipe;
        throw new System.IndexOutOfRangeException();
    }

    public bool CellIsEmpty(Vector3Int cellPosition)
    {
        try
        {
            (int x, int y) = CellPositionToArrayIndex2D(cellPosition);
            return CellIsEmpty(x, y);
        }
        catch (System.Exception e) { throw e; }
    }
    #endregion CellIsEmpty

    #region Place & Remove Pipe
    #region Place Pipe
    public PipeSO PlacePipe(PipeSO pipeSO, int x, int y)
    {
        if (!pipeSO || pipeSO == m_EmptyPipe) return m_EmptyPipe;
        Pipe pipe = GetPipe(x, y);
        PipeSO originalPipeSO = pipe.CurrentPipeSO;
        pipe.CurrentPipeSO = pipeSO;
        pipe.CurrentPipeAngle = PipeRotationAngle.Zero;
        CheckWaterCanReachEnd();
        return originalPipeSO;
    }

    public PipeSO PlacePipe(PipeSO pipeSO, Vector3Int cellPosition)
    {
        (int x, int y) = CellPositionToArrayIndex2D(cellPosition);
        return PlacePipe(pipeSO, x, y);
    }
    #endregion Place Pipe

    #region Remove Pipe
    public PipeSO RemovePipe(int x, int y)
    {
        Pipe pipe = GetPipe(x, y);
        PipeSO originalPipeSO = pipe.CurrentPipeSO;
        pipe.CurrentPipeSO = m_EmptyPipe;
        pipe.CurrentPipeAngle = PipeRotationAngle.Zero;
        return originalPipeSO;
    }

    public PipeSO RemovePipe(Vector3Int cellPosition)
    {
        (int x, int y) = CellPositionToArrayIndex2D(cellPosition);
        return RemovePipe(x, y);
    }

    public PipeSO RemovePipe(Pipe pipe)
    {
        PipeSO originalPipeSO = pipe.CurrentPipeSO;
        pipe.CurrentPipeSO = m_EmptyPipe;
        pipe.CurrentPipeAngle = PipeRotationAngle.Zero;
        return originalPipeSO;
    }
    #endregion Remove Pipe
    #endregion Place & Remove Pipe

    #region Rotate Pipe
    #region Rotate Right
    public void RotatePipeRight(int x, int y)
    {
        if (CellIsEmpty(x, y)) return;
        Pipe pipe = GetPipe(x, y);
        pipe.RotateRight();
        CheckWaterCanReachEnd();
    }

    public void RotatePipeRight(Vector3Int cellPosition)
    {
        if (CellIsEmpty(cellPosition)) return;
        Pipe pipe = GetPipe(cellPosition);
        pipe.RotateRight();
        CheckWaterCanReachEnd();
    }
    #endregion Rotate Right

    #region Rotate Left
    public void RotatePipeLeft(int x, int y)
    {
        if (CellIsEmpty(x, y)) return;
        Pipe pipe = GetPipe(x, y);
        pipe.RotateLeft();
        CheckWaterCanReachEnd();
    }

    public void RotatePipeLeft(Vector3Int cellPosition)
    {
        if (CellIsEmpty(cellPosition)) return;
        Pipe pipe = GetPipe(cellPosition);
        pipe.RotateLeft();
        CheckWaterCanReachEnd();
    }
    #endregion Rotate Left
    #endregion Rotate Pipe

    #region IndexOfCellOnSide
    (int x, int y) IndexOfCellOnSide(PipeSide side, int x, int y) => side switch
    {
        PipeSide.Left => (x - 1, y),
        PipeSide.Top => (x, y + 1),
        PipeSide.Right => (x + 1, y),
        PipeSide.Bottom => (x, y - 1),
        _ => throw new System.ArgumentException("Somehow you input a side that doesn't exist")
    };

    #region SafeIndexOfCellOnSide
    public (bool bIsValid, int x, int y) SafeIndexOfCellOnSide(PipeSide side, int x, int y)
    {
        try
        {
            bool bIsValid = true;
            (int oX, int oY) = IndexOfCellOnSide(side, x, y);
            if (!m_PipeCells.ContainsIndex(oX, oY))
            {
                bIsValid = false;
                oX = -1;
                oY = -1;
            }
            return (bIsValid, oX, oY);
        }
        catch (System.Exception e) { throw e; }
    }

    public (bool bIsValid, int x, int y) SafeIndexOfCellOnSide(PipeSide side, Vector3Int cellPos)
    {
        try
        {
            (int x, int y) = CellPositionToArrayIndex2D(cellPos);
            return SafeIndexOfCellOnSide(side, x, y);
        }
        catch (System.Exception e) { throw e; }
    }

    public (bool bIsValid, int x, int y) SafeIndexOfCellOnSide(PipeSide side, Pipe pipe)
    {
        try
        {
            (int x, int y) = GetIndexOf(pipe);
            return SafeIndexOfCellOnSide(side, x, y);
        }
        catch (System.Exception e) { throw e; }
    }
    #endregion SafeIndexOfCellOnSide
    #endregion IndexOfCellOnSide

    bool IsPlacedPipe(Pipe pipe) => pipe && pipe.CurrentPipeSO != m_EmptyPipe;

    bool PipesConnect(Pipe from, int fromX, int fromY, PipeSide side)
    {
        if (fromX < 0 || fromY < 0) return false;
        if (!IsPlacedPipe(from) || !from.CurrentOrientation.HasHole(side)) return false;
        (bool bSideValid, int toX, int toY) = SafeIndexOfCellOnSide(side, fromX, fromY);
        if (!bSideValid) return false;
        Pipe to = m_PipeCells[toX, toY];
        return IsPlacedPipe(to) && to.CurrentOrientation.HasHole(PipeSideUtil.Opposite(side));
    }

    #region PipeOpenOnSide
    bool InternalPipeOpenOnSide(PipeSide side, Pipe pipe, int x, int y)
    {
        if (!pipe) return false;
        try
        {
            (bool bSideValid, int rX, int rY) = SafeIndexOfCellOnSide(side, x, y);
            if (!bSideValid) return pipe.CurrentOrientation.HasHole(side);
            return PipesConnect(pipe, x, y, side);
        }
        catch (System.Exception e) { throw e; }
    }

    public bool PipeOpenOnSide(PipeSide side, Pipe pipe)
    {
        try
        {
            (int x, int y) = GetIndexOf(pipe);
            return InternalPipeOpenOnSide(side, pipe, x, y);
        }
        catch (System.Exception e) { throw e; }
    }

    public bool PipeOpenOnSide(PipeSide side, int x, int y)
    {
        try
        {
            Pipe pipe = GetPipe(x, y);
            return InternalPipeOpenOnSide(side, pipe, x, y);
        }
        catch (System.Exception e) { throw e; }
    }

    public bool PipeOpenOnSide(PipeSide side, Vector3Int cellPos)
    {
        try
        {
            Pipe pipe = GetPipe(cellPos);
            (int x, int y) = CellPositionToArrayIndex2D(cellPos);
            return InternalPipeOpenOnSide(side, pipe, x, y);
        }
        catch (System.Exception e) { throw e; }
    }
    #endregion PipeOpenOnSide

    #region Pipe Flow
    #region BFS
    void AddPipeIfAdjacent(ref List<Pipe> pipes, PipeSide side, ref Pipe pipe)
    {
        (int x, int y) = GetIndexOf(pipe);
        if (x < 0 || y < 0 || !PipesConnect(pipe, x, y, side)) return;
        (bool bIsValid, int adjacentX, int adjacentY) = SafeIndexOfCellOnSide(side, x, y);
        if (!bIsValid) return;
        pipes.Add(m_PipeCells[adjacentX, adjacentY]);
    }

    public void AddAdjacentPipes(ref List<Pipe> pipes, ref Pipe pipe)
    {
        AddPipeIfAdjacent(ref pipes, PipeSide.Left, ref pipe);
        AddPipeIfAdjacent(ref pipes, PipeSide.Top, ref pipe);
        AddPipeIfAdjacent(ref pipes, PipeSide.Right, ref pipe);
        AddPipeIfAdjacent(ref pipes, PipeSide.Bottom, ref pipe);
    }

    void TryEnqueueReachableNeighbor(int x, int y, PipeSide side, HashSet<Vector2Int> searched, Queue<Vector2Int> toSearch)
    {
        if (!PipesConnect(m_PipeCells[x, y], x, y, side)) return;
        (bool bIsValid, int adjacentX, int adjacentY) = SafeIndexOfCellOnSide(side, x, y);
        if (!bIsValid) return;
        Vector2Int adjacentIndex = new Vector2Int(adjacentX, adjacentY);
        if (searched.Contains(adjacentIndex)) return;
        searched.Add(adjacentIndex);
        toSearch.Enqueue(adjacentIndex);
    }

    HashSet<Vector2Int> FindReachableCells(List<Vector2Int> startIndices)
    {
        HashSet<Vector2Int> searched = new HashSet<Vector2Int>();
        Queue<Vector2Int> toSearch = new Queue<Vector2Int>();
        foreach (Vector2Int startIndex in startIndices)
        {
            if (searched.Contains(startIndex)) continue;
            searched.Add(startIndex);
            toSearch.Enqueue(startIndex);
        }
        while (toSearch.Count > 0)
        {
            Vector2Int current = toSearch.Dequeue();
            TryEnqueueReachableNeighbor(current.x, current.y, PipeSide.Left, searched, toSearch);
            TryEnqueueReachableNeighbor(current.x, current.y, PipeSide.Top, searched, toSearch);
            TryEnqueueReachableNeighbor(current.x, current.y, PipeSide.Right, searched, toSearch);
            TryEnqueueReachableNeighbor(current.x, current.y, PipeSide.Bottom, searched, toSearch);
        }
        return searched;
    }
    #endregion BFS

    #region Water Flow Check
    bool IsOpenStartEndPipe(ResolvedStartEndPipe startEndPipe)
    {
        Pipe pipe = GetPipe(startEndPipe.ArrayIndex.x, startEndPipe.ArrayIndex.y);
        return IsPlacedPipe(pipe) && pipe.CurrentOrientation.HasHole(startEndPipe.EntranceExitSide);
    }

    static List<Vector2Int> OpenIndices(ResolvedStartEndPipe[] startEndPipes, System.Func<ResolvedStartEndPipe, bool> isOpen)
    {
        List<Vector2Int> indices = new List<Vector2Int>(startEndPipes.Length);
        foreach (ResolvedStartEndPipe startEndPipe in startEndPipes) if (isOpen(startEndPipe)) indices.Add(startEndPipe.ArrayIndex);
        return indices;
    }

    public void CheckWaterCanReachEnd()
    {
        List<Vector2Int> openStartIndices = OpenIndices(m_StartPipes, IsOpenStartEndPipe);
        List<Vector2Int> openEndIndices = OpenIndices(m_EndPipes, IsOpenStartEndPipe);
        if (openStartIndices.Count != m_StartPipes.Length || openEndIndices.Count != m_EndPipes.Length) return;
        HashSet<Vector2Int> reachableFromStarts = FindReachableCells(openStartIndices);
        foreach (Vector2Int endIndex in openEndIndices) if (!reachableFromStarts.Contains(endIndex)) return;
        HashSet<Vector2Int> reachableFromEnds = FindReachableCells(openEndIndices);
        foreach (Vector2Int startIndex in openStartIndices) if (!reachableFromEnds.Contains(startIndex)) return;
        List<Pipe> reachedPipes = new List<Pipe>(m_StartPipes.Length + m_EndPipes.Length);
        foreach (ResolvedStartEndPipe startPipe in m_StartPipes) reachedPipes.Add(startPipe.PipeCell);
        foreach (ResolvedStartEndPipe endPipe in m_EndPipes) reachedPipes.Add(endPipe.PipeCell);
        EndMinigame(reachedPipes);
    }
    #endregion Water Flow Check
    #endregion Pipe Flow
}
