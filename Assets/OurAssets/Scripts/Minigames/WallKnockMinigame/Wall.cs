using System.Collections.Generic;
using UnityEngine;
using Util.SystemUtils;

[System.Serializable]
public struct EchoIntensity
{
	[field: SerializeField, Min(0f)]
	public float Distance { get; private set; }
	[field: SerializeField, Min(1)]
	public int NumberOfCircles { get; private set; }
}

[RequireComponent(typeof(Collider))]
public class Wall : MonoBehaviour
{
	[SerializeField]
	Transform m_UnscaledTransform;
	[SerializeField]
	Player m_Player;
	[SerializeField]
	GameObject m_HolePrefab;
	[SerializeField, Min(1)]
	int m_MaxTries = 3;
	[SerializeField]
	Vector3 m_PipeSpawnLowerBound;
	[SerializeField]
	Vector3 m_PipeSpawnUpperBound;
	[SerializeField, Min(0.01f)]
	float m_BreakTolerance = 0.675f;
	[SerializeField]
	WallEcho m_WallEchoPrefab;
	[SerializeField]
	AnimationCurve m_DistanceCurve;
	[SerializeField]
	EchoIntensity m_ClosestIntensity;
	[SerializeField]
	EchoIntensity m_FurthestIntensity;

	bool m_bAlreadyPlaying;

	List<GameObject> m_Holes = new List<GameObject>();

	Vector3 m_GlobalPipeSpawnLowerBound;
	Vector3 m_GlobalPipeSpawnUpperBound;

	Vector3 RandomPipePosition
	{
		get
		{
			float xPos = Random.Range(m_GlobalPipeSpawnLowerBound.x, m_GlobalPipeSpawnUpperBound.x);
			float yPos = Random.Range(m_GlobalPipeSpawnLowerBound.y, m_GlobalPipeSpawnUpperBound.y);
			float zPos = Random.Range(m_GlobalPipeSpawnLowerBound.z, m_GlobalPipeSpawnUpperBound.z);
			return new Vector3(xPos, yPos, zPos);
		}
	}

	Vector3 m_PipePosition;

	int m_AvailableTries;

	void OnValidate() => EnsureBoundsAreValid();

	void OnEnable() => EnsureBoundsAreValid();

	void EnsureBoundsAreValid()
	{
		float dot0 = Vector3.Dot(Vector3.right, (m_PipeSpawnUpperBound - m_PipeSpawnLowerBound).normalized);
		float dot1 = Vector3.Dot(Vector3.forward, (m_PipeSpawnUpperBound - m_PipeSpawnLowerBound).normalized);
		if (dot0 <= 0f || dot1 <= 0f) m_PipeSpawnUpperBound = m_PipeSpawnLowerBound + Vector3.forward + Vector3.right;
		Vector3 lower = transform.rotation * m_PipeSpawnLowerBound;
		Vector3 upper = transform.rotation * m_PipeSpawnUpperBound;
		m_GlobalPipeSpawnLowerBound = transform.position + lower + transform.up * 0.01f;
		m_GlobalPipeSpawnUpperBound = transform.position + upper + transform.up * 0.01f;
	}

	Mesh m_GizmoMesh;

#if UNITY_EDITOR
	void OnDrawGizmosSelected()
	{
		Gizmos.DrawSphere(m_GlobalPipeSpawnLowerBound, 0.1f);
		Gizmos.DrawSphere(m_GlobalPipeSpawnUpperBound, 0.1f);
		if (!m_GizmoMesh) m_GizmoMesh = new Mesh();
		m_GizmoMesh.vertices = GizmoMeshVertices;
		m_GizmoMesh.normals = GizmoMeshNormals;
		m_GizmoMesh.triangles = GizmoMeshTriangles;
		Gizmos.DrawWireMesh(m_GizmoMesh);
		if (m_bAlreadyPlaying) Gizmos.DrawSphere(m_PipePosition, m_BreakTolerance);
	}

	Vector3[] GizmoMeshVertices
	{
		get
		{
			Vector3 p0 = m_GlobalPipeSpawnLowerBound;
			Vector3 p1 = m_GlobalPipeSpawnUpperBound;
			float angleP0P1 = Vector3.Angle(transform.right, (p1 - p0).normalized);
			float distP0P1 = Vector3.Distance(p0, p1);
			float distP0P2 = Mathf.Cos(Mathf.Deg2Rad * angleP0P1) * distP0P1;
			Vector3 p2 = p0 + transform.right * distP0P2;
			float distP0P3 = Mathf.Sin(Mathf.Deg2Rad * angleP0P1) * distP0P1;
			Vector3 p3 = p0 + transform.forward * distP0P3;
			return new Vector3[] { p0, p1, p2, p3 };
		}
	}
	Vector3[] GizmoMeshNormals => new Vector3[] { transform.up, transform.up, transform.up, transform.up };

	int[] GizmoMeshTriangles => new int[] { 0, 1, 2, 0, 3, 1 };
#endif

	void Awake()
	{
#if !UNITY_EDITOR
		if (m_GizmoMesh) Destroy(m_GizmoMesh);
#endif
	}

	void Start()
	{
		//StartWallKnockMinigame();
	}

	public void StartWallKnockMinigame()
	{
		if (m_bAlreadyPlaying) return;
		m_bAlreadyPlaying = true;
		m_AvailableTries = m_MaxTries;
		EnsureBoundsAreValid();
		m_PipePosition = RandomPipePosition;
		m_UnscaledTransform.gameObject.SetActive(true);
	}

	void EndWallKnockMinigame(bool bWon)
	{
		m_bAlreadyPlaying = false;
		Debug.Log(bWon);
		if (bWon)
		{
			if (!m_Player) m_Player = FindAnyObjectByType<Player>();
			Sys.Assert(m_Player, "Player doesn't exist");
			m_Player.OnMinigameBeaten();
			m_Player.ChangeActionMap("Player");
			ClearHoles();
			m_UnscaledTransform.gameObject.SetActive(false);
			//m_Player.ChangeActionMap("PipePlayer");
		}
		if (!bWon) ResetMinigame();
	}

	void ClearHoles()
	{
		for (int i = m_Holes.Count - 1; i >= 0; --i)
		{
			Destroy(m_Holes[i]);
		}
		m_Holes.Clear();
	}

	void ResetMinigame()
	{
		ClearHoles();
		StartWallKnockMinigame();
	}

	public void KnockWall(Vector3 position)
	{
		float distanceToPipe = Vector3.Distance(position, m_PipePosition);
		int numCircles = CalculateNumCircles(distanceToPipe);
		CreateEcho(position, numCircles);
	}

	public void BreakWall(Vector3 position)
	{
		--m_AvailableTries;
		GameObject hole = Instantiate(m_HolePrefab, m_UnscaledTransform);
		hole.transform.position = position + transform.up * 0.02f;
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		materialPropertyBlock.SetFloat("_HoleSize", m_BreakTolerance);
		hole.GetComponent<Renderer>().SetPropertyBlock(materialPropertyBlock);
		m_Holes.Add(hole);
		if (Vector3.Distance(position, m_PipePosition) <= m_BreakTolerance) EndWallKnockMinigame(true);
		else if (m_AvailableTries <= 0) EndWallKnockMinigame(false);
	}

	int CalculateNumCircles(float distance)
	{
		float distance01 = Mathf.Clamp01((distance - m_ClosestIntensity.Distance) / (m_FurthestIntensity.Distance - m_ClosestIntensity.Distance));
		float numCirclesF = Mathf.Lerp(m_ClosestIntensity.NumberOfCircles, m_FurthestIntensity.NumberOfCircles, distance01);
		return Mathf.RoundToInt(numCirclesF);
	}

	void CreateEcho(Vector3 position, int numCircles)
	{
		GameObject go = Instantiate(m_WallEchoPrefab.gameObject, m_UnscaledTransform);
		go.transform.position = position + transform.up * 0.01f;
		go.GetComponent<WallEcho>().StartEcho(numCircles);
	}
}
