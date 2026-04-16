using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class PlaneGridGenerator : MonoBehaviour
{
    [SerializeField]
    Vector2Int m_GridSize = new Vector2Int(10, 10);

    /// <summary>
    /// Recommended to do even numbers for the grid
    /// </summary>
    public Vector2Int GridSize
    {
        get => m_GridSize;
        set
        {
            m_GridSize = EnsureMinSize(value);
            Resize();
        }
    }

    readonly Vector2Int MIN = new Vector2Int(1, 1);
    readonly Vector2 SIZE = new Vector2(10f, 10f);

    void OnValidate()
    {
        m_GridSize = EnsureMinSize(m_GridSize);
        Resize();
    }

    void OnEnable()
    {
        m_GridSize = EnsureMinSize(m_GridSize);
        Resize();
    }

    Vector2Int EnsureMinSize(Vector2Int value) => Vector2Int.Max(MIN, value);

    void Resize()
    {
        float sizeX = GridSize.x / SIZE.x;
        float sizeY = GridSize.y / SIZE.y;
        transform.localScale = new Vector3(sizeX, 1f, sizeY);
    }
}
