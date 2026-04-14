using UnityEngine;
using Util;

public class WirePlayerCharacterInitData : IPlayerCharacterInitData { }

public class WirePlayerCharacterUpdateData : IPlayerCharacterUpdateData
{
    public float DeltaTime { get; set; }
    public Quaternion CameraRotation { get; set; }
    public MouseInfo MouseInfo { get; set; }
    
    public bool ClickedThisFrame { get; set; }
}

public class WirePlayerCharacter : PlayerCharacter
{
    public override bool MouseVisible => true;
    public override bool DoCameraRotation => false;
    public override bool UseMouseScreenPosition => true;

    Camera m_Camera;
    WireBoard m_CurrentWireBoard = null;
    Wire m_CurrentlyHeldWire = null;

    public override void Init(IPlayerCharacterInitData playerCharacterInitData)
    {
        Sys.Assert(playerCharacterInitData is WirePlayerCharacterInitData, "playerCharacterInitData must be type WirePlayerCharacterInitData");
    }

    public override void UpdateCharacter(ref IPlayerCharacterUpdateData playerCharacterUpdateData)
    {
        Sys.Assert(playerCharacterUpdateData is WirePlayerCharacterUpdateData, "playerCharacterUpdateData must be type WirePlayerCharacterUpdateData");
        if (playerCharacterUpdateData is not WirePlayerCharacterUpdateData input) return;
        if (!input.MouseInfo.DidHitObject)
        {
            if (m_CurrentlyHeldWire) ReleaseWire(Vector3.negativeInfinity);
            return;
        }
        if (!input.ClickedThisFrame)
        {
            if (m_CurrentlyHeldWire) ReleaseWire(input.MouseInfo.HitInfo.point);
            return;
        }
        if (m_CurrentlyHeldWire) HoldWire(input.MouseInfo.HitInfo.point);
        else
        {
            m_CurrentWireBoard = input.MouseInfo.HitInfo.GetComponent<WireBoard>();
            GrabWire(input.MouseInfo.HitInfo.point);
        }
    }

    void GrabWire(Vector3 position)
    {
        if (!m_CurrentWireBoard) return;
        m_CurrentlyHeldWire = m_CurrentWireBoard.TryGrabWire(position);
        if (!m_CurrentlyHeldWire) m_CurrentWireBoard = null;
        else m_CurrentlyHeldWire.HoldWire(position);
    }

    void HoldWire(Vector3 position)
    {
        if (!m_CurrentWireBoard)
        {
            if (m_CurrentlyHeldWire) ReleaseWire(Vector3.negativeInfinity);
            return;
        }
        if (m_CurrentlyHeldWire) m_CurrentlyHeldWire.HoldWire(position);
    }

    void ReleaseWire(Vector3 position)
    {
        if (!m_CurrentWireBoard || !m_CurrentlyHeldWire) return;
        if (position == Vector3.negativeInfinity) m_CurrentlyHeldWire.ReleaseWire(false);
        else m_CurrentlyHeldWire.ReleaseWire(m_CurrentWireBoard.TryReleaseWire(position));
        m_CurrentWireBoard = null;
        m_CurrentlyHeldWire = null;
    }
}
