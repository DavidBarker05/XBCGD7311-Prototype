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

    public Pipe GetPipe(int x, int y) => m_PipeCells[x, y];

    public Pipe GetPipe(Vector3Int cellPosition)
    {
        (int x, int y) = CellPositionToArrayIndex2D(cellPosition);
        return GetPipe(x, y);
    }

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

    (int x, int y) IndexOfCellOnSide(PipeSide side, int x, int y) => side switch
    {
        PipeSide.Left => (x - 1, y),
        PipeSide.Top => (x, y + 1),
        PipeSide.Right => (x + 1, y),
        PipeSide.Bottom => (x, y - 1),
        _ => throw new System.ArgumentException("Somehow you input a side that doesn't exist")
    };

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
}
