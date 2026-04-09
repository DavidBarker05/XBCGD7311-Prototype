using UnityEngine;

public class Pipe : MonoBehaviour
{
    [SerializeField]
    Transform m_PipeTransform;

    GameObject m_CurrentPipePrefab;
    PipeSO m_CurrentPipeSO;
    public PipeSO CurrentPipeSO
    {
        get => m_CurrentPipeSO;
        set
        {
            if (m_CurrentPipePrefab) Destroy(m_CurrentPipePrefab);
            if (value.Model)
            {
                GameObject go = Instantiate(value.Model, m_PipeTransform);
                m_CurrentPipePrefab = go;
            }
            m_CurrentPipeSO = value;
        }
    }

    public PipeOrientation CurrentOrientation { get; private set; }

    public bool CanConnectLeft => CurrentOrientation.HasLeftHole;
    public bool CanConnectTop => CurrentOrientation.HasTopHole;
    public bool CanConnectRight => CurrentOrientation.HasRightHole;
    public bool CanConnectBottom => CurrentOrientation.HasBottomHole;

    PipeRotationAngle m_CurrentAngle;
    public PipeRotationAngle CurrentPipeAngle
    {
        get => m_CurrentAngle;
        set
        {
            m_PipeTransform.rotation = Quaternion.Euler(0f, (int)value, 0f);
            CurrentOrientation = CurrentPipeSO.GetOrientationFromAngle(value);
            m_CurrentAngle = value;
        }
    }

    public void RotateRight() => CurrentPipeAngle = PipeRotationAngleUtil.NextAngleRight(m_CurrentAngle);

    public void RotateLeft() => CurrentPipeAngle = PipeRotationAngleUtil.NextAngleLeft(m_CurrentAngle);
}
