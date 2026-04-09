using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlaneGridGenerator), typeof(Grid))]
public class PipeGrid : MonoBehaviour
{
    [SerializeField]
    Pipe m_PipePrefab;
    [SerializeField]
    PipeSO m_EmptyPipe;

    PlaneGridGenerator m_PlaneGrid;
    Vector2Int Size => m_PlaneGrid.GridSize;
    Grid m_Grid;

    Pipe[,] m_PipeCells;

    void OnEnable()
    {
        m_PlaneGrid = GetComponent<PlaneGridGenerator>();
        m_Grid = GetComponent<Grid>();
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
                GameObject go = Instantiate(m_PipePrefab.gameObject);
                Vector3Int cp = ArrayIndex2DToCellPosition(x, y);
                Vector3 wp = grid.CellToWorld(cp);
                go.transform.position = wp;
                go.transform.up = transform.up;
                Pipe pipe = go.GetComponent<Pipe>();
                pipe.CurrentPipeSO = m_EmptyPipe;
                m_PipeCells[x, y] = pipe;
            }
        }
    }
    #endregion

    #region GetIndex & GetPipe
    public (int x, int y) GetIndexOf(Pipe pipe)
    {
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
    public Pipe GetPipe(int x, int y) => m_PipeCells[x, y];

    public Pipe GetPipe(Vector3Int cellPosition)
    {
        (int x, int y) = CellPositionToArrayIndex2D(cellPosition);
        return GetPipe(x, y);
    }
    #endregion
    #endregion

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
    #endregion

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
    #endregion

    #region IndexofCellOnSide
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
            if (oX < 0 || oX >= m_PipeCells.GetLength(0) || oY < 0 || oY >= m_PipeCells.GetLength(1))
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
    #endregion
    #endregion

    #region PipeOpenOnSide
    bool InternalPipeOpenOnSide(PipeSide side, Pipe pipe, int x, int y)
    {
        try
        {
            (bool bSideValid, int rX, int rY) = SafeIndexOfCellOnSide(side, x, y);
            Pipe sidePipe = m_PipeCells[rX, rY];
            return pipe.CurrentOrientation.HasHole(side) && bSideValid && sidePipe.CurrentPipeSO != m_EmptyPipe;
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
            Pipe pipe = m_PipeCells[x, y];
            return InternalPipeOpenOnSide(side, pipe, x, y);
        }
        catch (System.Exception e) { throw e; }
    }

    public bool PipeOpenOnSide(PipeSide side, Vector3Int cellPos)
    {
        try
        {
            (int x, int y) = CellPositionToArrayIndex2D(cellPos);
            return PipeOpenOnSide(side, x, y);
        }
        catch (System.Exception e) { throw e; }
    }
    #endregion

    #region Pipe Flow
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

    public List<Pipe> FindPath(Pipe start, Pipe end) => BreadthFirstSearch(start, end);
    #endregion
}
