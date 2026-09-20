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
        LeaveHousePending,
        Complete
    }

    public Stage CurrentStage { get; private set; } = Stage.NotStarted;

    [Header("NPC")]
    [SerializeField]
    TutorialNPC m_NPC;
    [SerializeField]
    Sprite m_TalkWaypointIcon;

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
    ChaseMinigameInteract m_DisconnectInteractable;
    [SerializeField]
    Sprite m_DisconnectWaypointIcon;

    [Header("Reward")]
    [SerializeField, Min(0)]
    int m_MoneyReward = 50;

    [Header("Leave House")]
    [SerializeField]
    PlayerHouseDoor m_PlayerHouseDoor;
    [SerializeField]
    Sprite m_LeaveHouseWaypointIcon;

    readonly DisplayTask m_TalkDisplayTask = new DisplayTask("Talk to Nomsa", 1, 0, false);
    readonly DisplayTask m_ElectricalDisplayTask = new DisplayTask("Fix the electrical box", 1, 0, false);
    readonly DisplayTask m_PipeDisplayTask = new DisplayTask("Fix the leaking pipe", 1, 0, false);
    readonly DisplayTask m_DisconnectDisplayTask = new DisplayTask("Disconnect the illegal wiring", 1, 0, false);
    readonly DisplayTask m_LeaveHouseDisplayTask = new DisplayTask("Head outside to start helping others", 1, 0, false);

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
        TaskList.Instance?.AddTask(m_TalkDisplayTask);
    }

    void HideTalkWaypoint()
    {
        if (m_NPC) WaypointManager.Instance?.RemoveWaypoint(m_NPC.transform);
        TaskList.Instance?.RemoveTask(m_TalkDisplayTask);
    }
    #endregion Waypoints

    void SetLights(bool bOn)
    {
        foreach (Light light in m_HouseLights) if (light) light.enabled = bOn;
    }

    #region Electrical
    public void OnWelcomeDialogueFinished()
    {
        CurrentStage = Stage.ElectricalPending;
        HideTalkWaypoint();
        if (!m_ElectricalBoxInteractable) return;
        m_ElectricalBoxInteractable.gameObject.SetActive(true);
        WaypointManager.Instance?.AddWaypoint(m_ElectricalBoxInteractable.transform, m_ElectricalWaypointIcon);
        TaskList.Instance?.AddTask(m_ElectricalDisplayTask);
    }

    void OnElectricalMinigameCompleted()
    {
        CurrentStage = Stage.ElectricalDone;
        if (m_ElectricalBoxInteractable) WaypointManager.Instance?.RemoveWaypoint(m_ElectricalBoxInteractable.transform);
        TaskList.Instance?.RemoveTask(m_ElectricalDisplayTask);
        SetLights(true);
        ShowTalkWaypoint();
    }
    #endregion Electrical

    #region Pipe
    public void OnPipeDialogueFinished()
    {
        CurrentStage = Stage.PipePending;
        HideTalkWaypoint();
        if (!m_PipeWallInteractable) return;
        m_PipeWallInteractable.gameObject.SetActive(true);
        WaypointManager.Instance?.AddWaypoint(m_PipeWallInteractable.transform, m_PipeWaypointIcon);
        TaskList.Instance?.AddTask(m_PipeDisplayTask);
    }

    void OnPipeMinigameCompleted()
    {
        CurrentStage = Stage.PipeDone;
        if (m_PipeWallInteractable) WaypointManager.Instance?.RemoveWaypoint(m_PipeWallInteractable.transform);
        TaskList.Instance?.RemoveTask(m_PipeDisplayTask);
        ShowTalkWaypoint();
    }
    #endregion Pipe

    #region Disconnect
    public void OnDisconnectDialogueFinished()
    {
        CurrentStage = Stage.DisconnectPending;
        HideTalkWaypoint();
        if (!m_DisconnectInteractable) return;
        m_DisconnectInteractable.gameObject.SetActive(true);
        WaypointManager.Instance?.AddWaypoint(m_DisconnectInteractable.transform, m_DisconnectWaypointIcon);
        TaskList.Instance?.AddTask(m_DisconnectDisplayTask);
    }

    public void OnDisconnectInteracted()
    {
        if (m_DisconnectInteractable) WaypointManager.Instance?.RemoveWaypoint(m_DisconnectInteractable.transform);
        TaskList.Instance?.RemoveTask(m_DisconnectDisplayTask);
    }

    void OnDisconnectMinigameCompleted()
    {
        CurrentStage = Stage.DisconnectDone;
        ShowTalkWaypoint();
    }
    #endregion Disconnect

    #region Reward
    public void OnRewardDialogueFinished()
    {
        CurrentStage = Stage.LeaveHousePending;
        HideTalkWaypoint();
        PlayerSaveManager.CurrentSaveData.Money += m_MoneyReward;
        PlayerSaveManager.SaveGame();
        if (m_PlayerHouseDoor) WaypointManager.Instance?.AddWaypoint(m_PlayerHouseDoor.transform, m_LeaveHouseWaypointIcon);
        TaskList.Instance?.AddTask(m_LeaveHouseDisplayTask);
    }
    #endregion Reward

    #region Leave House
    public void OnLeaveHouseInteracted()
    {
        if (CurrentStage != Stage.LeaveHousePending) return;
        CurrentStage = Stage.Complete;
        if (m_PlayerHouseDoor) WaypointManager.Instance?.RemoveWaypoint(m_PlayerHouseDoor.transform);
        TaskList.Instance?.RemoveTask(m_LeaveHouseDisplayTask);
    }
    #endregion Leave House
}
