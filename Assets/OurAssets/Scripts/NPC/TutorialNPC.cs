using UnityEngine;

public class TutorialNPC : Interactable
{
    [SerializeField]
    TutorialHouse m_TutorialHouse;
    [Header("Dialogue")]
    [SerializeField]
    TextAsset m_WelcomeDialogue;
    [SerializeField]
    TextAsset m_ElectricalReminderDialogue;
    [SerializeField]
    TextAsset m_PipeIntroDialogue;
    [SerializeField]
    TextAsset m_PipeReminderDialogue;
    [SerializeField]
    TextAsset m_DisconnectIntroDialogue;
    [SerializeField]
    TextAsset m_DisconnectReminderDialogue;
    [SerializeField]
    TextAsset m_RewardDialogue;
    [SerializeField]
    TextAsset m_LeaveHouseReminderDialogue;
    [SerializeField]
    TextAsset m_OutroDialogue;

    public override InteractionStatus Interact(params object[] inputParameters)
    {
        if (inputParameters.Length != 0)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"WARNING: TutorialNPC objects needs 0 input parameters. Received {inputParameters.Length} input parameters");
#endif
            return new InteractionStatus() { EndInteraction = true };
        }

        (TextAsset dialogueAsset, System.Action onFinished) = m_TutorialHouse.CurrentStage switch
        {
            TutorialHouse.Stage.NotStarted => (m_WelcomeDialogue, (System.Action)m_TutorialHouse.OnWelcomeDialogueFinished),
            TutorialHouse.Stage.ElectricalPending => (m_ElectricalReminderDialogue, null),
            TutorialHouse.Stage.ElectricalDone => (m_PipeIntroDialogue, (System.Action)m_TutorialHouse.OnPipeDialogueFinished),
            TutorialHouse.Stage.PipePending => (m_PipeReminderDialogue, null),
            TutorialHouse.Stage.PipeDone => (m_DisconnectIntroDialogue, (System.Action)m_TutorialHouse.OnDisconnectDialogueFinished),
            TutorialHouse.Stage.DisconnectPending => (m_DisconnectReminderDialogue, null),
            TutorialHouse.Stage.DisconnectDone => (m_RewardDialogue, (System.Action)m_TutorialHouse.OnRewardDialogueFinished),
            TutorialHouse.Stage.LeaveHousePending => (m_LeaveHouseReminderDialogue, null),
            TutorialHouse.Stage.Complete => (m_OutroDialogue, null),
            _ => (null, null)
        };
        if (dialogueAsset)
        {
            Dialogue dialogue = JsonUtility.FromJson<SerializedDialogue>(dialogueAsset.text).Deserialized;
            DialogueManager.Instance.StartDialogue(dialogue, onFinished);
        }
#if UNITY_EDITOR
        else Debug.LogWarning($"WARNING: {name} has no dialogue assigned for stage \"{m_TutorialHouse.CurrentStage}\"");
#endif
        return new InteractionStatus() { EndInteraction = true };
    }
}
