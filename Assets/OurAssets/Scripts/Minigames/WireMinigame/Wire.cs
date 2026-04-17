using System.Collections.Generic;
using UnityEngine;
using Util.UnityUtils;

public class WireColour
{
	public static readonly WireColour None = new WireColour(Color.black);
	public static readonly WireColour Red = new WireColour(Color.red);
	public static readonly WireColour Green = new WireColour(Color.green);
	public static readonly WireColour Blue = new WireColour(Color.blue);
	public static readonly WireColour Yellow = new WireColour(Color.yellow);
	public static readonly WireColour Magenta = new WireColour(Color.magenta);

	public static readonly WireColour[] WireColours = new WireColour[]
	{
		None,
		Red,
		Green,
		Blue,
		Yellow,
		Magenta
	};

	public Color Colour { get; private set; }

	private WireColour(Color colour) { Colour = colour; }
}

[RequireComponent(typeof(LineRenderer))]
public class Wire : MonoBehaviour
{
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

    WireColour m_WireColour = WireColour.None;
    public WireColour Colour
    {
        get => m_WireColour;
        set
        {
			m_Line.material.color = value.Colour;
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

    public void Init(Vector3 startPosition, Vector3 tipStartPosition, WireColour wireColour)
    {
        StartPosition = startPosition;
        TipStartPosition = tipStartPosition;
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

    public void ReleaseWire(Vector3 snapPosition)
    {
        m_bBeingHeld = false;
		if (snapPosition.IsNegativeInfinity()) SetLineEndPosition(TipStartPosition);
		else SetLineEndPosition(snapPosition);
        CanBeGrabbed = snapPosition.IsNegativeInfinity();
    }

    void SetLineEndPosition(Vector3 endPosition)
    {
		if (!m_Line) m_Line = GetComponent<LineRenderer>();
        Vector3 localPosition = endPosition - transform.position;
        m_Line.SetPosition(m_Line.positionCount - 1, localPosition);
    }
}
