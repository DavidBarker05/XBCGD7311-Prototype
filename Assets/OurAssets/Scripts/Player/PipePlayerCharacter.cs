using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Util.SystemUtils;
using Util.UnityUtils;

public class PipePlayerCharacterInitData : IPlayerCharacterInitData { }

public class PipePlayerCharacterUpdateData : IPlayerCharacterUpdateData
{
    public float DeltaTime { get; set; }
    public Quaternion CameraRotation { get; set; }
    public MouseInfo MouseInfo { get; set; }

    public bool ClickedThisFrame { get; set; }
}

public class PipePlayerCharacter : PlayerCharacter
{
    [SerializeField]
    GameObject m_CellIndicatorPrefab;
    [SerializeField]
    PipeSO m_EmptyPipe;
    [SerializeField]
    List<PipeSO> m_PlaceablePipes;
    [SerializeField]
    bool m_Debug = false;

	public override bool HasBeenInitialised { get; protected set; }

    public override bool MouseVisible => true;
    public override bool DoCameraRotation => false;
    public override bool UseMouseScreenPosition => true;

    GameObject m_CellIndicator;
    PipeSO m_CurrentlySelectedPipe;
    Dictionary<PipeSO, uint> m_PipeQuantities = new Dictionary<PipeSO, uint>();

    void OnValidate() => CleanupPlaceablePipes(); // Can't fully clean in OnValidate because unity's + adds duplicate of last item

    void OnEnable() => FullyCleanPlaceablePipes();

    void CleanupPlaceablePipes() => m_PlaceablePipes.RemoveAll(pipe => pipe == m_EmptyPipe);

    void RemoveDuplicatePlaceablePipes()
    {
        HashSet<PipeSO> uniquePipes = new HashSet<PipeSO>(m_PlaceablePipes);
        m_PlaceablePipes = new List<PipeSO>(uniquePipes);
    }

    void FullyCleanPlaceablePipes()
    {
        CleanupPlaceablePipes();
        RemoveDuplicatePlaceablePipes();
    }

    public override void Init(IPlayerCharacterInitData playerCharacterInitData)
    {
		PipePlayerCharacterInitData initData = Sys.AssertType<PipePlayerCharacterInitData>(playerCharacterInitData, nameof(playerCharacterInitData));
#if !UNITY_EDITOR
        m_Debug = false;
#endif
        m_CellIndicator = Instantiate(m_CellIndicatorPrefab);
        m_CurrentlySelectedPipe = m_Debug && m_PlaceablePipes.Count > 0 ? m_PlaceablePipes[0] : m_EmptyPipe;
        FullyCleanPlaceablePipes(); // Just in case
        foreach (PipeSO pipe in m_PlaceablePipes)
            m_PipeQuantities.Add(pipe, (m_Debug ? 1u : 0u));
		HasBeenInitialised = true;
    }

    public override void UpdateCharacter(ref IPlayerCharacterUpdateData playerCharacterUpdateData)
    {
		Sys.Assert(HasBeenInitialised, "PipePlayerCharacter hasn't been initialised");
		PipePlayerCharacterUpdateData input = Sys.AssertType<PipePlayerCharacterUpdateData>(playerCharacterUpdateData, nameof(playerCharacterUpdateData));
        DoGridFunctions(ref input);
        input.ClickedThisFrame = false;
    }

    void DoGridFunctions(ref PipePlayerCharacterUpdateData input)
    {
        if (!input.MouseInfo.DidHitObject) return;
        RaycastHit hit = input.MouseInfo.HitInfo;
        Grid grid = hit.GetComponent<Grid>();
        if (!grid) return;
        Vector3Int cp = grid.WorldToCell(hit.point);
        MoveCellIndicator(ref grid, cp, hit.normal);
        if (input.ClickedThisFrame)
        {
            PipeGrid pipeGrid = hit.GetComponent<PipeGrid>();
            if (!pipeGrid) return;
            PlacePipe(ref pipeGrid, cp);
        }
    }

    public void ClearPipeQuantity(PipeSO pipe)
    {
        if (!pipe || pipe == m_EmptyPipe) return;
        if (!m_PipeQuantities.ContainsKey(pipe)) m_PipeQuantities.Add(pipe, 0);
        else m_PipeQuantities[pipe] = 0;
    }

    public void ClearPipeQuantities()
    {
        foreach (PipeSO pipe in m_PipeQuantities.Keys)
            m_PipeQuantities[pipe] = 0;
    }

    public void SetPipeQuantity(PipeSO pipe, uint quantity)
    {
        if (!pipe || pipe == m_EmptyPipe) return;
        if (!m_PipeQuantities.ContainsKey(pipe)) m_PipeQuantities.Add(pipe, quantity);
        else m_PipeQuantities[pipe] = quantity;
    }

    public void SelectPipe(PipeSO pipe) // Added to the buttons on click event
    {
        if (pipe && pipe != m_EmptyPipe) m_CurrentlySelectedPipe = pipe;
    }

	public uint GetPipeQuantity(PipeSO pipe)
	{
		if (!pipe || pipe == m_EmptyPipe || !m_PipeQuantities.ContainsKey(pipe)) return 0;
		return m_PipeQuantities[pipe];
	}

    public void PlacePipe(ref PipeGrid pipeGrid, Vector3Int cellPosition) // public in case need to access in another class
    {
        if (!pipeGrid || !m_CurrentlySelectedPipe || m_CurrentlySelectedPipe == m_EmptyPipe || !m_PipeQuantities.ContainsKey(m_CurrentlySelectedPipe) || (m_PipeQuantities[m_CurrentlySelectedPipe] == 0 && !m_Debug)) return;
        PipeSO originalPipeInCell = pipeGrid.PlacePipe(m_CurrentlySelectedPipe, cellPosition);
        --m_PipeQuantities[m_CurrentlySelectedPipe];
        if (originalPipeInCell != m_EmptyPipe)
        {
            if (!m_PipeQuantities.ContainsKey(originalPipeInCell)) m_PipeQuantities.Add(originalPipeInCell, 1);
            else ++m_PipeQuantities[originalPipeInCell];
        }
        if (!m_Debug) m_CurrentlySelectedPipe = m_EmptyPipe;
    }

    void MoveCellIndicator(ref Grid grid, Vector3Int cellPosition, Vector3 up)
    {
        Vector3 worldPosition = grid.CellToWorld(cellPosition);
        m_CellIndicator.transform.position = worldPosition;
        m_CellIndicator.transform.up = up;
    }
}
