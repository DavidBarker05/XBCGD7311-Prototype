using UnityEngine;
using Util.SystemUtils;
using Util.UnityUtils;

class WallKnockPlayerCharacterInitData : IPlayerCharacterInitData { }

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

	public override bool MouseVisible => true;
	public override bool DoCameraRotation => false;
	public override bool UseMouseScreenPosition => true;

	public override void Init(IPlayerCharacterInitData playerCharacterInitData)
	{
		WallKnockPlayerCharacterInitData initData = Sys.AssertType<WallKnockPlayerCharacterInitData>(playerCharacterInitData, nameof(playerCharacterInitData));
		HasBeenInitialised = true;
	}

	public override void UpdateCharacter(ref IPlayerCharacterUpdateData playerCharacterUpdateData)
	{
		Sys.Assert(HasBeenInitialised, "WallKnockPlayerCharacter hasn't been initialised");
		WallKnockPlayerCharacterUpdateData input = Sys.AssertType<WallKnockPlayerCharacterUpdateData>(playerCharacterUpdateData, nameof(playerCharacterUpdateData));
		if (!input.MouseInfo.DidHitObject) return;
		RaycastHit hit = input.MouseInfo.HitInfo;
		Wall wall = hit.GetComponent<Wall>();
		if (!wall) return;
		if (input.LeftClickedThisFrame) wall.KnockWall(hit.point);
		if (input.RightClickedThisFrame) wall.BreakWall(hit.point);
		input.LeftClickedThisFrame = false;
		input.RightClickedThisFrame = false;
	}
}
