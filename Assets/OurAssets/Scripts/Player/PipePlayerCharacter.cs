using UnityEngine;

public struct PipeCharacterInput
{
    public bool bClickedThisFrame;
    public MouseInfo PipeMouseInfo;
}

public class PipePlayerCharacter : MonoBehaviour
{
    [field: SerializeField]
    public Transform CameraTarget { get; private set; }
    [field: SerializeField]
    public LayerMask HitLayer { get; private set; }
    [SerializeField]
    GameObject m_CellIndicatorPrefab;

    GameObject m_CellIndicator;

    public void Init() => m_CellIndicator = Instantiate(m_CellIndicatorPrefab);

    public void UpdatePipeCharacter(ref PipeCharacterInput input)
    {
        if (input.PipeMouseInfo.bHitObject)
        {
            RaycastHit hit = input.PipeMouseInfo.HitInfo;
            Grid grid = hit.collider.gameObject.GetComponent<Grid>();
            if (!grid) return;
            Vector3Int cp = grid.WorldToCell(hit.point);
            MoveCellIndicator(ref grid, cp, hit.normal);
            PipeGrid pipeGrid = hit.collider.gameObject.GetComponent<PipeGrid>();
            if (!pipeGrid) return;
        }
        input.bClickedThisFrame = false;
    }

    void MoveCellIndicator(ref Grid grid, Vector3Int cellPosition, Vector3 up)
    {
        Vector3 worldPosition = grid.CellToWorld(cellPosition);
        m_CellIndicator.transform.position = worldPosition;
        m_CellIndicator.transform.up = up;
    }
}
