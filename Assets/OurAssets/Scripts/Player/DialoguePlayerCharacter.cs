using UnityEngine;
using Util.SystemUtils;

public class DialoguePlayerCharacterInitData : IPlayerCharacterInitData { }

public class DialoguePlayerCharacterUpdateData : IPlayerCharacterUpdateData
{
    public float DeltaTime { get; set; }
    public Quaternion CameraRotation { get; set; }
    public MouseInfo MouseInfo { get; set; }
}

public class DialoguePlayerCharacter : PlayerCharacter
{
    public override bool HasBeenInitialised { get; protected set; }

    public override string ActionMap => "DialoguePlayer";
    public override bool MouseVisible => true;
    public override bool DoCameraRotation => false;
    public override bool UseMouseScreenPosition => false;

    public override void Init(IPlayerCharacterInitData playerCharacterInitData)
    {
        Sys.AssertType<DialoguePlayerCharacterInitData>(playerCharacterInitData, nameof(playerCharacterInitData));
        HasBeenInitialised = true;
    }

    public override void UpdateCharacter(ref IPlayerCharacterUpdateData playerCharacterUpdateData) => Sys.Assert(HasBeenInitialised, "DialoguePlayerCharacter hasn't been initialised");

    public override void OnPausePressed() => Sys.Assert(HasBeenInitialised, "DialoguePlayerCharacter hasn't been initialised");
}
