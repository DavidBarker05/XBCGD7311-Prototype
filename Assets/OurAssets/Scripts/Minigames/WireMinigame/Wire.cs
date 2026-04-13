using System.Collections.Generic;
using UnityEngine;

public enum WireColour
{
    None,
    Red,
    Green,
    Blue,
    Yellow
}

[RequireComponent(typeof(LineRenderer))]
public class Wire : MonoBehaviour
{
    public static readonly Dictionary<WireColour, Color> WireColours = new Dictionary<WireColour, Color>()
    {
        { WireColour.None, Color.black },
        { WireColour.Red, Color.red },
        { WireColour.Green, Color.green },
        { WireColour.Blue, Color.blue },
        { WireColour.Yellow, Color.yellow },
    };

    Vector3 m_StartPosition;
    public Vector3 StartPosition
    {
        get => m_StartPosition;
        set
        {
            transform.position = value;
            m_StartPosition = value;
        }
    }

    Vector3 m_TipStartPosition;
    public Vector3 TipStartPosition
    {
        get => m_TipStartPosition;
        set
        {
            m_TipStartPosition = value;
        }
    }

    public Vector3 EndPosition { get; set; }

    WireColour m_WireColour = WireColour.None;
    public WireColour Colour
    {
        get => m_WireColour;
        set
        {
            ChangeLineColour(value);
            m_WireColour = value;
        }
    }

    public bool CanBeGrabbed { get; set; } = true;
    public bool BeingHeld { get; set; } = false;

    LineRenderer m_Line;

    void Awake() => m_Line = GetComponent<LineRenderer>();
    public void Init(Vector3 startPosition, Vector3 tipStartPosition, Vector3 endPosition, WireColour wireColour)
    {
        StartPosition = startPosition;
        TipStartPosition = tipStartPosition;
        EndPosition = endPosition;
        Colour = wireColour;
        CanBeGrabbed = true;
        BeingHeld = false;
    }

    public void HoldWire(Vector3 holdPosition)
    {
        if (CanBeGrabbed || BeingHeld)
        {
            if (CanBeGrabbed) CanBeGrabbed = false;
            SetLineEndPosition(holdPosition);
        }
    }

    public void ReleaseWire(bool snapToEnd)
    {
        BeingHeld = false;
        SetLineEndPosition(snapToEnd ? EndPosition : TipStartPosition);
        CanBeGrabbed = !snapToEnd;
    }

    void SetLineEndPosition(Vector3 endPosition)
    {
        Vector3 lp = endPosition - m_StartPosition;
        m_Line.SetPosition(m_Line.positionCount - 1, lp);
    }

    void ChangeLineColour(WireColour wireColour)
    {
        Color colour = WireColours[wireColour];
        m_Line.startColor = colour;
        m_Line.endColor = colour;
    }
}
