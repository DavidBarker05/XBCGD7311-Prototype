using UnityEngine;

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

    public override void Init(IPlayerCharacterInitData playerCharacterInitData)
    {
        CustomUtils.Sys.Assert(playerCharacterInitData is WirePlayerCharacterInitData, "playerCharacterInitData must be type WirePlayerCharacterInitData");
    }

    public override void UpdateCharacter(ref IPlayerCharacterUpdateData playerCharacterUpdateData)
    {
        CustomUtils.Sys.Assert(playerCharacterUpdateData is WirePlayerCharacterUpdateData, "playerCharacterUpdateData must be type WirePlayerCharacterUpdateData");
    }
}
