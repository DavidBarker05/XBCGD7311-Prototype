using UnityEngine;
using UnityEngine.Events;

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
    [SerializeField]
    Vector3 m_TalkWaypointOffset = new Vector3(0f, 1f);

    [Header("Electrical")]
    [SerializeField]
    WireMinigameInteractable m_ElectricalBoxInteractable;
    [SerializeField]
    CeilingLight[] m_HouseLights;
    [SerializeField]
    Sprite m_ElectricalWaypointIcon;
    [SerializeField]
    Vector3 m_ElectricalWaypointOffset = Vector3.zero;

    [Header("Pipe")]
    [SerializeField]
    WallKnockInteractable m_PipeWallInteractable;
    [SerializeField]
    Sprite m_PipeWaypointIcon;
    [SerializeField]
    Vector3 m_PipeWaypointOffset = Vector3.zero;

    [Header("Disconnect")]
    [SerializeField]
    ChaseMinigameInteract m_DisconnectInteractable;
    [SerializeField]
    Sprite m_DisconnectWaypointIcon;
    [SerializeField]
    Vector3 m_DisconnectWaypointOffset = Vector3.zero;

    [Header("Reward")]
    [field: SerializeField, Min(0)]
    public int MoneyReward { get; private set; } = 50;

    [Header("Leave House")]
    [SerializeField]
    TutorialHouseDoor m_TutorialHouseDoor;
    [SerializeField]
    Sprite m_LeaveHouseWaypointIcon;
    [SerializeField]
    Vector3 m_LeaveHouseWaypointOffset = Vector3.zero;

    public UnityEvent OnEnteredHouse;
    public UnityEvent OnTutorialComplete;

    readonly DisplayTask m_TalkDisplayTask = new DisplayTask("Talk to Themba", 1, 0, false);
    readonly DisplayTask m_ElectricalDisplayTask = new DisplayTask("Fix the electrical box", 1, 0, false);
    readonly DisplayTask m_PipeDisplayTask = new DisplayTask("Fix the leaking pipe", 1, 0, false);
    readonly DisplayTask m_DisconnectDisplayTask = new DisplayTask("Disconnect the illegal wiring", 1, 0, false);
    readonly DisplayTask m_LeaveHouseDisplayTask = new DisplayTask("Head outside to go back home", 1, 0, false);

    void Awake()
    {
        if (m_ElectricalBoxInteractable) m_ElectricalBoxInteractable.gameObject.SetActive(false);
        if (m_PipeWallInteractable) m_PipeWallInteractable.gameObject.SetActive(false);
        if (m_DisconnectInteractable) m_DisconnectInteractable.gameObject.SetActive(false);
        if (m_TutorialHouseDoor) m_TutorialHouseDoor.gameObject.SetActive(true);
        SetLights(false);
    }

    void Start()
    {
        if (TutorialMinigameManager.Instance) TutorialMinigameManager.Instance.OnMinigameCompleted += HandleMinigameCompleted;
    }

    void OnDestroy()
    {
        if (TutorialMinigameManager.Instance) TutorialMinigameManager.Instance.OnMinigameCompleted -= HandleMinigameCompleted;
    }

    public void OnPlayerEnteredHouse()
    {
        if (CurrentStage != Stage.NotStarted) return;
        ShowTalkWaypoint();
        OnEnteredHouse?.Invoke();
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
        if (m_NPC) WaypointManager.Instance?.AddWaypoint(m_NPC.transform, m_TalkWaypointIcon, m_TalkWaypointOffset);
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
        foreach (CeilingLight light in m_HouseLights)
        {
            if (!light) continue;
            if (bOn) light.TurnOn();
            else light.TurnOff();
        }
    }

    #region Electrical
    public void OnWelcomeDialogueFinished()
    {
        CurrentStage = Stage.ElectricalPending;
        HideTalkWaypoint();
        if (!m_ElectricalBoxInteractable) return;
        m_ElectricalBoxInteractable.gameObject.SetActive(true);
        WaypointManager.Instance?.AddWaypoint(m_ElectricalBoxInteractable.transform, m_ElectricalWaypointIcon, m_ElectricalWaypointOffset);
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
        WaypointManager.Instance?.AddWaypoint(m_PipeWallInteractable.transform, m_PipeWaypointIcon, m_PipeWaypointOffset);
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
        if (m_TutorialHouseDoor) m_TutorialHouseDoor.gameObject.SetActive(false);
        m_DisconnectInteractable.gameObject.SetActive(true);
        WaypointManager.Instance?.AddWaypoint(m_DisconnectInteractable.transform, m_DisconnectWaypointIcon, m_DisconnectWaypointOffset);
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
        if (m_DisconnectInteractable) m_DisconnectInteractable.gameObject.SetActive(false);
        if (m_TutorialHouseDoor) m_TutorialHouseDoor.gameObject.SetActive(true);
        ShowTalkWaypoint();
    }
    #endregion Disconnect

    #region Reward
    public void OnRewardDialogueFinished()
    {
        CurrentStage = Stage.LeaveHousePending;
        HideTalkWaypoint();
        CurrencyManager.Instance?.ReceiveMoney(MoneyReward);
        if (m_TutorialHouseDoor) WaypointManager.Instance?.AddWaypoint(m_TutorialHouseDoor.transform, m_LeaveHouseWaypointIcon, m_LeaveHouseWaypointOffset);
        TaskList.Instance?.AddTask(m_LeaveHouseDisplayTask);
    }
    #endregion Reward

    #region Leave House
    public void OnLeaveHouseInteracted()
    {
        if (CurrentStage != Stage.LeaveHousePending) return;
        CurrentStage = Stage.Complete;
        if (m_TutorialHouseDoor) WaypointManager.Instance?.RemoveWaypoint(m_TutorialHouseDoor.transform);
        TaskList.Instance?.RemoveTask(m_LeaveHouseDisplayTask);
        OnTutorialComplete?.Invoke();
    }
    #endregion Leave House
}
