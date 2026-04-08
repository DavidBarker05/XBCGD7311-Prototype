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
            Vector3Int lp = grid.WorldToCell(hit.point);
            Vector3 wp = grid.CellToWorld(lp);
            m_CellIndicator.transform.position = wp;
            m_CellIndicator.transform.up = hit.normal;
        }
        input.bClickedThisFrame = false;
    }
}
