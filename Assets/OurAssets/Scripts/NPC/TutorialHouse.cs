using UnityEngine;

public class TutorialHouse : MonoBehaviour
{
    public enum Stage
    {
        NotStarted,
        ElectricalPending,
        ElectricalDone,
        PipePending,
        PipeDone,
        DisconnectPending,
        DisconnectDone,
        Complete
    }

    public Stage CurrentStage { get; private set; } = Stage.NotStarted;

    [Header("NPC")]
    [SerializeField]
    TutorialNPC m_NPC;
    [SerializeField]
    Sprite m_TalkWaypointIcon; // Falls back to WaypointManager's default icon if left unset

    [Header("Electrical")]
    [SerializeField]
    WireMinigameInteractable m_ElectricalBoxInteractable;
    [SerializeField]
    Light[] m_HouseLights;
    [SerializeField]
    Sprite m_ElectricalWaypointIcon;

    [Header("Pipe")]
    [SerializeField]
    WallKnockInteractable m_PipeWallInteractable;
    [SerializeField]
    Sprite m_PipeWaypointIcon;

    [Header("Disconnect")]
    [SerializeField]
    ChaseMinigameInteract m_DisconnectInteractable; // Visually a door, same convention as the main house's chase door swap
    [SerializeField]
    Sprite m_DisconnectWaypointIcon;

    [Header("Reward")]
    [SerializeField, Min(0)]
    int m_MoneyReward = 50;

    void Awake()
    {
        if (m_ElectricalBoxInteractable) m_ElectricalBoxInteractable.gameObject.SetActive(false);
        if (m_PipeWallInteractable) m_PipeWallInteractable.gameObject.SetActive(false);
        if (m_DisconnectInteractable) m_DisconnectInteractable.gameObject.SetActive(false);
        SetLights(false);
    }

    void Start()
    {
        if (TutorialMinigameManager.Instance) TutorialMinigameManager.Instance.OnMinigameCompleted += HandleMinigameCompleted;
        ShowTalkWaypoint();
    }

    void OnDestroy()
    {
        if (TutorialMinigameManager.Instance) TutorialMinigameManager.Instance.OnMinigameCompleted -= HandleMinigameCompleted;
    }

    void HandleMinigameCompleted(MinigameType type)
    {
        if (type == MinigameType.Wires && CurrentStage == Stage.ElectricalPending) OnElectricalMinigameCompleted();
        else if (type == MinigameType.WallKnockAndPipes && CurrentStage == Stage.PipePending) OnPipeMinigameCompleted();
        else if (type == MinigameType.ChaseMinigame && CurrentStage == Stage.DisconnectPending) OnDisconnectMinigameCompleted();
    }

    #region Waypoints
    void ShowTalkWaypoint()
    {
        if (m_NPC) WaypointManager.Instance?.AddWaypoint(m_NPC.transform, m_TalkWaypointIcon);
    }

    void HideTalkWaypoint()
    {
        if (m_NPC) WaypointManager.Instance?.RemoveWaypoint(m_NPC.transform);
    }
    #endregion Waypoints

    void SetLights(bool bOn)
    {
        foreach (Light light in m_HouseLights) if (light) light.enabled = bOn;
    }

    #region Electrical
    // Called by TutorialNPC once the welcome dialogue finishes
    public void OnWelcomeDialogueFinished()
    {
        CurrentStage = Stage.ElectricalPending;
        HideTalkWaypoint();
        if (!m_ElectricalBoxInteractable) return;
        m_ElectricalBoxInteractable.gameObject.SetActive(true);
        WaypointManager.Instance?.AddWaypoint(m_ElectricalBoxInteractable.transform, m_ElectricalWaypointIcon);
    }

    void OnElectricalMinigameCompleted()
    {
        CurrentStage = Stage.ElectricalDone;
        if (m_ElectricalBoxInteractable) WaypointManager.Instance?.RemoveWaypoint(m_ElectricalBoxInteractable.transform);
        SetLights(true);
        ShowTalkWaypoint();
    }
    #endregion Electrical

    #region Pipe
    // Called by TutorialNPC once the "electricity's fixed, now the pipe" dialogue finishes
    public void OnPipeDialogueFinished()
    {
        CurrentStage = Stage.PipePending;
        HideTalkWaypoint();
        if (!m_PipeWallInteractable) return;
        m_PipeWallInteractable.gameObject.SetActive(true);
        WaypointManager.Instance?.AddWaypoint(m_PipeWallInteractable.transform, m_PipeWaypointIcon);
    }

    void OnPipeMinigameCompleted()
    {
        CurrentStage = Stage.PipeDone;
        if (m_PipeWallInteractable) WaypointManager.Instance?.RemoveWaypoint(m_PipeWallInteractable.transform);
        ShowTalkWaypoint();
    }
    #endregion Pipe

    #region Disconnect
    // Called by TutorialNPC once the "go disconnect the illegal connections" dialogue finishes
    public void OnDisconnectDialogueFinished()
    {
        CurrentStage = Stage.DisconnectPending;
        HideTalkWaypoint();
        if (!m_DisconnectInteractable) return;
        m_DisconnectInteractable.gameObject.SetActive(true);
        WaypointManager.Instance?.AddWaypoint(m_DisconnectInteractable.transform, m_DisconnectWaypointIcon);
    }

    // Hooked to ChaseMinigameInteract.OnChaseStarted in the Inspector
    // Removes the marker the moment the player actually engages it,
    // rather than waiting for the whole chase to finish
    public void OnDisconnectInteracted()
    {
        if (m_DisconnectInteractable) WaypointManager.Instance?.RemoveWaypoint(m_DisconnectInteractable.transform);
    }

    void OnDisconnectMinigameCompleted()
    {
        CurrentStage = Stage.DisconnectDone;
        ShowTalkWaypoint();
    }
    #endregion Disconnect

    #region Reward
    // Called by TutorialNPC once the reward dialogue finishes
    public void OnRewardDialogueFinished()
    {
        CurrentStage = Stage.Complete;
        HideTalkWaypoint();
        PlayerSaveManager.CurrentSaveData.Money += m_MoneyReward;
        PlayerSaveManager.SaveGame();
    }
    #endregion Reward
}
