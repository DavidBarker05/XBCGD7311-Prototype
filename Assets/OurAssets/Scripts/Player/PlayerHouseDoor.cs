using UnityEngine;
using UnityEngine.Events;

public class PlayerHouseDoor : Interactable
{
    [SerializeField]
    Transform m_InsideSpot;
    [SerializeField]
    Transform m_OutsideSpot;

    [Header("Day 10 ending")]
    [SerializeField]
    GameObject m_WalkOutEndingScreen;
    [SerializeField]
    MenuCharacter m_MenuCharacter;
    [SerializeField]
    GameObject m_HUD;

    public UnityEvent OnPlayerLeft;

    bool m_bPlayerInside = true;
    int m_LastMarkersShownDay = int.MinValue;

    public override InteractionStatus Interact(params object[] inputParameters)
    {
        if (inputParameters.Length != 1)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"WARNING: PlayerHouseDoor objects needs 1 input parameter. Received {inputParameters.Length} input parameters");
#endif
        }
        else if (inputParameters[0] is FirstPersonPlayerCharacter player)
        {
            if (m_bPlayerInside && EndingNPC.Instance && EndingNPC.Instance.IsEndingDay && !EndingNPC.Instance.HasTalkedToday)
            {
#if UNITY_EDITOR
                Debug.Log("Door is locked until you've spoken to Themba");
#endif
                return new InteractionStatus() { EndInteraction = true };
            }

            bool bChoosingToLeave = m_bPlayerInside && EndingNPC.Instance && EndingNPC.Instance.IsChoiceActive;
            if (bChoosingToLeave) EndingNPC.Instance.NotifyEndingChosen();

            Transform destination = m_bPlayerInside ? m_OutsideSpot : m_InsideSpot;
            if (destination)
            {
                CharacterController cc = player.GetComponent<CharacterController>();
                cc.enabled = false;
                player.transform.SetPositionAndRotation(destination.position, destination.rotation);
                cc.enabled = true;
            }
            m_bPlayerInside = !m_bPlayerInside;
            if (!m_bPlayerInside)
            {
                TryShowDailyHouseMarkers();
                Debug.Log($"[PlayerHouseDoor] leaving, frame={Time.frameCount} time={Time.realtimeSinceStartup:F3} OnPlayerLeft listeners={OnPlayerLeft?.GetPersistentEventCount()}");
                OnPlayerLeft?.Invoke();
                Debug.Log($"[PlayerHouseDoor] OnPlayerLeft.Invoke() returned, frame={Time.frameCount}");
            }

            if (bChoosingToLeave) m_MenuCharacter.OnMenuOpen(player, m_HUD, m_WalkOutEndingScreen);
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning($"WARNING: PlayerHouseDoor expects FirstPersonPlayerCharacter as inputParameters[0]. Received type {inputParameters[0].GetType()}");
#endif
        }
        return new InteractionStatus() { EndInteraction = true };
    }

    void TryShowDailyHouseMarkers()
    {
        int day = PlayerSaveManager.CurrentSaveData?.DayNumber ?? int.MinValue;
        if (day == m_LastMarkersShownDay) return;
        m_LastMarkersShownDay = day;
        NPCHouseDailyManager.Instance?.OnPlayerLeftOwnHouse();
    }
}
