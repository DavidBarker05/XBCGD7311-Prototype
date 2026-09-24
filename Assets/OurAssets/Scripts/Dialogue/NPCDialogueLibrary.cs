using System.Collections.Generic;
using UnityEngine;

public static class NPCDialogueLibrary
{
    static readonly string[] s_Names =
    {
        "Sipho", "Thabo", "Lindani", "Bongani", "Kagiso", "Mandla", "Sizwe",
        "Andile", "Vusi", "Jabulani", "Kabelo", "Tumelo", "Nkosi", "Lwazi"
    };

    static readonly string[] s_GreetingTemplates =
    {
        "Oh, hi there!.",
        "Hey, thanks for stopping by.",
        "Howzit!",
        "Hello!",
        "Ah, hi.",
    };

    static readonly string[] s_TaskIntroSingular =
    {
        "I could really use a hand with {0}.",
        "Would you mind sorting out {0} for me?",
        "There's just {0} that needs looking at.",
        "I've been meaning to fix {0}. Think you could help?",
    };

    static readonly string[] s_TaskIntroPlural =
    {
        "I could really use a hand with a few things: {0}.",
        "There's quite a bit to sort out {0}.",
        "Would you mind helping me out with {0}?",
        "I've been putting off {0}, could you take a look?",
    };

    static readonly string[] s_ReminderSingular =
    {
        "Still hoping you can sort out {0} for me.",
        "{0} still needs doing, whenever you get a chance.",
        "No rush, but {0} is still waiting on you.",
    };

    static readonly string[] s_ReminderPlural =
    {
        "Still hoping you can help with {0}.",
        "{0} still on the list, whenever you're ready.",
        "No rush, but I could still use a hand with {0}.",
    };

    static readonly string[] s_ThanksLines =
    {
        "Thanks again for helping me out today!",
        "You're a lifesaver, thank you!",
        "I really appreciate you sorting that out for me.",
        "Thanks so much, couldn't have done it without you.",
    };

    public static string RandomName() => s_Names[Random.Range(0, s_Names.Length)];

    static string RandomOf(string[] options) => options[Random.Range(0, options.Length)];

    static string TaskNoun(MinigameType type) => type switch
    {
        MinigameType.Wires => "the electrical wiring",
        MinigameType.WallKnockAndPipes => "the wall and pipes",
        MinigameType.ChaseMinigame => "the illegal connections to my house",
        _ => "something around here"
    };

    static string JoinTaskPhrases(List<string> phrases)
    {
        if (phrases.Count == 1) return phrases[0];
        return string.Join(", ", phrases.GetRange(0, phrases.Count - 1)) + " and " + phrases[^1];
    }

    static List<string> OutstandingTaskPhrases(HouseProgress progress)
    {
        HashSet<MinigameType> seen = new HashSet<MinigameType>();
        List<string> phrases = new List<string>();
        for (int i = 0; i < progress.MinigameTypes.Length; ++i)
        {
            if (progress.MinigamesBeaten[i]) continue;
            MinigameType type = progress.MinigameTypes[i];
            if (!seen.Add(type)) continue;
            phrases.Add(TaskNoun(type));
        }
        return phrases;
    }

    public static string RandomGreeting() => RandomOf(s_GreetingTemplates);

    public static string RandomTaskIntro(HouseProgress progress)
    {
        List<string> phrases = OutstandingTaskPhrases(progress);
        string joined = JoinTaskPhrases(phrases);
        return string.Format(RandomOf(phrases.Count > 1 ? s_TaskIntroPlural : s_TaskIntroSingular), joined);
    }

    public static string RandomReminder(HouseProgress progress)
    {
        List<string> phrases = OutstandingTaskPhrases(progress);
        string joined = JoinTaskPhrases(phrases);
        return string.Format(RandomOf(phrases.Count > 1 ? s_ReminderPlural : s_ReminderSingular), joined);
    }

    public static string RandomThanks() => RandomOf(s_ThanksLines);
}
