using UnityEngine;
using Util.SystemUtils;
using Util.ArrayUtils;
using Util.ComparisonUtils;
using System.Collections.Generic;
using System.Collections;

public class WireBoard : MonoBehaviour
{
    [SerializeField]
    bool m_Debug = false;
    [SerializeField]
    Transform m_UnscaledTransform;
	[SerializeField]
	Player m_Player;
	[SerializeField, Range(0f, 1f)]
	float m_TimeActiveAfterCompleted = 0.3f;
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
	[SerializeField]
	LineRenderer[] m_WireStarts;
	[SerializeField]
	LineRenderer[] m_WireEnds;

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
	Dictionary<WireColour, int> m_WireColoursUsed;

	bool m_bIsAlreadyPlaying = false;

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
		Sys.Assert(Arrays.IsValid(m_WireStarts), "m_WireStarts is not a valid array");
		Sys.Assert(Arrays.IsValid(m_WireEnds), "m_WireEnds is not a valid array");
        Sys.Assert(m_WireStartingPositions.Length.Equals(m_WireTipStartingPositions.Length, m_WireEndPositions.Length, m_WireStarts.Length, m_WireEnds.Length), "Mismatched array lengths");
#if !UNITY_EDITOR
        m_Debug = false;
#endif
        //if (m_Debug) CreateWires(Random.Range(m_MinWires - 1, m_WireStartingPositions.Length) + 1);
    }

	public void StartWireMinigame()
	{
		if (m_bIsAlreadyPlaying) return;
		m_bIsAlreadyPlaying = true;
		CreateWires(Random.Range(m_MinWires, m_WireStartingPositions.Length + 1));
		m_UnscaledTransform.gameObject.SetActive(true);
	}

	void EndWireMinigame()
	{
		m_Player.OnMinigameBeaten();
		StartCoroutine(CloseMinigame());
	}

	IEnumerator CloseMinigame()
	{
		yield return new WaitForSeconds(m_TimeActiveAfterCompleted);
		m_bIsAlreadyPlaying = false;
		m_Player.ChangeActionMap("Player");
		for (int i = m_Wires.Length - 1; i >= 0; --i)
		{
			Destroy(m_Wires[i].gameObject);
		}
		m_UnscaledTransform.gameObject.SetActive(false);
	}

	void CreateWires(int numWires)
    {
		m_UsedReleasePoints = new HashSet<GrabReleasePoint>();
        m_Wires = new Wire[numWires];
        m_GrabPoints = new GrabReleasePoint[numWires];
        m_ReleasePoints = new GrabReleasePoint[numWires];
		m_WireColoursUsed = new Dictionary<WireColour, int>();
        for (int i = 0; i < numWires; ++i)
        {
            CreateWire(i);
            CreateGrabPoint(i);
            CreateReleasePoint(i);
        }
		ShuffleReleasePoints();
		CreateWireStarts(numWires);
		CreateWireEnds(numWires);
	}

    void CreateWire(int index)
    {
        Sys.Assert(Arrays.IsValid(m_Wires), "m_Wires is not a valid array");
        Sys.Assert(m_Wires.ContainsIndex(index), $"{index} is not a valid index for m_Wires");
        GameObject go = new GameObject($"Wire ({index})");
        go.transform.SetParent(m_UnscaledTransform);
        LineRenderer lineRenderer = go.AddComponent<LineRenderer>();
		lineRenderer.material = m_WireMaterial;
		lineRenderer.startWidth = m_WireStarts[index].startWidth;
		lineRenderer.endWidth = m_WireStarts[index].endWidth;
        m_Wires[index] = go.AddComponent<Wire>();
		WireColour[] coloursNoNone = WireColour.WireColours.SubArray(1, WireColour.WireColours.Length - 1);
		WireColour randomColour;
		do
		{
			randomColour = coloursNoNone.GetRandomElement<WireColour>();
			if (!m_WireColoursUsed.ContainsKey(randomColour)) m_WireColoursUsed.Add(randomColour, 0);
		} while (m_WireColoursUsed[randomColour] >= m_Wires.Length / 2);
		++m_WireColoursUsed[randomColour];
		m_Wires[index].Init(m_WireStartingPositions[index].position, m_WireTipStartingPositions[index].position, randomColour);
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

	void ShuffleReleasePoints()
	{
		Sys.Assert(Arrays.IsValid(m_ReleasePoints), "m_ReleasePoints is not a valid array");
		m_ReleasePoints.Shuffle();
		for (int i = 0; i < m_ReleasePoints.Length; ++i)
		{
			m_ReleasePoints[i].Position = m_WireEndPositions[i].position;
		}
	}

	void CreateWireStarts(int numWires)
	{
		for (int i = 0; i < m_WireEnds.Length; ++i)
		{
			if (i >= numWires)
			{
				m_WireStarts[i].gameObject.SetActive(false);
				continue;
			}
			m_WireStarts[i].gameObject.SetActive(true);
			m_WireStarts[i].material.color = m_GrabPoints[i].Colour.Colour;
		}
	}

	void CreateWireEnds(int numWires)
	{
		for (int i = 0; i < m_WireEnds.Length; ++i)
		{
			if (i >= numWires)
			{
				m_WireEnds[i].gameObject.SetActive(false);
				continue;
			}
			m_WireEnds[i].gameObject.SetActive(true);
			m_WireEnds[i].material.color = m_ReleasePoints[i].Colour.Colour;
		}
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

    public Vector3 TryReleaseWire(Wire wire, Vector3 position)
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
					if (m_UsedReleasePoints.Count == m_Wires.Length) EndWireMinigame();
					return m_ReleasePoints[i].Position;
				}
				else return Vector3.negativeInfinity;
			}
        }
        return Vector3.negativeInfinity;
    }
}
