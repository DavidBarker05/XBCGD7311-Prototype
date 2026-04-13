using System.Collections.Generic;
using UnityEngine;

public enum PipeSide
{
    Left,
    Top,
    Right,
    Bottom
}

[System.Serializable]
public enum PipeRotationAngle
{
    Zero = 0,
    Ninety = 90,
    OneEighty = 180,
    TwoSeventy = 270
}

public static class PipeRotationAngleUtil
{
    public static PipeRotationAngle NextAngleRight(PipeRotationAngle angle) => angle switch
    {
        PipeRotationAngle.Zero => PipeRotationAngle.Ninety,
        PipeRotationAngle.Ninety => PipeRotationAngle.OneEighty,
        PipeRotationAngle.OneEighty => PipeRotationAngle.TwoSeventy,
        PipeRotationAngle.TwoSeventy => PipeRotationAngle.Zero,
        _ => throw new System.ArgumentException("Somehow you put in an invalid valid angle")
    };

    public static PipeRotationAngle NextAngleLeft(PipeRotationAngle angle) => angle switch
    {
        PipeRotationAngle.Zero => PipeRotationAngle.TwoSeventy,
        PipeRotationAngle.Ninety => PipeRotationAngle.Zero,
        PipeRotationAngle.OneEighty => PipeRotationAngle.Ninety,
        PipeRotationAngle.TwoSeventy => PipeRotationAngle.OneEighty,
        _ => throw new System.ArgumentException("Somehow you put in an invalid valid angle")
    };
}

[System.Serializable]
public struct PipeOrientation
{
    [field: SerializeField]
    public bool HasLeftHole { get; private set; }
    [field: SerializeField]
    public bool HasTopHole { get; private set; }
    [field: SerializeField]
    public bool HasRightHole { get; private set; }
    [field: SerializeField]
    public bool HasBottomHole { get; private set; }

    public PipeOrientation(bool bLeft, bool bTop, bool bRight, bool bBottom)
    {
        HasLeftHole = bLeft;
        HasTopHole = bTop;
        HasRightHole = bRight;
        HasBottomHole = bBottom;
    }

    public bool HasHole(PipeSide side) => side switch
    {
        PipeSide.Left => HasLeftHole,
        PipeSide.Top => HasTopHole,
        PipeSide.Right => HasRightHole,
        PipeSide.Bottom => HasBottomHole,
        _ => throw new System.ArgumentException("Somehow you input a side that doesn't exist")
    };

    public PipeOrientation NextRightOrientation()
    {
        bool bLeft = HasBottomHole;
        bool bTop = HasLeftHole;
        bool bRight = HasTopHole;
        bool bBottom = HasRightHole;
        return new PipeOrientation(bLeft, bTop, bRight, bBottom);
    }

    public PipeOrientation NextLeftOrientation()
    {
        bool bLeft = HasTopHole;
        bool bTop = HasRightHole;
        bool bRight = HasBottomHole;
        bool bBottom = HasLeftHole;
        return new PipeOrientation(bLeft, bTop, bRight, bBottom);
    }
}

[CreateAssetMenu(fileName = "PipeSO", menuName = "Pipe Place Minigame/PipeSO")]
public class PipeSO : ScriptableObject
{
    [field: SerializeField]
    public GameObject Model { get; private set; }
    [SerializeField]
    PipeOrientation m_DefaultOrientation;

    Dictionary<PipeRotationAngle, PipeOrientation> m_PipeOrientations = new Dictionary<PipeRotationAngle, PipeOrientation>();

    void OnValidate() => PopulateDictionary();

    void OnEnable() => PopulateDictionary();

    public PipeOrientation GetOrientationFromAngle(PipeRotationAngle angle) => m_PipeOrientations[angle];

    PipeOrientation NextOrientation(PipeRotationAngle previousAngle) => m_PipeOrientations[previousAngle].NextRightOrientation();

    void PopulateDictionary()
    {
        m_PipeOrientations.Clear();
        m_PipeOrientations.Add(PipeRotationAngle.Zero, m_DefaultOrientation);
        m_PipeOrientations.Add(PipeRotationAngle.Ninety, NextOrientation(PipeRotationAngle.Zero));
        m_PipeOrientations.Add(PipeRotationAngle.OneEighty, NextOrientation(PipeRotationAngle.Ninety));
        m_PipeOrientations.Add(PipeRotationAngle.TwoSeventy, NextOrientation(PipeRotationAngle.OneEighty));
    }
}
