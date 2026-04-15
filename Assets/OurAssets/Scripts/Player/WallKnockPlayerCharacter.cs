using UnityEngine;
using Util.SystemUtils;

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
	public override bool MouseVisible => throw new System.NotImplementedException();

	public override bool DoCameraRotation => throw new System.NotImplementedException();

	public override bool UseMouseScreenPosition => throw new System.NotImplementedException();

	public override void Init(IPlayerCharacterInitData playerCharacterInitData)
	{
		Sys.AssertType<WallKnockPlayerCharacterInitData>(playerCharacterInitData, nameof(playerCharacterInitData));
	}

	public override void UpdateCharacter(ref IPlayerCharacterUpdateData playerCharacterUpdateData)
	{
		Sys.AssertType<WallKnockPlayerCharacterUpdateData>(playerCharacterUpdateData, nameof(playerCharacterUpdateData));
		if (playerCharacterUpdateData is not WallKnockPlayerCharacterUpdateData input) return;
	}
}
