using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class DisplayTask
{
    public string Text { get; set; }
    public int AmountNeeded { get; set; }
    public int AmountDone { get; set; }
    public bool StrikeThroughOnCompletion { get; set; }

    public DisplayTask(string text, int amountNeeded, int amountDone, bool strikeThroughOnCompletion)
    {
        Text = text;
        AmountNeeded = amountNeeded;
        AmountDone = amountDone;
        StrikeThroughOnCompletion = strikeThroughOnCompletion;
    }
}

[RequireComponent(typeof(TMP_Text))]
public class TaskList : MonoBehaviour
{
    public static TaskList Instance { get; private set; }

    [SerializeField]
    string m_Heading = "<size=50>Tasks:</size>";

    TMP_Text m_Text;

    readonly List<DisplayTask> m_TasksToDisplay = new List<DisplayTask>();

    void Awake()
    {
        if (Instance && Instance != this) Destroy(gameObject);
        else Instance = this;
        m_Text = GetComponent<TMP_Text>();
        m_Text.text = m_Heading;
    }

    void UpdateText()
    {
        if (m_TasksToDisplay.Capacity == 0)
        {
            m_Text.text = m_Heading;
            return;
        }
        StringBuilder stringBuilder = new StringBuilder(m_Heading);
        foreach (DisplayTask displayTask in m_TasksToDisplay)
        {
            bool bStrike = displayTask.AmountDone == displayTask.AmountNeeded;
            var (strikeStart, strikeEnd) = bStrike ? ("<s>", "</s>") : ("", "");
            bool bDisplayAmountNeeded = displayTask.AmountNeeded > 1;
            string amountDisplay = bDisplayAmountNeeded
                                    ? $" ({displayTask.AmountDone}/{displayTask.AmountNeeded})"
                                    : "";
            string finalText = $"\n- {strikeStart}{displayTask.Text}{amountDisplay}{strikeEnd}";
            stringBuilder.Append(finalText);
        }
        m_Text.text = stringBuilder.ToString();
    }

    bool AddTaskInternal(DisplayTask displayTask)
    {
        if (displayTask == null || m_TasksToDisplay.Contains(displayTask)) return false;
        if (displayTask.AmountDone == displayTask.AmountNeeded && !displayTask.StrikeThroughOnCompletion)
            return false;
        m_TasksToDisplay.Add(displayTask);
        return true;
    }

    public void AddTask(DisplayTask displayTask)
    {
        if (AddTaskInternal(displayTask)) UpdateText();
    }

    public void AddTasks(DisplayTask[] displayTasks)
    {
        if (displayTasks == null || displayTasks.Length == 0) return;
        bool bUpdateText = false;
        foreach (DisplayTask displayTask in displayTasks) bUpdateText |= AddTaskInternal(displayTask);
        if (bUpdateText) UpdateText();
    }

    public void RemoveTask(DisplayTask displayTask)
    {
        if (m_TasksToDisplay.Remove(displayTask)) UpdateText();
    }

    public void RemoveTasks(DisplayTask[] displayTasks)
    {
        bool bUpdateText = false;
        foreach (DisplayTask displayTask in displayTasks)
            bUpdateText |= m_TasksToDisplay.Remove(displayTask);
        if (bUpdateText) UpdateText();
    }

    bool IncrementAmountDoneForTaskInternal(DisplayTask displayTask)
    {
        if (displayTask == null) return false;
        ++displayTask.AmountDone;
        int index = m_TasksToDisplay.IndexOf(displayTask);
        if (index == -1) return false;
        if (displayTask.AmountDone == displayTask.AmountNeeded && !displayTask.StrikeThroughOnCompletion)
            m_TasksToDisplay.RemoveAt(index);
        return true;
    }

    public void IncrementAmountDoneForTask(DisplayTask displayTask)
    {
        if (IncrementAmountDoneForTaskInternal(displayTask)) UpdateText();
    }

    public void IncrementAmountsDoneForTasks(DisplayTask[] displayTasks)
    {
        if (displayTasks == null || displayTasks.Length == 0) return;
        bool bUpdateText = false;
        foreach (DisplayTask displayTask in displayTasks)
            bUpdateText |= IncrementAmountDoneForTaskInternal(displayTask);
        if (bUpdateText) UpdateText();
    }

    public void ClearAllTasks()
    {
        if (m_TasksToDisplay.Count == 0) return;
        m_TasksToDisplay.Clear();
        m_Text.text = m_Heading;
    }
}
