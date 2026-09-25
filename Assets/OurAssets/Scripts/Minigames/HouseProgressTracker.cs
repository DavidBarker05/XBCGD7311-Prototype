using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HouseProgress
{
    public Vector3 HousePosition;
    public MinigameType[] MinigameTypes;
    public bool[] MinigamesBeaten;
    public int? PendingCompletedSlot;
    public bool IsPlayerInside;
    public DoorType DoorType = DoorType.Entry;
    public bool HasBeatenHouse;
    public bool HasTalkedToNPC;
    public Vector3 PlayerPosition;
    public Quaternion PlayerRotation;
    public Quaternion CameraRotation;

    public bool AllMinigamesBeaten
    {
        get
        {
            foreach (bool beaten in MinigamesBeaten) if (!beaten) return false;
            return true;
        }
    }

    public int CountOfType(MinigameType type)
    {
        int count = 0;
        foreach (MinigameType minigameType in MinigameTypes) if (minigameType == type) ++count;
        return count;
    }

    public void MarkSlotBeaten(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MinigamesBeaten.Length) return;
        MinigamesBeaten[slotIndex] = true;
    }
}

public static class HouseProgressTracker
{
    const float PositionKeyPrecision = 100f;

    static readonly Dictionary<Vector3, HouseProgress> s_Houses = new Dictionary<Vector3, HouseProgress>();

    static Vector3? s_ActiveHousePosition;
    static int? s_ActiveTaskSlotIndex;

    static Vector3 KeyFor(Vector3 housePosition) => new Vector3(
        Mathf.Round(housePosition.x * PositionKeyPrecision) / PositionKeyPrecision,
        Mathf.Round(housePosition.y * PositionKeyPrecision) / PositionKeyPrecision,
        Mathf.Round(housePosition.z * PositionKeyPrecision) / PositionKeyPrecision);

    public static void ClearAll()
    {
        s_Houses.Clear();
        s_ActiveHousePosition = null;
        s_ActiveTaskSlotIndex = null;
    }

    public static HouseProgress GetOrRegisterHouse(Vector3 housePosition, Func<MinigameType[]> generatePlan)
    {
        Vector3 key = KeyFor(housePosition);
        if (s_Houses.TryGetValue(key, out HouseProgress existing)) return existing;

        MinigameType[] plan = generatePlan();
        HouseProgress progress = new HouseProgress()
        {
            HousePosition = housePosition,
            MinigameTypes = plan,
            MinigamesBeaten = new bool[plan.Length]
        };
        s_Houses[key] = progress;
        return progress;
    }

    public static HouseProgress GetHouse(Vector3 housePosition) => s_Houses.TryGetValue(KeyFor(housePosition), out HouseProgress progress) ? progress : null;

    #region Active House
    public static void SetActiveHouse(Vector3? housePosition) => s_ActiveHousePosition = housePosition;

    public static void SetActiveTaskSlot(int? slotIndex) => s_ActiveTaskSlotIndex = slotIndex;

    public static void ReportMinigameCompleted(MinigameType type)
    {
        if (!s_ActiveHousePosition.HasValue) return;
        HouseProgress progress = GetHouse(s_ActiveHousePosition.Value);
        if (progress == null) return;
        int slotIndex = s_ActiveTaskSlotIndex ?? FindFirstUnbeatenSlot(progress, type);
        if (slotIndex < 0) return;
        progress.PendingCompletedSlot = slotIndex;
    }

    static int FindFirstUnbeatenSlot(HouseProgress progress, MinigameType type)
    {
        for (int i = 0; i < progress.MinigameTypes.Length; ++i)
        {
            if (progress.MinigameTypes[i] == type && !progress.MinigamesBeaten[i]) return i;
        }
        return -1;
    }

    public static int? ConsumePendingCompletedSlot(Vector3 housePosition)
    {
        HouseProgress progress = GetHouse(housePosition);
        if (progress == null) return null;
        int? pending = progress.PendingCompletedSlot;
        progress.PendingCompletedSlot = null;
        return pending;
    }
    #endregion Active House

    public static void UpdateActiveHousePlayerTransform(Vector3 playerPosition, Quaternion playerRotation, Quaternion cameraRotation)
    {
        if (!s_ActiveHousePosition.HasValue) return;
        HouseProgress progress = GetHouse(s_ActiveHousePosition.Value);
        if (progress == null) return;
        progress.PlayerPosition = playerPosition;
        progress.PlayerRotation = playerRotation;
        progress.CameraRotation = cameraRotation;
    }
}
