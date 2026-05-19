using System.Collections.Generic;
using UnityEngine;
using Util.ArrayUtils;
using Util.SystemUtils;

[RequireComponent(typeof(PlaneGridGenerator), typeof(Grid))]
public class PipeGrid : MonoBehaviour
{
    [SerializeField]
    Transform unscaledTransform;
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

    PlaneGridGenerator m_PlaneGrid;
    Vector2Int Size => m_PlaneGrid.GridSize;
    Grid m_Grid;

    Pipe[,] m_PipeCells;

    public struct StartEndPipe
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
    StartEndPipe m_StartPipe;
    Pipe m_StartOutsidePipe;
    StartEndPipe m_EndPipe;
    Pipe m_EndOutsidePipe;

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
        if (m_StartOutsidePipe) Destroy(m_StartOutsidePipe.gameObject);
        if (m_EndOutsidePipe) Destroy(m_EndOutsidePipe.gameObject);
    }

    void InitCells(ref Pipe[,] pipeCells, ref Grid grid, Vector2Int size)
    {
        if (pipeCells != null && pipeCells.Length > 0) DeletePipes(ref pipeCells);
        pipeCells = new Pipe[size.x, size.y];
        for (int x = 0; x < size.x; ++x)
        {
            for (int y = 0; y < size.y; ++y)
            {
                GameObject go = Instantiate(m_PipePrefab.gameObject, unscaledTransform);
                Vector3Int cp = ArrayIndex2DToCellPosition(x, y);
                Vector3 wp = grid.CellToWorld(cp);
                go.transform.position = wp;
                Pipe pipe = go.GetComponent<Pipe>();
                pipe.CurrentPipeSO = m_EmptyPipe;
                pipe.CurrentPipeAngle = PipeRotationAngle.Zero;
                m_PipeCells[x, y] = pipe;
            }
        }
        GameObject sop = Instantiate(m_PipePrefab.gameObject, unscaledTransform);
        m_StartOutsidePipe = sop.GetComponent<Pipe>();
        m_StartOutsidePipe.CurrentPipeSO = m_OutsidePipe;
        GameObject eop = Instantiate(m_PipePrefab.gameObject, unscaledTransform);
        m_EndOutsidePipe = eop.GetComponent<Pipe>();
        m_EndOutsidePipe.CurrentPipeSO = m_OutsidePipe;
    }
    #endregion Delete & Init

    #region Start & End Minigame
    public void StartMinigame(PipeGridData pipeGridData)
    {
        if (!m_PlaneGrid) m_PlaneGrid = GetComponent<PlaneGridGenerator>();
        if (!m_Grid) m_Grid = GetComponent<Grid>();
        m_PlaneGrid.GridSize = pipeGridData.GridSize;
        unscaledTransform.gameObject.SetActive(true);
        if (m_PipeUI) m_PipeUI.SetActive(true);
        InitCells(ref m_PipeCells, ref m_Grid, Size);
        Pipe startPipe = GetPipe(pipeGridData.StartPipe.CellPosition.x, pipeGridData.StartPipe.CellPosition.y);
        Sys.Assert(startPipe, $"({pipeGridData.StartPipe.CellPosition}) was not a valid index");
        m_StartPipe = new StartEndPipe() { PipeCell = startPipe, ArrayIndex = pipeGridData.StartPipe.CellPosition, EntranceExitSide = pipeGridData.StartPipe.EntranceExitSide };
        Vector3Int sopPosCP = ArrayIndex2DToCellPosition(pipeGridData.StartPipe.CellPosition.x, pipeGridData.StartPipe.CellPosition.y) + m_StartPipe.HoleSideToOutsidePipePos;
        Vector3 sopPosWP = m_Grid.CellToWorld(sopPosCP);
        m_StartOutsidePipe.transform.position = sopPosWP;
        m_StartOutsidePipe.CurrentPipeAngle = m_StartPipe.HoleSideToOutsidePipeAngle;
        Pipe endPipe = GetPipe(pipeGridData.EndPipe.CellPosition.x, pipeGridData.EndPipe.CellPosition.y);
        Sys.Assert(endPipe, $"({pipeGridData.EndPipe.CellPosition}) was not a valid index");
        m_EndPipe = new StartEndPipe() { PipeCell = endPipe, ArrayIndex = pipeGridData.EndPipe.CellPosition, EntranceExitSide = pipeGridData.EndPipe.EntranceExitSide };
        Vector3Int eopPosCP = ArrayIndex2DToCellPosition(pipeGridData.EndPipe.CellPosition.x, pipeGridData.EndPipe.CellPosition.y) + m_EndPipe.HoleSideToOutsidePipePos;
        Vector3 eopPosWP = m_Grid.CellToWorld(eopPosCP);
        m_EndOutsidePipe.transform.position = eopPosWP;
        m_EndOutsidePipe.CurrentPipeAngle = m_EndPipe.HoleSideToOutsidePipeAngle;
    }

    void EndMinigame(List<Pipe> path) // path is in case we want to do some kind of flowing animation
    {
        if (m_PipeUI) m_PipeUI.SetActive(false);
        if (m_Player && m_FirstPersonPlayerCharacter) m_Player.ChangeCharacter(m_FirstPersonPlayerCharacter);
        MinigameManager.Instance?.OnMinigameBeaten();
        DeletePipes(ref m_PipeCells);
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
        CheckWaterCanReachEnd(m_StartPipe, m_EndPipe);
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
        CheckWaterCanReachEnd(m_StartPipe, m_EndPipe);
    }

    public void RotatePipeRight(Vector3Int cellPosition)
    {
        if (CellIsEmpty(cellPosition)) return;
        Pipe pipe = GetPipe(cellPosition);
        pipe.RotateRight();
        CheckWaterCanReachEnd(m_StartPipe, m_EndPipe);
    }
    #endregion Rotate Right

    #region Rotate Left
    public void RotatePipeLeft(int x, int y)
    {
        if (CellIsEmpty(x, y)) return;
        Pipe pipe = GetPipe(x, y);
        pipe.RotateLeft();
        CheckWaterCanReachEnd(m_StartPipe, m_EndPipe);
    }

    public void RotatePipeLeft(Vector3Int cellPosition)
    {
        if (CellIsEmpty(cellPosition)) return;
        Pipe pipe = GetPipe(cellPosition);
        pipe.RotateLeft();
        CheckWaterCanReachEnd(m_StartPipe, m_EndPipe);
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

    void TryEnqueueCellNeighbor(int x, int y, PipeSide side, ref HashSet<Vector2Int> searched, ref Queue<Vector2Int> toSearch, ref Dictionary<Vector2Int, Vector2Int> previousCells)
    {
        if (!PipesConnect(m_PipeCells[x, y], x, y, side)) return;
        (bool bIsValid, int adjacentX, int adjacentY) = SafeIndexOfCellOnSide(side, x, y);
        if (!bIsValid) return;
        Vector2Int adjacentIndex = new Vector2Int(adjacentX, adjacentY);
        if (searched.Contains(adjacentIndex)) return;
        searched.Add(adjacentIndex);
        toSearch.Enqueue(adjacentIndex);
        previousCells.Add(adjacentIndex, new Vector2Int(x, y));
    }

    List<Pipe> BreadthFirstSearch(int startX, int startY, int endX, int endY)
    {
        if (!m_PipeCells.ContainsIndex(startX, startY) || !m_PipeCells.ContainsIndex(endX, endY)) return null;
        if (!IsPlacedPipe(m_PipeCells[startX, startY]) || !IsPlacedPipe(m_PipeCells[endX, endY])) return null;
        Vector2Int startIndex = new Vector2Int(startX, startY);
        Vector2Int endIndex = new Vector2Int(endX, endY);
        HashSet<Vector2Int> searched = new HashSet<Vector2Int>();
        Queue<Vector2Int> toSearch = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> previousCells = new Dictionary<Vector2Int, Vector2Int>();
        searched.Add(startIndex);
        toSearch.Enqueue(startIndex);
        previousCells.Add(startIndex, new Vector2Int(-1, -1));
        while (toSearch.Count > 0)
        {
            Vector2Int current = toSearch.Dequeue();
            if (current == endIndex)
            {
                List<Pipe> shortestPath = new List<Pipe>();
                Vector2Int node = endIndex;
                while (true)
                {
                    shortestPath.Insert(0, m_PipeCells[node.x, node.y]);
                    Vector2Int previous = previousCells[node];
                    if (previous.x < 0) break;
                    node = previous;
                }
                return shortestPath;
            }
            TryEnqueueCellNeighbor(current.x, current.y, PipeSide.Left, ref searched, ref toSearch, ref previousCells);
            TryEnqueueCellNeighbor(current.x, current.y, PipeSide.Top, ref searched, ref toSearch, ref previousCells);
            TryEnqueueCellNeighbor(current.x, current.y, PipeSide.Right, ref searched, ref toSearch, ref previousCells);
            TryEnqueueCellNeighbor(current.x, current.y, PipeSide.Bottom, ref searched, ref toSearch, ref previousCells);
        }
        return null;
    }
    #endregion BFS

    #region Water Flow Check
    bool WaterCanReachEnd(Pipe startPipe, PipeSide entranceSide, Pipe endPipe, PipeSide exitSide, int startX, int startY, int endX, int endY, out List<Pipe> path)
    {
        if (!IsPlacedPipe(startPipe) || !IsPlacedPipe(endPipe) || !startPipe.CurrentOrientation.HasHole(entranceSide) || !endPipe.CurrentOrientation.HasHole(exitSide))
        {
            path = null;
            return false;
        }
        path = BreadthFirstSearch(startX, startY, endX, endY);
        return path != null;
    }

    public void CheckWaterCanReachEnd(Pipe startPipe, PipeSide entranceSide, Pipe endPipe, PipeSide exitSide)
    {
        (int startX, int startY) = GetIndexOf(startPipe);
        (int endX, int endY) = GetIndexOf(endPipe);
        if (WaterCanReachEnd(startPipe, entranceSide, endPipe, exitSide, startX, startY, endX, endY, out List<Pipe> path)) EndMinigame(path);
    }

    public void CheckWaterCanReachEnd(int startX, int startY, PipeSide entranceSide, int endX, int endY, PipeSide exitSide)
    {
        Pipe startPipe = GetPipe(startX, startY);
        Pipe endPipe = GetPipe(endX, endY);
        if (WaterCanReachEnd(startPipe, entranceSide, endPipe, exitSide, startX, startY, endX, endY, out List<Pipe> path)) EndMinigame(path);
    }

    public void CheckWaterCanReachEnd(Vector3Int startCellPosition, PipeSide entranceSide, Vector3Int endCellPosition, PipeSide exitSide)
    {
        (int startX, int startY) = CellPositionToArrayIndex2D(startCellPosition);
        (int endX, int endY) = CellPositionToArrayIndex2D(endCellPosition);
        Pipe startPipe = GetPipe(startX, startY);
        Pipe endPipe = GetPipe(endX, endY);
        if (WaterCanReachEnd(startPipe, entranceSide, endPipe, exitSide, startX, startY, endX, endY, out List<Pipe> path)) EndMinigame(path);
    }

    public void CheckWaterCanReachEnd(StartEndPipe startPipe, StartEndPipe endPipe)
    {
        int startX = startPipe.ArrayIndex.x, startY = startPipe.ArrayIndex.y;
        int endX = endPipe.ArrayIndex.x, endY = endPipe.ArrayIndex.y;
        Pipe start = GetPipe(startX, startY);
        Pipe end = GetPipe(endX, endY);
        if (WaterCanReachEnd(start, startPipe.EntranceExitSide, end, endPipe.EntranceExitSide, startX, startY, endX, endY, out List<Pipe> path))
            EndMinigame(path);
    }
    #endregion Water Flow Check
    #endregion Pipe Flow
}
