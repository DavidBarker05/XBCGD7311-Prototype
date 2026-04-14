using UnityEngine;
using Util.SystemUtils;
using Util.ArrayUtils;
using Util.ComparisonUtils;
using System.Linq;

public class WireBoard : MonoBehaviour
{
    [SerializeField]
    bool m_Debug = false;
    [SerializeField]
    Transform m_UnscaledTransform;
	[SerializeField]
	Material m_WireMaterial;
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
        Sys.Assert(Arrays.IsValid(m_WireStartingPositions), "m_WireStartingPositions is not a valid array");
        Sys.Assert(Arrays.IsValid(m_WireTipStartingPositions), "m_WireTipStartingPositions is not a valid array");
        Sys.Assert(Arrays.IsValid(m_WireEndPositions), "m_WireEndPositions is not a valid array");
        Sys.Assert(m_WireStartingPositions.Length.Equals(m_WireTipStartingPositions.Length, m_WireEndPositions.Length), "Mismatched number of wire positions");
#if !UNITY_EDITOR
        m_Debug = false;
#endif
        if (m_Debug) StartWireMinigame(Random.Range(0, m_WireStartingPositions.Length) + 1);
    }

    public void StartWireMinigame(int numWires)
    {
        Sys.Assert(m_WireStartingPositions.ContainsIndex(numWires - 1), $"{numWires} is an invalid number of wires");
        m_Wires = new Wire[numWires];
        m_GrabPoints = new GrabReleasePoint[numWires];
        m_ReleasePoints = new GrabReleasePoint[numWires];
        for (int i = 0; i < numWires; ++i)
        {
            CreateWire(i);
            CreateGrabPoint(i);
            CreateReleasePoint(i);
        }
		m_ReleasePoints.Shuffle();
    }

    void CreateWire(int index)
    {
        Sys.Assert(Arrays.IsValid(m_Wires), "m_Wires is not a valid array");
        Sys.Assert(m_Wires.ContainsIndex(index), $"{index} is not a valid index for m_Wires");
        GameObject go = new GameObject($"Wire ({index})");
        go.transform.SetParent(m_UnscaledTransform);
        LineRenderer lineRenderer = go.AddComponent<LineRenderer>();
		lineRenderer.material = m_WireMaterial;
        m_Wires[index] = go.AddComponent<Wire>();
		WireColour[] colours = (WireColour[])System.Enum.GetValues(typeof(WireColour));
		colours = colours.Skip(1).ToArray();
        m_Wires[index].Init(m_WireStartingPositions[index].position, m_WireTipStartingPositions[index].position, m_WireEndPositions[index].position, colours.GetRandomElement<WireColour>()); // TODO: Wire colour
    }

    void CreateGrabPoint(int index)
    {
        Sys.Assert(Arrays.IsValid(m_GrabPoints), "m_GrabPoints is not a valid array");
        Sys.Assert(m_GrabPoints.ContainsIndex(index), $"{index} is not a valid index for m_GrabPoints");
        m_GrabPoints[index] = new GrabReleasePoint()
        {
            Position = (m_WireStartingPositions[index].position + m_WireTipStartingPositions[index].position) / 2,
            Radius = m_GrabTolerance
        };
    }

    void CreateReleasePoint(int index)
    {
        Sys.Assert(Arrays.IsValid(m_ReleasePoints), "m_ReleasePoints is not a valid array");
        Sys.Assert(m_ReleasePoints.ContainsIndex(index), $"{index} is not a valid index for m_ReleasePoints");
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
            if (Vector3.Distance(position, grabPosition) <= m_GrabTolerance && m_Wires[i].CanBeGrabbed) return m_Wires[i];
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
