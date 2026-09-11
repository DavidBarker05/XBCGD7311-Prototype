using UnityEngine;
using Util.SystemUtils;
using Util.UnityUtils;

public class WirePlayerCharacterInitData : IPlayerCharacterInitData
{
    public PauseCharacter PauseCharacter { get; set; }
}

public class WirePlayerCharacterUpdateData : IPlayerCharacterUpdateData
{
    public float DeltaTime { get; set; }
    public Quaternion CameraRotation { get; set; }
    public MouseInfo MouseInfo { get; set; }

    public bool ClickedThisFrame { get; set; }
}

public class WirePlayerCharacter : PlayerCharacter
{
    public override bool HasBeenInitialised { get; protected set; }

    public override string ActionMap => "WirePlayer";
    public override bool MouseVisible => true;
    public override bool DoCameraRotation => false;
    public override bool UseMouseScreenPosition => true;

    PauseCharacter m_PauseCharacter;

    WireBoard m_CurrentWireBoard = null;
    Wire m_CurrentlyHeldWire = null;

    public override void Init(IPlayerCharacterInitData playerCharacterInitData)
    {
        WirePlayerCharacterInitData initData = Sys.AssertType<WirePlayerCharacterInitData>(playerCharacterInitData, nameof(playerCharacterInitData));
        m_PauseCharacter = initData.PauseCharacter;
        HasBeenInitialised = true;
    }

    public override void UpdateCharacter(ref IPlayerCharacterUpdateData playerCharacterUpdateData)
    {
        Sys.Assert(HasBeenInitialised, "WirePlayerCharacter hasn't been initialised");
        WirePlayerCharacterUpdateData updateData = Sys.AssertType<WirePlayerCharacterUpdateData>(playerCharacterUpdateData, nameof(playerCharacterUpdateData));
        if (!updateData.MouseInfo.DidHitObject)
        {
            if (m_CurrentlyHeldWire) ReleaseWire(Vector3.negativeInfinity);
            return;
        }
        if (!updateData.ClickedThisFrame)
        {
            if (m_CurrentlyHeldWire) ReleaseWire(updateData.MouseInfo.HitInfo.point);
            return;
        }
        if (m_CurrentlyHeldWire) HoldWire(updateData.MouseInfo.HitInfo.point);
        else
        {
            m_CurrentWireBoard = updateData.MouseInfo.HitInfo.GetComponent<WireBoard>();
            GrabWire(updateData.MouseInfo.HitInfo.point);
        }
    }

    public override void OnPausePressed()
    {
        Sys.Assert(HasBeenInitialised, "WirePlayerCharacter hasn't been initialised");
        if (!m_CurrentlyHeldWire) m_PauseCharacter.PauseGame(this);
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
        if (position.IsNegativeInfinity())
            m_CurrentlyHeldWire.ReleaseWire(new WireReleaseInfo() { ReleaseStatus = WireReleaseStatus.SnapToStart });
        else
            m_CurrentlyHeldWire.ReleaseWire(m_CurrentWireBoard.TryReleaseWire(m_CurrentlyHeldWire, position));
        m_CurrentWireBoard = null;
        m_CurrentlyHeldWire = null;
    }
}
