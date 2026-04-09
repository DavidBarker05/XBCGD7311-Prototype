using UnityEngine;

[RequireComponent(typeof(Grid))]
public class GridResizer : MonoBehaviour
{
    Grid m_Grid;
    Vector3 m_DefaultCellSize;
    Vector3 lastScale;

    void OnEnable()
    {
        m_Grid = GetComponent<Grid>();
        m_DefaultCellSize = m_Grid.cellSize;
        ResizeCells();
    }

    void Update()
    {
        if (transform.localScale != lastScale) ResizeCells();
    }

    void ResizeCells()
    {
        lastScale = transform.localScale;
        float sizeX = m_DefaultCellSize.x / lastScale.x;
        float sizeY = m_DefaultCellSize.y / lastScale.y;
        float sizeZ = m_DefaultCellSize.z / lastScale.z;
        m_Grid.cellSize = new Vector3(sizeX, sizeY, sizeZ);
    }
}
