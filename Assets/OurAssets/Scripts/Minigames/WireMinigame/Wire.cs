using System.Collections.Generic;
using UnityEngine;

public class WireColour
{
	public static readonly WireColour None = new WireColour();
	public static readonly WireColour Red = new WireColour();
	public static readonly WireColour Green = new WireColour();
	public static readonly WireColour Blue = new WireColour();
	public static readonly WireColour Yellow = new WireColour();

	public static readonly WireColour[] WireColours = new WireColour[5]
	{
		WireColour.None,
		WireColour.Red,
		WireColour.Green,
		WireColour.Blue,
		WireColour.Yellow,
	};

	private WireColour() { }
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
            SetLineEndPosition(value);
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

    public bool CanBeGrabbed { get; private set; } = true;
    bool m_bBeingHeld = false;

    LineRenderer m_Line;

    void Awake()
    {
        m_Line = GetComponent<LineRenderer>();
        m_Line.useWorldSpace = false;
    }

    public void Init(Vector3 startPosition, Vector3 tipStartPosition, Vector3 endPosition, WireColour wireColour)
    {
        StartPosition = startPosition;
        TipStartPosition = tipStartPosition;
        EndPosition = endPosition;
        Colour = wireColour;
        CanBeGrabbed = true;
        m_bBeingHeld = false;
    }

    public void HoldWire(Vector3 holdPosition)
    {
        if (CanBeGrabbed || m_bBeingHeld)
        {
            CanBeGrabbed = false;
            m_bBeingHeld = true;
            SetLineEndPosition(holdPosition);
        }
    }

    public void ReleaseWire(bool snapToEnd)
    {
        m_bBeingHeld = false;
        SetLineEndPosition(snapToEnd ? EndPosition : TipStartPosition);
        CanBeGrabbed = !snapToEnd;
    }

    void SetLineEndPosition(Vector3 endPosition)
    {
        Vector3 localPosition = endPosition - transform.position;
        m_Line.SetPosition(m_Line.positionCount - 1, localPosition);
    }

    void ChangeLineColour(WireColour wireColour)
    {
        Color colour = WireColours[wireColour];
        m_Line.startColor = colour;
        m_Line.endColor = colour;
    }
}
