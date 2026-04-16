using UnityEngine;
using Util.SystemUtils;
using Util.ArrayUtils;
using Util.ComparisonUtils;
using System.Collections.Generic;

public class WireBoard : MonoBehaviour
{
    [SerializeField]
    bool m_Debug = false;
    [SerializeField]
    Transform m_UnscaledTransform;
	[SerializeField, Min(0)]
	int m_MinWires = 3;
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
		public WireColour Colour { get; set; }
    }

	HashSet<GrabReleasePoint> m_UsedReleasePoints;
    Wire[] m_Wires;
    GrabReleasePoint[] m_GrabPoints;
    GrabReleasePoint[] m_ReleasePoints;

	void OnValidate() => EnsureMinWires();

	void OnEnable() => EnsureMinWires();

	void EnsureMinWires()
	{
		if (m_WireStartingPositions != null) m_MinWires = Mathf.Min(m_MinWires, m_WireStartingPositions.Length);
	}

	void Awake()
    {
        Sys.Assert(Arrays.IsValid(m_WireStartingPositions), "m_WireStartingPositions is not a valid array");
        Sys.Assert(Arrays.IsValid(m_WireTipStartingPositions), "m_WireTipStartingPositions is not a valid array");
        Sys.Assert(Arrays.IsValid(m_WireEndPositions), "m_WireEndPositions is not a valid array");
        Sys.Assert(m_WireStartingPositions.Length.Equals(m_WireTipStartingPositions.Length, m_WireEndPositions.Length), "Mismatched number of wire positions");
#if !UNITY_EDITOR
        m_Debug = false;
#endif
        if (m_Debug) StartWireMinigame(Random.Range(m_MinWires - 1, m_WireStartingPositions.Length) + 1);
    }

    public void StartWireMinigame(int numWires)
    {
		numWires = Mathf.Max(numWires, m_MinWires);
        Sys.Assert(m_WireStartingPositions.ContainsIndex(numWires - 1), $"{numWires} is an invalid number of wires");
		m_UsedReleasePoints = new HashSet<GrabReleasePoint>();
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
		for (int i = 0; i < m_ReleasePoints.Length; ++i)
		{
			m_ReleasePoints[i].Position = m_WireEndPositions[i].position;
		}
	}

	public void EndWireMinigame()
	{
		// TODO: End minigame
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
		WireColour[] coloursNoNone = WireColour.WireColours.SubArray(1, WireColour.WireColours.Length - 1);
        m_Wires[index].Init(m_WireStartingPositions[index].position, m_WireTipStartingPositions[index].position, coloursNoNone.GetRandomElement<WireColour>());
    }

    void CreateGrabPoint(int index)
    {
        Sys.Assert(Arrays.IsValid(m_GrabPoints), "m_GrabPoints is not a valid array");
        Sys.Assert(m_GrabPoints.ContainsIndex(index), $"{index} is not a valid index for m_GrabPoints");
        m_GrabPoints[index] = new GrabReleasePoint()
        {
            Position = (m_WireStartingPositions[index].position + m_WireTipStartingPositions[index].position) / 2,
            Radius = m_GrabTolerance,
			Colour = m_Wires[index].Colour
        };
    }

    void CreateReleasePoint(int index)
    {
        Sys.Assert(Arrays.IsValid(m_ReleasePoints), "m_ReleasePoints is not a valid array");
        Sys.Assert(m_ReleasePoints.ContainsIndex(index), $"{index} is not a valid index for m_ReleasePoints");
        m_ReleasePoints[index] = new GrabReleasePoint()
        {
            Radius = m_ReleaseTolerance,
			Colour = m_Wires[index].Colour
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

    public (Vector3 snapPosition, bool bDidWin) TryReleaseWire(Wire wire, Vector3 position)
    {
        if (m_IgnoreDepthAxis) position = Vector3.ProjectOnPlane(position, transform.up);
        for (int i = 0; i < m_ReleasePoints.Length; ++i)
        {
			if (m_UsedReleasePoints.Contains(m_ReleasePoints[i])) continue;
            Vector3 releasePosition = m_IgnoreDepthAxis ? Vector3.ProjectOnPlane(m_ReleasePoints[i].Position, transform.up) : m_ReleasePoints[i].Position;
			if (Vector3.Distance(position, releasePosition) <= m_ReleaseTolerance)
			{
				if (m_ReleasePoints[i].Colour == wire.Colour)
				{
					m_UsedReleasePoints.Add(m_ReleasePoints[i]);
					return (m_ReleasePoints[i].Position, m_UsedReleasePoints.Count == m_Wires.Length);
				}
				else return (Vector3.negativeInfinity, false);
			}
        }
        return (Vector3.negativeInfinity, false);
    }
}
