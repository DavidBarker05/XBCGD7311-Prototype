using UnityEngine;
using Util.SystemUtils;
using Util.UnityUtils;

class WallKnockPlayerCharacterInitData : IPlayerCharacterInitData
{
	public PauseCharacter PauseCharacter { get; set; }
}

class WallKnockPlayerCharacterUpdateData : IPlayerCharacterUpdateData
{
	public float DeltaTime { get; set; }
	public Quaternion CameraRotation { get; set; }
	public MouseInfo MouseInfo { get; set; }

	public bool LeftClickedThisFrame { get; set; }
	public bool RightClickedThisFrame { get; set; }
}

public class WallKnockPlayerCharacter : PlayerCharacter
{
	public override bool HasBeenInitialised { get; protected set; }

	public override string ActionMap => "WallKnockPlayer";
	public override bool MouseVisible => true;
	public override bool DoCameraRotation => false;
	public override bool UseMouseScreenPosition => true;

	PauseCharacter m_PauseCharacter;

	public override void Init(IPlayerCharacterInitData playerCharacterInitData)
	{
		WallKnockPlayerCharacterInitData initData = Sys.AssertType<WallKnockPlayerCharacterInitData>(playerCharacterInitData, nameof(playerCharacterInitData));
		m_PauseCharacter = initData.PauseCharacter;
		HasBeenInitialised = true;
	}

	public override void UpdateCharacter(ref IPlayerCharacterUpdateData playerCharacterUpdateData)
	{
		Sys.Assert(HasBeenInitialised, "WallKnockPlayerCharacter hasn't been initialised");
		WallKnockPlayerCharacterUpdateData updateData = Sys.AssertType<WallKnockPlayerCharacterUpdateData>(playerCharacterUpdateData, nameof(playerCharacterUpdateData));
		if (!updateData.MouseInfo.DidHitObject) return;
		RaycastHit hit = updateData.MouseInfo.HitInfo;
		Wall wall = hit.GetComponent<Wall>();
		if (!wall) return;
		if (updateData.LeftClickedThisFrame) wall.KnockWall(hit.point);
		if (updateData.RightClickedThisFrame) wall.BreakWall(hit.point);
		updateData.LeftClickedThisFrame = false;
		updateData.RightClickedThisFrame = false;
	}

	public override void OnPausePressed()
	{
		Sys.Assert(HasBeenInitialised, "WallKnockPlayerCharacter hasn't been initialised");
		m_PauseCharacter.PauseGame(this);
	}
}
