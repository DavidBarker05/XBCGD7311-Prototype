using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlaneGridGenerator), typeof(Grid))]
public class PipeGrid : MonoBehaviour
{
    [SerializeField]
    Transform unscaledTransform;
    [SerializeField]
    Pipe m_PipePrefab;
    [SerializeField]
    PipeSO m_EmptyPipe;

    PlaneGridGenerator m_PlaneGrid;
    Vector2Int Size => m_PlaneGrid.GridSize;
    Grid m_Grid;

    Pipe[,] m_PipeCells;

    public struct StartEndPipe
    {
        public Pipe PipeCell;
        public PipeSide EntranceExitSide;
    }
    StartEndPipe m_StartPipe;
    StartEndPipe m_EndPipe;

    void OnEnable()
    {
        if (!m_PlaneGrid) m_PlaneGrid = GetComponent<PlaneGridGenerator>();
        if (!m_Grid) m_Grid = GetComponent<Grid>();
        InitCells(ref m_PipeCells, ref m_Grid, Size);
    }

    void OnDisable() => DeletePipes(ref m_PipeCells);

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
                m_PipeCells[x, y] = pipe;
            }
        }
    }
    #endregion Delete & Init

    #region Start & End Minigame
    #region Start Minigame
    public void StartMinigame(Vector2Int gridSize, int startX, int startY, PipeSide entranceSide, int endX, int endY, PipeSide exitSide)
    {
        m_PlaneGrid.GridSize = gridSize;
        if (gameObject.activeSelf) InitCells(ref m_PipeCells, ref m_Grid, Size);
        else gameObject.SetActive(true);
        Pipe startPipe = GetPipe(startX, startY);
        OwnUtils.Sys.Assert(startPipe, $"({startX}, {startY}) was not a valid index");
        m_StartPipe = new StartEndPipe() { PipeCell = startPipe, EntranceExitSide = entranceSide };
        Pipe endPipe = GetPipe(endX, endY);
        OwnUtils.Sys.Assert(endPipe, $"({endX}, {endY}) was not a valid index");
        m_EndPipe = new StartEndPipe() { PipeCell = endPipe, EntranceExitSide = exitSide };
    }

    public void StartMinigame(int width, int height, int startX, int startY, PipeSide entranceSide, int endX, int endY, PipeSide exitSide)
    {
        Vector2Int gridSize = new Vector2Int(width, height);
        StartMinigame(gridSize, startX, startY, entranceSide, endX, endY, exitSide);
    }
    #endregion Start Minigame

    void EndMinigame(List<Pipe> path) // path is in case we want to do some kind of flowing animation
    {
        // TODO: End minigame
    }
    #endregion Start & End Minigame

    #region GetIndex & GetPipe
    public (int x, int y) GetIndexOf(Pipe pipe)
    {
        int[] indices = OwnUtils.Arrays.IndexOf(m_PipeCells, pipe);
        int x = indices[0], y = indices[1];
        return (x, y);
    }

    #region GetPipe
    public Pipe GetPipe(int x, int y) => OwnUtils.Arrays.IsValidIndex(m_PipeCells, x, y) ? m_PipeCells[x, y] : null;

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
        if(CellIsEmpty(cellPosition)) return;
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
            if (!OwnUtils.Arrays.IsValidIndex(m_PipeCells, oX, oY))
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

    #region PipeOpenOnSide
    bool InternalPipeOpenOnSide(PipeSide side, Pipe pipe, int x, int y)
    {
        if (!pipe) return false;
        try
        {
            (bool bSideValid, int rX, int rY) = SafeIndexOfCellOnSide(side, x, y);
            if (!bSideValid) return false;
            Pipe sidePipe = m_PipeCells[rX, rY];
            return pipe.CurrentOrientation.HasHole(side) && sidePipe.CurrentPipeSO != m_EmptyPipe;
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
        if (!PipeOpenOnSide(side, pipe)) return;
        (bool bIsValid, int x, int y) = SafeIndexOfCellOnSide(side, pipe);
        if (!bIsValid) return;
        Pipe adjacent = m_PipeCells[x, y];
        pipes.Add(adjacent);
    }

    public void AddAdjacentPipes(ref List<Pipe> pipes, ref Pipe pipe)
    {
        AddPipeIfAdjacent(ref pipes, PipeSide.Left, ref pipe);
        AddPipeIfAdjacent(ref pipes, PipeSide.Top, ref pipe);
        AddPipeIfAdjacent(ref pipes, PipeSide.Right, ref pipe);
        AddPipeIfAdjacent(ref pipes, PipeSide.Bottom, ref pipe);
    }

    List<Pipe> BreadthFirstSearch(Pipe start, Pipe end)
    {
        if (!start || start == m_EmptyPipe || !end || end == m_EmptyPipe) return null;
        HashSet<Pipe> searched = new HashSet<Pipe>();
        Queue<Pipe> toSearch = new Queue<Pipe>();
        Dictionary<Pipe, Pipe> previousPipes = new Dictionary<Pipe, Pipe>();
        searched.Add(start);
        toSearch.Enqueue(start);
        previousPipes.Add(start, null);
        while (toSearch.Count > 0)
        {
            Pipe front = toSearch.Dequeue();
            if (front == end)
            {
                List<Pipe> shortestPath = new List<Pipe>();
                Pipe current = end;
                while (current != null)
                {
                    shortestPath.Insert(0, current);
                    current = previousPipes[current];
                }
                return shortestPath;
            }
            List<Pipe> adjacentPipes = new List<Pipe>();
            AddAdjacentPipes(ref adjacentPipes, ref front);
            foreach (Pipe adjacent in adjacentPipes)
            {
                if (!searched.Contains(adjacent))
                {
                    searched.Add(adjacent);
                    toSearch.Enqueue(adjacent);
                    previousPipes.Add(adjacent, front);
                }
            }
        }
        return null;
    }
    #endregion BFS

    #region Water Flow Check
    bool WaterCanReachEnd(Pipe startPipe, PipeSide entranceSide, Pipe endPipe, PipeSide exitSide, out List<Pipe> path)
    {
        if (!startPipe || startPipe == m_EmptyPipe || !endPipe || endPipe == m_EmptyPipe || !startPipe.CurrentOrientation.HasHole(entranceSide) || !endPipe.CurrentOrientation.HasHole(exitSide))
        {
            path = null;
            return false;
        }
        path = BreadthFirstSearch(startPipe, endPipe);
        return path != null;
    }

    public void CheckWaterCanReachEnd(Pipe startPipe, PipeSide entranceSide, Pipe endPipe, PipeSide exitSide)
    {
        if (WaterCanReachEnd(startPipe, entranceSide, endPipe, exitSide, out List<Pipe> path)) EndMinigame(path);
    }

    public void CheckWaterCanReachEnd(int startX, int startY, PipeSide entranceSide, int endX, int endY, PipeSide exitSide)
    {
        Pipe startPipe = GetPipe(startX, startY);
        Pipe endPipe = GetPipe(endX, endY);
        if (WaterCanReachEnd(startPipe, entranceSide, endPipe, exitSide, out List<Pipe> path)) EndMinigame(path);
    }

    public void CheckWaterCanReachEnd(Vector3Int startCellPosition, PipeSide entranceSide, Vector3Int endCellPosition, PipeSide exitSide)
    {
        Pipe startPipe = GetPipe(startCellPosition);
        Pipe endPipe = GetPipe(endCellPosition);
        if (WaterCanReachEnd(startPipe, entranceSide, endPipe, exitSide, out List<Pipe> path)) EndMinigame(path);
    }

    public void CheckWaterCanReachEnd(StartEndPipe startPipe, StartEndPipe endPipe)
    {
        Pipe start = startPipe.PipeCell;
        PipeSide entrance = startPipe.EntranceExitSide;
        Pipe end = endPipe.PipeCell;
        PipeSide exit = endPipe.EntranceExitSide;
        if (WaterCanReachEnd(start, entrance, end, exit, out List<Pipe> path)) EndMinigame(path);
    }
    #endregion Water Flow Check
    #endregion Pipe Flow
}
