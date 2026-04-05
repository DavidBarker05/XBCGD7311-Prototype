using UnityEngine;

public struct PipeCharacterInput
{
    public bool bClickedThisFrame;
    public (bool bSuccessful, RaycastHit hitInfo) MouseHitInWorld;
}

public class PipePlayerCharacter : MonoBehaviour
{
    [field: SerializeField]
    public Transform CameraTarget { get; private set; }
    [field: SerializeField]
    public LayerMask HitLayer { get; private set; }

    public void UpdatePipeCharacter(ref PipeCharacterInput input)
    {
        if (input.MouseHitInWorld.bSuccessful)
        {
            if (!input.bClickedThisFrame) return;
            RaycastHit hit = input.MouseHitInWorld.hitInfo;
            Color colour = input.bClickedThisFrame ? Color.red : Color.green;
            Grid grid = hit.collider.gameObject.GetComponent<Grid>();
            if (!grid) return;
            Vector3Int lp = grid.WorldToCell(hit.point);
            Vector3 wp = grid.CellToWorld(lp);
            Debug.DrawRay(wp, hit.normal, colour, 5f);
        }
        input.bClickedThisFrame = false;
    }
}
