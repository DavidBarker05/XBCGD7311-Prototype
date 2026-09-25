using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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
	PipePlaceMinigameGenerator m_PipePlaceMinigameGenerator;
	[SerializeField]
	WallKnockPlayerCharacter m_WallKnockPlayerCharacter;
	[SerializeField]
	GameObject m_HUD;
	[SerializeField]
	GameObject m_HolePrefab;
	[field: SerializeField, Min(1)]
	public int MaxTries { get; private set; } = 3;
	[SerializeField]
	Vector3 m_PipeSpawnLowerBound;
	[SerializeField]
	Vector3 m_PipeSpawnUpperBound;
	[SerializeField, Min(0.01f)]
	float m_BreakTolerance = 1f;
	[SerializeField, Min(0.01f)]
	float m_BreakToleranceUpgrade1 = 1.125f;
	[SerializeField, Min(0.01f)]
	float m_BreakToleranceUpgrade2 = 1.25f;
	[SerializeField, Min(0.01f)]
	float m_BreakToleranceUpgrade3 = 1.375f;
	[SerializeField]
	WallEcho m_WallEchoPrefab;
	[SerializeField]
	AnimationCurve m_DistanceCurve;
	[SerializeField]
	EchoIntensity m_ClosestIntensity;
	[SerializeField]
	EchoIntensity m_FurthestIntensity;
	[Header("Money Reward")]
	[SerializeField, Min(0f)]
	float m_FastCompletionTime = 30f;
	[SerializeField, Min(0f)]
	float m_SlowCompletionTime = 90f;
	[Header("Tutorial Instructions")]
	[SerializeField]
	GameObject m_InstructionsScreen;
	[SerializeField]
	MenuCharacter m_MenuCharacter;
	[SerializeField, Min(1)]
	int m_RetriesToShowTutorial = 3;

	public UnityEvent OnWallEnd;

	bool m_bAlreadyPlaying;
	float m_TimeTaken;

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
	int m_TimesFailed;

	bool m_bInTutorial;

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

	void Update()
	{
		if (m_bAlreadyPlaying) m_TimeTaken += Time.deltaTime;
	}

	void ShowTutorialScreen() => m_MenuCharacter.OnMenuOpen(m_WallKnockPlayerCharacter, null, m_InstructionsScreen);

	void PartialStartWallKnockMinigame()
	{
		m_bAlreadyPlaying = true;
		m_AvailableTries = MaxTries;
		if (m_bInTutorial && m_TimesFailed % m_RetriesToShowTutorial == 0) ShowTutorialScreen();
		m_PipePosition = RandomPipePosition;
	}

	public void StartWallKnockMinigame()
	{
		if (m_bAlreadyPlaying) return;
		m_bInTutorial = TutorialMinigameManager.Instance;
		m_TimeTaken = 0.0f;
		m_TimesFailed = 0;
		EnsureBoundsAreValid();
		m_UnscaledTransform.gameObject.SetActive(true);
		m_HUD.SetActive(false);
		PartialStartWallKnockMinigame();
	}

	void EndWallKnockMinigame(bool bWon)
	{
		m_bAlreadyPlaying = false;
		OnWallEnd?.Invoke();
		if (bWon)
		{
			ClearHoles();
			m_UnscaledTransform.gameObject.SetActive(false);
			m_PipePlaceMinigameGenerator.StartPipeMinigame(CalculateSpeedMultiplier());
		}
		if (!bWon) ResetMinigame();
	}

	float CalculateSpeedMultiplier()
	{
		float timeT = Mathf.InverseLerp(m_FastCompletionTime, m_SlowCompletionTime, m_TimeTaken);
		return Mathf.Lerp(1.2f, 0.8f, timeT);
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
		++m_TimesFailed;
		ClearHoles();
		PartialStartWallKnockMinigame();
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
		float breakTolerance = PlayerUpgradeSystem.GetLevel(PlayerUpgrade.BiggerWallBreakHole) switch
		{
			1 => m_BreakToleranceUpgrade1,
			2 => m_BreakToleranceUpgrade2,
			3 => m_BreakToleranceUpgrade3,
			_ => m_BreakTolerance
		};
		materialPropertyBlock.SetFloat("_HoleSize", breakTolerance);
		hole.GetComponent<Renderer>().SetPropertyBlock(materialPropertyBlock);
		m_Holes.Add(hole);
		if (Vector3.Distance(position, m_PipePosition) <= breakTolerance) EndWallKnockMinigame(true);
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
		// Don't parent to transform because then doesn't destroy when wall disappears
		WallEcho echo = Instantiate(m_WallEchoPrefab);
		echo.transform.SetPositionAndRotation(position + transform.up * 0.01f, m_UnscaledTransform.rotation);
		echo.StartEcho(this, numCircles);
	}
}
