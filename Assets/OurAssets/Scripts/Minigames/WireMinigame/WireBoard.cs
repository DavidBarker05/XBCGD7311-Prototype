using UnityEngine;

public class WireBoard : MonoBehaviour
{
    [SerializeField]
    Transform m_UnscaledTransform;
    [SerializeField, Min(0f)]
    float m_GrabTolerance = 1f;
    [SerializeField, Min(0f)]
    float m_ReleaseTolerance = 1f;
    [SerializeField]
    bool m_IgnoreDepthAxis = true;
    [SerializeField]
    Transform[] m_WireStartingPositions;
    [SerializeField]
    Transform[] m_WireTipStartingPositions;
    [SerializeField]
    Transform[] m_WireEndPositions;

    public struct GrabReleasePoint
    {
        public Vector3 Position { get; set; }
        public float Radius { get; set; }
    }

    Wire[] m_Wires;
    GrabReleasePoint[] m_GrabPoints;
    GrabReleasePoint[] m_ReleasePoints;

    void Awake()
    {
        Util.Sys.Assert(Util.Compare.MultiEqual(m_WireStartingPositions.Length, m_WireTipStartingPositions.Length, m_WireEndPositions.Length), "Mismatched number of wire positions");
    }

    public void StartWireMinigame(int numWires)
    {
        Util.Sys.Assert(Util.Arrays.IsValidIndex(m_WireStartingPositions, numWires - 1), $"{numWires} is an invalid number of wires");
        m_Wires = new Wire[numWires];
        m_GrabPoints = new GrabReleasePoint[numWires];
        m_ReleasePoints = new GrabReleasePoint[numWires];
        for (int i = 0; i < numWires; ++i)
        {
            CreateWire(i);
            CreateGrabPoint(i);
            CreateReleasePoint(i);
        }
    }

    void CreateWire(int index)
    {
        if (!Util.Arrays.IsValidArray(m_WireStartingPositions) || !Util.Arrays.IsValidArray(m_WireTipStartingPositions) || !Util.Arrays.IsValidArray(m_WireEndPositions)) return;
        if (!Util.Compare.MultiEqual(m_WireStartingPositions.Length, m_WireTipStartingPositions.Length, m_WireEndPositions.Length)) return;
        if (!Util.Arrays.IsValidIndex(m_WireTipStartingPositions, index)) return;
        GameObject go = new GameObject($"Wire ({index})");
        go.transform.SetParent(m_UnscaledTransform);
        m_Wires[index] = go.AddComponent<Wire>();
        m_Wires[index].Init(m_WireStartingPositions[index].position, m_WireTipStartingPositions[index].position, m_WireEndPositions[index].position, WireColour.None); // TODO: Wire colour
    }

    void CreateGrabPoint(int index)
    {
        if (!Util.Arrays.IsValidArray(m_WireStartingPositions) || !Util.Arrays.IsValidArray(m_WireTipStartingPositions) || !Util.Arrays.IsValidArray(m_GrabPoints)) return;
        if (!Util.Compare.MultiEqual(m_WireStartingPositions.Length, m_WireTipStartingPositions.Length, m_GrabPoints.Length)) return;
        if (!Util.Arrays.IsValidIndex(m_GrabPoints, index)) return;
        m_GrabPoints[index] = new GrabReleasePoint()
        {
            Position = (m_WireStartingPositions[index].position + m_WireTipStartingPositions[index].position) / 2,
            Radius = m_GrabTolerance
        };
    }

    void CreateReleasePoint(int index)
    {
        if (!Util.Arrays.IsValidArray(m_WireEndPositions) || !Util.Arrays.IsValidArray(m_ReleasePoints)) return;
        if (m_WireEndPositions.Length != m_ReleasePoints.Length) return;
        if (!Util.Arrays.IsValidIndex(m_ReleasePoints, index)) return;
        m_ReleasePoints[index] = new GrabReleasePoint()
        {
            Position = m_WireEndPositions[index].position,
            Radius = m_ReleaseTolerance
        };
    }

    public Wire TryGrabWire(Vector3 position)
    {
        if (m_IgnoreDepthAxis) position = Vector3.ProjectOnPlane(position, transform.up);
        for (int i = 0; i < m_GrabPoints.Length; ++i)
        {
            Vector3 grabPosition = m_IgnoreDepthAxis ? Vector3.ProjectOnPlane(m_GrabPoints[i].Position, transform.up) : m_GrabPoints[i].Position;
            if (Vector3.Distance(position, grabPosition) <= m_GrabTolerance) return m_Wires[i];
        }
        return null;
    }

    public bool TryReleaseWire(Vector3 position)
    {
        if (m_IgnoreDepthAxis) position = Vector3.ProjectOnPlane(position, transform.up);
        for (int i = 0;i < m_ReleasePoints.Length; ++i)
        {
            Vector3 releasePosition = m_IgnoreDepthAxis ? Vector3.ProjectOnPlane(m_ReleasePoints[i].Position, transform.up) : m_ReleasePoints[i].Position;
            if (Vector3.Distance(position, releasePosition) <= m_ReleaseTolerance) return true;
        }
        return false;
    }
}
